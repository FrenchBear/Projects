// my_glob library
// Attempt to implement an efficient glob in Rust
//
// 2025-03-25   PV      First version, experiments around various options to select the fastest
// 2025-03-26   PV      Second version, use my own algorithm, and use regex for Filter segments match check
// 2025-03-26   PV      Third version, a non-recursive version of explore to prepare for iterator version
// 2025-03-27   PV      Fourth version, iterator
// 2025-03-27   PV      1.0  First official version of the crate
// 2025-03-28   PV      1.1  Proper conversion from glob to regex with glob_to_segments
// 2025-03-29   PV      1.2  Test cases, documentation of regex, bug of \ inside a [ ] fixed
// 2025-03-29   PV      1.3  Now returns files and directories

#![allow(unused_variables, dead_code, unused_imports)]

mod tests;

use regex::Regex;
use std::{
    fs::{self, File},
    io::Error,
    path::{Iter, Path, PathBuf},
};

// Internal structure, store one segment of a glob pattern, either a constant string, a recurse tag (**), or a glob filter, converted into a Regex
#[derive(Debug)]
enum Segment {
    Constant(String),
    Recurse,
    Filter(Regex),
}

/// Main struct of myglob, string information such as root part, glob, dirs to ignore, ...
#[derive(Debug)]
pub struct MyGlobSearch {
    root: String,
    segments: Vec<Segment>,
    ignore_dirs: Vec<String>,
}

/// Error returned by MyGlob, either a Regex error or an io::Error
#[derive(Debug)]
pub enum MyGlobError {
    IoError(std::io::Error),
    RegexError(regex::Error),
    GlobError(String),
}

impl MyGlobSearch {
    /// Constructs a new MyGlobSearch based on pattern glob expression, or return an error if there is Glob/Regex error
    pub fn build(globstr: &str) -> Result<Self, MyGlobError> {
        if globstr.ends_with('\\') || globstr.ends_with('/') {
            return Err(MyGlobError::GlobError("Glob pattern can't end with \\ or /".to_string()));
        }
        // Add a final \ so that we don't have duplicate code to process last segment
        let glob = globstr.to_string() + "\\";

        // First get root part, the constant segments at the beginning
        let mut cut = 0;
        for (ix, c) in glob.chars().enumerate() {
            if "*?[{".contains(c) {
                break;
            } else if c == '/' || c == '\\' {
                // Note that \ have a special meaning between [ ] but we break the loop at the first [ so it's Ok
                cut = ix;
            }
        }
        let mut root = globstr[..cut].to_string();
        if root.is_empty() {
            root.push('.');
        }

        // Then build segments
        let segments = if cut == globstr.len() {
            Vec::<Segment>::new()
        } else {
            Self::glob_to_segments(&glob[(cut + 1)..])?
        };

        Ok(MyGlobSearch {
            root,
            segments,
            ignore_dirs: vec![
                String::from("$recycle.bin"),
                String::from("system volume information"),
                String::from(".git"),
            ],
        })
    }

