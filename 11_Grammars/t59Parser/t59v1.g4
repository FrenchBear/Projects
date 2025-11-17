// Parser for TI58/TI59 programming language for Antlr
// 2025-11-08   PV      First version

grammar t59v1;

options {
    caseInsensitive = true;
}

// ----------------------------------
// Lexico: white space, comments and mnemonics

WS: [ \r\n\t]+;

LineComment: '#' ~[\r\n]*;

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

I21_2nd: '2nd';
I22_invert: 'Inv';
I23_ln: 'lnx';
I24_correct_entry: 'CE';
I25_clear: 'CLR';
I26_2nd_2nd: '2nd\'';
I27_2nd_invert: 'INV\'';
I28_log: 'log';
I29_clear_program: 'CP';
I20_2nd_clear: 'CLR\'';

I31_learn: 'LRN';
I32_exchange_x_and_t: 'x<>t' | 'x/t';
I33_square: 'x²' | 'x^2' | 'X2';
I34_square_root: '√' | '√x' | 'SQR' | 'SQRT';
I35_reciprocal: '1/x';
I36_program: 'Pgm';
I37_polar_to_rectangular: 'P->R' | 'P/R';
I38_sin: 'sin';
I39_cos: 'cos';
I30_tan: 'tan';

I41_single_step: 'SST';
I42_store: 'STO';
I43_recall: 'RCL';
I44_sum: 'SUM';
I45_power: '^' | 'y^x' | 'YX' | '**';
I46_insert: 'Ins';
I47_clear_memory: 'CMs';
I48_exchange: 'Exc';
I49_product: 'Prd';
I40_indirect: 'Ind';

I51_backstep: 'BST';
I52_exponent: 'EE';
I53_left_parenthesis: '(';
I54_right_parenthesis: ')';
I55_divide: '/' | '÷';
I56_delete: 'Del';
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
I68_nop: 'Nop';
I69_operation: 'Op';
I60_degrees: 'Deg';

I71_subroutine: 'SBR';
I72_store_indirect: 'ST*';
I73_recall_indirect: 'RC*';
I74_sum_indirect: 'SM*';
I75_subtract: '-';
I76_label: 'Lbl';
I77_x_greater_or_equal_than_t: 'x>=t' | 'GE';
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

// ----------------------------------

// Can't define these as lexical elements since they overlap and create conflicts
d: '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9';
number: '-'? d+ ('.' d+)? ('E' ('+'|'-')? d+)?;             // Generalisation allowed by this language
single_digit:   '0'? d;
memory:         d d?;                                       // Register number
pgm_number:     d d?;
address_label:  (d d d)|('0' d WS d d);                     // The version nn<space>nn is for T59 programs
numeric_key_label: ('1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9') d; // 10..99, an extension as a substitute to keys labels (25=CLR)
flag_number:    '0'? ('0'|'1'|'2'|'3'|'4'|'5'|'6'|'7');
op_number:      (('0'|'1'|'2'|'3') d) | ('4' '0');          // TI-58C has Op 40 (printer detection)


startRule
    : program EOF
    | WS EOF    // Allow empty program without allowing empty statement
    ;

program
    : WS? instruction_or_comment (WS instruction_or_comment)* WS?
    ;

instruction_or_comment
    : instruction
    | LineComment
    ;
    
instruction
    : number
    | I93_dot
	| I94_change_sign
	| exponent
	| I53_left_parenthesis
	| I54_right_parenthesis
	| operator
	| I95_equals
	| invert '!'        // Trick to allow a standalone Inv while keeping grammar simple
    | fix
	| atomic_instruction
	| memory_or_flag_instruction
	| label_instruction
	| branch_instruction
	| conditional_instruction
    | op_instruction
    | pgm_instruction
    ;
    
exponent: (invert WS?)* I52_exponent;

operator
    : I85_add
    | I75_subtract
    | I65_multiply
    | I55_divide
    | I45_power
    ;

invert: I22_invert;

fix:    I58_fix WS? single_digit_or_indirect;

atomic_instruction 
	: (invert WS?)* I23_ln
	| I123_e_power_x
	| (invert WS?)* I28_log
	| I128_10_power_x
    | I24_correct_entry
    | I25_clear
    | I29_clear_program
	| I32_exchange_x_and_t
	| I33_square
	| I34_square_root
	| I35_reciprocal
	| (invert WS?)* I37_polar_to_rectangular
	| (invert WS?)* I38_sin
	| (invert WS?)* I39_cos
	| (invert WS?)* I30_tan
    | I47_clear_memory
	| (invert WS?)* I57_engineering
	| (invert WS?)* I58_fix
	| (invert WS?)* I59_integer
	| I50_absolute
	| I66_pause
	| I68_nop
	| I60_degrees
	| (invert WS?)* I78_sigma_plus
	| (invert WS?)* I79_average
	| I70_radians
	| (invert WS?)* I88_dms
	| I89_pi
	| I80_grades
	| I91_run_stop
	| I96_write
	| I98_advance
	| I99_print
	| I90_list
    ;
	
