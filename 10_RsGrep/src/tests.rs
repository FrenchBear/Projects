// rsgrep tests
//
// 2025-03-14   PV

#[cfg(test)]
pub mod readtext {
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
}

#[cfg(test)]
pub mod grepiterator {
    use regex::Regex;

    #[test]
    fn test_iterator() {
        let re = Regex::new("(?m)pommes").unwrap();
        let haystack = "Recette de la tarte aux pommes\r\nPréparez la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nLaisser refroidire\r\nDéguster!";
        
    }
}