    // Conversion of a glob string into a Vec<Segment>, or an error if glob syntax is invalid
    pub(crate) fn glob_to_segments(globstr: &str) -> Result<Vec<Segment>, MyGlobError> {
        // globstr ends with \ so no duplicate code to process last segment
        assert!(globstr.ends_with('\\'));

        let mut segments = Vec::<Segment>::new();
        let mut regex_buffer = String::new();
        let mut constant_buffer = String::new();
        let mut brace_depth = 0;
        let mut iter = globstr.chars().peekable();
        while let Some(c) = iter.next() {
            if c != '\\' && c != '/' {
                constant_buffer.push(c);
            }

            match c {
                '*' => regex_buffer.push_str(".*"),
                '?' => regex_buffer.push('.'),
                '{' => {
                    brace_depth += 1;
                    regex_buffer.push('(');
                }
                ',' if brace_depth > 0 => regex_buffer.push('|'),
                '}' => {
                    brace_depth -= 1;
                    if brace_depth < 0 {
                        return Err(MyGlobError::GlobError("Extra closing }".to_string()));
                    }
                    regex_buffer.push(')');
                }
                '\\' | '/' => {
                    if brace_depth > 0 {
                        return Err(MyGlobError::GlobError("Invalid \\ between { }".to_string()));
                    }

                    if constant_buffer == "**" {
                        segments.push(Segment::Recurse);
                    } else if constant_buffer.contains("**") {
                        return Err(MyGlobError::GlobError("Glob pattern ** must be alone between \\".to_string()));
                    } else if constant_buffer.chars().any(|c| "*?[{".contains(c)) {
                        if brace_depth > 0 {
                            return Err(MyGlobError::GlobError("Unclosed {".to_string()));
                        }

                        let repat = format!("(?i)^{}$", regex_buffer);
                        let resre = Regex::new(&repat);
                        match resre {
                            Ok(re) => segments.push(Segment::Filter(re)),
                            Err(e) => {
                                return Err(MyGlobError::RegexError(e));
                            }
                        }
                    } else {
                        segments.push(Segment::Constant(constant_buffer.clone()));
                    }
                    regex_buffer.clear();
                    constant_buffer.clear();
                }
                '[' => {
                    regex_buffer.push('[');
                    let mut depth = 1;

                    // Special case, ! at the beginning of a glob match is converted to a ^ in regex syntax
                    match iter.peek() {
                        Some(next_c) => {
                            if *next_c == '!' {
                                iter.next();
                                regex_buffer.push('^');
                            }
                        }
                        None => {}
                    }

                    while let Some(inner_c) = iter.next() {
                        match inner_c {
                            ']' => {
                                regex_buffer.push(inner_c);
                                depth -= 1;
                                if depth == 0 {
                                    break;
                                }
                            }
                            '\\' => {
                                if let Some(next_c) = iter.next() {
                                    regex_buffer.push('\\');
                                    regex_buffer.push(next_c);
                                } else {
                                    regex_buffer.push('\\'); //Handle trailing backslash
                                }
                            }
                            _ => regex_buffer.push(inner_c),
                        }
                    }
                }
                '.' | '+' | '(' | ')' | '|' | '^' | '$' => {
                    regex_buffer.push('\\');
                    regex_buffer.push(c);
                }
                _ => regex_buffer.push(c),
            }
        }
        if !regex_buffer.is_empty() {
            return Err(MyGlobError::GlobError("Internal error".to_string()));
        }

        Ok(segments)
    }

    /// Iterator returning all files matching glob pattern
    pub fn explore_iter(&self) -> impl Iterator<Item = MyGlobMatch> {
        // Special case, segments is empty, only search for file
        // It's actually a but faster to process it before iterator loop, so there is no special case to handle at the beginning of each iterator call
        if self.segments.is_empty() {
            let p = Path::new(&self.root);
            let mut stack: Vec<SearchPendingData> = Vec::new();
            if p.is_file() {
                stack.push(SearchPendingData::File(p.to_path_buf()));
            }
            return MyGlobIteratorState {
                stack,
                segments: &self.segments,
                ignore_dirs: &self.ignore_dirs,
            };
        }

        // Normal case, start iterator at root
        MyGlobIteratorState {
            stack: vec![SearchPendingData::DirToExplore(Path::new(&self.root).to_path_buf(), 0, false)],
            segments: &self.segments,
            ignore_dirs: &self.ignore_dirs,
        }
    }
}

// Enum returned by iterator
#[derive(Debug)]
pub enum MyGlobMatch {
    File(PathBuf),
    Dir(PathBuf),
    Error(Error),
}

// Internal state of iterator
struct MyGlobIteratorState<'a> {
    stack: Vec<SearchPendingData>,
    segments: &'a Vec<Segment>,
    ignore_dirs: &'a Vec<String>,
}

// Internal structure of derecursived search, pending data to explore or return, stored in stack
#[derive(Debug)]
enum SearchPendingData {
    File(PathBuf),                      // Data to return
    Dir(PathBuf),                       // Data to return
    DirToExplore(PathBuf, usize, bool), // Dir not explored yet
    Error(Error),
}