memory_or_flag_instruction
    : I42_store WS? memory_or_indirect
	| I72_store_indirect WS? memory
	| I43_recall WS? memory_or_indirect
	| I73_recall_indirect WS? memory
	| I48_exchange WS? memory_or_indirect
	| I63_exchange_indirect WS? memory
	| (invert WS?)* I44_sum WS? memory_or_indirect
	| (invert WS?)* I74_sum_indirect WS? memory
	| (invert WS?)* I49_product WS? memory_or_indirect
	| (invert WS?)* I64_product_indirect WS? memory
	| (invert WS?)* I86_set_flag WS? flag_or_indirect
	| I82_hir WS? memory
    ;
	
memory_or_indirect
    : memory 
    | indirect_memory
    ;

indirect_memory: I40_indirect WS? memory;

label_instruction: I76_label WS? ( key_label | numeric_key_label );

branch_instruction
    : I61_goto WS? address_or_label_or_indirect
	| I83_goto_indirect WS? memory
	| I71_subroutine WS? address_or_label_or_indirect
	| I92_return WS? | invert WS? I71_subroutine
	| I81_reset
	| I11_a | I12_b | I13_c | I14_d | I15_e
	| I16_a_prime | I17_b_prime | I18_c_prime | I19_d_prime | I10_e_prime
    ;

address_or_label_or_indirect
    : address_label
	| key_label
	| numeric_key_label
	| indirect_memory
    ;

key_label
    : I11_a | I12_b | I13_c | I14_d | I15_e
	| I16_a_prime | I17_b_prime | I18_c_prime | I19_d_prime | I10_e_prime

	| I21_2nd | I22_invert | I23_ln | I24_correct_entry | I25_clear
	| I26_2nd_2nd | I27_2nd_invert | I28_log | I29_clear_program | I20_2nd_clear
	
	| I31_learn | I32_exchange_x_and_t | I33_square | I34_square_root | I35_reciprocal
	| I36_program | I37_polar_to_rectangular | I38_sin | I39_cos | I30_tan

	| I41_single_step | I42_store | I43_recall | I44_sum | I45_power
	| I46_insert | I47_clear_memory | I48_exchange | I49_product | I40_indirect

	| I51_backstep | I52_exponent | I53_left_parenthesis | I54_right_parenthesis | I55_divide
	| I56_delete | I57_engineering | I58_fix | I59_integer | I50_absolute

	| I61_goto | I62_program_indirect | I63_exchange_indirect | I64_product_indirect | I65_multiply
	| I66_pause | I67_x_equals_t | I68_nop | I69_operation | I60_degrees

	| I71_subroutine | I72_store_indirect | I73_recall_indirect | I74_sum_indirect | I75_subtract
	| I76_label | I77_x_greater_or_equal_than_t | I78_sigma_plus | I79_average | I70_radians

	| I81_reset | I82_hir | I83_goto_indirect | I84_operation_indirect | I85_add
	| I86_set_flag | I87_if_flag | I88_dms | I89_pi | I80_grades

	| I91_run_stop | I92_return | I93_dot | I94_change_sign | I95_equals
	| I96_write | I97_dsz | I98_advance | I99_print | I90_list
    ;

flag_or_indirect
    : flag_number
    | indirect_memory
    ;

conditional_instruction
    : x_equals_t_statement
	| x_greater_or_equal_than_t_statement
	| decrement_and_skip_on_zero_statement
	| test_flag_statement
    ;

// x=t
x_equals_t_statement: (invert WS?)* I67_x_equals_t WS? address_or_label_or_indirect;

// x>=t
x_greater_or_equal_than_t_statement: (invert WS?)* I77_x_greater_or_equal_than_t WS? address_or_label_or_indirect;

// Dsz (note that while officially only memories from 0 to 9 are supported, in fact all 99 memories are supported)
decrement_and_skip_on_zero_statement: (invert WS?)* I97_dsz WS? single_digit_or_indirect WS address_or_label_or_indirect;

single_digit_or_indirect: single_digit | indirect_memory;

// If flg
test_flag_statement: (invert WS?)* I87_if_flag WS? flag_or_indirect WS? address_or_label_or_indirect;

// Op
op_instruction
    : I69_operation WS? op_number_or_indirect
	| I84_operation_indirect WS? memory
    ;

op_number_or_indirect
    : op_number
    | indirect_memory 
    ;

// Pgm
pgm_instruction
    : I36_program WS? pgm_number_or_indirect
	| I62_program_indirect WS? pgm_number
    ;

pgm_number_or_indirect
    : pgm_number
    | indirect_memory
    ;
