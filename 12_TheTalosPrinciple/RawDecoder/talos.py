# Solver of hex ASCII codes for the Talos Principle
#
# 2025-12-29    PV

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


# file oxyrhynchus.txt (entrée B)
s = '4C 49 46 45 20 49 4E 20 44 45 41 54 48 20 42 45 47 49 4E 53 20 41 4E 45 57'

# hell.txt (entrée C)
s = '4F 6E 52 65 20 74 68 6F 75 67 68 74 2E 20 66 69 6C 6C 73 20 69 6D 6D 65 6E 73 69 74 79 2E'

# A03: teams_leads.eml
# ugh inaction, allow humanity to come to harm
s = '75 67 68 20 69 6E 61 63 74 69 6F 6E 2C 20 61 6C 6C 6F 77 20 68 75 6D 61 6E 69 74 79 20 74 6F 20 63 6F 6D 65 20 74 6F 20 68 61 72 6D'

# A02 Extra, error.log
# The road of excess leads
# That day I oft remember, when from sleep
# I first awaked, and found myself reposed
# e palace of wisdom.
# Under a shade on flowers, much wondering where
# And what I was, whence thither brought, and how.
#  to th
# ERROR: UNKNOWN ERROR
s = """
54 68 65 20 72 6F 61 64 20 6F 66 20
65 78 63 65 73 73 20 6C 65 61 64 73
That day I oft remember, when from sleep
I first awaked, and found myself reposed
65 20 70 61 6C 61 63 65 20 6F 66 20 77
69 73 64 6F 6D 2E
Under a shade on flowers, much wondering where
And what I was, whence thither brought, and how.
20 74 6F 20 74 68
ERROR: UNKNOWN ERROR"""
talos_decode(s)

# A02 Extra, the_human_machine.html
# facebook.com/croteam
# s="""1500-2000 words. The 26th is the final deadline, no
# extensions will be granted. Submit via email or
# 66 61 63 65 62 6F 6F 6B 2E 63 6F 6D 2F 63 72 6F
# 74 65 61 6D"""


