// rcheckfiles tests
//
// 2025-04-08   PV

#[cfg(test)]
pub mod balanced_tests {
    use crate::is_balanced;

    #[test]
    fn balanced1() { assert!(is_balanced("simple texte")); }

    #[test]
    fn balanced2() { assert!(is_balanced("a (b) [c] {d} «e» ‹f›")); }

    #[test]
    fn balanced3() { assert!(is_balanced("({[«‹Hello›»]})")); }

    #[test]
    fn balanced4() { assert!(is_balanced("((a[[b]c]d)e)")); }

    #[test]
    fn balanced5() { assert!(is_balanced("")); }

    #[test]
    fn not_balanced1() { assert!(!is_balanced("((a((b)cd)e)")); }

    #[test]
    fn not_balanced2() { assert!(!is_balanced("a(b[c]}")); }

    #[test]
    fn not_balanced3() { assert!(!is_balanced("pom)me")); }
}
