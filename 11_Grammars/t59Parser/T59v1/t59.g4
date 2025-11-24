// Parser for TI58/TI59 programming language for Antlr
//
// Note: to use INV alone or in a non-standard context, use INV! instead
// This enables a clean integration of INV with instructions supporting it
// Extensions: 
// - Multiple (case-insensitive) mnemonic names: 'x²' 'x^2' and 'X2' are identical
// - Comments: # until end of line
// - Extra instructions such as e^x for INV lnx...
// - Numbers accepted in usual form: 6.02e23, -56.8612, ...
// - Two digits are accepted as equivalents of key names for labels: Lbl 25 is equivalent to Lbl CLR
// Some weird mnemonic variants support format of .t59 files such as YX for y^x,
// addresses such as 02 73, X2 for x², IXI for |x|, STA for Σ+, AVR for x̄...
// Some mnemonics are not valid instructions (SST, LRN, Ins...) but are accepted
// as labels (Lbl SST, Lbl LRN, Lbl Ins...)
//
// INVALID_TOKEN catches invalid top level instructions sur as ZAP, but can't be used inside a parser rule as a catch-all element since
// it accumulates as many characters as possible, so it has priority over D D in STO 12 for instance.
// Reducing it to one character doesn't work since it will recognize only one character at a time, so ZAP will be tokenized as Z INVALID_TOKEN,
// but then AP is not a separated invalid instructions since spaces are required between instructions, so we need a different catch-all mechanism
// inside parser rules: without a catch-all, visiting terminals won't see ZAP in STO ZAP, so we can't colorize it as wrong.
//
// 2025-11-08   PV      First version
// 2025-11-09   PV      Version 2, flatten model
// 2025-11-10   PV      Added missing invert instructions (INV Write, INV List, INV ^), 10 flags and not 7
// 2025-11-11   PV      Renamed rules for consistency; Ind can't be a Key label (otherwise GTO Ind xx is misinterpreted); x≥t ant x>=t are two distinct variants!; Added EQ* and GE*
// 2025-11-13   PV      Added END statement to allow multiple grograms in the same source text
// 2025-11-19   PV      This grammar does work with well-formed programs, but not with invalid instructions or invalid statements
// 2025-11-24   PV      Reduced INVALID_TOKEN to a single char

grammar t59;

options {
    caseInsensitive = true;
}

// ----------------------------------
// Lexico: white space, comments and mnemonics

WS: [ \r\n\t]+;

LineComment: '#' ~[\r\n]*;

Bang: '!';

Program_separator: 'END' ;

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
I22_invert: 'INV';
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

// ----------------------------------

// Can't define these as lexical elements since they overlap and create conflicts
d: '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9';
number: '-'? ((d+ ('.' d+)?)|('.' d+)) ('E' ('+'|'-')? d+)?;    // Generalisation allowed
single_digit:   '0'? d;                                         // Fix or Flag
memory:         d d?;                                           // Register number
indmemory:      d d?;                                           // Indirect Register number
pgm_number:     d d?;
address_label:  (d d d)|('0' d WS d d);                         // The version nn<space>nn is for T59 programs
numeric_key_label: ('1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9') d;     // 10..99, an extension as a substitute to keys labels (25=CLR)
op_number:      (('0'|'1'|'2'|'3') d) | ('4' '0');              // TI-58C has Op 40 (printer detection)

INVALID_TOKEN: ~[ \t\r\n];      // Can't use + otherwise it will absorb too much inside parser rules (ex: 12 is INVALID_TOKEN INVALID_TOKEN for STO 12)

// ----------------------------------

startRule
    : programs EOF
    | WS EOF    // Allow empty program without allowing empty statement
    ;

programs: program (Program_separator program)* ;

program
    : WS? instruction_or_comment (WS instruction_or_comment)* WS?
    ;

instruction_or_comment
    : instruction
    | LineComment
    ;
    
instruction
    : number
    | instruction_invert_isolated
	| instruction_atomic_simple
	| instruction_atomic_invertible
    | instruction_atomic_inverted
    | instruction_fix                       // single number or indirect
    | instruction_setflag                   // invertible; single number or indirect
    | instruction_op                        // 00..40 or indirect
    | instruction_pgm                       // 00..99 or indirect
	| instruction_memory
    | instruction_label
	| instruction_branch
	| instruction_conditional
    | instruction_invalid
    ;

instruction_invert_isolated: inv Bang;      // Trick to allow a standalone Inv while keeping grammar simple, just INV!
    
