# Module UniData.py
# Reads and stores Unicode UCD file UnicodeData.txt (currently use version from Unicode 13)
# Provides official name and category for each valid codepoint. Not restricted to BMP.
# Can't name it UnicodeData since there is already a module with this name in Python
# Use Python 3.6/3.7 features
#
# 2018-09-06    PV
# 2020-09-03    PV      1.1: Unicode 13, Tcl support >0xFFFF, list contains UTF-8 and UTF-16
# 2020-12-31    PV      1.2: Added Scripts.txt, GetUnknown(cp)

from types import MappingProxyType
from typing import Dict, List
from dataclasses import dataclass


MAXCODEPOINT = 0x10FFFF

# # Python 3.6, an immutable namedtuple (import collections)
# Can't use immutable anymore, since scripts are read in a 2nd pass after records are created
# Good news, @dataClass from Python 3.7 is mutable!
# class CharacterRecord(NamedTuple):
#     Codepoint: int
#     Name: str
#     Category: str
#     Script: str

def GetCodepointHexa(Codepoint: int) -> str:
    return f'U+{Codepoint:04X}'

def GetUTF8String(Codepoint: int) -> str:
    if Codepoint <= 0x7F:
        return f"{Codepoint:02X}"
    if Codepoint <= 0x7FF:
        return f"{0xC0 + Codepoint // 0x40:02X} {0x80 + Codepoint % 0x40:02X}"
    if Codepoint <= 0xFFFF:
        return f"{0xE0 + (Codepoint // 0x40) // 0x40:02X} {0x80 + (Codepoint // 0x40) % 0x40:02X} {0x80 + Codepoint % 0x40:02X}"
    if Codepoint <= 0x1FFFFF:
        return f"{0xF0 + ((Codepoint // 0x40) // 0x40) // 0x40:02X} {0x80 + ((Codepoint // 0x40) // 0x40) % 0x40:02X} {0x80 + (Codepoint // 0x40) % 0x40:02X} {0x80 + Codepoint % 0x40:02X}"
    return "?{Codepoint}?"

def GetUTF16String(Codepoint: int) -> str:
    return f'{Codepoint:04X}' if Codepoint <= 0xD7FF or 0xE000 <= Codepoint <= 0xFFFF else f'{0xD800 + ((Codepoint - 0x10000) >> 10):04X} {0xDC00 + (Codepoint & 0x3ff):04X}'


@dataclass
class CharacterRecord:
    Codepoint: int
    Name: str
    Category: str
    Script: str

    @property
    def CodepointHexa(self) -> str:
        return GetCodepointHexa(self.Codepoint)

    @property
    def UTF16(self) -> str:
        return GetUTF16String(self.Codepoint)

    @property
    def UTF8(self) -> str:
        return GetUTF8String(self.Codepoint)


# Read information file UnicodeVersion.txt
UnicodeVersion = ''
with open('UnicodeVersion.txt', encoding='utf-8') as uv:
    UnicodeVersion = uv.readline()

_codepoint_map: Dict[int, CharacterRecord] = {}

# Read name and category from UnicodeData.txt
with open('UnicodeData.txt', encoding='utf-8') as ud:
    for line in (ud):
        fields: List[str] = line.split(';')
        codepoint = int(fields[0], 16)
        char_name = fields[1]
        char_category = fields[2]

        # Special name overrides
        if codepoint == 28:
            char_name = "CONTROL - FILE SEPARATOR"
        else:
            if codepoint == 29:
                char_name = "CONTROL - GROUP SEPARATOR"
            else:
                if codepoint == 30:
                    char_name = "CONTROL - RECORD SEPARATOR"
                else:
                    if codepoint == 31:
                        char_name = "CONTROL - UNIT SEPARATOR"
                    else:
                        if codepoint < 32 or codepoint >= 0x7f and codepoint < 0xA0:
                            char_name = "CONTROL - " + (fields[10] if len(fields[10]) > 0 else fields[0][2:])

        is_range = char_name.endswith(', First>')
        if is_range:    # add all characters within a specified range
            char_name = char_name.replace(', First>', '').replace('<', '').upper()  # remove range indicator from name
            line = next(ud)
            fields = line.split(';')
            end_char_code = int(fields[0], 16)
            if not fields[1].endswith(', Last>'):
                raise Exception('Expected end-of-range indicator.')
            for code_in_range in range(codepoint, end_char_code+1):
                _codepoint_map[code_in_range] = CharacterRecord(
                    code_in_range, f'{char_name} - {code_in_range:X}', char_category, 'Unknown')
        else:
            _codepoint_map[codepoint] = CharacterRecord(codepoint, char_name, char_category, 'Unknown')

