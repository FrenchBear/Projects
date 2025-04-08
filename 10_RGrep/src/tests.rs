// rgrep tests
//
// 2025-03-14   PV
// 2025-04-01   PV      Adapted tests to read_text_file_2

#[cfg(test)]
pub mod read_text {
    use std::{io, path::Path};

    #[test]
    fn text_utf8() {
        let r = crate::read_text_file_2(Path::new(
            r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf8.txt",
        ));
        assert!(r.is_ok());
        let res = r.unwrap();
        assert!(res.0.is_some());
        let s = res.0.unwrap();
        assert_eq!(&s[25..35], "géraldine");
    }

    #[test]
    fn text_1252() {
        let r = crate::read_text_file_2(Path::new(
            r"C:\DocumentsOD\Doc tech\Encodings\prenoms-ansi,1252.txt",
        ));
        assert!(r.is_ok());
        let res = r.unwrap();
        assert!(res.0.is_some());
        let s = res.0.unwrap();
        assert_eq!(&s[25..35], "géraldine");
    }

    #[test]
    fn text_utf16() {
        let r = crate::read_text_file_2(Path::new(
            r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf16lebom.txt",
        ));
        assert!(r.is_ok());
        let res = r.unwrap();
        assert!(res.0.is_some());
        let s = res.0.unwrap();
        assert_eq!(&s[25..35], "géraldine");
    }

    #[test]
    fn binary_file() {
        let r = crate::read_text_file_2(Path::new(r"C:\Utils\BookApps\Astructw.exe"));
        assert!(r.is_ok());
        let res = r.unwrap();
        assert!(res.0.is_none());
    }

    #[test]
    fn inexistent_file() {
        let r = crate::read_text_file_2(Path::new(r"C:\Utils\BookApps\Astructw.com"));
        assert!(r.is_err());
        let e = r.err().unwrap();
        assert_eq!(e.kind(), io::ErrorKind::NotFound);
    }
}

#[cfg(test)]
pub mod grep_iterator {
    use crate::grepiterator::GrepLineMatches;
    use regex::Regex;

    #[test]
    fn iterator() {
        let re = Regex::new("(?imR)pommes").unwrap();
        let haystack = "RECETTE DE LA TARTE AUX POMMES\r\nPréparer la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nVous préférez froid? Laisser refroidir\r\nDéguster!";

        let res: Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(), 2);
        assert_eq!(res[0].line, "RECETTE DE LA TARTE AUX POMMES");
        assert_eq!(res[0].ranges.len(), 1);
        assert_eq!(res[0].ranges[0], 24..30);

        assert_eq!(res[1].line, "Peler les pommes et ajouter les pommes");
        assert_eq!(res[1].ranges.len(), 2);
        assert_eq!(res[1].ranges[0], 10..16);
        assert_eq!(res[1].ranges[1], 32..38);
    }
}

#[cfg(test)]
pub mod build_re {
    use crate::grepiterator::GrepLineMatches;
    use crate::{Options, build_re};

    #[test]
    fn case_sensitive() {
        let haystack = "RECETTE DE LA TARTE AUX POMMES\r\nPréparer la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nVous préférez froid? Laisser refroidir\r\nDéguster!";
        let options = Options {
            pattern: String::from("pommes"),
            ignore_case: false,
            ..Default::default()
        };
        let re = build_re(&options).unwrap();
        let res: Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(), 1);
    }

    #[test]
    fn case_insensitive() {
        let haystack = "RECETTE DE LA TARTE AUX POMMES\r\nPréparer la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nVous préférez froid? Laisser refroidir\r\nDéguster!";
        let options = Options {
            pattern: String::from("pommes"),
            ignore_case: true,
            ..Default::default()
        };
        let re = build_re(&options).unwrap();
        let res: Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(), 2);
    }

    #[test]
    fn full_line() {
        let haystack = "RECETTE DE LA TARTE AUX POMMES\r\nPréparer la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nVous préférez froid? Laisser refroidir\r\nDéguster!";
        let options = Options {
            pattern: String::from("^Préparer la pâte$"),
            ..Default::default()
        };
        let re = build_re(&options).unwrap();
        let res: Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(), 1);
    }

    #[test]
    fn fixed_string() {
        let haystack = "RECETTE DE LA TARTE AUX POMMES\r\nPréparer la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nVous préférez froid? Laisser refroidir\r\nDéguster!";
        let options = Options {
            pattern: String::from("froid? Laisser"),
            fixed_string: true,
            ..Default::default()
        };
        let re = build_re(&options).unwrap();
        let res: Vec<GrepLineMatches> = GrepLineMatches::new(haystack, &re).collect();
        assert_eq!(res.len(), 1);
    }
}
