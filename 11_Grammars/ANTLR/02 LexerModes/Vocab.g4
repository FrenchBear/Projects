lexer grammar Vocab;

options {
    caseInsensitive = true;
}
 
I22_invert: 'INV';
I24_correct_entry: 'CE';
I25_clear: 'CLR';
I38_sin: 'sin';
I39_cos: 'cos';
I30_tan: 'tan';
I40_indirect: 'Ind';
I42_store: 'STO' -> pushMode(di_mode);
I43_recall: 'RCL' -> pushMode(di_mode);
I44_sum: 'SUM' -> pushMode(di_mode);

I61_goto: 'GTO' -> pushMode(lai_mode);
I62_program_indirect: 'PG*' -> pushMode(d_mode);
I63_exchange_indirect: 'EX*' -> pushMode(d_mode);
I64_product_indirect: 'PD*' -> pushMode(d_mode);
I65_multiply: '*' | '×';
I66_pause: 'Pause' | 'PAU';
I67_x_equals_t: ('x=t' | 'EQ') -> pushMode(lai_mode);
I67_x_equals_t_indextra: 'EQ*' -> pushMode(d_mode);         // Extension, indextra means that Ind must be generated
I68_nop: 'Nop';
I72_store_indirect: 'ST*' -> pushMode(d_mode);
I73_recall_indirect: 'RC*' -> pushMode(d_mode);
I74_sum_indirect: 'SM*' -> pushMode(d_mode);
I76_label: 'Lbl' -> pushMode(label_mode);
I77_x_greater_or_equal_than_t: ('x≥t' | 'x>=t' | 'GE') -> pushMode(lai_mode);
I77_x_greater_or_equal_than_t_indextra: 'GE*' -> pushMode(d_mode);         // Extension, indextra means that Ind must be generated
I82_hir: 'HIR' -> pushMode(d_mode);

I87_if_flag: 'IFF' -> pushMode(di_lai_mode_a);
I97_dsz: 'Dsz' -> pushMode(di_lai_mode_a);


// This rule is only active in the default mode.
NUMBER: [0-9]+;

WS: [ \t\r\n]+ -> skip; // Skip whitespace

INVALID_TOKEN: ~[ \t\r\n]+;

// DD or Ind DD
mode di_mode;
	INDIRECT1: 'Ind';
    DD1: [0-9][0-9]? -> popMode;
    WS1: [ \t\r\n]+ -> skip;
    INVALID1: ~[ \t\r\n]+ -> popMode;

// DD (indirect or HIR)
mode d_mode;
    DD2: [0-9][0-9]? -> popMode;
    WS2: [ \t\r\n]+ -> skip;
    INVALID2: ~[ \t\r\n]+ -> popMode;

// Label, address or indirect
mode lai_mode;
	INDIRECT3: 'Ind';
    MNEMONIC3: ('CLR'|'STO'|'RCL'|'CE') -> popMode;
    ADD3b: '0'[0-9] [ \t\r\n]* [0-9][0-9] -> popMode;
    ADD3a: [0-9][0-9][0-9] -> popMode;
    DD3: [0-9][0-9]? -> popMode;
    WS3: [ \t\r\n]+ -> skip;
    INVALID3: ~[ \t\r\n]+ -> popMode;

mode di_lai_mode_a;
	INDIRECT4a: 'Ind';
    DD4a: [0-9][0-9]? -> mode(di_lai_mode_b);
    WS4a: [ \t\r\n]+ -> skip;
    INVALID4a: ~[ \t\r\n]+ -> popMode;

mode di_lai_mode_b;
	INDIRECT4b: 'Ind';
    MNEMONIC4: ('CLR'|'STO'|'RCL'|'CE') -> popMode;
    DD4b: [1-9][0-9]? -> popMode;
    ADD4b: '0'[0-9] [ \t\r\n]* [0-9][0-9] -> popMode;
    ADD4a: [0-9][0-9][0-9] -> popMode;
    WS4b: [ \t\r\n]+ -> skip;
    INVALID4b: ~[ \t\r\n]+ -> popMode;

// Lbl
mode label_mode;
    DD5: [1-9][0-9]? -> popMode;
    MNEMONIC5: ('CLR'|'STO'|'RCL'|'CE') -> popMode;
    WS5: [ \t\r\n]+ -> skip;
    INVALID5: ~[ \t\r\n]+ -> popMode;
