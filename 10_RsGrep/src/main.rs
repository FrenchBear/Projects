// rsgrep: Basic grep project in Rust
//
// 2025-03-13	PV      First version
// 2025-03-16	PV      1.0.1   Extended help, support reading from stdin
// 2025-03-25	PV      1.1.0   Global constants; Ignore $RECYCLE.BIN
// 2025-03-27   PV      1.2.0   Option -2 to use MyGlob crate (experimental)
// 2025-03-28   PV      1.2.1   Option -1 to use glob crate, glob syntax documented in extended help

// standard library imports
use std::error::Error;
use std::fs::File;
use std::io::{self, BufReader, ErrorKind, Read, Write};
use std::path::{Path, PathBuf};
use std::process;
use std::time::Instant;

// external crates imports
use encoding_rs::{Encoding, UTF_8, UTF_16LE, WINDOWS_1252};
use getopt::Opt;
use glob::{MatchOptions, glob_with};
use myglob::{MyGlobMatch, MyGlobSearch};
use regex::Regex;
use termcolor::{Color, ColorChoice, ColorSpec, StandardStream, WriteColor};

// -----------------------------------
// Submodules

pub mod grepiterator;
pub mod tests;

// -----------------------------------
// Global constants

const APP_NAME: &str = "rsgrep";
const APP_VERSION: &str = "1.1.0";

// ==============================================================================================
// Options processing

// Dedicated struct to store command line arguments
#[derive(Debug, Default)]
pub struct Options {
    pattern: String,
    sources: Vec<String>,
    ignore_case: bool,
    whole_word: bool,
    fixed_string: bool,
    recurse: bool,
    show_path: bool,
    search_create: u8,
    out_level: u8, // 0: normal output, 1: (-l) matching filenames only, 2: (-c) filenames and matching lines count, 3: (-c -l) only matching filenames and matching lines count
    verbose: u8,
}

impl Options {
    fn header() {
        eprintln!(
            "{APP_NAME} {APP_VERSION}\n\
            Simplified grep in rust"
        );
    }

    fn usage() {
        Options::header();
        eprintln!(
            "\nUsage: {APP_NAME} [?|-?|-h|??] [-1|-2] [-i] [-w] [-F] [-r] [-v] [-c] [-l] pattern source...\n\
            ?|-?|-h  Show this message\n\
            ??       Show advanced usage notes\n\
            -i       Ignore case during search\n\
            -1|-2    For glob search, -1: Use glob crate, -2: Use MyGlob crate\n\
            -w       Whole word search\n\
            -F       Fixed string search (no regexp interpretation)\n\
            -r       Recurse search in subfolders (add **/ ahead of glob not containing /)\n\
            -c       Suppress normal output, show count of matching lines for each file\n\
            -l       Suppress normal output, show matching file names only\n\
            -v       Verbose output\n\
            pattern  Regular expression to search\n\
            source   File or folder where to search, glob syntax supported"
        );
    }

