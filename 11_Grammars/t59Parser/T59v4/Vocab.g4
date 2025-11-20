// Fourth variant of T59 grammar - parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

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
I42_store: 'STO';
I43_recall: 'RCL';
I44_sum: 'SUM';
I61_goto: 'GTO';
I62_program_indirect: 'PG*';
I63_exchange_indirect: 'EX*';
I64_product_indirect: 'PD*';
I65_multiply: '*' | '×';
I66_pause: 'Pause' | 'PAU';
I67_x_equals_t: ('x=t' | 'EQ');
I67_x_equals_t_indextra: 'EQ*'; // indextra means that Ind must be generated
I68_nop: 'Nop';
I71_subroutine: 'SBR';
I72_store_indirect: 'ST*';
I73_recall_indirect: 'RC*';
I74_sum_indirect: 'SM*';
I76_label: 'Lbl';
I77_x_greater_or_equal_than_t: ('x≥t' | 'x>=t' | 'GE');
I77_x_greater_or_equal_than_t_indextra: 'GE*';  // indextra means that Ind must be generated
I82_hir: 'HIR';
I87_if_flag: 'IFF';
I92_return: 'RTN';
I97_dsz: 'Dsz';

D1: [0-9];
D2: [0-9][0-9];
A3: [0-9][0-9][0-9];
A4: '0'[0-9] [ \t\r\n]* [0-9][0-9];
NUM: [0-9]+;            // Should be scientific number, simplified here

WS: [ \t\r\n]+ -> skip; // Skip whitespace