// Prefix for invertible instructions
inv: I22_invert WS?;

instruction_atomic_simple 
	: I11_a 
    | I12_b 
    | I13_c 
    | I14_d 
    | I15_e
	| I16_a_prime 
    | I17_b_prime 
    | I18_c_prime 
    | I19_d_prime 
    | I10_e_prime
    | I24_correct_entry
    | I25_clear
    | I29_clear_program
	| I32_exchange_x_and_t
	| I33_square
	| I34_square_root
	| I35_reciprocal
    | I47_clear_memory
	| I53_left_parenthesis
	| I54_right_parenthesis
    | I55_divide
	| I50_absolute
    | I65_multiply
	| I66_pause
	| I68_nop
	| I60_degrees
    | I75_subtract
	| I70_radians
	| I81_reset
    | I85_add
	| I89_pi
	| I80_grades
	| I91_run_stop
    | I93_dot
	| I94_change_sign
	| I95_equals
	| I98_advance
	| I99_print
	| I92_return
    ;

instruction_atomic_invertible
	: inv? I23_ln
	| inv? I28_log
	| inv? I37_polar_to_rectangular
	| inv? I38_sin
	| inv? I39_cos
	| inv? I30_tan
    | inv? I45_power
    | inv? I52_exponent
	| inv? I57_engineering
	| inv? I58_fix
	| inv? I59_integer
	| inv? I78_sigma_plus
	| inv? I79_average
	| inv? I88_dms
	| inv? I96_write
	| inv? I90_list
    | inv I71_subroutine            // Normally I92_return, but the split format is also valid
    ;

instruction_atomic_inverted
	: I123_e_power_x
	| I128_10_power_x
    ;

// INV Fix is a atomic_instruction_invertible without argument
instruction_fix: I58_fix WS? single_digit_or_indirect;

instruction_setflag: inv? I86_set_flag WS? single_digit_or_indirect;

single_digit_or_indirect: single_digit | indirect_memory ;

instruction_op
    : I69_operation WS? op_number_or_indirect
	| I84_operation_indirect WS? indmemory
    ;

op_number_or_indirect: op_number | indirect_memory ;

indirect_memory: I40_indirect WS? indmemory;

instruction_pgm
    : I36_program WS? pgm_number_or_indirect
	| I62_program_indirect WS? indmemory
    ;

pgm_number_or_indirect: pgm_number | indirect_memory ;

instruction_memory
    : I42_store WS? memory_or_indirect
	| I43_recall WS? memory_or_indirect
	| I48_exchange WS? memory_or_indirect
	| I82_hir WS? memory
	| I72_store_indirect WS? indmemory
	| I73_recall_indirect WS? indmemory
	| I63_exchange_indirect WS? indmemory
	| inv? I44_sum WS? memory_or_indirect
	| inv? I49_product WS? memory_or_indirect
	| inv? I74_sum_indirect WS? indmemory
	| inv? I64_product_indirect WS? indmemory
    ;

memory_or_indirect: memory | indirect_memory;

instruction_label: I76_label WS? ( key_label | numeric_key_label );

instruction_branch
    : I61_goto WS? address_or_label_or_indirect
	| I83_goto_indirect WS? indmemory
	| I71_subroutine WS? address_or_label_or_indirect
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
	| I46_insert | I47_clear_memory | I48_exchange | I49_product        // | I40_indirect       Actually, Ind is forbidden!

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

instruction_conditional
    : instruction_x_equals_t
	| instruction_x_greater_or_equal_than_t
	| instruction_decrement_and_skip_on_zero
	| instruction_test_flag
    ;

// x=t
instruction_x_equals_t
    : inv? I67_x_equals_t WS? address_or_label_or_indirect
    | inv? I67_x_equals_t_indextra WS? indmemory
    ;

// x>=t
instruction_x_greater_or_equal_than_t
    : inv? I77_x_greater_or_equal_than_t WS? address_or_label_or_indirect
    | inv? I77_x_greater_or_equal_than_t_indextra WS? indmemory
    ;

// Dsz 
// While officially only memories from 0 to 9 are supported, in fact all 99 memories are supported, with the exeption of
// register 40: Opcodes 97 40 rr means Dsz Ind rr
instruction_decrement_and_skip_on_zero: inv? I97_dsz WS? memory_or_indirect WS address_or_label_or_indirect;

// If flg
instruction_test_flag: inv? I87_if_flag WS? single_digit_or_indirect WS? address_or_label_or_indirect;

instruction_invalid: INVALID_TOKEN ;