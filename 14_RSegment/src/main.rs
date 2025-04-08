// rsegment: Analyze books segments
//
// 2025-04-07	PV      First version

#![allow(unused)]

use std::collections::HashMap;
// standard library imports
use std::fmt::Debug;
use std::path::{Path, PathBuf};
use std::process;
use std::time::Instant;
use std::{error::Error, ops::Index};

// external crates imports
use myglob::{MyGlobMatch, MyGlobSearch};
use regex::{Captures, Regex};
use terminal_size::{Width, terminal_size};
use unicode_normalization::UnicodeNormalization;

// -----------------------------------
// Submodules

mod logging;
mod tests;

use logging::*;

// -----------------------------------
// Global constants

const APP_NAME: &str = "rsegment";
const APP_VERSION: &str = "1.0.0";

// ==============================================================================================
// Options processing

// Dedicated struct to store command line arguments
#[derive(Debug, Default)]
struct Options {
    sources: Vec<String>,
    verbose: bool,
}

impl Options {
    fn header() {
        eprintln!(
            "{APP_NAME} {APP_VERSION}
            Check book names"
        );
    }

    fn usage() {
        Options::header();
        eprintln!(
            "\nUsage: {APP_NAME} [?|-?|-h|??] [-v] source...
?|-?|-h     Show this message
??          Show advanced usage notes
-v          Verbose output
source      File or folder where to search, glob syntax supported (see advanced notes)"
        );
    }

    fn extended_usage() {
        Options::header();
        let width = if let Some((Width(w), _)) = terminal_size() {
            w as usize
        } else {
            80usize
        };
        let text = "Copyright ©2025 Pierre Violent
Advanced usage notes
--------------------

(ToDo)";

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

        Ok(options)
    }
}

// -----------------------------------
// Main

#[derive(Debug, Default)]
struct DataBag {
    files_count: usize,
    errors_count: usize,
    books: Vec<BookName>,
}

fn main() {
    // Process options
    let mut options = Options::new().unwrap_or_else(|err| {
        let msg = format!("{}", err);
        if msg.is_empty() {
            process::exit(0);
        }
        logln(&mut None, format!("*** {APP_NAME}: Problem parsing arguments: {}", err).as_str());
        process::exit(1);
    });

    // Just for dev
    if options.sources.is_empty() {
        //options.sources.push(r"W:\Livres\Informatique\**\*.pdf".to_string());
        options.sources.push(r"C:\DocumentsOD\A_Lire\**\*[\[]*.pdf".to_string());
    }

    // Prepare log writer
    let mut writer = logging::new(options.verbose);
    let mut b = DataBag { ..Default::default() };

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
        for source in sources.iter() {
            logln(&mut writer, format!("- {}", source.0).as_str());
        }
    }

    // First collect information on files in DataBag
    for gs in sources.iter() {
        for ma in gs.1.explore_iter() {
            match ma {
                MyGlobMatch::File(pb) => process_file(&mut writer, &mut b, pb),

                MyGlobMatch::Dir(_) => {}

                MyGlobMatch::Error(err) => {
                    if options.verbose {
                        logln(&mut writer, format!("{APP_NAME}: MyGlobMatch error {}", err).as_str());
                    }
                }
            }
        }
    }

    if b.books.is_empty() {
        logln(&mut writer, "*** No book found, nothing to report.");
    } else {
        logln(&mut writer, (format!("{} book(s) found, consolidating data", b.books.len()).as_str()));

        fn getter(book: &BookName) -> &str {
            &book.editor
        }
        let data_name = "Editor";

        // Counters
        let mut counter = HashMap::<&str, u32>::new();
        let mut counter_ics = HashMap::<String, (u32, HashMap<&str, u32>)>::new(); // Ignore case and spaces
        for book in b.books.iter() {
            let data = getter(book);
            *counter.entry(data).or_insert(0) += 1;

            let data_ics = filter_alphanum(data);
            let entry_ics = counter_ics.entry(data_ics).or_insert((0, HashMap::<&str, u32>::new()));
            (*entry_ics).0 += 1;
            let subentry_ics = entry_ics.1.entry(data).or_insert(0);
            *subentry_ics += 1;
        }

        // Sort and print direct counter
        println!("\n{data_name}: Simple groups, at least 2 values");
        let mut vec: Vec<(&&str, &u32)> = counter.iter().collect();
        vec.sort_by(|&a, &b| b.1.cmp(a.1));
        for (key, value) in vec.into_iter().take_while(|&x| *(x.1) > 1) {
            let skey = if key.is_empty() { "(empty)" } else { *key };
            logln(&mut writer, format!("{}: {}", skey, value).as_str());
        }

        // Sort and print case-insensitive space-insensitive direct counter
        println!("\n{data_name}: Groups ignoring case and spaces");
        let mut vec_ics: Vec<(&String, &(u32, HashMap<&str, u32>))> = counter_ics.iter().collect();
        vec_ics.sort_by(|&a, &b| (b.1.0).cmp(&a.1.0));
        let mut vec_repr = Vec::<(&str, &str)>::new(); // Collect representants for Levenshtein distance
        for (key, value) in vec_ics.into_iter() /* .take_while(|&x| *(&x.1.0) > 1) */ {
            // Now sort subvector
            let mut subvec: Vec<(&&str, &u32)> = value.1.iter().collect();
            if subvec.len() == 1 {
                // Single form class
                let (ukey, uvalue) = *subvec.first().unwrap();
                let sukey = if ukey.is_empty() { "(empty)" } else { *ukey };
                vec_repr.push((key, sukey));
                logln(&mut writer, format!("{}: {}", sukey, uvalue).as_str());
            } else {
                // Representant of class is the most encountered element
                subvec.sort_by(|a, b| b.1.cmp(a.1));
                let repr = subvec.first().unwrap();
                let (rkey, rvalue) = *repr;
                let srkey = if rkey.is_empty() { "(empty)" } else { *rkey };
                vec_repr.push((key, srkey));
                // Print representant and total count
                log(&mut writer, format!("{}: {}\t", srkey, rvalue).as_str());
                // Print all variants and individual count
                for (vkey, vvalue) in subvec.iter() {
                    let svkey = if vkey.is_empty() { "(empty)" } else { *vkey };
                    log(&mut writer, format!("{}: {}\t", svkey, vvalue).as_str());
                }
                logln(&mut writer, "");
            }
        }

        // Find close representants
        println!("\n{data_name}: Possible confusions, Levenshtein distance=1");
        for i in 0..vec_repr.len() {
            let (cnorm1, crepr1) = vec_repr[i];
            for j in i+1..vec_repr.len() {
                let (cnorm2, crepr2) = vec_repr[j];
                let d = levenshtein_distance(cnorm1, cnorm2, 1);
                if d == 1 {
                    // Found a close pair, print it
                    let s1 = if crepr1.is_empty() { "(empty)" } else { crepr1 };
                    let s2 = if crepr2.is_empty() { "(empty)" } else { crepr2 };
                    logln(&mut writer, format!("{} <-> {}", s1, s2).as_str());
                }
            }
        }
    }

    let duration = start.elapsed();
    log(&mut writer, format!("{} files(s)", b.files_count).as_str());
    log(&mut writer, format!(", {} error(s)", b.errors_count).as_str());
    logln(&mut writer, format!(" found in {:.3}s", duration.as_secs_f64()).as_str());
}

