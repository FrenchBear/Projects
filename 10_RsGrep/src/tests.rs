// rsgrep tests
//
// 2025-03-14   PV

#[cfg(test)]
pub mod read_text {
    use std::{io, path::Path};

    #[test]
    fn text_utf8() {
        let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf8.txt"));
        assert!(r.is_ok());
        assert_eq!(&r.unwrap()[25..35], "géraldine");
    }

    #[test]
    fn text_1252() {
        let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-ansi,1252.txt"));
        assert!(r.is_ok());
        assert_eq!(&r.unwrap()[25..35], "géraldine");
    }

    #[test]
    fn text_utf16() {
        let r = crate::read_text_file(Path::new(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf16lebom.txt"));
        assert!(r.is_ok());
        assert_eq!(&r.unwrap()[25..35], "géraldine");
    }

    #[test]
    fn binary_file() {
        let r = crate::read_text_file(Path::new(r"C:\Utils\BookApps\Astructw.exe"));
        assert!(r.is_err());
        let e = r.err().unwrap();
        assert_eq!(e.kind() , io::ErrorKind::InvalidData);
    }

    #[test]
    fn inexistent_file() {
        let r = crate::read_text_file(Path::new(r"C:\Utils\BookApps\Astructw.com"));
        assert!(r.is_err());
        let e = r.err().unwrap();
        assert_eq!(e.kind() , io::ErrorKind::NotFound);
    }
}

#[cfg(test)]
pub mod grep_iterator {
    use regex::Regex;

    use crate::grepiterator::GrepLineMatches;

    #[test]
    fn iterator() {
        let re = Regex::new("(?m)pommes").unwrap();
        let haystack = "Recette de la tarte aux pommes\r\nPréparez la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nLaisser refroidire\r\nDéguster!";
        
        let res:Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(),2);
        assert_eq!(res[0].line,"Recette de la tarte aux pommes");
        assert_eq!(res[0].matches.len(), 1);
        assert_eq!(res[0].matches[0], 24..30);

        assert_eq!(res[1].line,"Peler les pommes et ajouter les pommes");
        assert_eq!(res[1].matches.len(), 2);
        assert_eq!(res[1].matches[0], 10..16);
        assert_eq!(res[1].matches[1], 32..38);
    }
}