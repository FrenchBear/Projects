// Calc grammar
// Play with ANTR4 book examples
//
// 2025-11-17   PV
// 2025-11-19   PV      Added clear statement

grammar LabeledExpr;

import CommonLexerRules; // includes all rules from CommonLexerRules.g4

prog: stat+ ;

stat: expr NEWLINE				# printExpr
	| ID '=' expr NEWLINE		# assign
	| NEWLINE					# blank
    | 'clear' NEWLINE           # clear
	;

expr: expr op=('*'|'/') expr	# MulDiv
	| expr op=('+'|'-') expr 	# AddSub
	| INT 						# int
	| ID  						# id
	| '(' expr ')' 				# parens
	;
	
MUL : '*' ; // assigns token name to '*' used above in grammar
DIV : '/' ;
ADD : '+' ;
SUB : '-' ;

