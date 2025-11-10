grammar pi;

options {
    caseInsensitive = true;
}

main
    : Pi EOF
    ;

Pi
    : 'π'
    | 'Pi'
    | '3.14'
    | 'x²'
    | 'Σ+'
    | 'x≥t'
    ;
