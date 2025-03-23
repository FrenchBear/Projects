// recheckfiles: Detect and fix incorrect filenames
//
// 2025-03-23	PV      First version

//#![allow(unused_imports, unused_variables, dead_code)]

// standard library imports
use std::collections::HashSet;
use std::error::Error;
use std::fs::{self, File, Metadata};
use std::io::{BufWriter, Write};
use std::os::windows::prelude::*;
use std::path::Path;
use std::process;
use std::time::Instant;

// external crates imports
use chrono::{DateTime, Local};
use getopt::Opt;
use termcolor::{Color, ColorChoice, ColorSpec, StandardStream, WriteColor};
use unicode_normalization::{is_nfc, UnicodeNormalization};

// -----------------------------------
// Submodules

pub mod tests;

// -----------------------------------
// Globals

const APP_NAME: &str = "rscheckfiles";
const APP_VERSION: &str = "1.0.0";

const SPECIAL_CHARS: &str = "€®™©–—…×·•∶⧹⧸／⚹†‽¿🎜🎝“”⚡♥";

// Confusables for space
const CONF_SPC: [char; 13] = [
    '\u{2000}', // U+2000	EN QUAD
    '\u{2001}', // U+2001	EM QUAD
    '\u{2002}', // U+2002	EN SPACE
    '\u{2003}', // U+2003	EM SPACE
    '\u{2004}', // U+2004	THREE-PER-EM SPACE
    '\u{2005}', // U+2005	FOUR-PER-EM SPACE
    '\u{2006}', // U+2006	SIX-PER-EM SPACE
    '\u{2007}', // U+2007	FIGURE SPACE
    '\u{2008}', // U+2008	PUNCTUATION SPACE
    '\u{2009}', // U+2009	THIN SPACE
    '\u{200A}', // U+200A	HAIR SPACE
    '\u{202F}', // U+202F	NARROW NO-BREAK SPACE
    '\u{205F}', // U+205F	MEDIUM MATHEMATICAL SPACE
];

// Confusables for apostrophe
const CONF_APO: [char; 33] = [
    '\u{00B4}', // ´ U+00B4	ACUTE ACCENT
    '\u{02B9}', // ʹ U+02B9	MODIFIER LETTER PRIME
    '\u{02BB}', // ʻ U+02BB	MODIFIER LETTER TURNED COMMA
    '\u{02BC}', // ʼ U+02BC	MODIFIER LETTER APOSTROPHE
    '\u{02BD}', // ʽ U+02BD	MODIFIER LETTER REVERSED COMMA
    '\u{02BE}', // ʾ U+02BE	MODIFIER LETTER RIGHT HALF RING
    '\u{02C8}', // ˈ U+02C8	MODIFIER LETTER VERTICAL LINE
    '\u{02CA}', // ˊ U+02CA	MODIFIER LETTER ACUTE ACCENT
    '\u{02CB}', // ˋ U+02CB	MODIFIER LETTER GRAVE ACCENT
    '\u{02F4}', // ˴ U+02F4	MODIFIER LETTER MIDDLE GRAVE ACCENT
    '\u{0374}', // ʹ U+0374	GREEK NUMERAL SIGN
    '\u{0384}', // ΄ U+0384	GREEK TONOS
    '\u{055A}', // ՚ U+055A	ARMENIAN APOSTROPHE
    '\u{055D}', // ՝ U+055D	ARMENIAN COMMA
    '\u{05D9}', // י U+05D9	HEBREW LETTER YOD
    '\u{05F3}', // ׳ U+05F3	HEBREW PUNCTUATION GERESH
    '\u{07F4}', // ߴ U+07F4	NKO HIGH TONE APOSTROPHE
    '\u{07F5}', // ߵ U+07F5	NKO LOW TONE APOSTROPHE
    '\u{144A}', // ᑊ U+144A	CANADIAN SYLLABICS WEST-CREE P
    '\u{16CC}', // ᛌ U+16CC	RUNIC LETTER SHORT-TWIG-SOL S
    '\u{1FBD}', // ᾽ U+1FBD	GREEK KORONIS
    '\u{1FBF}', // ᾿ U+1FBF	GREEK PSILI
    '\u{1FEF}', // ` U+1FEF	GREEK VARIA
    '\u{1FFD}', // ´ U+1FFD	GREEK OXIA
    '\u{1FFE}', // ῾ U+1FFE	GREEK DASIA
    '\u{2018}', // ‘ U+2018	LEFT SINGLE QUOTATION MARK
    '\u{2019}', // ’ U+2019	RIGHT SINGLE QUOTATION MARK
    '\u{201B}', // ‛ U+201B	SINGLE HIGH-REVERSED-9 QUOTATION MARK
    '\u{2032}', // ′ U+2032	PRIME
    '\u{2035}', // ‵ U+2035	REVERSED PRIME
    '\u{A78C}', // ꞌ U+A78C	LATIN SMALL LETTER SALTILLO
    '\u{FF07}', // ＇ U+FF07	FULLWIDTH APOSTROPHE
    '\u{FF40}', // ｀ U+FF40	FULLWIDTH GRAVE ACCENT
];

