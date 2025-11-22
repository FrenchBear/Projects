// 5th variant of T59 grammar - parser
// Only use ANTLR lexical parsing in this version
//
// 2025-11-22   PV

lexer grammar Vocab;

options {
    caseInsensitive = true;
}

// White space
//WS: [ \t\r\n]+ -> channel(HIDDEN); // Skip whitespace     // NOTE: ignore may be more efficient since we drop WS in L1Tokenizer
WS: [ \t\r\n]+ -> skip;

LINE_COMMENT: '//' ~[\r\n]*;

PROGRAM_SEPARATOR: 'END' ;

// Instructions (1-digit are covered by D1 -> number)
I11_a: 'A';
I12_b: 'B';
I13_c: 'C';
I14_d: 'D';
I15_e: 'E';
I16_a_prime: 'A\'';
I17_b_prime: 'B\'';
I18_c_prime: 'C\'';
I19_d_prime: 'D\'';
I10_e_prime: 'E\'';

//I21_2nd: '2nd';
I22_invert: 'INV';
I23_ln: 'lnx';
I24_correct_entry: 'CE';
I25_clear: 'CLR';
//I26_2nd_2nd: '2nd\'';
I27_2nd_invert: 'INV\'';
I28_log: 'log';
I29_clear_program: 'CP';
I20_2nd_clear: 'CLR\'';

//I31_learn: 'LRN';
I32_exchange_x_and_t: 'x<>t' | 'x/t';
I33_square: 'x²' | 'x^2' | 'X2';
I34_square_root: '√' | '√x' | 'SQR' | 'SQRT';
I35_reciprocal: '1/x';
I36_program: 'Pgm';
I37_polar_to_rectangular: 'P->R' | 'P/R';
I38_sin: 'sin';
I39_cos: 'cos';
I30_tan: 'tan';

//I41_single_step: 'SST';
I42_store: 'STO';
I43_recall: 'RCL';
I44_sum: 'SUM';
I45_power: '^' | 'y^x' | 'YX' | '**';
//I46_insert: 'Ins';
I47_clear_memory: 'CMs';
I48_exchange: 'Exc';
I49_product: 'Prd';
I40_indirect: 'Ind';

//I51_backstep: 'BST';
I52_exponent: 'EE';
I53_left_parenthesis: '(';
I54_right_parenthesis: ')';
I55_divide: '/' | '÷';
//I56_delete: 'Del';
I57_engineering: 'Eng';
I58_fix: 'Fix';
I59_integer: 'Int';
I50_absolute: '|x|' | 'ABS' | 'IXI';

I61_goto: 'GTO';
I62_program_indirect: 'PG*';
I63_exchange_indirect: 'EX*';
I64_product_indirect: 'PD*';
I65_multiply: '*' | '×';
I66_pause: 'Pause' | 'PAU';
I67_x_equals_t: 'x=t' | 'EQ';
I67_x_equals_t_indextra: 'EQ*';         // Extension, indextra means that Ind must be generated
I68_nop: 'Nop';
I69_operation: 'Op';
I60_degrees: 'Deg';

I71_subroutine: 'SBR';
I72_store_indirect: 'ST*';
I73_recall_indirect: 'RC*';
I74_sum_indirect: 'SM*';
I75_subtract: '-';
I76_label: 'Lbl';
I77_x_greater_or_equal_than_t: 'x≥t' | 'x>=t' | 'GE';
I77_x_greater_or_equal_than_t_indextra: 'GE*';         // Extension, indextra means that Ind must be generated
I78_sigma_plus: 'Σ+' | 'SIG+' | 'STA';
I79_average: 'x̄' | 'AVG' | 'AVR';
I70_radians: 'Rad';

I81_reset: 'RST';
I82_hir: 'HIR';
I83_goto_indirect: 'GO*';
I84_operation_indirect: 'Op*';
I85_add: '+';
I86_set_flag: 'STF';
I87_if_flag: 'IFF';
I88_dms: 'DMS' | 'D.MS';
I89_pi: 'π' | 'PI';
I80_grades: 'Grad' | 'GRD';

I91_run_stop: 'R/S';
I92_return: 'RTN';
I93_dot: '.';
I94_change_sign: '+/-';
I95_equals: '=';
I96_write: 'Write' | 'Wrt';
I97_dsz: 'Dsz';
I98_advance: 'Adv';
I99_print: 'Prt';
I90_list: 'List' | 'LST';

// Extra instructions
I123_e_power_x: 'e^x';
I128_10_power_x: '10^x';

// Digits and addresses
D1: [0-9];
D2: [0-9][0-9];
A3: [0-9][0-9][0-9];
//A4: '0'[0-9] [ \t\r\n]* [0-9][0-9];       // Moved to parser to avoid DSZ 05 25 issue, using dynamic recognition
NUM: '-'? (([0-9]+ ('.' [0-9]*)?) | ('.' [0-9]+)) ('E' ('+'|'-')? [0-9]+)? ;            // Standard scientific number

// Extra labels (case insensitive, so includes a-z)
TAG: '@' [A-Z_][A-Z0-9_]*;
COLON: ':';

// Add a catch-all rule so the lexer won't raise errors. L1Tokenizer winn group consecutive INVALID_CHAR into L1InvalidToken
INVALID_CHAR: . ;
