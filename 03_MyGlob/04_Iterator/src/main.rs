// my_glob
// Attempt to implement an efficient glob in Rust - Main program, for testing
//
// 2025-03-25   PV  First version

//#![allow(unused_variables)]

use std::time::Instant;
use myglob::{MyGlobSearch, MyGlobMatch};

fn main() {
    // Simple existing file
    //test_myglob(r"C:\temp\f1.txt");

    // Should find 4 files
    //test_myglob(r"C:\Temp\testroot - Copy\**\Espace incorrect\*.txt");

    // Should find C:\Development\GitHub\Projects\10_RsGrep\target\release\rsgrep.d
    //test_myglob(r"C:\Development\**\projects\**\target\release\rsgrep.d");

    // SHould find 4 files
    //test_myglob(r"C:\Development\**\rsgrep.d");
    test_myglob(r"C:\Development\Git*\**\rsgrep.d");
    //test_myglob(r"C:\Development\Git*\*.txt");
}

// Entry point for testing
pub fn test_myglob(pattern: &str) {
    for pass in 0..3 {
        println!("\nTest #{pass}");

        let start = Instant::now();
        let resgs = MyGlobSearch::build(pattern);

        match resgs {
            Ok(gs) => {
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

            Err(e) => println!("Error building MyGlob: {:?}", e),
        }
    }
}
