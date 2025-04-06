// rfind: A Rust version of find/XFind/Search
//
// 2025-03-29	PV      First version
// 2025-03-31	PV      1.1.0 Action Dir
// 2025-04-03	PV      1.2.0 Core reorganization, logging module
// 2025-04-06	PV      1.3.0 Use fs::remove_dir_all instead of fs::remove_dir to delete non-empty directories

//#![allow(unused)]

// standard library imports
use std::collections::HashSet;
use std::error::Error;
use std::fmt::Debug;
use std::path::Path;
use std::process;
use std::time::Instant;

// external crates imports
use myglob::{MyGlobMatch, MyGlobSearch};
use terminal_size::{Width, terminal_size};

// -----------------------------------
// Submodules

mod logging;
mod actions;
mod tests;

use logging::*;

// -----------------------------------
// Global constants

const APP_NAME: &str = "rfind";
const APP_VERSION: &str = "1.3.0";

// -----------------------------------
// Traits

trait Action: Debug {
    fn action(&self, lw: &mut LogWriter, path: &Path, noaction: bool, verbose: bool);
    fn name(&self) -> &'static str;
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
    no_recycle: bool,
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
"\nUsage: {APP_NAME} [?|-?|-h|??] [-v] [-n] [-f|-type f|-d|-type d] [-[no]recycle] [action...] source...
?|-?|-h     Show this message
??          Show advanced usage notes
-v          Verbose output
-n          No action: display actions, but don't execute them
-f|-type f  Search for files
-d|-type d  Search for directories
-norecycle  Delete forever (default: -recycle, delete local files to recycle bin)
source      File or folder where to search, glob syntax supported (see advanced notes)

Actions:
-print      Default, print matching files names and dir names (dir names end with \\)
-dir        Variant of -print, with last modification date and size
-delete     Delete matching files
-rmdir      Delete matching directories, whether empty or not"
        );
    }

    fn extended_usage() {
        Options::header();
        let width = if let Some((Width(w), _)) = terminal_size() {
            w as usize
        } else {
            80usize
        };
        let text =
"Copyright ©2025 Pierre Violent\n
Advanced usage notes\n--------------------\n
Glob pattern nules:
•   ? matches any single character.
•   * matches any (possibly empty) sequence of characters.
•   ** matches the current directory and arbitrary subdirectories. To match files in arbitrary subdiretories, use **\\*. This sequence must form a single path component, so both **a and b** are invalid and will result in an error.
•   [...] matches any character inside the brackets. Character sequences can also specify ranges of characters, as ordered by Unicode, so e.g. [0-9] specifies any character between 0 and 9 inclusive. An unclosed bracket is invalid.
•   [!...] is the negation of [...], i.e. it matches any characters not in the brackets.
•   The metacharacters ?, *, [, ] can be matched by using brackets (e.g. [?]). When a ] occurs immediately following [ or [! then it is interpreted as being part of, rather then ending, the character set, so ] and NOT ] can be matched by []] and [!]] respectively. The - character can be specified inside a character sequence pattern by placing it at the start or the end, e.g. [abc-].
•   {choice1,choice2...}  match any of the comma-separated choices between braces. Can be nested, and include ?, * and character classes.
•   Character classes [ ] accept regex syntax, see https://docs.rs/regex/latest/regex/#character-classes for character classes and escape sequences supported.";

        println!("{}", Self::format_text(text, width));
    }

    fn format_text(text: &str, width: usize) -> String {
        let mut s = String::new();
        for line in text.split('\n') {
            if !s.is_empty() {
                s.push('\n');
            }
            s.push_str(Self::format_line(line, width).as_str());
        }
        s
    }

    fn format_line(line: &str, width: usize) -> String {
        let mut result = String::new();
        let mut current_line_length = 0;

        let left_margin = if line.starts_with('•') { "  " } else { "" };

        for word in line.split_whitespace() {
            let word_length = word.len();

            if current_line_length + word_length + 1 <= width {
                if !result.is_empty() {
                    result.push(' ');
                    current_line_length += 1; // Add space
                }
                result.push_str(word);
                current_line_length += word_length;
            } else {
                if !result.is_empty() {
                    result.push('\n');
                    current_line_length = if !left_margin.is_empty() {
                        result.push_str(left_margin);
                        2
                    } else {
                        0
                    };
                }
                result.push_str(word);
                current_line_length += word_length;
            }
        }
        result
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

                    "recycle" => options.no_recycle = false,
                    "norecycle" => options.no_recycle = true,

                    "print" => {
                        options.actions_names.insert("print");
                    }
                    "dir" => {
                        options.actions_names.insert("dir");
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
        logln(&mut None, format!("*** {APP_NAME}: Problem parsing arguments: {}", err).as_str());
        process::exit(1);
    });

    // Prepare log writer
    let mut writer = logging::new(options.verbose);

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
        logln(&mut writer, format!("*** No source specified. Use {APP_NAME} ? to show usage.").as_str());
        process::exit(1);
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
    for action_name in options.actions_names.iter() {
        match *action_name {
            "print" => {
                if options.actions_names.contains("dir") {
                    logln(&mut writer, "*** Both actions print and dir used, action print ignored.");
                } else {
                    actions.push(Box::new(actions::ActionPrint::new(false)))
                }
            }
            "dir" => actions.push(Box::new(actions::ActionPrint::new(true))),
            "delete" => actions.push(Box::new(actions::ActionDelete::new(options.no_recycle))),
            "rmdir" => actions.push(Box::new(actions::ActionRmdir::new(options.no_recycle))),
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
        if files_count + dirs_count > 0 {
            logln(&mut writer, "");
        }
        if options.search_files {
            log(&mut writer, format!("{files_count} files(s)").as_str());
        }
        if options.search_dirs {
            if options.search_files {
                log(&mut writer, ", ");
            }
            log(&mut writer, format!("{dirs_count} dir(s)").as_str());
        }
        logln(&mut writer, format!(" found in {:.3}s", duration.as_secs_f64()).as_str());
    }
}
