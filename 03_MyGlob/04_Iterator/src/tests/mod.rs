// myglob tests
// Mostly tests of conversion glob->regexp and matching strings
//
// 20245-03-29  PV

#![cfg(test)]

use super::*;
use regex::Regex;

#[derive(Debug, PartialEq, Clone)]
enum ConvResult {
    CRError,
    Constant,
    Recurse,
    Filter,
}

fn glob_one_segment_test(glob_pattern: &str, cr: ConvResult, test_string: &str, is_match: bool) {
    let res = MyGlobSearch::glob_to_segments((glob_pattern.to_string() + "\\").as_str());
    match res {
        Err(e) => {
            assert!(
                cr == ConvResult::CRError,
                "Conversion of «{}» produced an error {:?} instead of {:?}",
                glob_pattern,
                e,
                cr
            );
            return;
        }

        Ok(seg_vec) => {
            if glob_pattern!="**" {
                assert_eq!(seg_vec.len(), 1);
            }
            match &seg_vec[0] {
                Segment::Constant(k) => {
                    assert_eq!(cr, ConvResult::Constant, "Conversion of «{}» produced a Constant instead of a {:?}", glob_pattern, cr);
                    assert_eq!(is_match, k.to_lowercase() == test_string.to_lowercase(), "Match «{}» with «{}» did not produce the expected result", glob_pattern, test_string);
                }
                Segment::Recurse => {
                    assert_eq!(cr, ConvResult::Recurse, "Conversion of «{}» produced a Recurse instead of a {:?}", glob_pattern, cr);
                    assert_eq!(is_match, true);     // With Recurse, any string should match
                }
                Segment::Filter(re) => {
                    println!("{}", re);
                    assert_eq!(cr, ConvResult::Filter, "Conversion of «{}» produced a Filter instead of a {:?}", glob_pattern, cr);
                    assert_eq!(re.is_match(test_string), is_match, "Match «{}» with «{}» did not produce the expected result", glob_pattern, test_string);
                }
            }
        }
    }
}