    fn extended_usage() {
        Options::header();
        eprintln!(
"Copyright ©2025 Pierre Violent\n
Advanced usage notes\n--------------------\n
Options -c (show count of matching lines) and -l (show matching file names only) can be used together to show matching lines count only for matching files.\n
Glob supports recursive search without using option -r: C:\\Development\\GitVSTS\\**\\Net[7-9]\\**\\*.cs (current version does not support brances extension).\n
Only UTF-8, UTF-16 LE and Windows 1252 text files are currently supported, but automatic format detection using heuristics may not be always correct. Other formats are silently ignored unless verbose output is requested.\n
Glob crate pattern nules:
•   ? matches any single character.
•   * matches any (possibly empty) sequence of characters.
•   ** matches the current directory and arbitrary subdirectories. To match files in arbitrary subdiretories, use **\\*. This sequence must form a single path component, so both **a and b** are invalid and will result in an error. A sequence of more than two consecutive * characters is also invalid.
•   [...] matches any character inside the brackets. Character sequences can also specify ranges of characters, as ordered by Unicode, so e.g. [0-9] specifies any character between 0 and 9 inclusive. An unclosed bracket is invalid.
•   [!...] is the negation of [...], i.e. it matches any characters not in the brackets.
•   The metacharacters ?, *, [, ] can be matched by using brackets (e.g. [?]). When a ] occurs immediately following [ or [! then it is interpreted as being part of, rather then ending, the character set, so ] and NOT ] can be matched by []] and [!]] respectively. The - character can be specified inside a character sequence pattern by placing it at the start or the end, e.g. [abc-].\n
MyGlob care rule patters: Include all above patterns, plus:
•   {{choice1,choice2...}}  match any of the comma-separated choices between braces. Can be nested.
•   character classes [ ] accept regex syntax, see https://docs.rs/regex/latest/regex/#character-classes for character classes and escape sequences supported."
        );
    }

    /// Build a new struct Options analyzing command line parameters.<br/>
    /// Some invalid/inconsistent options or missing arguments return an error.
    fn new() -> Result<Options, Box<dyn Error>> {
        let mut args: Vec<String> = std::env::args().collect();
        if args.len() > 1 {
            if args[1].to_lowercase() == "help" {
                Self::usage();
                return Err("".into());
            }

            if args[1] == "??" || args[1] == "-??" {
                Self::extended_usage();
                return Err("".into());
            }
        }

        let mut options = Options { ..Default::default() };
        let mut opts = getopt::Parser::new(&args, "h?12iwFrvcl");

        loop {
            match opts.next().transpose()? {
                None => break,
                Some(opt) => match opt {
                    Opt('h', None) | Opt('?', None) => {
                        Self::usage();
                        return Err("".into());
                    }

                    Opt('1', None) => {
                        options.search_create = 1;
                    }

                    Opt('2', None) => {
                        options.search_create = 2;
                    }

                    Opt('i', None) => {
                        options.ignore_case = true;
                    }

                    Opt('w', None) => {
                        options.whole_word = true;
                    }

                    Opt('F', None) => {
                        options.fixed_string = true;
                    }

                    Opt('r', None) => {
                        options.recurse = true;
                    }

                    Opt('l', None) => {
                        options.out_level |= 1;
                    }

                    Opt('c', None) => {
                        options.out_level |= 2;
                    }

                    Opt('v', None) => {
                        options.verbose += 1;
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

            if arg.starts_with("-") {
                return Err(format!("Invalid/unsupported option {}", arg).into());
            }

            if options.pattern.is_empty() {
                options.pattern = arg;
            } else {
                options.sources.push(arg);
            }
        }

        if options.pattern.is_empty() {
            Self::header();
            eprintln!("\nNo pattern specified.\nUse {APP_NAME} ? to show options or {APP_NAME} ?? for advanced usage notes.");
            return Err("".into());
        }

        // Special tolerant case, recurse search without specifying source does not search from stdin but from all files
        if options.recurse && options.sources.is_empty() {
            options.sources.push("*.*".to_string());
        }

        Ok(options)
    }
}

// -----------------------------------
// Main

fn main() {
    // Process options
    let mut options = Options::new().unwrap_or_else(|err| {
        let msg = format!("{}", err);
        if msg.is_empty() {
            process::exit(0);
        }
        eprintln!("{APP_NAME}: Problem parsing arguments: {}", err);
        process::exit(1);
    });

    let re = build_re(&options);
    if re.is_err() {
        eprintln!("{APP_NAME}: Problem with search pattern: {:?}", re.err().unwrap());
        process::exit(1);
    }
    let re = re.unwrap();

    // MatchOptions has trait Copy, so only 1 global version is enough
    let mo = MatchOptions {
        case_sensitive: false,
        require_literal_separator: false,
        require_literal_leading_dot: false,
    };

    let start = Instant::now();

    // Building list of files
    // ToDo: It could be better to process file just when it's returned by iterator rather than stored in a Vec and processed later...
    let mut files: Vec<PathBuf> = Vec::new();
    for source in options.sources.clone() {
        // If file is a simple name, no path, no drive, and recurse option is specified, then we search in subfolders
        let source2 = if options.recurse && !source.contains('/') && !source.contains('\\') && !source.contains(':') {
            format!("**/{}", source)
        } else {
            source.clone()
        };

        let mut count = 0;

        if options.search_create==2 {
            // Use my own crate MyGlob, a bit faster than glob, and ignore $RECYCLE.BIN, System Volume Information and .git folders.
            // Also supports {} alternations
            let resgs = MyGlobSearch::build(&source2);
            match resgs {
                Ok(gs) => {
                    for ma in gs.explore_iter() {
                        match ma {
                            MyGlobMatch::File(pb) => {
                                count += 1;
                                files.push(pb);
                            }
                            MyGlobMatch::Error(err) => {
                                if options.verbose>0 {
                                    eprintln!("{APP_NAME}: error {}", err);
                                }
                            }
                        }
                    }
                }

                Err(e) => {
                    eprintln!("{APP_NAME}: Error building MyGlob: {:?}", e);
                    count = -1; // No need to display "no file found" in this case
                }
            }
        } else {
            // Use standard glob crate, that returns everything, including $RECYCLE.BIN content for instance.
            // See extended help to see options, or https://docs.rs/glob/latest/glob/struct.Pattern.html
            match glob_with(source2.as_str(), mo) {
                Ok(paths) => {
                    for entry in paths {
                        match entry {
                            Ok(pb) => {
                                if !pb.to_string_lossy().contains("$RECYCLE.BIN") {
                                    count += 1;
                                    files.push(pb);
                                }
                            }
                            Err(err) => {
                                if options.verbose>0 {
                                    eprintln!("{APP_NAME}: error {}", err);
                                }
                            }
                        };
                    }
                }
                Err(err) => {
                    eprintln!("{APP_NAME}: pattern error {}", err);
                    count = -1; // No need to display "no file found" in this case
                }
            }
        }
        
        if count == 0 {
            println!("{APP_NAME}: no file found matching {}", source);
        }
    }

    // Finally processing files, if more than 1 file, prefix output with file
    if options.sources.is_empty() {
        if options.verbose>0 {
            println!("Reading from stdin");
        }
        let s = io::read_to_string(io::stdin()).unwrap();
        process_text(&re, s.as_str(), "(stdin)", &options);
    } else {
        if files.len() > 1 {
            options.show_path = true;
        }
        for pb in &files {
            if options.verbose>1 {
                println!("Process: {}", pb.display());
            }
            process_path(&re, pb, &options);
        }
    }
    let duration = start.elapsed();

    if options.verbose>0 {
        if files.is_empty() {
            print!("\nstdin");
        } else {
            print!("\n{} file", files.len());
            if files.len() > 1 {
                print!("s");
            }
        }
        println!(" searched in {:.3}s", duration.as_secs_f64());
    }
}

/// Helper, build Regex according to options (case, fixed string, whole word).<br/>
/// Return an error in case of invalid Regex.
pub fn build_re(options: &Options) -> Result<Regex, regex::Error> {
    let spat = if options.fixed_string {
        regex::escape(options.pattern.as_str())
    } else if options.whole_word {
        format!("\\b{}\\b", options.pattern)
    } else {
        options.pattern.clone()
    };
    let spat = String::from(if options.ignore_case { "(?imR)" } else { "(?mR)" }) + spat.as_str();
    Regex::new(spat.as_str())
}

/// Helper detecting if path is a text file, detect encoding and return content as an utf-8 String.<br/>
/// Only UTF-8, UTF-16 LE and Windows 1252 are detected using heuristics (mays not be always correct).<br/>
/// Unrecognized files return an io::ErrorKind::InvalidData.
pub fn read_text_file(path: &Path) -> Result<String, io::Error> {
    let file = File::open(path)?;
    let mut reader = BufReader::new(file);
    let mut buffer = Vec::new();
    reader.read_to_end(&mut buffer)?;

    // Define the encodings to try, in order of preference.
    let encodings: [&'static Encoding; 3] = [UTF_8, UTF_16LE, WINDOWS_1252];

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
                    if (32..128).contains(&b) || b == 9 || b == 10 || b == 13 {
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

/// First step processing a file, read text content from path and call process_text.
fn process_path(re: &Regex, path: &Path, options: &Options) {
    let txtres = read_text_file(path);
    if let Err(e) = txtres {
        if e.kind() == ErrorKind::InvalidData {
            // Non-text files are ignored
            if options.verbose==1 {
                println!("{APP_NAME}: ignored non-text file {}", path.display());
            };
        }
        return;
    }
    let txt = &txtres.unwrap()[..];
    let filename = path.display().to_string();
    process_text(re, txt, filename.as_str(), options);
}

/// Core rsgrep process, search for re in txt, read from filename, according to options.
fn process_text(re: &Regex, txt: &str, filename: &str, options: &Options) {
    let mut stdout = StandardStream::stdout(ColorChoice::Always);
    let mut match_color = ColorSpec::new();
    match_color.set_fg(Some(Color::Red)).set_bold(true);
    let mut file_color = ColorSpec::new();
    file_color.set_fg(Some(Color::Black)).set_intense(true);

    // search
    let mut matchlinecount = 0;
    for gi in grepiterator::GrepLineMatches::new(txt, re) {
        matchlinecount += 1;

        if options.out_level == 1 {
            println!("{}", filename);
            return;
        }

        if options.out_level == 0 {
            if options.show_path {
                let _ = stdout.set_color(&file_color);
                let _ = write!(&mut stdout, "{}: ", filename);
                let _ = stdout.reset();
            }

            let mut p: usize = 0;
            for ma in gi.ranges {
                let e = ma.end;
                print!("{}", &gi.line[p..ma.start]);
                let _ = stdout.set_color(&match_color);
                let _ = write!(&mut stdout, "{}", &gi.line[ma]);
                let _ = stdout.reset();
                p = e;
            }
            println!("{}", &gi.line[p..]);
        }
    }
    // Note: both options -c and -l (out_level==3) is not supported by Linux version
    if options.out_level == 2 || (options.out_level == 3 && matchlinecount > 0) {
        println!("{}:{}", filename, matchlinecount);
    }
}
