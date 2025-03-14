// rsgrep: Basic grep project in Rust
//
// 2025-03-13	PV      First version

#![allow(dead_code, unused_variables, unreachable_code, unused_imports)]

// standard library imports
use encoding_rs::{Encoding, UTF_16LE, UTF_8, WINDOWS_1252};
use std::error::Error;
use std::fs::{self, File};
use std::io::ErrorKind;
use std::io::{self, BufRead, BufReader, Read};
use std::path::Path;
use std::path::PathBuf;
use std::process;

// external crates imports
use getopt::Opt;
use glob::glob;
use glob::glob_with;
use glob::GlobError;
use glob::MatchOptions;
use glob::Paths;
use glob::PatternError;
use regex::Regex;

// -----------------------------------
// Submodules

pub mod tests;
pub mod grepiterator;

// ==============================================================================================

// Dedicated struct to store command line arguments
#[derive(Debug)]
pub struct Options {
    pattern: String,
    sources: Vec<String>,
    ignore_case: bool,
    recurse: bool,
    verbose: bool,
}

impl Options {
    fn usage() {
        eprintln!(
            "rsgrep, simplified grep in rust\n\
            Usage: rsgrep [?|-?|-h] [-i] [-r] [-v] pattern glob...\n\
            -h       Shows this message\n\
            -i       Ignore case during search\n\
            -r       Recurse search in subfolders (add **/ ahead of glob not containing /)\n\
            -v       Verbose output\n\
            pattern  Regular expression to search\n\
            glob     File or folder where to search, glob syntax supported"
        );
    }

    fn new() -> Result<Options, Box<dyn Error>> {
        // default
        let mut options = Options {
            pattern: String::new(),
            sources: Vec::new(),
            ignore_case: false,
            recurse: false,
            verbose: false,
        };

        let mut args: Vec<String> = std::env::args().collect();
        let mut opts = getopt::Parser::new(&args, "h?irv");

        loop {
            match opts.next().transpose()? {
                None => break,
                Some(opt) => match opt {
                    Opt('h', None) | Opt('?', None) => {
                        Self::usage();
                        return Err("".into());
                    }

                    Opt('i', None) => {
                        options.ignore_case = true;
                    }

                    Opt('r', None) => {
                        options.recurse = true;
                    }

                    Opt('v', None) => {
                        options.verbose = true;
                    }

                    _ => unreachable!(),
                },
            }
        }

        // Check for extra argument
        for arg in args.split_off(opts.index()) {
            if arg == "?" || arg == "help" {
                Self::usage();
                return Err("".into());
            }

            // -r, -i accpter after pattern
            if arg == "-r" {
                options.recurse = true;
                continue;
            }

            if arg == "-i" {
                options.ignore_case = true;
                continue;
            }

            if arg == "-v" {
                options.verbose = true;
                continue;
            }

            if arg.starts_with("-") {
                return Err(format!("Invalid/unsupported option {}", arg).into());
            }

            if options.pattern.is_empty() {
                options.pattern = arg;
            } else {
                options.sources.push(arg);
            }
        }

        if options.sources.is_empty() {
            return Err("No source specified for search, this version doesn't support yet reading from stdin".into());
        }

        Ok(options)
    }
}

fn main() {
    test_iterator();
}

fn test_iterator() {
    let re = Regex::new("(?m)pommes").unwrap();
    let haystack = "Recette de la tarte aux pommes\r\nPréparez la pâte\r\nPrécuire la pâte 10 minutes\r\nPeler les pommes et ajouter les pommes\r\nFaire cuire\r\nLaisser refroidire\r\nDéguster!";
    for gi in grepiterator::GrepLineMatches::new(haystack, &re) {
        println!("\nLine: <{}>", gi.line);
        for ma in gi.matches {
            println!("- {}..{}", ma.start, ma.end);
        }
    }
}

