// drive_type.rs
// Calls GetDriveTypeW to find out the type of drive behind a path
//
// 2025-04-03   PV      First version

use std::ffi::OsStr;
use std::os::windows::ffi::OsStrExt;
use std::path::Path;
use std::env;
use windows::Win32::Storage::FileSystem::GetDriveTypeW;
use windows::core::PCWSTR;

#[derive(Debug, PartialEq, Eq, Clone, Copy)]
#[repr(u32)]
#[allow(non_camel_case_types)]
pub enum DriveType {
    DRIVE_UNKNOWN = 0,
    DRIVE_NO_ROOT_DIR = 1,      // Also returned for inexistent drives
    DRIVE_REMOVABLE = 2,
    DRIVE_FIXED = 3,
    DRIVE_REMOTE = 4,
    DRIVE_CDROM = 5,
    DRIVE_RAMDISK = 6,
}

pub fn drive_type(path: &Path) -> Result<DriveType, String> {
    // Replace relative paths starting with a . by current directory, which is an absolute path
    let path_abs = if path.is_relative() {
        let cd = env::current_dir();
        match cd {
            Ok(pb) => &pb.clone(),
            Err(e) => return Err(format!("Error retrieving current drive: {e}"))
        }
    } else {
        path
    };

    let root_path = match path_abs.components().next() {
        Some(std::path::Component::Prefix(prefix)) => {
            match prefix.kind() {
                std::path::Prefix::Disk(disk) => &format!("{}:\\", disk as char),
                std::path::Prefix::VerbatimDisk(disk) => &format!("{}:\\", disk as char),
                std::path::Prefix::UNC(server, share) => {
                    &format!("\\\\{}\\{}\\", server.to_string_lossy(), share.to_string_lossy())
                }
                _ => {
                    return Err("Path is not a drive or UNC path".to_string());
                }
            }
        }
        _ => {
            return Err("Path does not have a drive component".to_string());
        }
    };

    //println!("Drive_str: {:?}", drive_letter);
    let lprootpathname: Vec<u16> = OsStr::new(root_path).encode_wide().chain(std::iter::once(0)).collect();

    let res = unsafe {
        let resu32 = GetDriveTypeW(PCWSTR(lprootpathname.as_ptr()));
        if resu32 <= 6 {
            Ok(std::mem::transmute(resu32))
        } else {
            Err(format!("Unknown value returned by GetDriveTypeW: {resu32}"))
        }
    };

    res
}
