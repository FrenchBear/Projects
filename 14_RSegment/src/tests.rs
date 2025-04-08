// rsegment tests
//
// 2025-03-29   PV


use super::*;

#[cfg(test)]

#[test]
fn test_simple_name_4_parts() {
    let bres = get_book_name(PathBuf::from(r"W:\Livres\Physique\Physique des particules (2è ed, 2017) - [Dunod] - Benoît Clément - ISBN 978-2-10-077183-7.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Physique des particules (2è ed, 2017)");
    assert_eq!(b.editor, "[Dunod]");
    assert_eq!(b.authors, "Benoît Clément");
    assert_eq!(b.edition_year, "2è ed, 2017");
    assert_eq!(b.edition, "2è");
    assert_eq!(b.year, "2017");
    assert_eq!(b.braced, "");
    assert_eq!(b.isbn, "ISBN 978-2-10-077183-7");
}

#[test]
fn test_simple_name_3_parts() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title - [Editor] - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "");
    assert_eq!(b.braced, "");
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
    assert_eq!(b.braced, "");
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
    assert_eq!(b.braced, "");
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
    assert_eq!(b.braced, "");
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
    assert_eq!(b.braced, "");
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
    assert_eq!(b.braced, "");
}

#[test]
fn test_year_braced_1() {
    let bres = get_book_name(PathBuf::from(r"C:\Temp\Title (2è ed, 2024) {Scan} - [Editor] - Author.pdf"));
    assert!(bres.is_ok());
    let b = bres.unwrap();
    assert_eq!(b.full_title, "Title (2è ed, 2024) {Scan}");
    assert_eq!(b.base_title, "Title");
    assert_eq!(b.editor, "[Editor]");
    assert_eq!(b.authors, "Author");
    assert_eq!(b.edition_year, "2è ed, 2024");
    assert_eq!(b.edition, "2è");
    assert_eq!(b.year, "2024");
    assert_eq!(b.braced, "Scan");
}

