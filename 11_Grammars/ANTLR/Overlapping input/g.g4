grammar g;

I25_clear: 'CLR';
I38_sin: 'SIN';
I59_integer: 'INT';

WS: [ \t\r\n]+ -> skip; // Skip whitespace

program: statement+;

statement: I25_clear|I38_sin|I59_integer;
