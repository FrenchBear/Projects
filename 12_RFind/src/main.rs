// rfind: A Rust version of find/XFind/Search
//
// 2025-03-29	PV      First version

//#![allow(unused)]

// standard library imports
use std::collections::HashSet;
use std::error::Error;
use std::fmt::Debug;
use std::fs::File;
use std::io::{BufWriter, Write};
use std::path::Path;
use std::process;
use std::time::Instant;

// external crates imports
use chrono::{DateTime, Local};
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

// -----------------------------------
// Traits

trait Action: Debug {
    fn action(&self, writer: &mut BufWriter<File>, path: &Path, noaction: bool, verbose: bool);
    fn name(&self) -> &'static str;
}

// ==============================================================================================
// Logging

pub fn logln(writer: &mut BufWriter<File>, msg: &str) {
    if msg.starts_with("***") {
        let mut stdout = StandardStream::stdout(ColorChoice::Always);
        let mut err_color = ColorSpec::new();
        err_color.set_fg(Some(Color::Red)).set_bold(true);

        let _ = stdout.set_color(&err_color);
        let _ = writeln!(&mut stdout, "{}", msg);
        let _ = stdout.reset();
    } else {
        println!("{}", msg);
    }
    let _ = writeln!(writer, "{}", msg);
}

#[allow(unused)]
fn log(writer: &mut BufWriter<File>, msg: &str) {
    print!("{}", msg);
    let _ = write!(writer, "{}", msg);
}

// ==============================================================================================
// Options processing

// Dedicated struct to store command line arguments
#[derive(Debug, Default)]
struct Options {
    sources: Vec<String>,
    actions_names: HashSet<&'static str>,
    search_files: bool,
    search_dirs: bool,
    norecycle: bool,
    noaction: bool,
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
            "\nUsage: {APP_NAME} [?|-?|-h|??] [-v] [-n] [-f|-type f|-d|-type d] [-[no]recycle] source...
            ?|-?|-h     Show this message
            ??          Show advanced usage notes
            -v          Verbose output
            -n          No action: display actions, but don't execute them
            -f|-type f  Search for files
            -d|-type d  Search for directories
            -norecycle  Delete forever (default: -recycle, delete local files to recycle bin)
            source      File or folder where to search, glob syntax supported (see advanced notes)"
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
                    "n" => options.noaction = true,

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

                    "norecycle" => options.norecycle = true,

                    "print" => {
                        options.actions_names.insert("print");
                    }
                    "rm" | "del" | "delete" => {
                        options.actions_names.insert("delete");
                    }
                    "rd" | "rmdir" => {
                        options.actions_names.insert("rmdir");
                    }

                    //"print" => options.actions.push(Box::new(actions::ActionPrint::new())),
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
        if options.actions_names.is_empty() {
            options.actions_names.insert("print");
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

    // Prepare log writer
    let now: DateTime<Local> = Local::now();
    let formatted_now = now.format("%Y-%m-%d-%H.%M.%S");
    let logpath = format!("c:\\temp\\{APP_NAME}-{formatted_now}.txt");
    let file = File::create(logpath.clone());
    if file.is_err() {
        eprintln!("{APP_NAME}: Error when crating log file {logpath}: {:?}", file.err());
        process::exit(1);
    }
    let mut writer = BufWriter::new(file.unwrap());
    if options.verbose {
        logln(&mut writer, &format!("{APP_NAME} {APP_VERSION}"));
    }

    let start = Instant::now();

    // Convert String sources into MyGlobSearch structs
    let mut sources: Vec<(&String, MyGlobSearch)> = Vec::new();
    for source in options.sources.iter() {
        let resgs = MyGlobSearch::build(source);
        match resgs {
            Ok(gs) => sources.push((source, gs)),
            Err(e) => {
                logln(&mut writer, format!("*** Error building MyGlob: {:?}", e).as_str());
            }
        }
    }
    if sources.is_empty() {
        logln(&mut writer, "*** No source specified");
    }

    if options.verbose {
        log(&mut writer, "\nSources(s): ");
        if options.search_dirs && options.search_files {
            logln(&mut writer, "(search for files and directories)");
        } else if options.search_dirs {
            logln(&mut writer, "(search for directories)");
        } else {
            logln(&mut writer, "(search for files)");
        }

        for source in sources.iter() {
            logln(&mut writer, format!("- {}", source.0).as_str());
        }
    }

    let mut actions = Vec::<Box<dyn Action>>::new();
    for action_name in options.actions_names {
        match action_name {
            "print" => actions.push(Box::new(actions::ActionPrint::new())),
            "delete" => actions.push(Box::new(actions::ActionDelete::new(options.norecycle))),
            "rmdir" => actions.push(Box::new(actions::ActionRmdir::new(options.norecycle))),
            _ => panic!("{APP_NAME}: Internal error, unknown action_name {action_name}"),
        }
    }
    if options.verbose {
        log(&mut writer, "\nAction(s): ");
        if options.noaction {
            logln(&mut writer, "(no action will be actually performed)");
        } else {
            logln(&mut writer, "");
        }
        for ba in actions.iter() {
            logln(&mut writer, format!("- {}", (**ba).name()).as_str());
        }
        logln(&mut writer, "");
    }

    let mut files_count = 0;
    let mut dirs_count = 0;
    for gs in sources.iter() {
        for ma in gs.1.explore_iter() {
            match ma {
                MyGlobMatch::File(pb) => {
                    if options.search_files {
                        files_count += 1;
                        for ba in actions.iter() {
                            (**ba).action(&mut writer, &pb, options.noaction, options.verbose);
                        }
                    }
                }

                MyGlobMatch::Dir(pb) => {
                    if options.search_dirs {
                        dirs_count += 1;
                        for ba in actions.iter() {
                            (**ba).action(&mut writer, &pb, options.noaction, options.verbose);
                        }
                    }
                }

                MyGlobMatch::Error(err) => {
                    if options.verbose {
                        logln(&mut writer, format!("{APP_NAME}: MyGlobMatch error {}", err).as_str());
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
