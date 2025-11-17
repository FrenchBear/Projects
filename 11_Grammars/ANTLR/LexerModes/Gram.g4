grammar Gram;

options {
  tokenVocab=Vocab;
}

program: statement+ EOF;

statement
    : storage_statement
    | other_statement
    ;

storage_statement: STO_KEYWORD STO_ADDRESS;

// A rule that uses a normal number
other_statement: LBL_KEYWORD NUMBER;