fn zzmain() {
    /*
    // -------------------------------------------
    // Process options
    let options = Options::new().unwrap_or_else(|err| {
        let msg = format!("{}", err);
        if msg.is_empty() {
            process::exit(0);
        }
        eprintln!("rsgrep: Problem parsing arguments: {}", err);
        process::exit(1);
    });

    // println!("rsgrep");
    // println!("{:?}", options);
    */

    // For testing
    let options = Options {
        pattern: String::from("tine"),
        sources: vec![String::from(r"C:\DocumentsOD\Doc tech\Encodings\prenoms-utf8.txt")],
        ignore_case: false,
        recurse: false,
        verbose: true,
    };
    // Make sure search pattern is valid
    let pat = match Regex::new(options.pattern.as_str()) {
        Ok(p) => p,
        Err(pe) => {
            eprintln!("rsgrep: Problem with search pattern: {}", pe);
            process::exit(1);
        }
    };

    // MatchOptions has trait Copy, so only 1 global version is enough
    let mo = MatchOptions {
        case_sensitive: false,
        require_literal_separator: false,
        require_literal_leading_dot: false,
    };


    // Getting list of files
    for source in options.sources.clone() {
        println!("\n---------------------\nSource: {}", source);

        // If file is a simple name, no path, no drive, and recurse option is specified, then we search in subfolders
        let source2 = if options.recurse && !source.contains('/') && !source.contains('\\') && !source.contains(':') {
            format!("{}", source)
        } else {
            source.clone()
        };

        let mut count = 0;
        match glob_with(&source2.as_str(), mo) {
            Ok(paths) => {
                for entry in paths {
                    match entry {
                        Ok(pb) => {
                            count += 1;
                            process_path(&pat, &pb, &options);
                        }
                        Err(err) => {
                            // A GlobError is actually an io:Error and a Path
                            // For now, just ignore
                            //println!("%%% Glob error {}", err);

                            /*
                            let pa = String::from(err.path().to_str().unwrap());
                            let ww = err.into_error();
                            match ww.kind() {
                                ErrorKind::NotFound => {
                                    println!("Path not found: {}", pa);
                                }
                                ErrorKind::PermissionDenied => {
                                    println!("Permission denied opening path: {}", pa);
                                }
                                _ => {
                                    eprintln!("Other error opening path {}: {}", pa, ww);
                                }
                            };
                            */
                        }
                    };
                }
            }
            Err(err) => {
                println!("rsglob: pattern error {}", err);
                count = -1; // No need to display "no file found" in this case
            }
        }
        if count == 0 {
            print!("rsgrep: no file found mtching {}", source);
        }
    }
}

pub fn read_text_file(path: &Path) -> Result<String, io::Error> {
    let file = File::open(path)?;
    let mut reader = BufReader::new(file);
    let mut buffer = Vec::new();
    reader.read_to_end(&mut buffer)?;

    // Define the encodings to try, in order of preference.
    let encodings: [&'static Encoding; 3] = [&UTF_8, &UTF_16LE, &WINDOWS_1252];

    for encoding in encodings {
        let (decoded_string, used_encoding, had_errors) = encoding.decode(&buffer);
        if !had_errors {
            // For UTF-16, we need to confirm. Count 0 in odd positions, should be >=40% of file size
            if used_encoding == UTF_16LE {
                let mut zcount = 0;
                let mut ix = 1;
                while ix < buffer.len() {
                    if buffer[ix] == 0 {
                        zcount += 1
                    };
                    ix += 2;
                }
                if zcount * 10 < 4 * buffer.len() {
                    continue;
                }
            }

            // For Windows 1252, we need to confirm. Characters in {0x20..0x7F, 9, 10, 13} should be >=90% of file size
            // And no \0 in file
            if used_encoding == WINDOWS_1252 {
                let mut acount = 0;
                let mut ix = 0;
                while ix < buffer.len() {
                    let b = buffer[ix];
                    if b >= 32 && b < 128 || b == 9 || b == 10 || b == 13 {
                        acount += 1;
                    } else if b == 0 {
                        acount = 0;
                        break;
                    }
                    ix += 1;
                }
                if acount * 10 < 9 * buffer.len() {
                    continue;
                }
            }

            // If decoding succeeded without errors, return the string.
            return Ok(decoded_string.into_owned());
        }
    }

    // If none of the encodings worked without errors, return an error.
    Err(io::Error::new(
        io::ErrorKind::InvalidData,
        "File does not appear to be UTF-8, UTF-16 or Windows-1252 encoded.",
    ))
}

fn process_path(pat: &Regex, pb: &PathBuf, options: &Options) {
    //println!("{}", pb.display());
    //let pb = Path::new(r"D:\Pierre\OneDrive\Calculators\Performances\Crible\Basic\frmCrible1.frm");
    let txtres = fs::read_to_string(pb);
    if let Err(e) = txtres {
        if e.kind() == ErrorKind::InvalidData {
            // Files not containing UTF-8 are ignored
            if options.verbose {
                println!("rsrust: ignored non-text file {}", pb.display());
            } else {
                println!("rsrust: error reading file {}: {}", pb.display(), e);
            }
        }
        return;
    }

    // search...
    println!("Searching...");
}
