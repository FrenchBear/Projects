// actions.rs, definition of actions
//
// 2025-03-29	PV      First version

use super::*;

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
    fn action(&self, path: &Path, do_it: bool, verbose: bool) {
        //println!("Action Print\n  Path: {}\n  do_it: {do_it}\n  verbose: {verbose}", path.display());
        if path.is_file() {
            println!("{}", path.display());
        } else {
            println!("{}\\", path.display());
        }
    }

    fn name(&self) -> &'static str {
        "Print"
    }
}

// ===============================================================
// Delete action

#[derive(Debug)]
pub struct ActionDelete {
    use_recycle_bin: bool,
}

impl ActionDelete {
    pub fn new(use_recycle_bin: bool) -> Self {
        ActionDelete { use_recycle_bin }
    }
}

impl Action for ActionDelete {
    fn action(&self, path: &Path, do_it: bool, verbose: bool) {
        println!(
            "Action Delete\n  Path: {}\n  do_it: {do_it}\n  verbose: {verbose}\n  use_recycle_bin: {}",
            path.display(),
            self.use_recycle_bin
        );
    }

    fn name(&self) -> &'static str {
        "Delete"
    }
}

