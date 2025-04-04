// reparse.rs
// Calls Win32 to find out whether a path is a NTFS reparse point (typically symbolic link or junction)
// Big help from ChatGPT for the code, while Gemini was hallucinating inexistent functions and useless
//
// 2025-04-03   PV      First version

use std::ffi::OsStr;
use std::fs::OpenOptions;
use std::io;
use std::os::raw::c_void;
use std::os::windows::ffi::OsStrExt;
use std::os::windows::fs::OpenOptionsExt;
use std::os::windows::io::AsRawHandle;
use std::path::Path;
use windows::{
    Win32::Foundation::{HANDLE, INVALID_HANDLE_VALUE},
    Win32::Storage::FileSystem::{FILE_ATTRIBUTE_REPARSE_POINT, GetFileAttributesW, MAXIMUM_REPARSE_DATA_BUFFER_SIZE},
    Win32::System::IO::DeviceIoControl,
    core::PCWSTR,
};

// Define used constants
const FSCTL_GET_REPARSE_POINT: u32 = 0x900a8;
const IO_REPARSE_TAG_MOUNT_POINT: u32 = 0xA0000003;
const IO_REPARSE_TAG_SYMLINK: u32 = 0xA000000C;
const FILE_FLAG_OPEN_REPARSE_POINT: u32 = 0x00200000;
const FILE_FLAG_BACKUP_SEMANTICS: u32 = 0x02000000;

// Windows file attributes (https://www.pinvoke.dev/filesystem/file_flags_and_attributes)
const FILE_ATTRIBUTE_READONLY: u32 = 0x00000001;
const FILE_ATTRIBUTE_HIDDEN: u32 = 0x00000002;
const FILE_ATTRIBUTE_SYSTEM: u32 = 0x00000004;
const FILE_ATTRIBUTE_DIRECTORY: u32 = 0x00000010;
const FILE_ATTRIBUTE_ARCHIVE: u32 = 0x00000020;
const FILE_ATTRIBUTE_DEVICE: u32 = 0x00000040;
const FILE_ATTRIBUTE_NORMAL: u32 = 0x00000080;
const FILE_ATTRIBUTE_TEMPORARY: u32 = 0x00000100;
const FILE_ATTRIBUTE_SPARSE_FILE: u32 = 0x00000200;
const FILE_ATTRIBUTE_REPARSE_POINT_FLAG: u32 = 0x00000400; // Added _FLAG to avoid conflict
const FILE_ATTRIBUTE_COMPRESSED: u32 = 0x00000800;
const FILE_ATTRIBUTE_OFFLINE: u32 = 0x00001000;
const FILE_ATTRIBUTE_NOT_CONTENT_INDEXED: u32 = 0x00002000;
const FILE_ATTRIBUTE_ENCRYPTED: u32 = 0x00004000;
const FILE_ATTRIBUTE_INTEGRITY_STREAM: u32 = 0x00008000;
const FILE_ATTRIBUTE_VIRTUAL: u32 = 0x00010000;
const FILE_ATTRIBUTE_NO_SCRUB_DATA: u32 = 0x00020000;
const FILE_ATTRIBUTE_EA: u32 = 0x00040000;
const FILE_ATTRIBUTE_PINNED: u32 = 0x00080000;
const FILE_ATTRIBUTE_UNPINNED: u32 = 0x00100000;
const FILE_ATTRIBUTE_RECALL_ON_OPEN: u32 = 0x00040000;
const FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS: u32 = 0x00400000;

/// Values returned by reparse_type
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
#[repr(u32)]
#[allow(non_camel_case_types)]
pub enum ReparseType {
    NO_REPARSE = 0,
    STUB = 1,               // Not actually a reparse point, but acts like one...  For OneDrive stubs
    SYMLINK = 2,
    JUNCTION = 3,
    OTHER = 4,
}

#[repr(C)]
struct ReparseDataBuffer {
    reparse_tag: u32,
    reparse_data_length: u16,
    reserved: u16,
    data_buffer: [u8; 1], // Placeholder for variable-length data
}

pub fn reparse_type(path: &Path) -> Result<ReparseType, String> {
    // WARNING!
    // Can't use path.metadata().unwrap().file_attributes()... since REPARSE_POINT flag is always 0!!!!  Why?
    // Convert path to wide string
    let path_wide: Vec<u16> = path.as_os_str().encode_wide().chain(std::iter::once(0)).collect();

    // Check if path has FILE_ATTRIBUTE_REPARSE_POINT
    let attributes = unsafe { GetFileAttributesW(PCWSTR(path_wide.as_ptr())) };
    if attributes == u32::MAX {
        return Err(io::Error::last_os_error().to_string());
    }
    if attributes & FILE_ATTRIBUTE_REPARSE_POINT.0 == 0 {
    // OneDrive stubs have flag FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x00400000
    if attributes & FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS != 0 {
            return Ok(ReparseType::STUB);
        }
        return Ok(ReparseType::NO_REPARSE); // Not a reparse point
    }

    // Open file handle to query reparse data
    let file = match OpenOptions::new()
        .read(true)
        .custom_flags(FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS)
        .open(path)
    {
        Ok(f) => f,
        Err(e) => return Err(e.to_string()),
    };
    let handle = HANDLE(file.as_raw_handle());

    if handle == INVALID_HANDLE_VALUE {
        return Err(io::Error::last_os_error().to_string());
    }

    // Prepare buffer and call DeviceIoControl
    let mut buffer = vec![0u8; MAXIMUM_REPARSE_DATA_BUFFER_SIZE as usize];
    let mut bytes_returned: u32 = 0;

    let success = unsafe {
        DeviceIoControl(
            handle,
            FSCTL_GET_REPARSE_POINT,
            None, // No input buffer
            0,    // Input buffer size
            Some(buffer.as_mut_ptr() as *mut _),
            buffer.len() as u32,
            Some(&mut bytes_returned),
            None, // No overlapped structure
        )
    };

    if success.is_err() {
        return Err(io::Error::last_os_error().to_string());
    }

    // Interpret reparse tag
    let reparse_data = unsafe { &*(buffer.as_ptr() as *const ReparseDataBuffer) };
    let tag = reparse_data.reparse_tag;

    match tag {
        IO_REPARSE_TAG_SYMLINK => Ok(ReparseType::SYMLINK),
        IO_REPARSE_TAG_MOUNT_POINT => Ok(ReparseType::JUNCTION),
        _ => Ok(ReparseType::OTHER),
    }
}
