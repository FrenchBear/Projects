// rsgrep tests
//
// 2025-03-14   PV

#[cfg(test)]
use std::{io, path::Path};

#[test]
fn test_read_text_utf8() {
    let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf8.txt"));
    assert!(r.is_ok());
    assert_eq!(&r.unwrap()[25..35], "géraldine");
}

#[test]
fn test_read_text_1252() {
    let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-ansi,1252.txt"));
    assert!(r.is_ok());
    assert_eq!(&r.unwrap()[25..35], "géraldine");
}

#[test]
fn test_read_text_utf16() {
    let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf16lebom.txt"));
    assert!(r.is_ok());
    assert_eq!(&r.unwrap()[25..35], "géraldine");
}

#[test]
fn test_read_text_binary() {
    let r = crate::read_text_file(Path::new(r"C:\Utils\BookApps\Astructw.exe"));
    assert!(r.is_err());
    let e = r.err().unwrap();
    assert!(e.kind() == io::ErrorKind::InvalidData);
}

#[test]
fn test_read_text_inexistent() {
    let r = crate::read_text_file(Path::new(r"C:\Utils\BookApps\Astructw.com"));
    assert!(r.is_err());
    let e = r.err().unwrap();
    assert!(e.kind() == io::ErrorKind::NotFound);
}
