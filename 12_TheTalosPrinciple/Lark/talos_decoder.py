# talos_decoder.py
# decode hexadecimal characters in text
#
# 2025-12-31    PV

from colorama import Fore, Style

def ishex(s: str) -> bool:
    if len(s) != 2:
        return False

    def ish1(c: str) -> bool:
        return '0' <= c <= '9' or 'A' <= c <= 'F'
    return ish1(s[0]) and ish1(s[1])


def talos_decode(s: str):
    s = s.replace('\r\n', ' | ').replace('\n', ' | ')

    last = 0
    for h in s.split():
        if h == '|':
            print()
            last = 0
        elif ishex(h):
            c = int(h, 16)
            if last == 2:
                print(' ', end='')
            print(Fore.YELLOW + chr(c) + Style.RESET_ALL, end='')
            last = 1
        else:
            if last != 0:
                print(' ', end='')
            print(h, end='')
            last = 2
    print()
