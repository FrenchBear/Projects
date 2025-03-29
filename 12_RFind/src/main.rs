// rfind: A Rust version of find/XFind/Search
//
// 2025-03-29	PV      First version

// ToDo: Logfile, errors in red

#![allow(unused)]

// standard library imports
use std::error::Error;
use std::fmt::Debug;
use std::fs::File;
use std::io::{self, BufReader, ErrorKind, Read, Write};
use std::path::{Path, PathBuf};
use std::process;
use std::time::Instant;

// external crates imports
use myglob::{MyGlobMatch, MyGlobSearch};
use termcolor::{Color, ColorChoice, ColorSpec, StandardStream, WriteColor};

// -----------------------------------
// Submodules

mod actions;
mod tests;

// -----------------------------------
// Global constants

const APP_NAME: &str = "rfind";
const APP_VERSION: &str = "1.0.0";

// ==============================================================================================
// Actions

trait Action: Debug {
    fn action(&self, path: &Path, do_it: bool, verbose: bool);
    fn name(&self) -> &'static str;
}

// ==============================================================================================
// Options processing

// Dedicated struct to store command line arguments
#[derive(Debug, Default)]
struct Options {
    sources: Vec<String>,
    actions: Vec<Box<dyn Action>>,
    search_files: bool,
    search_dirs: bool,
    verbose: bool,
}

impl Options {
    fn header() {
        eprintln!(
            "{APP_NAME} {APP_VERSION}\n\
            Searching files in Rust"
        );
    }

    fn usage() {
        Options::header();
        eprintln!(
            "\nUsage: {APP_NAME} [?|-?|-h|??] [-f|-type f|-d|-type d] [-v] source...
            ?|-?|-h    Show this message
            ??         Show advanced usage notes
            -f|-type f Search for files
            -d|-type d Search for directories
            -v       Verbose output
            pattern  Regular expression to search
            source   File or folder where to search, glob syntax supported"
        );
    }

    fn extended_usage() {
        Options::header();
        eprintln!(
"Copyright ©2025 Pierre Violent\n
Advanced usage notes\n--------------------\n
Glob pattern nules:
•   ? matches any single character.
•   * matches any (possibly empty) sequence of characters.
•   ** matches the current directory and arbitrary subdirectories. To match files in arbitrary subdiretories, use **\\*. This sequence must form a single path component, so both **a and b** are invalid and will result in an error. A sequence of more than two consecutive * characters is also invalid.
•   [...] matches any character inside the brackets. Character sequences can also specify ranges of characters, as ordered by Unicode, so e.g. [0-9] specifies any character between 0 and 9 inclusive. An unclosed bracket is invalid.
•   [!...] is the negation of [...], i.e. it matches any characters not in the brackets.
•   The metacharacters ?, *, [, ] can be matched by using brackets (e.g. [?]). When a ] occurs immediately following [ or [! then it is interpreted as being part of, rather then ending, the character set, so ] and NOT ] can be matched by []] and [!]] respectively. The - character can be specified inside a character sequence pattern by placing it at the start or the end, e.g. [abc-].\n
•   {{choice1,choice2...}}  match any of the comma-separated choices between braces. Can be nested.
•   character classes [ ] accept regex syntax, see https://docs.rs/regex/latest/regex/#character-classes for character classes and escape sequences supported."
        );
    }

    /// Build a new struct Options analyzing command line parameters.<br/>
    /// Some invalid/inconsistent options or missing arguments return an error.
    fn new() -> Result<Options, Box<dyn Error>> {
        let args: Vec<String> = std::env::args().collect();

        let mut options = Options { ..Default::default() };

        // Since we have non-standard long options, don't use getopt for options processing but a manual loop
        let mut args_iter = args.iter();
        args_iter.next(); // Skip application eexecutable
        while let Some(arg) = args_iter.next() {
            if arg.starts_with('-') || arg.starts_with('/') {
                // Options are case insensitive
                let arglc = arg[1..].to_lowercase();

                match &arglc[..] {
                    "?" | "h" | "help" | "-help" => {
                        Self::usage();
                        return Err("".into());
                    }

                    "??" => {
                        Self::extended_usage();
                        return Err("".into());
                    }

                    "v" => options.verbose = true,

                    "f" => options.search_files = true,
                    "d" => options.search_dirs = true,
                    "type" => {
                        if let Some(search_type) = args_iter.next() {
                            match search_type.to_lowercase().as_str() {
                                "f" => options.search_files = true,
                                "d" => options.search_dirs = true,
                                _ => return Err(format!("Invalid argument {search_type} for pption -type, valid arguments are f or d").into()),
                            }
                        } else {
                            return Err(format!("Option -type requires an argument f or d").into());
                        }
                    }

                    "print" => options.actions.push(Box::new(actions::ActionPrint::new())),

                    _ => {
                        return Err(format!("Invalid/unsupported option {}", arg).into());
                    }
                }
            } else {
                // Non-option, some values are special
                match &arg.to_lowercase()[..] {
                    "?" | "h" | "help" => {
                        Self::usage();
                        return Err("".into());
                    }

                    "??" => {
                        Self::extended_usage();
                        return Err("".into());
                    }

                    // Everything else is considered as a source (a glob pattern), it will be validated later
                    _ => options.sources.push(arg.clone()),
                }
            }
        }

        // If neither filtering files or dirs has been requested, then we search for both
        if !options.search_dirs && !options.search_files {
            options.search_dirs = true;
            options.search_files = true;
        }

        // If no action is specified, then print action is default
        if options.actions.is_empty() {
            options.actions.push(Box::new(actions::ActionPrint::new()));
        }

        Ok(options)
    }
}

// -----------------------------------
// Main

fn main() {
    // Process options
    let options = Options::new().unwrap_or_else(|err| {
        let msg = format!("{}", err);
        if msg.is_empty() {
            process::exit(0);
        }
        eprintln!("{APP_NAME}: Problem parsing arguments: {}", err);
        process::exit(1);
    });

    let start = Instant::now();

    // Convert String sources into MyGlobSearch structs
    let mut glob_sources: Vec<MyGlobSearch> = Vec::new();
    for source in options.sources.iter() {
        let resgs = MyGlobSearch::build(source);
        match resgs {
            Ok(gs) => glob_sources.push(gs),
            Err(e) => {
                eprintln!("{APP_NAME}: Error building MyGlob: {:?}", e);
            }
        }
    }

    let do_it = true;
    let mut files_count = 0;
    let mut dirs_count = 0;
    for gs in glob_sources.iter() {
        for ma in gs.explore_iter() {
            match ma {
                MyGlobMatch::File(pb) => {
                    if options.search_files {
                        files_count += 1;
                        for ba in options.actions.iter() {
                            (**ba).action(&pb, do_it, options.verbose);
                        }
                    }
                }

                MyGlobMatch::Dir(pb) => {
                    if options.search_dirs {
                        dirs_count += 1;
                        for ba in options.actions.iter() {
                            (**ba).action(&pb, do_it, options.verbose);
                        }
                    }
                }

                MyGlobMatch::Error(err) => {
                    if options.verbose {
                        eprintln!("{APP_NAME}: error {}", err);
                    }
                }
            }
        }
    }

    let duration = start.elapsed();

    if options.verbose {
        if options.search_files {
            print!("{files_count} files(s)");
        }
        if options.search_dirs {
            if options.search_files {
                print!(", ");
            }
            print!("{dirs_count} dir(s)");
        }
        println!(" found in {:.3}s", duration.as_secs_f64());
    }
}
