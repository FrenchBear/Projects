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
    assert_eq!(b.full_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "");
}

#[test]
fn test_simple_name_2_parts_1() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title - [Editor].pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "");
    assert_eq!(b.edition_year, "");
}

#[test]
fn test_simple_name_2_parts_2() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title");
    assert_eq!(b.editor, "");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "");
}

#[test]
fn test_simple_name_1_part() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title");
    assert_eq!(b.base_title, "Title");
    assert_eq!(b.editor, "");
    assert_eq!(b.edition_year, "");
    assert_eq!(b.authors, "");
}

#[test]
fn test_year_version_1() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title (2è ed, 2022) - [Editor] - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title (2è ed, 2022)");
    assert_eq!(b.base_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "2è ed, 2022");
    assert_eq!(b.edition, "2è");
    assert_eq!(b.year, "2022");
}

#[test]
fn test_year_version_2() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title (2025) - [Editor] - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title (2025)");
    assert_eq!(b.base_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "2025");
    assert_eq!(b.edition, "");
    assert_eq!(b.year, "2025");
}
