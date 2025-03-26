// my_glob library
// Attempt to implement an efficient glob in Rust
//
// 2025-03-25   PV      First version, experiments around various options to select the fastest

#![allow(unused_variables, dead_code, unused_imports)]

use glob::{MatchOptions, glob_with};
use glob_match::glob_match;
use regex::Regex;
use std::{
    fs,
    path::{Path, PathBuf},
    time::Instant,
};

#[derive(Debug)]
enum Segment {
    Constant(String),
    Recurse,
    Filter(String, Regex),
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
                    let repat = format!("(?i){}", s.replace(".", r"\.").replace("*", r".*"));
                    segments.push(Segment::Filter(s.to_lowercase(), Regex::new(&repat).unwrap()));
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

    fn explore(&self, use_regex: bool) -> Vec<PathBuf> {
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

        my_glob_search(&mut res, Path::new(&self.root), &self.segments, false, &self.ignore_folders, use_regex);
        res
    }
}

/// Helper retuning file name (without folder), converted to lowercase
fn file_name_lowercase(path: &Path) -> String {
    path.file_name().unwrap().to_string_lossy().to_lowercase()
}

fn my_glob_search(res: &mut Vec<PathBuf>, root: &Path, segments: &[Segment], recurse: bool, ignore_folders: &[String], use_regex: bool) {
    //println!("Explore «{}»  filter:{:?}  recurse:{}", root.display(), &segments[0], recurse);

    match &segments[0] {
        Segment::Constant(name) => {
            let pb = PathBuf::from(root).join(name);
            if segments.len() == 1 {
                // Final segment, can only match a file
                if pb.is_file() {
                    res.push(pb);
                }
            } else {
                // non-final segment, can only match a folder
                if pb.is_dir() {
                    // Found a matching dir, ve continue exploration
                    my_glob_search(res, &pb, &segments[1..], false, ignore_folders, use_regex);
                }
            }

            // Then if recurse mode, we also search in all subfolders
            if recurse {
                if let Ok(contents) = fs::read_dir(root) {
                    for resentry in contents {
                        if let Ok(entry) = resentry {
                            let ft = entry.file_type().unwrap();
                            if ft.is_dir() {
                                let p = entry.path();
                                let fnlc = file_name_lowercase(&p);
                                if !ignore_folders.iter().any(|ie| *ie == fnlc) {
                                    my_glob_search(res, &p, segments, recurse, ignore_folders, use_regex);
                                }
                            }
                        }
                    }
                }
            }
        }

        Segment::Recurse => my_glob_search(res, root, &segments[1..], true, ignore_folders, use_regex),

        Segment::Filter(filter, re) => {
            // Search all files, return the ones that match
            let contents = fs::read_dir(root);
            if contents.is_err() {
                //logln(pwriter, &format!("*** Error enumerating folder {}: {:?}", pb.display(), contents.err()));
                // Silently ignore folers we can't read
                return;
            }

            let mut dirs: Vec<PathBuf> = Vec::new();
            for entry in contents.unwrap() {
                if entry.is_err() {
                    // logln(pwriter, &format!("*** Error accessing entry: {:?}", entry.err()));
                    // Silently ignore errors
                    continue;
                }
                let entry = entry.unwrap();
                let ft = entry.file_type().unwrap();
                let pb = entry.path();
                let flnc = file_name_lowercase(&pb);

                if ft.is_file() {
                    if segments.len() == 1 {
                        let ma = if use_regex { re.is_match(&flnc) } else { glob_match(filter, &flnc) };
                        if ma {
                            res.push(pb);
                        }
                    }
                } else if ft.is_dir() {
                    if !ignore_folders.iter().any(|ie| *ie == flnc) {
                        if segments.len() > 1 {
                            let ma = if use_regex { re.is_match(&flnc) } else { glob_match(filter, &flnc) };
                            if ma {
                                my_glob_search(res, &pb, &segments[1..], false, ignore_folders, use_regex);
                            }
                        }
                        dirs.push(pb);
                    }
                }
            }

            // Then if recurse mode, we also search in all subfolders (already collected in dirs in previous loop)
            if recurse {
                for dir in dirs {
                    my_glob_search(res, &dir, segments, true, ignore_folders, use_regex);
                }
            }
        }
    }
}

// Entry point for testing
pub fn my_glob_main_0(pattern: &str, use_regex: bool) {
    let start = Instant::now();
    let gs = MyGlobSearch::build(pattern);

    for pb in gs.explore(use_regex) {
        println!("{}", pb.display())
    }
    let duration = start.elapsed();
    println!("Search #0 ({}) in {:.3}s", if use_regex {"regex"} else {"glob"}, duration.as_secs_f64());
}

pub fn my_glob_main(pattern: &str) {
    my_glob_main_0(pattern, false); // My algo, match one segment at a time, use glob
    my_glob_main_0(pattern, true); // My algo, match one segment at a time, use regex
    // my_glob_main_1(pattern);     // Brute force, glob on full path
    // my_glob_main_2(pattern);     // Variant of #2, return String instead of PathBuf
    // my_glob_main_3(pattern);     // Brute force, regex on full path
    my_glob_main_4(pattern); // Use glob crate
}

