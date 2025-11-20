grammar g;

D1: [0-9];
D2: [0-9][0-9];

I25_clear: 'CLR';
I40_indirect: 'IND';
I42_store: 'STO';

WS: [ \t\r\n]+ -> skip; // Skip whitespace

program: statement+;

statement: store_statement|clear_statement;

clear_statement: I25_clear;

d1_or_d2: D1|D2;

store_statement: I42_store ind=I40_indirect? d1_or_d2;