/// Only keep letters and digits, converted to ASCII lowercase
fn filter_alphanum(s: &str) -> String {
    s.chars()
        .nfd()
        .filter(|c| c.is_ascii_alphanumeric())
        .map(|c| c.to_ascii_lowercase())
        .collect::<String>()
}

fn process_file(writer: &mut LogWriter, b: &mut DataBag, pb: PathBuf) {
    b.files_count += 1;
    let book_name = get_book_name(pb);
    match book_name {
        Ok(book) => b.books.push(book),
        Err(e) => logln(writer, format!("*** {}", e).as_str()),
    }
}

#[derive(Debug)]
struct BookName {
    pb: PathBuf,
    full_title: String,
    base_title: String,
    editor: String,
    authors: String,
    isbn: String,
    edition_year: String,
    edition: String,
    year: String,
    braced: String,
}

fn get_book_name(pb: PathBuf) -> Result<BookName, String> {
    let filefp = pb.to_str().unwrap();
    let file = pb.file_name().unwrap().to_str().unwrap();
    let stem = pb.file_stem().unwrap().to_str().unwrap();
    let extension = pb.extension().unwrap().to_str().unwrap();

    if !is_balanced(file) {
        return Err(format!("Err: Unbalanced braces: {}", file));
    }

    let t = stem.split(" - ").collect::<Vec<&str>>();

    let (full_title, editor, authors, isbn) = match t.len() {
        1 => (stem, "", "", ""),
        2 => {
            if t[1].starts_with('[') {
                (t[0], t[1], "", "")
            } else {
                (t[0], "", t[1], "")
            }
        }
        3 => (t[0], t[1], t[2], ""),
        4 => (t[0], t[1], t[2], t[3]),
        _ => return Err(format!("Err: >4 seg: {}", filefp)),
    };

    if !editor.is_empty() {
        if !editor.starts_with('[') && editor.ends_with(']') {
            return Err(format!("Err: Invalid editor: {}", file));
        }
    }

    if !isbn.is_empty() {
        if !isbn.starts_with("ISBN ") {
            return Err(format!("Err: Invalid ISBN: {}", file));
        }
    }

    // Transform &str in String to free mutable borrow of pb
    let full_title = String::from(full_title);
    let editor = editor.into();
    let authors = authors.into();
    let isbn = isbn.into();

    let (base_title, edition_year, edition, year) = if full_title.contains('(') {
        let ix_start = full_title.find('(').unwrap();
        let Some(ix_end) = find_from_position(&full_title, ')', ix_start + 1) else {
            return Err(format!("Err: Missing closing parenthesis: {}", filefp));
        };
        let blockp = &full_title[ix_start + 1..ix_end];
        let base_title = String::from(&full_title[..ix_start]) + &full_title[ix_end + 1..];

        use std::sync::LazyLock;
        static BLOCK_PAR: LazyLock<Regex> =
            LazyLock::new(|| Regex::new(r"^(?:(1ère|[12]?\dè|[2-9]?1st|[2-9]?2nd|[2-9]?3rd|\d?[04-9]th|11th|12th|13th) ed, )?(\d{4}|X)$").unwrap());
        let Some(ca) = BLOCK_PAR.captures(blockp) else {
            return Err(format!("Err: Invalid block between parentheses: {}", filefp));
        };

        // Since capture group 1 is optional, can't use indexed access that will panic if captire 1 is None --> use get(1)
        let year = match ca.get(1) {
            Some(ma) => ma.as_str(),
            None => "",
        };

        (
            String::from(String::from(base_title.trim())),
            String::from(blockp),
            String::from(year),
            String::from(&ca[2]),
        )
    } else {
        (full_title.clone(), String::new(), String::new(), String::new())
    };

    let (base_title, braced) = if base_title.contains('{') {
        let ix_start = base_title.find('{').unwrap();
        let Some(ix_end) = find_from_position(&base_title, '}', ix_start + 1) else {
            return Err(format!("Err: Missing closing curly brace: {}", filefp));
        };
        let blockb = &base_title[ix_start + 1..ix_end];
        let base_title = String::from(&base_title[..ix_start]) + &base_title[ix_end + 1..];

        (String::from(base_title.trim_end()), String::from(blockb))
    } else {
        (base_title, String::new())
    };

    Ok(BookName {
        pb,
        full_title,
        base_title,
        editor,
        authors,
        isbn,
        edition_year,
        edition,
        year,
        braced,
    })
}