// ==============================================================================================
// Options processing

// Dedicated struct to store command line arguments
#[derive(Debug, Default)]
pub struct Options {
    sources: Vec<String>,
    fixit: bool,
}

impl Options {
    fn header() {
        eprintln!(
            "{APP_NAME} {APP_VERSION}\n\
            Detect and fix incorrect filenames"
        );
    }

    fn usage() {
        Options::header();
        eprintln!(
            "\nUsage: {APP_NAME} [?|-?|-h] [-f] source...\n\
            ?|-?|-h  Show this message\n\
            -f       Automatic problems fixing\n\
            source   File or folder to analyze"
        );
    }

    /// Build a new struct Options analyzing command line parameters.<br/>
    /// Some invalid/inconsistent options or missing arguments return an error.
    fn new() -> Result<Options, Box<dyn Error>> {
        let mut args: Vec<String> = std::env::args().collect();
        if args.len() > 1 && args[1].to_lowercase() == "help" {
            Self::usage();
            return Err("".into());
        }

        let mut options = Options { ..Default::default() };
        let mut opts = getopt::Parser::new(&args, "h?f");

        loop {
            match opts.next().transpose()? {
                None => break,
                Some(opt) => match opt {
                    Opt('h', None) | Opt('?', None) => {
                        Self::usage();
                        return Err("".into());
                    }

                    Opt('f', None) => {
                        options.fixit = true;
                    }

                    _ => unreachable!(),
                },
            }
        }

        // Check for extra argument
        for arg in args.split_off(opts.index()) {
            if arg == "?" || arg == "help" {
                Self::usage();
                return Err("".into());
            }

            if arg.starts_with("-") {
                return Err(format!("Invalid/unsupported option {}", arg).into());
            }

            options.sources.push(arg);
        }

        Ok(options)
    }
}

// -----------------------------------
// Main

#[derive(Default)]
struct Statistics {
    total: i32, // Total files/folders processed
    nnn: i32,   // Non-normalized names
    apo: i32,   // Incorrect apostrophe
    spc: i32,   // Incorrect space
    car: i32,   // Maybe incorrect char
    sp2: i32,   // Double space
    fix: i32,   // Number of path fixed
    err: i32,   // Number of errors
}

struct Confusables {
    space: HashSet<char>,
    apostrophe: HashSet<char>,
}

fn logln(writer: &mut BufWriter<File>, msg: &str) {
    if msg.starts_with("***") {
        let mut stdout = StandardStream::stdout(ColorChoice::Always);
        let mut err_color = ColorSpec::new();
        err_color.set_fg(Some(Color::Red)).set_bold(true);

        let _ = stdout.set_color(&err_color);
        let _ = writeln!(&mut stdout, "{}", msg);
        let _ = stdout.reset();
    } else {
        println!("{}", msg);
    }
    let _ = writeln!(writer, "{}", msg);
}

fn log(writer: &mut BufWriter<File>, msg: &str) {
    print!("{}", msg);
    let _ = write!(writer, "{}", msg);
}

