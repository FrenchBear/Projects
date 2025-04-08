// rsegment tests
//
// 2025-03-29   PV

use super::*;

#[cfg(test)]

#[test]
fn test_simple_name_3_parts() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title - [Editor] - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.base, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
}
