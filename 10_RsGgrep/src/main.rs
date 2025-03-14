// rsgrep: Basic grep project in Rust
//
// 2025-03-13	PV      First version

#![allow(dead_code, unused_variables, unreachable_code)]

// standard library imports
use std::error::Error;
use std::path::Path;
use std::process;

// external crates imports
use getopt::Opt;
use glob::glob;

// Dedicated struct to store command line arguments
#[derive(Debug)]
pub struct Options {
    patterns: Vec<String>,
    files: Vec<String>,
    folders: Vec<String>,
    filters: Vec<String>,
    ignore_case: bool,
    recurse: bool,
}

impl Options {
    fn usage() {
        eprintln!(
            "rsgrep, simplifieg grep in rust\n\
            Usage: rsgrep [?|-?|-h] [-i] [-r] [-t filter]... pattern|[-e pattern]... [file_or_folder]...\n\
            -h             Shows this message\n\
            -i             Ignore case during search\n\
            -r             Recurse search in subfolders\n\
            -t filter      File glob filter for folders, ex: *.cs, default *.*\n\
            pattern        Regular expression to search (use -e for each pattern if more than 1, or a pattern starting with -)\n\
            file_or_folder File(s) or folder(s) where to search"
        );
    }

    fn new() -> Result<Options, Box<dyn Error>> {
        // default
        let mut options = Options {
            patterns: Vec::new(),
            files: Vec::new(),
            folders: Vec::new(),
            filters: Vec::new(),
            ignore_case: false,
            recurse: false,
        };

        let mut args: Vec<String> = std::env::args().collect();
        let mut opts = getopt::Parser::new(&args, "h?t:ire:");

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

                    Opt('t', Some(arg)) => {
                        options.filters.push(arg);
                    }

                    Opt('e', Some(arg)) => {
                        options.patterns.push(arg);
                    }

                    _ => unreachable!(),
                },
            }
        }

        // Check for extra argument, only help is accepted (was a for loop, but clippy complains that it never loops...)
        for arg in args.split_off(opts.index()) {
            if arg == "?" || arg == "help" {
                Self::usage();
                return Err("".into());
            }

            if options.patterns.is_empty() {
                options.patterns.push(arg);
                continue;
            }

            // Determine if it's a file or a folder, and it there is a glob part
            let p = Path::new(&arg);

            // Is it a file?
            if p.is_file() {
                options.files.push(arg);
                continue;
            }

            // Is it a folder?
            if p.is_dir() {
                options.folders.push(arg);
                continue;
            }

            // Is it a folder followed by a glob pattern?
            let mut parent = match p.parent() {
                Some(p) => p,
                None => return Err(format!("Invalid path [1] {}", arg).into()),
            };
            let file = match p.file_name() {
                Some(p) => p,
                None => return Err(format!("Invalid path [2] {}", arg).into()),
            };
            if parent.to_str().unwrap().is_empty() {
                parent = Path::new(".");
            } else if !parent.is_dir() {
                return Err(format!("Invalid path [3] {}", arg).into());
            }

            let f = file.to_str().unwrap();
            if f.contains('*') || f.contains('?') || f.contains('{') {      // Is it a glob pattern?
                options.filters.push(f.to_string());
                options.folders.push(parent.to_str().unwrap().to_string());
                continue;
            } else if options.recurse && !f.contains('/') && !f.contains('\\') && !f.contains(':') {
                // No glob char, no path/drive and recurse mode, we accept it as a file pattern that will
                // be used in a recurse search, even if not present in current folder
                options.files.push(arg);
                continue;
            }

            // Nothing known
            return Err(format!("Invalid path [4] {}", arg).into());
        }

        // Early verifications
        if options.patterns.is_empty() {
            return Err("No search pattern specified".into());
        }

        // For now, we don't support reading from stdin if no file/folder has been specified
        // if options.files.is_empty() && options.folders.is_empty() {
        //     return Err("No input file/folder specified, this version doesn't support reading from stdin yet".into());
        // }

        Ok(options)
    }
}

fn main() {
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

    println!("rsgrep");
    println!("{:?}", options);

    // Getting list of files
    for file in options.files {
        println!("File: {}", file);

        // If file is a simple name, no path, no drive, and recurse option is specified, then we search in subfolders
        if options.recurse && !file.contains('/') && !file.contains('\\') && !file.contains(':') {
            let gp = format!("**/{}", file);
            for entry in glob(gp.as_str()).expect("Failed to read glob pattern") {
                println!("File (recurse): {}", entry.unwrap().display());
            }
        }
    }
}