fn find_from_position(s: &str, c: char, start_position: usize) -> Option<usize> {
    if start_position >= s.len() {
        return None; // Start position is out of bounds
    }

    let search_slice = &s[start_position..];
    // Note that the following map is NOT the usual iterator map, but Option::map
    // Maps an Option<T> to Option<U> by applying a function to a contained value (if Some) or returns None (if None).
    search_slice.find(c).map(|relative_position| start_position + relative_position)
}

/// Checks that () [] {} «» ‹› pairs are correctly embedded and closed in a string
pub fn is_balanced(s: &str) -> bool {
    // Unit tests in rcheckfiles
    let mut stack = Vec::<char>::new();
    let mut current_state = ' ';

    for c in s.chars() {
        match c {
            '(' | '[' | '{' | '«' | '‹' => {
                stack.push(current_state);
                current_state = c;
            }
            ')' | ']' | '}' | '»' | '›' => {
                if stack.len() == 0 {
                    return false;
                }

                let opener = match c {
                    ')' => '(',
                    ']' => '[',
                    '}' => '{',
                    '»' => '«',
                    '›' => '‹',
                    _ => unreachable!(),
                };
                if current_state == opener {
                    current_state = stack.pop().unwrap();
                } else {
                    return false;
                }
            }
            _ => {}
        }
    }

    current_state == ' '
}


/// Computes Levenshtein distance between two strings with early exit if the distance exceeds dmax.
pub fn levenshtein_distance(s1: &str, s2: &str, dmax: usize) -> usize {
    let len1 = s1.len();
    let len2 = s2.len();

    // If the difference in lengths exceeds dmax, return early
    if (len1 as isize - len2 as isize).abs() as usize > dmax {
        return dmax + 1;
    }

    let mut prev_row: Vec<usize> = (0..=len2).collect();
    let mut curr_row = vec![0; len2 + 1];

    for (i, c1) in s1.chars().enumerate() {
        curr_row[0] = i + 1;

        let mut min_in_row = curr_row[0]; // Track the minimum value in the current row
        for (j, c2) in s2.chars().enumerate() {
            let cost = if c1 == c2 { 0 } else { 1 };
            curr_row[j + 1] = (prev_row[j + 1] + 1)
                .min(curr_row[j] + 1)
                .min(prev_row[j] + cost);

            min_in_row = min_in_row.min(curr_row[j + 1]);
        }

        // Early exit if the minimum value in the row exceeds dmax
        if min_in_row > dmax {
            return dmax + 1;
        }

        prev_row.copy_from_slice(&curr_row);
    }

    let distance = curr_row[len2];
    if distance > dmax {
        dmax + 1
    } else {
        distance
    }
}