pub fn my_glob_main_1(pattern: &str) {
    let start = Instant::now();

    let mut res: Vec<PathBuf> = Vec::new();
    let mut count = 0;
    brute_search_1(
        &mut count,
        &mut res,
        Path::new(r"C:\Development"),
        &pattern.replace("\\", "/").to_lowercase(),
    );
    for pb in res {
        println!("{}", pb.display());
    }

    let duration = start.elapsed();
    println!("File count: {}", count);
    println!("Search #1 in {:.3}s", duration.as_secs_f64());
}

fn brute_search_1(count: &mut i32, res: &mut Vec<PathBuf>, path: &Path, pattern: &str) {
    let contents = fs::read_dir(path);
    if contents.is_err() {
        return;
    }
    for resentry in contents.unwrap() {
        if let Ok(entry) = resentry {
            let pb = entry.path();
            let ft = entry.file_type().unwrap();
            if ft.is_file() {
                let name = pb.to_string_lossy().replace("\\", "/").to_lowercase();
                if glob_match(pattern, &name) {
                    res.push(pb);
                }
                *count += 1;
            } else if ft.is_dir() && entry.file_name() != "$RECYCLE.BIN" && entry.file_name() != ".git" {
                brute_search_1(count, res, &pb, pattern);
            }
        }
    }
}

pub fn my_glob_main_2(pattern: &str) {
    let start = Instant::now();

    //let pattern = "**\rsgrep.d";

    let mut res: Vec<String> = Vec::new();
    let mut count = 0;
    brute_search_2(
        &mut count,
        &mut res,
        &String::from(r"C:\Development"),
        &pattern.replace("\\", "/").to_lowercase(),
    );
    for pb in res {
        println!("{}", pb);
    }

    let duration = start.elapsed();
    println!("File count: {}", count);
    println!("Search #2 in {:.3}s", duration.as_secs_f64());
}

fn brute_search_2(count: &mut i32, res: &mut Vec<String>, path: &str, pattern: &str) {
    let contents = fs::read_dir(path);
    if contents.is_err() {
        return;
    }
    for resentry in contents.unwrap() {
        if let Ok(entry) = resentry {
            let ft = entry.file_type().unwrap();
            if ft.is_file() {
                *count += 1;
                let filefp = format!("{}\\{}", path, entry.file_name().to_string_lossy());
                let fnlc = filefp.replace("\\", "/").to_lowercase();
                if glob_match(pattern, &fnlc) {
                    res.push(filefp);
                }
            } else if ft.is_dir() && entry.file_name() != "$RECYCLE.BIN" && entry.file_name() != ".git" {
                let filefp = format!("{}\\{}", path, entry.file_name().to_string_lossy());
                brute_search_2(count, res, &filefp, pattern);
            }
        }
    }
}

pub fn my_glob_main_3(pattern: &str) {
    let start = Instant::now();

    let re = Regex::new(r"(?i)C:\\Development\\Git[^\\]*\\.*\\rsgrep\.d").unwrap();

    let mut res: Vec<PathBuf> = Vec::new();
    let mut count = 0;
    brute_search_3(&mut count, &mut res, Path::new(r"C:\Development"), &re);
    for pb in res {
        println!("{}", pb.display());
    }
    println!("File count: {}", count);
    let duration = start.elapsed();
    println!("Search #3 in {:.3}s", duration.as_secs_f64());
}

fn brute_search_3(count: &mut i32, res: &mut Vec<PathBuf>, path: &Path, re: &Regex) {
    let contents = fs::read_dir(path);
    if contents.is_err() {
        return;
    }
    for resentry in contents.unwrap() {
        if let Ok(entry) = resentry {
            let ft = entry.file_type().unwrap();
            if ft.is_file() {
                *count += 1;
                let ma = re.is_match(&entry.path().to_string_lossy());
                if ma {
                    res.push(entry.path());
                }
            } else if ft.is_dir() && entry.file_name() != "$RECYCLE.BIN" && entry.file_name() != ".git" {
                brute_search_3(count, res, &entry.path(), re);
            }
        }
    }
}

pub fn my_glob_main_4(pattern: &str) {
    let start = Instant::now();

    let mo = MatchOptions {
        case_sensitive: false,
        require_literal_separator: false,
        require_literal_leading_dot: false,
    };

    let mut res: Vec<PathBuf> = Vec::new();
    let mut count = 0;

    match glob_with(pattern, mo) {
        Ok(paths) => {
            for entry in paths {
                match entry {
                    Ok(pb) => {
                        let s = pb.to_string_lossy();
                        if !s.contains("$RECYCLE.BIN") && !s.contains(r"\.git\") && pb.is_file() {
                            count += 1;
                            res.push(pb);
                        }
                    }
                    Err(err) => {}
                };
            }
        }
        Err(err) => {}
    }
    for pb in res {
        println!("{}", pb.display());
    }

    let duration = start.elapsed();
    println!("File count: {}", count);
    println!("Search #4 in {:.3}s", duration.as_secs_f64());
}