fn main() {
    // Process options
    let options = Options::new().unwrap_or_else(|err| {
        let msg = format!("{}", err);
        if msg.is_empty() {
            process::exit(0);
        }
        eprintln!("{APP_NAME}: Problem parsing arguments: {}", err);
        process::exit(1);
    });

    // // For dev testing
    // let options = Options {
    //     //sources: vec![String::from(r"c:\temp\Testroot - copy")],
    //     sources: vec![String::from(r"\\terazalt\books\Livres")],
    //     fixit: true,
    // };

    let confusables = Confusables {
        space: HashSet::from_iter(CONF_SPC),
        apostrophe: HashSet::from_iter(CONF_APO),
    };

    let now: DateTime<Local> = Local::now();
    let formatted_now = now.format("%Y-%m-%d-%H.%M.%S");
    let logpath = format!("c:\\temp\\{APP_NAME}-{formatted_now}.txt");
    let file = File::create(logpath.clone());
    if file.is_err() {
        eprintln!("{APP_NAME}: Error when crating log file {logpath}: {:?}", file.err());
        process::exit(1);
    }
    let mut writer = BufWriter::new(file.unwrap());
    logln(&mut writer, &format!("{APP_NAME} {APP_VERSION}"));

    let mut files_stats = Statistics { ..Default::default() };
    let mut folders_stats = Statistics { ..Default::default() };
    let start = Instant::now();

    for source in options.sources {
        logln(&mut writer, &format!("Analyzing {}", source));
        let p = Path::new(&source);
        let m = p.metadata();
        match m {
            Err(e) => logln(&mut writer, &format!("*** Error accessing path {source}: {e}")),
            Ok(m) => {
                if m.is_dir() {
                    process_folder(p, &m, &mut folders_stats, &mut files_stats, options.fixit, &mut writer, &confusables);
                } else if m.is_file() {
                    process_file(p, &mut files_stats, options.fixit, &mut writer, &confusables);
                } else {
                    logln(&mut writer, "*** Symlinks not supported: {source}");
                    continue;
                };
            }
        };

        let duration = start.elapsed();

        fn s(n: i32) -> &'static str {
            if n > 1 {
                "s"
            } else {
                ""
            }
        }

        fn final_status(pwriter: &mut BufWriter<File>, stats: &Statistics, typename: &str) {
            log(pwriter, &format!("{} {}{} checked", stats.total, typename, s(stats.total)));
            if stats.nnn > 0 {
                log(pwriter, &format!(", {} non-normalized", stats.nnn));
            }
            if stats.apo > 0 {
                log(pwriter, &format!(", {} wrong apostrophe", stats.apo));
            }
            if stats.spc > 0 {
                log(pwriter, &format!(", {} wrong space", stats.spc));
            }
            if stats.sp2 > 0 {
                log(pwriter, &format!(", {} multiple space", stats.sp2));
            }
            if stats.car > 0 {
                log(pwriter, &format!(", {} wrong character", stats.car));
            }
            if stats.fix > 0 {
                log(pwriter, &format!(", {} problem{} fixed", stats.fix, s(stats.fix)));
            }
            if stats.err > 0 {
                log(pwriter, &format!(", {} error{}", stats.err, s(stats.err)));
            }
            logln(pwriter, "");
        }

        logln(&mut writer, "");
        final_status(&mut writer, &folders_stats, "folder");
        final_status(&mut writer, &files_stats, "file");
        logln(&mut writer, &format!("Total duration: {:.3}s", duration.as_secs_f64()));

        let _ = writer.flush();
    }

    fn process_folder(
        pa: &Path,
        m: &Metadata,
        folders_stats: &mut Statistics,
        files_stats: &mut Statistics,
        fixit: bool,
        pwriter: &mut BufWriter<File>,
        pconfusables: &Confusables,
    ) {
        let mut pb = pa.to_path_buf();

        // Silently ignore hidden or system folders
        let attributes = m.file_attributes();
        if (attributes & 0x2/* Hidden */) > 0 || (attributes & 0x4/* System */) > 0 {
            return;
        }

        // First check folder basename
        folders_stats.total += 1;
        if let Some(new_name) = check_basename(&pb, "folder", folders_stats, pwriter, pconfusables) {
            if fixit {
                logln(pwriter, &format!("  --> rename folder \"{new_name}\""));
                let newpath = pb.parent().unwrap().join(Path::new(&new_name));
                //logln(pwriter, &format!("  --> \"{}\"", newpath.display()));
                match fs::rename(&pb, &newpath) {
                    Ok(_) => {
                        folders_stats.fix += 1;
                        pb = newpath;
                    }
                    Err(e) => logln(pwriter, &format!("*** Error {e}")), // Rename failed, but we continue anyway, don't really know if it's Ok or not...
                }
            }
        }

        // Then process folder content
        let contents = fs::read_dir(&pb);
        if contents.is_err() {
            logln(pwriter, &format!("*** Error enumerating folder {}: {:?}", pb.display(), contents.err()));
            return;
        }
        for entry in contents.unwrap() {
            if entry.is_err() {
                logln(pwriter, &format!("*** Error accessing entry: {:?}", entry.err()));
                continue;
            }
            let entry = entry.unwrap();
            let path = entry.path();
            let meta = path.metadata();
            if meta.is_err() {
                logln(
                    pwriter,
                    &format!("*** Error accessing metadata of entry {}: {:?}", path.display(), meta.err()),
                );
                continue;
            }
            if path.is_file() {
                process_file(&path, files_stats, fixit, pwriter, pconfusables);
            } else if path.is_dir() {
                let meta = meta.unwrap();
                process_folder(&path, &meta, folders_stats, files_stats, fixit, pwriter, pconfusables);
            }
        }
    }

    fn process_file(p: &Path, files_stats: &mut Statistics, fixit: bool, pwriter: &mut BufWriter<File>, pconfusables: &Confusables) {
        files_stats.total += 1;
        if let Some(new_name) = check_basename(p, "file", files_stats, pwriter, pconfusables) {
            if fixit {
                logln(pwriter, &format!("  --> rename file \"{new_name}\""));
                let newpath = p.parent().unwrap().join(Path::new(&new_name));
                //logln(pwriter, &format!("  --> \"{}\"", newpath.display()));
                match fs::rename(p, &newpath) {
                    Ok(_) => files_stats.fix += 1,
                    Err(e) => logln(pwriter, &format!("*** Error {e}")), // Rename failed
                }
            }
        }
    }

    fn check_basename(p: &Path, pt: &str, stats: &mut Statistics, pwriter: &mut BufWriter<File>, pconfusables: &Confusables) -> Option<String> {
        let fp = p.display();
        let file = p.file_name();
        if file.is_none() {
            stats.err += 1;
            logln(pwriter, &format!("*** Invalid (1) {pt} name {fp}, ignored"));
            return None;
        }
        let file = file.unwrap().to_str();
        if file.is_none() {
            stats.err += 1;
            logln(pwriter, &format!("*** Invalid (2) {pt} name {fp}, ignored"));
            return None;
        }

        let mut file = file.unwrap().to_string();
        let original_file = file.clone();

        // Check normalization
        if !is_nfc(&file) {
            logln(pwriter, &format!("Non-normalized {pt} name {fp}"));
            stats.nnn += 1;
            // Normalize it for the rest to avoind complaining on combining accents as invalid characters
            file = file.nfc().collect();
        }

        let mut vc: Vec<char> = file.chars().collect();

        // Check apostrophes
        let mut pbapo = false;
        for c in &mut vc {
            //if CONF_APO.contains(c) {
            if pconfusables.apostrophe.contains(c) {
                logln(pwriter, &format!("Invalid apostrophe in {pt} name {fp} -> {c} {:04X}", *c as i32));
                if !pbapo {
                    pbapo = true;
                    stats.apo += 1;
                }
                *c = '\'';
            }
        }

        // Check spaces
        let mut pbspc = false;
        for c in &mut vc {
            //if CONF_SPC.contains(c) {
            if pconfusables.space.contains(c) {
                logln(pwriter, &format!("Invalid space in {pt} name {fp} -> {:04X}", *c as i32));
                if !pbspc {
                    pbspc = true;
                    stats.spc += 1;
                }
                *c = ' ';
            }
        }

        if pbapo || pbspc {
            file = vc.into_iter().collect();
        }

        // Check multiple spaces (and space before extension)
        let mut pbsp2 = false;
        let mut vc: Vec<char> = Vec::new();
        let mut lastc = '_';
        for c in file.chars() {
            if c == ' ' {
                if lastc == ' ' {
                    if !pbsp2 {
                        logln(pwriter, &format!("Multiple spaces in {pt} name {fp}"));
                        pbsp2 = true;
                        stats.sp2 += 1;
                    }
                } else {
                    vc.push(c);
                }
            } else if c == '.' {
                if lastc == ' ' {
                    vc.pop();
                }
                vc.push(c);
            } else {
                vc.push(c);
            }
            lastc = c;
        }
        if pbsp2 {
            file = vc.iter().collect();
        }

        // Check characters
        let mut pbchr = false;
        for c in file.chars() {
            if !(c.is_alphanumeric() || (32..127).contains(&(c as i32)) || (160..256).contains(&(c as i32)) || SPECIAL_CHARS.contains(c)) {
                if !pbchr {
                    pbchr = true;
                    stats.car += 1;
                }
                logln(pwriter, &format!("Invalid char in {pt} name {fp} -> {c} {:04X}", c as i32));
            }
        }

        if file == original_file {
            None
        } else {
            Some(file)
        }
    }
}
