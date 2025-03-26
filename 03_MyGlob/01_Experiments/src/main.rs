// my_glob
// Attempt to implement an efficient glob in Rust - Main program, for testing
//
// 2025-03-25   PV  First version

#![allow(unused_variables)]

fn main() {
    // Simple existing file
    //my_glob::my_glob_main(r"C:\temp\f1.txt");

    // Should find 4 files
    //my_glob::my_glob_main(r"C:\Temp\testroot - Copy\**\Espace incorrect\*.txt");

    // Should find C:\Development\GitHub\Projects\10_RsGrep\target\release\rsgrep.d
    //my_glob::my_glob_main(r"C:\Development\**\projects\**\target\release\rsgrep.d");
    //my_glob::my_glob_main(r"C:\Development\**\rsgrep.d");
    my_glob::my_glob_main(r"C:\Development\Git*\**\rsgrep.d");
    //my_glob::my_glob_main(r"C:\Development\Git*\*.txt");
}
