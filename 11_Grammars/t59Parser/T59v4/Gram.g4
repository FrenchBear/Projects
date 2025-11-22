// Fourth variant of T59 grammar - parser
// Don't use INVALID_TOKEN anymore
//
// 2025-11-20   PV

grammar Gram;

options {
	tokenVocab = Vocab;
	caseInsensitive = true;
}

programs: program (PROGRAM_SEPARATOR program)* EOF;

program
    : statement_or_comment* EOF
    ;

statement_or_comment
    : statement
    | LINE_COMMENT
    | tag_declaration
    ;

tag_declaration: TAG COLON;

statement:
    number_statement
	| atomic_statement
	| d_statement           // Direct address
    | i_statement           // Indirect address
    | bd_statement          // Branch direct or mnemonic
    | bi_statement          // Branch indirect
    | t_statement           // 4 variants of test and branch
	| label_statement
    | inv_statement         // Must be last
    | invalid_statement
    ;

// Attempt to solve DSZ 05 25 issue if A4 is processed by lexer: 02 25 is recognized as A4, which breaks DSZ/IFF recognition
a4: D2 D2;

// These two rules xan't be defined in Vocab.g4
number_statement: NUM|D1|D2|A3|a4;

// According to TI doc V-56, 72 variants available, but 82 here: I accept merged indirect statements codes and HIR (82)
mnemonic: I11_a|I12_b|I13_c|I14_d|I15_e|I16_a_prime|I17_b_prime|I18_c_prime|I19_d_prime|I10_e_prime|I22_invert|I23_ln|I24_correct_entry|I25_clear|I27_2nd_invert|I28_log|I29_clear_program|I20_2nd_clear|I32_exchange_x_and_t|I33_square|I34_square_root|I35_reciprocal|I36_program|I37_polar_to_rectangular|I38_sin|I39_cos|I30_tan|I42_store|I43_recall|I44_sum|I45_power|I47_clear_memory|I48_exchange|I49_product|I52_exponent|I53_left_parenthesis|I54_right_parenthesis|I55_divide|I57_engineering|I58_fix|I59_integer|I50_absolute|I61_goto|I62_program_indirect|I63_exchange_indirect|I64_product_indirect|I65_multiply|I66_pause|I67_x_equals_t|I68_nop|I69_operation|I60_degrees|I71_subroutine|I72_store_indirect|I73_recall_indirect|I74_sum_indirect|I75_subtract|I76_label|I77_x_greater_or_equal_than_t|I78_sigma_plus|I79_average|I70_radians|I81_reset|I82_hir|I83_goto_indirect|I84_operation_indirect|I85_add|I86_set_flag|I87_if_flag|I88_dms|I89_pi|I80_grades|I91_run_stop|I92_return|I93_dot|I94_change_sign|I95_equals|I96_write|I97_dsz|I98_advance|I99_print|I90_list;

atomic_statement: 
    sta=(I10_e_prime|I11_a|I12_b|I123_e_power_x|I128_10_power_x|I13_c|I14_d|I15_e|I16_a_prime|I17_b_prime|I18_c_prime|I19_d_prime|I20_2nd_clear|I24_correct_entry|I25_clear|I27_2nd_invert|I29_clear_program|I32_exchange_x_and_t|I33_square|I34_square_root|I35_reciprocal|I47_clear_memory|I50_absolute|I53_left_parenthesis|I54_right_parenthesis|I55_divide|I60_degrees|I65_multiply|I66_pause|I68_nop|I70_radians|I75_subtract|I80_grades|I81_reset|I85_add|I89_pi|I91_run_stop|I92_return|I93_dot|I94_change_sign|I95_equals|I98_advance|I99_print)
    | inv=I22_invert? sta=(I23_ln|I28_log|I30_tan|I37_polar_to_rectangular|I38_sin|I39_cos|I45_power|I52_exponent|I57_engineering|I59_integer|I78_sigma_plus|I79_average|I88_dms|I90_list|I96_write)
    | inv=I22_invert sta=I58_fix
    ;

d_statement: 
    sta=(I36_program|I42_store|I43_recall|I48_exchange|I58_fix|I69_operation|I82_hir) t=(D1|D2)   // direct, non-invertible
    | inv=I22_invert? sta=(I44_sum|I49_product|I86_set_flag) t=(D1|D2)      // direct, invertible
    ;

i_statement:
    sta=(I62_program_indirect|I63_exchange_indirect|I72_store_indirect|I73_recall_indirect|I83_goto_indirect|I84_operation_indirect) t=(D1|D2)   // Merged non-invertible indirect statements that do not use Ind
    | inv=I22_invert? sta=(I64_product_indirect|I67_x_equals_t_indextra|I74_sum_indirect|I77_x_greater_or_equal_than_t_indextra) t=(D1|D2)  // Merged invertible indirect statements that do not use Ind
    | sta=(I36_program|I42_store|I43_recall|I48_exchange|I58_fix|I69_operation) ind=I40_indirect t=(D1|D2)   // Non-invertible statements that use Ind
    | inv=I22_invert? sta=(I44_sum|I49_product|I86_set_flag) ind=I40_indirect t=(D1|D2)   // Invertible statements that use Ind
    ;

ad_statement: mnemonic|a4|t=(D2|A3|TAG);
ai_statement: I40_indirect t=(D1|D2);
bd_statement:
    sta=(I61_goto|I71_subroutine) ad_statement      // SBR is here, but INV SBR is an atomic statement
    | inv=I22_invert? sta=(I67_x_equals_t|I77_x_greater_or_equal_than_t) ad_statement
    ;

bi_statement:
    sta=(I61_goto|I71_subroutine) ai_statement
    | inv=I22_invert? sta=(I67_x_equals_t|I77_x_greater_or_equal_than_t) ai_statement
    ;

t_statement: tdd_statement | tdi_statement | tid_statement | tii_statement;
td_statement: D1|D2;
ti_statement: I40_indirect (D1|D2);
tdd_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) td_statement ad_statement;
tdi_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) td_statement ai_statement;
tid_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) ti_statement ad_statement;
tii_statement: inv=I22_invert? sta=(I87_if_flag|I97_dsz) ti_statement ai_statement;

label_statement: sta=I76_label (D1|D2|mnemonic);

inv_statement: sta=I22_invert;

invalid_statement: I40_indirect | COLON;
