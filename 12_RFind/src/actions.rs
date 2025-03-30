// actions.rs, definition of actions
//
// 2025-03-29	PV      First version

use super::*;
use std::fs;
use trash::delete;

// ===============================================================
// Print action

#[derive(Debug)]
pub struct ActionPrint {}

impl ActionPrint {
    pub fn new() -> Self {
        ActionPrint {}
    }
}

impl Action for ActionPrint {
    fn action(&self, lw: &mut LogWriter, path: &Path, _do_it: bool, _verbose: bool) {
        //println!("Action Print\n  Path: {}\n  do_it: {do_it}\n  verbose: {verbose}", path.display());
        if path.is_file() {
            logln(lw, path.display().to_string().as_str());
        } else {
            logln(lw,(path.display().to_string()+"\\").as_str());
        }
    }

    fn name(&self) -> &'static str {
        "Print"
    }
}

// ===============================================================
// Delete action (remove files)

#[derive(Debug)]
pub struct ActionDelete {
    norecycle: bool,
}

impl ActionDelete {
    pub fn new(norecycle: bool) -> Self {
        ActionDelete { norecycle }
    }
}

impl Action for ActionDelete {
    fn action(&self, lw: &mut LogWriter, path: &Path, noaction: bool, verbose: bool) {
        if path.is_file() {
            let s = quoted_path(path);
            let qp = s.as_str();
            if self.norecycle {
                logln(lw, format!("DEL {}", qp).as_str());
                if !noaction {
                    match fs::remove_file(path) {
                        Ok(_) => if verbose { logln(lw, format!("File {} deleted successfully.", qp).as_str()); },
                        Err(e) => logln(lw, format!("*** Error deleting file (fs::remove_file) {}: {}", qp, e).as_str()),
                    }
                }
             } else {
                logln(lw, format!("PDEL {}", qp).as_str());
                if !noaction {
                    match delete(path) {
                        Ok(_) => if verbose { logln(lw, format!("File {} deleted successfully.", qp).as_str()); },
                        Err(e) => logln(lw, format!("*** Error deleting file (trash::delete) '{}': {}", qp, e).as_str()),
                    }
                }
            }
        }
    }

    fn name(&self) -> &'static str {
        if self.norecycle {
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
    norecycle: bool,
}

impl ActionRmdir {
    pub fn new(norecycle: bool) -> Self {
        ActionRmdir { norecycle }
    }
}

impl Action for ActionRmdir {
    fn action(&self, writer: &mut LogWriter, path: &Path, noaction: bool, verbose: bool) {
        if path.is_dir() {
            let s = quoted_path(path);
            let qp = s.as_str();
            if self.norecycle {
                logln(writer, format!("RD /S {}", qp).as_str());
                if !noaction {
                    match fs::remove_dir(path) {
                        Ok(_) => if verbose { logln(writer, format!("Dir '{}' deleted successfully.", qp).as_str()); },
                        Err(e) => logln(writer, format!("*** Error deleting dir (fs::remove_dir) '{}': {}", qp, e).as_str()),
                    }
                } 
            }else {
                logln(writer, format!("PRD /S {}", quoted_path(path)).as_str());
                if !noaction {
                    match delete(path) {
                        Ok(_) => if verbose { logln(writer, format!("Dir '{}' deleted successfully.", qp).as_str()); },
                        Err(e) => logln(writer, format!("*** Error deleting dir (trash::delete) '{}': {}", qp, e).as_str()),
                    }
                }
            }
        }
    }


    fn name(&self) -> &'static str {
        if self.norecycle {
            "Delete directories (permanent)"
        } else {
            "Delete directories (use recycle bin for local files, permanently for remote files)"
        }
    }
}

