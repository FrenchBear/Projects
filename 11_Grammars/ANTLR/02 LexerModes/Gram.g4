grammar Gram;

options {
	tokenVocab = Vocab;
	caseInsensitive = true;
}

program: statement+ EOF;

statement:
    number_statement
	| atomic_statement
	| di_statement
    | d_statement
    | lai_statement
//    | doi_lai_statement
	| label_statement
    | inv_statement         // Must be last
    | token_error
    ;

number_statement: NUMBER;

atomic_statement: 
    inv=I22_invert? I38_sin
    | inv=I22_invert? I39_cos
    | inv=I22_invert? I30_tan
    | I25_clear
    | I24_correct_entry
    ;

d_statement:
    I72_store_indirect DD2
    | I73_recall_indirect DD2
    | inv=I22_invert? I74_sum_indirect DD2
    | I82_hir DD2
    ;

di_statement: 
    I42_store INDIRECT1? DD1
    | I43_recall INDIRECT1? DD1
    | inv=I22_invert? I44_sum INDIRECT1? DD1
    ;

lai_statement:
    I61_goto ((INDIRECT3? DD3) | ADD3a | ADD3b | MNEMONIC3)
    | inv=I22_invert? I67_x_equals_t ((INDIRECT3 DD3) | ADD3a | ADD3b | MNEMONIC3 | DD3)
    ;

//doi_lai_statement:

label_statement: I76_label tag = (DD5 | MNEMONIC5);

inv_statement: I22_invert;

token_error: INVALID_TOKEN;