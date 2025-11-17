lexer grammar Vocab;

// When the lexer matches 'STO', it emits the STO_KEYWORD token
// AND switches the lexer into 'sto_mode'.
STO_KEYWORD: 'STO' -> pushMode(sto_mode);

// This rule is only active in the default mode.
NUMBER: [0-9]+;

// This rule is also only active in the default mode.
LBL_KEYWORD: 'LBL';

WS: [ \t\r\n]+ -> skip; // Skip whitespace


// --- LEXER MODE for STO ---
// These rules are ONLY active when the lexer is in 'sto_mode'
mode sto_mode;

    // This rule matches your 1 or 2 digits.
    // It creates the STO_ADDRESS token.
    // Crucially, it then pops the lexer back to DEFAULT_MODE.
    STO_ADDRESS: [0-9][0-9]? -> popMode;

    // We also want to skip whitespace while in this mode
    WS_STO: [ \t\r\n]+ -> skip;

    // Optional: If you see anything else, it's an error
    // ERROR: . -> type(INVALID_TOKEN), popMode;