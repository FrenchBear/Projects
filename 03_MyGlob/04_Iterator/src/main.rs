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
    //test_myglob(r"C:\Development\Git*\**\rsgrep.d");
    //test_myglob(r"C:\Development\Git*\*.txt");

    // test_myglob(r"C:\Development\Git*\**\rgrep.d");
    // test_myglob(r"C:\Development\Git*\**\target");

    //test_myglob(r"C:\Development\GitHub\Projects\03_MyGlob\04_Iterator\**");
    test_myglob(r"C:\Temp\zop");

    // let globstr = "file.[!0-9]s";
    // let mut iter = globstr.chars().peekable();
    // while let Some(c) = iter.next() {
    //     match c {
    //         '[' => {
    //             match iter.peek() {
    //                 Some(next_c) => {
    //                     if *next_c=='!' {
    //                         iter.next();
    //                         println!("[^")
    //                     }
    //                 },
    //                 None => println!("{}", c),
    //             }
    //         },
    //         _ =>         println!("{}", c)
    //     }
    // }
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
                let mut nd = 0;
                for ma in gs.explore_iter() {
                    match ma {
                        MyGlobMatch::File(pb) => {
                            println!("{}", pb.display());
                            nf += 1;
                        }
                        MyGlobMatch::Dir(pb) => {
                            println!("{}\\", pb.display());
                            nd += 1;
                        }
                        MyGlobMatch::Error(e) => {
                            println!("{}", e);
                        }
                    }
                }
                println!("{nf} file(s) found");
                println!("{nd} dir(s) found");
                let duration = start.elapsed();
                println!("Iterator search in {:.3}s", duration.as_secs_f64());
            }

            Err(e) => println!("Error building MyGlob: {:?}", e),
        }
    }
}
