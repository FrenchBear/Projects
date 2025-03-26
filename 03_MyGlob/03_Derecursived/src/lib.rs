// my_glob library
// Attempt to implement an efficient glob in Rust
//
// 2025-03-25   PV      First version, experiments around various options to select the fastest
// 2025-03-26   PV      Second version, use my own algorithm, and use regexp for Filter segments match check
// 2025-03-26   PV      Third version, a non-recursive version of explore to prepare for iterator version

#![allow(unused_variables, dead_code, unused_imports)]

use regex::Regex;
use std::{
    fs::{self, File},
    path::{Path, PathBuf},
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
        // Break pattern into root and a vector of Segmennts

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
            ignore_folders: vec![String::from("$recycle.bin"), String::from(".git")],
        }
    }

    fn explore(&self) -> Vec<PathBuf> {
        let mut res = Vec::<PathBuf>::new();

        // Special case, segments is empty, only search for file
        if self.segments.is_empty() {
            let p = Path::new(&self.root);
            if p.is_file() {
                res.push(p.to_path_buf());
            }

            return res;
        }

        // Maybe check root...

        my_glob_search(&mut res, Path::new(&self.root), &self.segments, false, &self.ignore_folders);
        res
    }
}

#[derive(Debug)]
enum FileOrFolder {
    Folder(PathBuf, usize, bool),
    File(PathBuf),
}

// Non-recursive version, use a stack to store parameters for recurse call
fn my_glob_search(res: &mut Vec<PathBuf>, initialroot: &Path, segments: &[Segment], recursezzz: bool, ignore_folders: &[String]) {
    // Stack initialization, we start with just root folder
    let mut stack: Vec<FileOrFolder> = Vec::new();
    stack.push(FileOrFolder::Folder(initialroot.to_path_buf(), 0, false));

    while !stack.is_empty() {
        let fof = stack.pop().unwrap();

        match fof {
            FileOrFolder::File(file) => {
                res.push(file);
                continue;
            }

            FileOrFolder::Folder(root, depth, recurse) => {
                match &segments[depth] {
                    Segment::Constant(name) => {
                        let pb = PathBuf::from(root.clone()).join(name);
                        if depth == segments.len() - 1 {
                            // Final segment, can only match a file
                            if pb.is_file() {
                                res.push(pb);
                            }
                        } else {
                            // non-final segment, can only match a folder
                            if pb.is_dir() {
                                // Found a matching dir, ve continue exploration
                                stack.push(FileOrFolder::Folder(pb, depth + 1, false));
                            }
                        }

                        // Then if recurse mode, we also search in all subfolders
                        if recurse {
                            if let Ok(contents) = fs::read_dir(root) {
                                for entry in contents.flatten() {
                                    if entry.file_type().unwrap().is_dir() {
                                        let p = entry.path();
                                        let fnlc = p.file_name().unwrap().to_string_lossy().to_lowercase();
                                        if !ignore_folders.iter().any(|ie| *ie == fnlc) {
                                            stack.push(FileOrFolder::Folder(p, depth, true));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Segment::Recurse => {
                        stack.push(FileOrFolder::Folder(root, depth + 1, true));
                    }

                    Segment::Filter(re) => {
                        // Search all files, return the ones that match
                        let contents = fs::read_dir(root);
                        if contents.is_err() {
                            // Silently ignore folers we can't read
                            continue;
                        }

                        let mut dirs: Vec<PathBuf> = Vec::new();
                        for entry in contents.unwrap() {
                            if entry.is_err() {
                                // Silently ignore errors
                                continue;
                            }
                            let entry = entry.unwrap();
                            let ft = entry.file_type().unwrap();
                            let pb = entry.path();
                            let fname = entry.file_name().to_string_lossy().to_string();

                            if ft.is_file() {
                                if depth==segments.len() - 1 && re.is_match(&fname) {
                                    res.push(pb);
                                }
                            } else if ft.is_dir() {
                                let flnc = fname.to_lowercase();
                                if !ignore_folders.iter().any(|ie| *ie == flnc) {
                                    if depth<segments.len() - 1 && re.is_match(&fname) {
                                        stack.push(FileOrFolder::Folder(pb.clone(), depth + 1, false));
                                    }
                                    dirs.push(pb);
                                }
                            }
                        }

                        // Then if recurse mode, we also search in all subfolders (already collected in dirs in previous loop)
                        if recurse {
                            for dir in dirs {
                                stack.push(FileOrFolder::Folder(dir, depth, true));
                            }
                        }
                    }
                }
            }
        }
    }

    // Return res
}

// Entry point for testing
pub fn my_glob_main(pattern: &str) {
    let start = Instant::now();
    let gs = MyGlobSearch::build(pattern);

    for pb in gs.explore() {
        println!("{}", pb.display())
    }
    let duration = start.elapsed();
    println!("Search in {:.3}s", duration.as_secs_f64());
}
