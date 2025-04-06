// actions.rs, definition of actions
//
// 2025-03-29	PV      First version
// 2025-03-31	PV      Action Print with option detail
// 2025-04-06	PV      Use fs::remove_dir_all instead of fs::remove_dir to delete non-empty directories

use super::*;

use chrono::{DateTime, Local, Utc};
use num_format::{Locale, ToFormattedString};
use std::fs;

use trash::delete;

// ===============================================================
// Print action

#[derive(Debug)]
pub struct ActionPrint {
    detailed_output: bool,
}

impl ActionPrint {
    pub fn new(detailed_output: bool) -> Self {
        ActionPrint { detailed_output }
    }
}

impl Action for ActionPrint {
    fn action(&self, lw: &mut LogWriter, path: &Path, _do_it: bool, _verbose: bool) {
        //println!("Action Print\n  Path: {}\n  do_it: {do_it}\n  verbose: {verbose}", path.display());
        if path.is_file() {
            if self.detailed_output {
                match path.metadata() {
                    Ok(meta) => {
                        // File size formatting
                        let file_size = meta.len();
                        let formatted_size = file_size.to_formatted_string(&Locale::fr); //Use en for now. Later we will find the user locale.

                        // Last modified time formatting
                        let modified_time = meta.modified().unwrap(); // Get last modified time
                        let datetime_utc: DateTime<Utc> = DateTime::<Utc>::from(modified_time);
                        let datetime_local = datetime_utc.with_timezone(&Local);
                        let formatted_time = datetime_local.format("%d/%m/%Y %H:%M:%S");

                        logln(lw, format!("{:>19}   {:>15} {}", formatted_time, formatted_size, path.display()).as_str());
                    }
                    Err(e) => {
                        logln(lw, format!("*** Error retrieving metadata for file {}: {e}", path.display()).as_str());
                    }
                }
            } else {
                logln(lw, path.display().to_string().as_str());
            }
        } else {
            if self.detailed_output {
                match path.metadata() {
                    Ok(meta) => {
                        // Last modified time formatting
                        let modified_time = meta.modified().unwrap(); // Get last modified time
                        let datetime_utc: DateTime<Utc> = DateTime::<Utc>::from(modified_time);
                        let datetime_local = datetime_utc.with_timezone(&Local);
                        let formatted_time = datetime_local.format("%d/%m/%Y %H:%M:%S");

                        logln(lw, format!("{:>19}   {:<15} {}\\", formatted_time, "<DIR>", path.display()).as_str());
                    }
                    Err(e) => {
                        logln(lw, format!("*** Error retrieving metadata for dir {}: {e}", path.display()).as_str());
                    }
                }
            } else {
                logln(lw, (path.display().to_string() + "\\").as_str());
            }
        }
    }

    fn name(&self) -> &'static str {
        if self.detailed_output {
            "Dir"
        } else {
            "Print"
        }
    }
}

// ===============================================================
// Delete action (remove files)

#[derive(Debug)]
pub struct ActionDelete {
    no_recycle: bool,
}

impl ActionDelete {
    pub fn new(no_recycle: bool) -> Self {
        ActionDelete { no_recycle }
    }
}

impl Action for ActionDelete {
    fn action(&self, lw: &mut LogWriter, path: &Path, noaction: bool, verbose: bool) {
        if path.is_file() {
            let s = quoted_path(path);
            let qp = s.as_str();
            if self.no_recycle {
                logln(lw, format!("DEL {}", qp).as_str());
                if !noaction {
                    match fs::remove_file(path) {
                        Ok(_) => {
                            if verbose {
                                logln(lw, format!("File {} deleted successfully.", qp).as_str());
                            }
                        }
                        Err(e) => logln(lw, format!("*** Error deleting file (fs::remove_file) {}: {}", qp, e).as_str()),
                    }
                }
            } else {
                logln(lw, format!("PDEL {}", qp).as_str());
                if !noaction {
                    match delete(path) {
                        Ok(_) => {
                            if verbose {
                                logln(lw, format!("File {} deleted successfully.", qp).as_str());
                            }
                        }
                        Err(e) => logln(lw, format!("*** Error deleting file (trash::delete) {}: {}", qp, e).as_str()),
                    }
                }
            }
        }
    }

    fn name(&self) -> &'static str {
        if self.no_recycle {
            "Delete files (permanent)"
        } else {
            "Delete files (use recycle bin for local files, permanently for remote files)"
        }
    }
}

fn quoted_path(path: &Path) -> String {
    let n = path.display().to_string();
    if n.contains(' ') { format!("\"{}\"", n) } else { n }
}

// ===============================================================
// Rmdir action (remove directories)

#[derive(Debug)]
pub struct ActionRmdir {
    no_recycle: bool,
}

impl ActionRmdir {
    pub fn new(no_recycle: bool) -> Self {
        ActionRmdir { no_recycle }
    }
}

impl Action for ActionRmdir {
    fn action(&self, writer: &mut LogWriter, path: &Path, noaction: bool, verbose: bool) {
        if path.is_dir() {
            let s = quoted_path(path);
            let qp = s.as_str();
            if self.no_recycle {
                logln(writer, format!("RD /S {}", qp).as_str());
                if !noaction {
                    match fs::remove_dir_all(path) {
                        Ok(_) => {
                            if verbose {
                                logln(writer, format!("Dir {} deleted successfully.", qp).as_str());
                            }
                        }
                        Err(e) => logln(writer, format!("*** Error deleting dir (fs::remove_dir_all) {}: {}", qp, e).as_str()),
                    }
                }
            } else {
                logln(writer, format!("PRD /S {}", quoted_path(path)).as_str());
                if !noaction {
                    match delete(path) {
                        Ok(_) => {
                            if verbose {
                                logln(writer, format!("Dir '{}' deleted successfully.", qp).as_str());
                            }
                        }
                        Err(e) => logln(writer, format!("*** Error deleting dir (trash::delete) {}: {}", qp, e).as_str()),
                    }
                }
            }
        }
    }

    fn name(&self) -> &'static str {
        if self.no_recycle {
            "Delete directories (permanent)"
        } else {
            "Delete directories (use recycle bin for local files, permanently for remote files)"
        }
    }
}