impl Iterator for MyGlobIteratorState<'_> {
    type Item = MyGlobMatch;

    fn next(&mut self) -> Option<Self::Item> {
        while let Some(fof) = self.stack.pop() {
            match fof {
                SearchPendingData::Error(e) => return Some(MyGlobMatch::Error(e)),

                SearchPendingData::File(pb) => return Some(MyGlobMatch::File(pb)),

                SearchPendingData::Dir(pb) => return Some(MyGlobMatch::Dir(pb)),

                SearchPendingData::DirToExplore(root, depth, recurse) => {
                    match &self.segments[depth] {
                        Segment::Constant(name) => {
                            let pb = root.join(name);
                            if depth == self.segments.len() - 1 {
                                // Final segment
                                if pb.is_file() {
                                    // Case-insensitive comparison is provided by filesystem
                                    self.stack.push(SearchPendingData::File(pb));
                                } else if pb.is_dir() {
                                    self.stack.push(SearchPendingData::Dir(pb.clone()));
                                }
                            } else {
                                // non-final segment, can only match a folder
                                if pb.is_dir() {
                                    // Found a matching directory, we continue exploration in next loop
                                    self.stack.push(SearchPendingData::DirToExplore(pb, depth + 1, false));
                                }
                            }

                            // Then if recurse mode, we also search in all subfolders
                            if recurse {
                                match fs::read_dir(&root) {
                                    Ok(contents) => {
                                        for resentry in contents {
                                            match resentry {
                                                Ok(entry) => {
                                                    if entry.file_type().unwrap().is_dir() {
                                                        let p = entry.path();
                                                        let fnlc = p.file_name().unwrap().to_string_lossy().to_lowercase();
                                                        if !self.ignore_dirs.iter().any(|ie| *ie == fnlc) {
                                                            self.stack.push(SearchPendingData::DirToExplore(p, depth, true));
                                                        }
                                                    }
                                                }

                                                Err(e) => {
                                                    let f = std::io::Error::new(e.kind(), format!("Error enumerating dir {}: {}", root.display(), e));
                                                    self.stack.push(SearchPendingData::Error(f));
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                    Err(e) => {
                                        let f = std::io::Error::new(e.kind(), format!("Error reading dir {}: {}", root.display(), e));
                                        self.stack.push(SearchPendingData::Error(f));
                                    }
                                }
                            }
                        }

                        Segment::Recurse => {
                            self.stack.push(SearchPendingData::DirToExplore(root, depth + 1, true));
                        }

                        Segment::Filter(re) => {
                            // Search all files, return the ones that match
                            let mut dirs: Vec<PathBuf> = Vec::new();

                            match fs::read_dir(&root) {
                                Ok(contents) => {
                                    for entry in contents {
                                        match entry {
                                            Ok(entry) => {
                                                let ft = entry.file_type().unwrap();
                                                let pb = entry.path();
                                                let fname = entry.file_name().to_string_lossy().to_string();

                                                if ft.is_file() {
                                                    if depth == self.segments.len() - 1 && re.is_match(&fname) {
                                                        self.stack.push(SearchPendingData::File(pb));
                                                    }
                                                } else if ft.is_dir() {
                                                    let flnc = fname.to_lowercase();
                                                    if !self.ignore_dirs.iter().any(|ie| *ie == flnc) {
                                                        if re.is_match(&fname) {
                                                            if depth == self.segments.len() - 1 {
                                                                self.stack.push(SearchPendingData::Dir(pb.clone()));
                                                            } else {
                                                                self.stack.push(SearchPendingData::DirToExplore(pb.clone(), depth + 1, false));
                                                            }
                                                        }
                                                        dirs.push(pb);
                                                    }
                                                }
                                            }

                                            Err(e) => {
                                                let f = std::io::Error::new(e.kind(), format!("Error enumerating dir {}: {}", root.display(), e));
                                                self.stack.push(SearchPendingData::Error(f));
                                                continue;
                                            }
                                        }
                                    }
                                }

                                Err(e) => {
                                    let f = std::io::Error::new(e.kind(), format!("Error reading dir {}: {}", root.display(), e));
                                    self.stack.push(SearchPendingData::Error(f));
                                }
                            }

                            // Then if recurse mode, we also search in all subfolders (already collected in dirs in previous loop to avoid enumerating directory twice)
                            if recurse {
                                for dir in dirs {
                                    self.stack.push(SearchPendingData::DirToExplore(dir, depth, true));
                                }
                            }
                        }
                    }
                }
            }
        }

        None
    }
}
