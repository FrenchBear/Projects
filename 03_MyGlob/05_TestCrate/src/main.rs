// my_glob
// Attempt to implement an efficient glob in Rust - Main program, for testing
//
// 2025-03-27   PV      Test library in a crate

//#![allow(unused_variables)]

use myglob::{MyGlobMatch, MyGlobSearch};
use std::time::Instant;

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
    let mut durations: Vec<i32> = Vec::new();

    for pass in 1..=3 {
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
                let duration = start.elapsed();
                durations.push((duration.as_secs_f64() * 1000.0+0.5) as i32);
                println!("{nf} file(s) found");
                println!("Iterator search in {:.3}s", duration.as_secs_f64());
            }

            Err(e) => println!("Error building MyGlob: {:?}", e),
        }
    }

    durations.sort();
    let med = if durations.len() & 1 != 0 {
        durations[durations.len() >> 1]
    } else {
        (durations[durations.len() >> 1] + durations[(durations.len() >> 1) - 1]) >> 1
    } as f64
        / 1000.0;
    println!("\nMedian duration: {:.3}s", med);
}
