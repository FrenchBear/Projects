// rgrep core iterator
// Iterates over lines of a text matching some pattern
//
// 2025-03-14   PV

use regex::{Match, Matches, Regex};
use std::ops::Range;

// Returned by grep_iterator
#[derive(Debug)]
pub struct GrepLineMatches {
    pub line: String,
    pub ranges: Vec<Range<usize>>,
}

impl GrepLineMatches {
    /// Build a line iterator over txt, returning lines with at leat a match, grouping all matches of a line together.<br/>
    /// A line is returned with all its matches.
    pub fn new<'a>(txt: &'a str, re: &'a Regex) -> impl Iterator<Item = GrepLineMatches> + 'a {
        GrepIteratorState {
            txt,
            fi: re.find_iter(txt),
            ma: None,
        }
    }
}

/// Private internal iterator object storing current iterator state
struct GrepIteratorState<'a> {
    txt: &'a str,
    fi: Matches<'a, 'a>,   // Find iterator
    ma: Option<Match<'a>>, // Match ahead (next fi already read)
}

impl Iterator for GrepIteratorState<'_> {
    type Item = GrepLineMatches;

    fn next(&mut self) -> Option<Self::Item> {
        let mut prevstartix: usize = usize::MAX;
        let mut currentline = String::new();
        let mut currentmatches = Vec::<Range<usize>>::new();
        loop {
            let ma = if self.ma.is_none() {
                let m = self.fi.next();
                if m.is_none() {
                    if prevstartix == usize::MAX {
                        return None;
                    }
                    return Some(GrepLineMatches {
                        line: currentline,
                        ranges: currentmatches,
                    });
                }
                m.unwrap()
            } else {
                let m = self.ma.unwrap();
                self.ma = None;
                m
            };

            // We have a match, find position of immediately preceding \r or \n or 0 if not found.
            // directly testing bytes is valid because of UTF-8 properties
            let mut matchix = ma.start();
            let mut startlineix: usize = 0;
            while matchix > 0 {
                matchix -= 1;
                let b = self.txt.as_bytes()[matchix];
                if b == 10 || b == 13 {
                    startlineix = matchix + 1;
                    break;
                }
            }

            if prevstartix == usize::MAX {
                prevstartix = startlineix;
                // First match for the line, find end of line
                let mut matchix = ma.end();
                let mut endlineix: usize = self.txt.len();
                while matchix < endlineix {
                    let b = self.txt.as_bytes()[matchix];
                    if b == 10 || b == 13 {
                        endlineix = matchix;
                        break;
                    }
                    matchix += 1;
                }
                currentline = String::from(&self.txt[prevstartix..endlineix]);
                currentmatches.push(ma.start() - prevstartix..ma.end() - prevstartix);
            } else if prevstartix == startlineix {
                currentmatches.push(ma.start() - prevstartix..ma.end() - prevstartix);
            } else {
                self.ma = Some(ma);
                return Some(GrepLineMatches {
                    line: currentline,
                    ranges: currentmatches,
                });
            }
        }
    }
}
