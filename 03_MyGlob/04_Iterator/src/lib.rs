// my_glob library
// Attempt to implement an efficient glob in Rust
//
// 2025-03-25   PV      First version, experiments around various options to select the fastest
// 2025-03-26   PV      Second version, use my own algorithm, and use regex for Filter segments match check
// 2025-03-26   PV      Third version, a non-recursive version of explore to prepare for iterator version
// 2025-03-27   PV      Fourth version, iterator

#![allow(unused_variables, dead_code, unused_imports)]

use regex::Regex;
use std::{
    fs::{self, File},
    io::Error,
    path::{Iter, Path, PathBuf},
    time::Instant,
};

#[derive(Debug)]
enum Segment {
    Constant(String),
    Recurse,
    Filter(Regex),
}

#[derive(Debug)]
struct MyGlobSearch {
    root: String,
    segments: Vec<Segment>,
    ignore_folders: Vec<String>,
}

impl MyGlobSearch {
    fn build(pattern: &str) -> Self {
        // Break pattern into root and a vector of Segments

        // Simple helper to detect recurse or filter segments
        // For now, we don't manage escape character to suppress special interpretation of * ? ...
        fn is_filter_segment(pat: &str) -> bool {
            pat.chars().any(|c| "*?[{".contains(c))
        }

        let v: Vec<&str> = pattern.split(&['/', '\\'][..]).collect();
        let k = v.iter().enumerate().find(|&(_, &s)| is_filter_segment(s));

        let (root, segments) = if k.is_none() {
            // No filter segment, the whole pattern is just a constant string
            (String::from(pattern), Vec::<Segment>::new())
        } else {
            let split = k.unwrap().0;
            let root = v[..split].join("\\");
            let mut segments: Vec<Segment> = Vec::new();
            for &s in &v[split..] {
                if s == "**" {
                    segments.push(Segment::Recurse);
                } else if is_filter_segment(s) {
                    // Simple basic translation glob->regex, to elaborate
                    let repat = format!("(?i){}", s.replace(".", r"\.").replace("*", r".*").replace("?", r"."));
                    segments.push(Segment::Filter(Regex::new(&repat).unwrap()));
                } else {
                    segments.push(Segment::Constant(String::from(s)));
                }
            }
            (root, segments)
        };

        MyGlobSearch {
            root,
            segments,
            ignore_folders: vec![
                String::from("$recycle.bin"),
                String::from("system volume information"),
                String::from(".git"),
            ],
        }
    }

    fn explore_iter(&self) -> impl Iterator<Item = MyGlobMatch> {
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
                ignore_folders: &self.ignore_folders,
            };
        }

        // Normal case, start iterator at root
        MyGlobIteratorState {
            stack: vec![SearchPendingData::Folder(Path::new(&self.root).to_path_buf(), 0, false)],
            segments: &self.segments,
            ignore_folders: &self.ignore_folders,
        }
    }
}

// Enum returned by iterator
// Looks loke a Result<PathBuf, Error>, but I prefer to use an ad-hoc Enum so I can expand it later (return folders, non-io errors, ...)
#[derive(Debug)]
enum MyGlobMatch {
    File(PathBuf),
    Error(Error),
}

// Internal state of iterator
struct MyGlobIteratorState<'a> {
    stack: Vec<SearchPendingData>,
    segments: &'a Vec<Segment>,
    ignore_folders: &'a Vec<String>,
}

// Internal structure of derecursived search, pending data to explore or return, stored in stack
#[derive(Debug)]
enum SearchPendingData {
    Folder(PathBuf, usize, bool),
    File(PathBuf),
    Error(Error),
}

impl Iterator for MyGlobIteratorState<'_> {
    type Item = MyGlobMatch;

    fn next(&mut self) -> Option<Self::Item> {
        while let Some(fof) = self.stack.pop() {
            match fof {
                SearchPendingData::Error(e) => {
                    return Some(MyGlobMatch::Error(e));
                }

                SearchPendingData::File(pb) => {
                    return Some(MyGlobMatch::File(pb));
                }

                SearchPendingData::Folder(root, depth, recurse) => {
                    match &self.segments[depth] {
                        Segment::Constant(name) => {
                            let pb = root.join(name);
                            if depth == self.segments.len() - 1 {
                                // Final segment, can only match a file
                                if pb.is_file() {
                                    self.stack.push(SearchPendingData::File(pb));
                                }
                            } else {
                                // non-final segment, can only match a folder
                                if pb.is_dir() {
                                    // Found a matching directory, we continue exploration in next loop
                                    self.stack.push(SearchPendingData::Folder(pb, depth + 1, false));
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
                                                        if !self.ignore_folders.iter().any(|ie| *ie == fnlc) {
                                                            self.stack.push(SearchPendingData::Folder(p, depth, true));
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
                            self.stack.push(SearchPendingData::Folder(root, depth + 1, true));
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
                                                    if !self.ignore_folders.iter().any(|ie| *ie == flnc) {
                                                        if depth < self.segments.len() - 1 && re.is_match(&fname) {
                                                            self.stack.push(SearchPendingData::Folder(pb.clone(), depth + 1, false));
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
                                    self.stack.push(SearchPendingData::Folder(dir, depth, true));
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

// Entry point for testing
pub fn my_glob_main(pattern: &str) {
    for pass in 0..3 {
        println!("\nTest #{pass}");

        let start = Instant::now();
        let gs = MyGlobSearch::build(pattern);

        let mut nf = 0;
        for ma in gs.explore_iter() {
            match ma {
                MyGlobMatch::File(pb) => {
                    println!("{}", pb.display());
                    nf += 1;
                }
                MyGlobMatch::Error(e) => {
                    println!("{}", e);
                }
            }
        }
        println!("{nf} file(s) found");
        let duration = start.elapsed();
        println!("Iterator search in {:.3}s", duration.as_secs_f64());
    }
}