#[test]
fn conversions_tests() {
    // Simple constant string, case insensitive
    glob_one_segment_test("Pomme", ConvResult::Constant, "pomme", true);
    glob_one_segment_test("Pomme", ConvResult::Constant, "pommerol", false);

    // * pattern, matches everything (except \)
    glob_one_segment_test("*", ConvResult::Filter, "rsgresp.d", true);
    glob_one_segment_test("*", ConvResult::Filter, "rsgresp.d.e.f", true);
    glob_one_segment_test("*.d", ConvResult::Filter, "rsgresp.d", true);
    glob_one_segment_test("*.*", ConvResult::Filter, "rsgresp.d", true);
    glob_one_segment_test("*.*", ConvResult::Filter, "rsgresp", false);
    glob_one_segment_test("*.*", ConvResult::Filter, "rsgresp", false);

    // ** pattern must be alone, and matches anything, including \
    glob_one_segment_test("**.d", ConvResult::CRError, "", false);
    glob_one_segment_test("**", ConvResult::Recurse, "", true);

    // Alternations
    glob_one_segment_test("a{b,c}d", ConvResult::Filter, "abd", true);
    glob_one_segment_test("a{b,c}d", ConvResult::Filter, "ad", false);
    glob_one_segment_test("a{{b,c},{d,e}}f", ConvResult::Filter, "acf", true);
    glob_one_segment_test("a{{b,c},{d,e}}f", ConvResult::Filter, "adf", true);
    glob_one_segment_test("a{{b,c},{d,e}}f", ConvResult::Filter, "acdf", false);
    glob_one_segment_test("a{b,c}{d,e}f", ConvResult::Filter, "acdf", true);
    glob_one_segment_test("file.{cs,py,rs,vb}", ConvResult::Filter, "file.bat", false);
    glob_one_segment_test("file.{cs,py,rs,vb}", ConvResult::Filter, "file.rs", true);
    
    // ? replace exactly one character
    glob_one_segment_test("file.?s", ConvResult::Filter, "file.rs", true);
    glob_one_segment_test("file.?s", ConvResult::Filter, "file.cds", false);
    glob_one_segment_test("file.?s", ConvResult::Filter, "file.py", false);

    // * replace 0 or more characters
    glob_one_segment_test("file.*s", ConvResult::Filter, "file.s", true);
    glob_one_segment_test("file.*s", ConvResult::Filter, "file.rs", true);
    glob_one_segment_test("file.*s", ConvResult::Filter, "file.chamallows", true);
    glob_one_segment_test("file.*s", ConvResult::Filter, "file.py", false);

    // [abc] matches any characters of the set
    glob_one_segment_test("file.[cr]s", ConvResult::Filter, "file.rs", true);
    glob_one_segment_test("file.[cr]s", ConvResult::Filter, "file.cs", true);
    glob_one_segment_test("file.[cr]s", ConvResult::Filter, "file.py", false);

    // [a-z] matches any character of the range
    glob_one_segment_test("file.[a-r]s", ConvResult::Filter, "file.rs", true);
    glob_one_segment_test("file.[a-r]s", ConvResult::Filter, "file.cs", true);
    glob_one_segment_test("file.[a-r]s", ConvResult::Filter, "file.zs", false);

    // a - at the beginning or end of a class actually matches a minus
    glob_one_segment_test("file.[-+]s", ConvResult::Filter, "file.-s", true);
    glob_one_segment_test("file.[+-]s", ConvResult::Filter, "file.-s", true);
    glob_one_segment_test("file.[-+]s", ConvResult::Filter, "file.+s", true);
    glob_one_segment_test("file.[-]s", ConvResult::Filter, "file.-s", true);

    // A ! (or a ^) at the bebinning of a class inverts filtering
    glob_one_segment_test("file.[!abc]s", ConvResult::Filter, "file.rs", true);
    glob_one_segment_test("file.[!abc]s", ConvResult::Filter, "file.cs", false);
    glob_one_segment_test("file.[!0-9]s", ConvResult::Filter, "file.3s", false);
    glob_one_segment_test("file.[!0-9]s", ConvResult::Filter, "file.cs", true);

    // A ] at the beginning of a class matches a ]
    glob_one_segment_test("file.[]]s", ConvResult::Filter, "file.]s", true);
    glob_one_segment_test("file.[]]s", ConvResult::Filter, "file.[s", false);
    glob_one_segment_test("file.[!]]s", ConvResult::Filter, "file.]s", false);
    glob_one_segment_test("file.[!]]s", ConvResult::Filter, "file.[s", true);

    // Some characters classes are supported, see regex character classes
    //   \d         digit (\p{Nd})
    //   \D         not digit
    //   \pX        Unicode character class identified by a one-letter name
    //   \p{Greek}  Unicode character class (general category or script)
    //   \PX        Negated Unicode character class identified by a one-letter name
    //   \P{Greek}  negated Unicode character class (general category or script)    
    //   [[:alpha:]]   ASCII character class ([A-Za-z])
    //   [[:^alpha:]]  Negated ASCII character class ([^A-Za-z])
    glob_one_segment_test(r"file[\d].cs", ConvResult::Filter, "file1.cs", true);
    glob_one_segment_test(r"file[\D].cs", ConvResult::Filter, "file2.cs", false);
    glob_one_segment_test(r"file[\D].cs", ConvResult::Filter, "filed.cs", true);
    glob_one_segment_test(r"file[\p{Greek}].cs", ConvResult::Filter, "fileζ.cs", true);

    // Actually, stuff between [ ] is directy passed into regex (only a ! at the beginning is replaced by ^), so it accepts other regex classes constructs
    //   [x[^xyz]]     Nested/grouping character class (matching any character except y and z)
    //   [a-y&&xyz]    Intersection (matching x or y)
    //   [0-9&&[^4]]   Subtraction using intersection and negation (matching 0-9 except 4)
    //   [0-9--4]      Direct subtraction (matching 0-9 except 4)
    //   [a-g~~b-h]    Symmetric difference (matching `a` and `h` only)
    //   [\[\]]        Escaping in character classes (matching [ or ])
    //   [a&&b]        An empty character class matching nothing

    // Any named character class may appear inside a bracketed [...] character class. For example, [\p{Greek}[:digit:]]
    // matches any ASCII digit or any codepoint in the Greek script. [\p{Greek}&&\pL] matches Greek letters.

    // Precedence in character classes, from most binding to least:
    //   Ranges: [a-cd] == [[a-c]d]
    //   Union: [ab&&bc] == [[ab]&&[bc]]
    //   Intersection, difference, symmetric difference. All three have equivalent precedence, and are evaluated in left-to-right order. For example, [\pL--\p{Greek}&&\p{Uppercase}] == [[\pL--\p{Greek}]&&\p{Uppercase}].
    //   Negation: [^a-z&&b] == [^[a-z&&b]].

    // Escape sequences
    //   \*              literal *, applies to all ASCII except [0-9A-Za-z<>]
    //   \a              bell (\x07)
    //   \f              form feed (\x0C)
    //   \t              horizontal tab
    //   \n              new line
    //   \r              carriage return
    //   \v              vertical tab (\x0B)
    //   \A              matches at the beginning of a haystack
    //   \z              matches at the end of a haystack
    //   \b              word boundary assertion
    //   \B              negated word boundary assertion
    //   \b{start}, \<   start-of-word boundary assertion
    //   \b{end}, \>     end-of-word boundary assertion
    //   \b{start-half}  half of a start-of-word boundary assertion
    //   \b{end-half}    half of a end-of-word boundary assertion
    //   \123            octal character code, up to three digits (when enabled)
    //   \x7F            hex character code (exactly two digits)
    //   \x{10FFFF}      any hex character code corresponding to a Unicode code point
    //   \u007F          hex character code (exactly four digits)
    //   \u{7F}          any hex character code corresponding to a Unicode code point
    //   \U0000007F      hex character code (exactly eight digits)
    //   \U{7F}          any hex character code corresponding to a Unicode code point
    //   \p{Letter}      Unicode character class
    //   \P{Letter}      negated Unicode character class
    //   \d, \s, \w      Perl character class
    //   \D, \S, \W      negated Perl character class
    
    // Perl character classes (Unicode friendly)
    // These classes are based on the definitions provided in UTS#18:
    //   \d     digit (\p{Nd})
    //   \D     not digit
    //   \s     whitespace (\p{White_Space})
    //   \S     not whitespace
    //   \w     word character (\p{Alphabetic} + \p{M} + \d + \p{Pc} + \p{Join_Control})
    //   \W     not word character
    
    // ASCII character classes
    // These classes are based on the definitions provided in UTS#18:
    //   [[:alnum:]]    alphanumeric ([0-9A-Za-z])
    //   [[:alpha:]]    alphabetic ([A-Za-z])
    //   [[:ascii:]]    ASCII ([\x00-\x7F])
    //   [[:blank:]]    blank ([\t ])
    //   [[:cntrl:]]    control ([\x00-\x1F\x7F])
    //   [[:digit:]]    digits ([0-9])
    //   [[:graph:]]    graphical ([!-~])
    //   [[:lower:]]    lower case ([a-z])
    //   [[:print:]]    printable ([ -~])
    //   [[:punct:]]    punctuation ([!-/:-@\[-`{-~])
    //   [[:space:]]    whitespace ([\t\n\v\f\r ])
    //   [[:upper:]]    upper case ([A-Z])
    //   [[:word:]]     word characters ([0-9A-Za-z_])
    //   [[:xdigit:]]   hex digit ([0-9A-Fa-f])
        

}

#[test]
fn glob_ending_with_recurse() {
    let res = MyGlobSearch::glob_to_segments("**\\").unwrap();
    assert_eq!(res.len(), 2);
    match &res[0] {
        Segment::Recurse => {},
        _ => panic!(),
    }
    match &res[1] {
        Segment::Filter(re) => assert_eq!(re.as_str(), "(?i)^.*$"),
        _ => panic!(),
    }
}