# Read script from Scripts.txt
with open('Scripts.txt', encoding='utf-8') as ud:
    for line in (ud):
        p = line.find('#')
        if p >= 0:
            line = line[:p]
        if len(line) == 0 or line == '\n':
            continue
        fields = line.split(';')
        codepoint_range = fields[0].strip()
        script_name = fields[1].strip()
        p = codepoint_range.find('..')
        if p < 0:
            codepoint = int(codepoint_range, 16)
            _codepoint_map[codepoint].Script = script_name
        else:
            from_codepoint = int(codepoint_range[:p], 16)
            to_codepoint = int(codepoint_range[p+2:], 16)
            for codepoint in range(from_codepoint, to_codepoint+1):
                _codepoint_map[codepoint].Script = script_name


# For public access, readonly dict of CharacterRecord
CharacterRecords: MappingProxyType[int, CharacterRecord] = MappingProxyType(_codepoint_map)


# Use another internal dict for efficient mapping name -> codepoint, though it's not used much in this app
_name_map: Dict[str, int] = {cr.Name.lower(): cr.Codepoint for cr in _codepoint_map.values()}


def GetCodepointFromName(name: str) -> int:
    cp = _name_map.get(name.lower(), -1)
    return cp


def AsString(cp: int) -> str:
    if cp<0:
        print('cp:', cp)
        pass
    return chr(cp)


# Value returned for unsupported codepoints
unknown = CharacterRecord(-1, 'Unknown character', '??', 'Unknown')

# def GetUnknown(cp: int) -> CharacterRecord:
#     return CharacterRecord(cp, f"Unassigned codepoint - {cp:04X}", '??', "Unknown")


def GetName(cp: int) -> str:
    cr = _codepoint_map.get(cp, None)
    return cr.Name if cr else f"Unassigned codepoint - {cp:04X}"

def GetCategory(cp: int) -> str:
    cr = _codepoint_map.get(cp, None)
    return cr.Category if cr else "??"

def GetScript(cp: int) -> str:
    cr = _codepoint_map.get(cp, None)
    return cr.Script if cr else "Unknown"

def IsValidCodepoint(cp: int) -> bool:
    return cp in _codepoint_map

def IsSurrogate(cp: int) -> bool:
    return 0xD800 <= cp <= 0xDFFF



# Tests
if __name__ == '__main__':
    # Commented out, not needed once it works.  And it raises an error in mypy (attempt to modify a readony variable)
    # # Check that CharacterRecord is immutable, not the case with @dataclass
    # print('----- Mutability tests')
    # cr = CharacterRecords[65]
    # try:
    #     cr.Name='Lettre A majuscule'
    #     print('We have a problem, CharacterRecord is mutable.')
    #     print(cr)
    # except AttributeError:
    #     print('CharacterRecord is immutable.')

    print('----- UCDData tests')
    print(GetCodepointFromName('BOAR'))
    print(AsString(0xCAFE))
    print(GetName(0xCAFE))
    print(GetCategory(0xCAFE))
    print(GetScript(0xCAFE))
    print(IsValidCodepoint(0x_BAD_CAFE))
    print(IsSurrogate(0xCAFE))
    print('|'+GetCodepointHexa(0xE)+'|'+GetName(0xE))
    print('|'+GetCodepointHexa(0x1D)+'|'+GetName(0x1D))
    print('|'+GetCodepointHexa(0x99)+'|'+GetName(0x99))
    print('|'+GetCodepointHexa(0xCAFE)+'|'+GetName(0xCAFE))
    print('|'+GetCodepointHexa(0x1CAFE)+'|'+GetName(0x1CAFE))
    print('|'+GetCodepointHexa(0x10CAFE)+'|'+GetName(0x10CAFE))
