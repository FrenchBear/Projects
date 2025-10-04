# UniView.py  Python version of UniView project using Tk
#
# 2018-09-05    PV
# 2020-09-03    PV      1.1: Unicode 13, Tcl support >0xFFFF, list contains UTF-8 and UTF-16
# 2020-12-31    PV      1.2: Added Scripts
# 2021-01-01    PV      1.3: Treeview version

import unicodedata
import re

import UniData
from TkToolTip import TkToolTip

import tkinter as tk
import tkinter.font as tkFont
import tkinter.messagebox as tkmb
import tkinter.ttk as ttk
from tkinter.scrolledtext import ScrolledText  # type: ignore

from typing import Match   # , Text

# General app info
APP_TITLE = "UniView Py Tk"
APP_VERSION = "1.3"
APP_DESCRIPTION = "Details Unicode characters entered in a text box, and support some Unicode transformations."
APP_PRODUCT = "UniView DevForFun App #8, Python+Tkinter"
APP_COPYRIGHT = "Copyright ©2018-2020 Pierre Violent"

# Treeview headers
list_headers = ['CP', 'Name', 'Script', 'Cat', 'UTF-16', 'UTF-8']

# To detect and replace special elements between braces in input text
UPLUSXBRACES_RE = re.compile(r"{U\+(1?[0-9a-fA-F]{4,5})}")       # {U+1234}
UPLUSX_RE = re.compile(r"U\+(1?[0-9a-fA-F]{4,5})")               # U+1234       Simplified form when not followed by valid hex character
RANGEUPLUSXBRACES_RE = re.compile(r"{U\+(1?[0-9a-fA-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9a-fA-F]{4,5})}")     # {U+1234..U+2345} or {U+1234..2345}, use - instead of .. to avoid inserting a CR at the end of each page of 32 characters
RANGEUPLUSX_RE = re.compile(r"U\+(1?[0-9a-fA-F]{4,5})(\.\.|-)(?:U\+)?(1?[0-9a-fA-F]{4,5})")             # U+1234..U+2345
NAME_RE = re.compile(r"{([^}]+)}")                               # {semicolon}

SamplesCollection = {
    "Complete": "Aé♫山𝄞🐗\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️",
    "Simple glyphs": "Aé♫山𝄞🐗",
    "Combining accent": "Où ça? Là!",
    "Outside BMP": "𝄞𝄡𝄢",
    "Multiple lines": "Line 1\r\nLine 2\rLine 3\nLine 4",
    "Macros": "U+0041{semicolon}{U+0042}",
    "Extreme combining": "aU+0300-036F",
    "Control characters C0+C1": "{U+0000-001F}{U+007F}{U+0080-009F}",
    "Line breakers": "{U+000A}{U+000D}{U+2028}{U+2029}",
    "Unassigned codepoints": "U+0380U+0381U+0382U+0383",
    "Not a character": "{U+FDD0-U+FDEF}{U+FFFF}",   # {U+FFFE} hangs up tk
    "Invalid surrogates": "{U+D834}{U+DD1E}",
    "Beyond U+10FFFF": "{U+110000}",
    "BMP 1st 4K": "U+0000..0FFF",
    "Ranges members": "U+0000U+001FU+3400U+4E00U+AC00U+E000U+17000U+18D00U+20000U+2A700U+2B740U+2B820U+2CEB0U+30000U+F0000U+100000",
    "Arrows": "←↑→↓\r￩￪￫￬\r🠀🠁🠂🠃\r🠄🠅🠆🠇\r🠈🠉🠊🠋\r🠐🠑🠒🠓\r🠔🠕🠖🠗\r🠘🠙🠚🠛\r🠜🠝🠞🠟\r🠠🠡🠢🠣\r🠤🠥🠦🠧\r🠨🠩🠪🠫\r🠬🠭🠮🠯\r🠰🠱🠲🠳\r🠴🠵🠶🠷\r🠸🠹🠺🠻\r🠼🠽🠾🠿\r🡀🡁🡂🡃\r🡄🡅🡆🡇\r🡐🡑🡒🡓\r🡠🡡🡢🡣\r🡰🡱🡲🡳\r🢀🢁🢂🢃\r🢐🢑🢒🢓\r🢔🢕🢖🢗\r🢘🢙🢚🢛\r◀▲▶▼\r◁△▷▽\r◂▴▸▾\r⏴⏵⏶⏷",
    "Digits": "U+0030-U+0039\rU+0660-U+0669\rU+06F0-U+06F9\rU+07C0-U+07C9\rU+0966-U+096F\rU+09E6-U+09EF\rU+0A66-U+0A6F\rU+0AE6-U+0AEF\rU+0B66-U+0B6F\rU+0BE6-U+0BEF\rU+0C66-U+0C6F\rU+0CE6-U+0CEF\rU+0D66-U+0D6F\rU+0DE6-U+0DEF\rU+0E50-U+0E59\rU+0ED0-U+0ED9\rU+0F20-U+0F29\rU+1040-U+1049\rU+1090-U+1099\rU+17E0-U+17E9\rU+1810-U+1819\rU+1946-U+194F\rU+19D0-U+19D9\rU+1A80-U+1A89\rU+1A90-U+1A99\rU+1B50-U+1B59\rU+1BB0-U+1BB9\rU+1C40-U+1C49\rU+1C50-U+1C59\rU+A620-U+A629\rU+A8D0-U+A8D9\rU+A8E0-U+A8E9\rU+A900-U+A909\rU+A9D0-U+A9D9\rU+A9F0-U+A9F9\rU+AA50-U+AA59\rU+ABF0-U+ABF9\rU+FF10-U+FF19\rU+104A0-U+104A9\rU+10D30-U+10D39\rU+11066-U+1106F\rU+110F0-U+110F9\rU+11136-U+1113F\rU+111D0-U+111D9\rU+112F0-U+112F9\rU+11450-U+11459\rU+114D0-U+114D9\rU+11650-U+11659\rU+116C0-U+116C9\rU+11730-U+11739\rU+118E0-U+118E9\rU+11950-U+11959\rU+11C50-U+11C59\rU+11D50-U+11D59\rU+11DA0-U+11DA9\rU+16A60-U+16A69\rU+16B50-U+16B59\rU+16E80-U+16E89\rU+1D7CE-U+1D7D7\rU+1D7D8-U+1D7E1\rU+1D7E2-U+1D7EB\rU+1D7EC-U+1D7F5\rU+1D7F6-U+1D7FF\rU+1E140-U+1E149\rU+1E2F0-U+1E2F9\rU+1E950-U+1E959\rU+1F101-U+1F10A\rU+1FBF0-U+1FBF9\r\rU+0F2A-U+0F32\rU+1369-U+1371\rU+2460-U+2468\rU+2488-U+2490\rU+24F5-U+24FD\rU+2776-U+277E\rU+278A-U+2792\rU+102E1-U+102E9\rU+10E60-U+10E68\rU+111E1-U+111E9\rU+1D360-U+1D368\rU+1E8C7-U+1E8CF\r",
    "Single-script confusable": "ǉeto ljeto",       # Croatian word for “summer”
    "Mixed-script confusable": "paypal pаypаl",     # Cyrillic a in 2nd form
    "Whole-script confusable": "scope ѕсоре",       # Full Cyrillic for 2nd form
}

# Main form
root = tk.Tk()
root.title('UniView Python Tk')

# Variables used ahead of definition
inputText: ScrolledText
resultText: ScrolledText
case = tk.IntVar(root, 0)
norm = tk.StringVar(root, 'None')
seq = tk.StringVar(root, 'CN')
countChars = tk.StringVar(root, "")

# Fonts used
lf = ('Segoe UI Semibold', 10)              # Label Font
tf = ('Segoe UI', 11)                       # Text Font
ff = ('Iosevka', 10)                        # Fixed font

# Callbacks on options
case.trace_add('write', lambda *args: Transform())
norm.trace_add('write', lambda *args: Transform())
seq.trace_add('write', lambda *args: Transform())

# Main processing after input text change or transformation option change
def Transform() -> None:
    s: str = inputText.get('1.0', 'end-1c')

    def ReplaceRange(ma: Match[str]) -> str:
        cpFrom = int(ma.group(1), 16)
        cpTo = int(ma.group(3), 16)
        if cpFrom > UniData.MAXCODEPOINT or cpTo > UniData.MAXCODEPOINT:
            return "[Codepoint limit is 10FFFF]"
        if cpTo - cpFrom >= 0x1000:
            return "[Range limited to 4K codepoints]"
        if cpTo < cpFrom:
            return "[Range from>to]"
        sb = ''
        insertCr = ma.group(2) == '..'
        for cp in range(cpFrom, cpTo+1):
            if (not UniData.IsSurrogate(cp)):   # For ranges, we silently ignore surrogates to enable large ranges including U+D800..U+DFFF
                sb += UniData.AsString(cp)
                if insertCr and cp % 32 == 31:
                    sb += '\r'
        return sb

    def ReplaceCodepoint(ma: Match[str]) -> str:
        cp = int(ma.group(1), 16)
        if cp > UniData.MAXCODEPOINT:
            return "[Codepoint limit is 10FFFF]"
        if UniData.IsSurrogate(cp):
            return "[Invalid codepoint in surrogates range D800..DFFFF]"
        return UniData.AsString(cp)

    def ReplaceName(ma: Match[str]) -> str:
        cp = UniData.GetCodepointFromName(ma.group(1))
        if cp>=0:
            return UniData.AsString(cp)
        return ma.group()

    s = re.sub(RANGEUPLUSXBRACES_RE, ReplaceRange, s)
    s = re.sub(RANGEUPLUSX_RE, ReplaceRange, s)

    s = re.sub(UPLUSXBRACES_RE, ReplaceCodepoint, s)
    s = re.sub(UPLUSX_RE, ReplaceCodepoint, s)

    # Replace {semicolon} by ;
    s = re.sub(NAME_RE, ReplaceName, s)

    if seq.get() == 'CN':
        if case.get() == 1:
            s = s.lower()
        elif case.get() == 2:
            s = s.upper()

    if norm.get() != 'None':
        s = unicodedata.normalize(norm.get(), s)        # type: ignore

    if seq.get() == 'NC':
        if case.get() == 1:
            s = s.lower()
        elif case.get() == 2:
            s = s.upper()

    resultText.delete('1.0', 'end')
    resultText.insert('1.0', s)

    countChars.set(f"{len(s)}")

    # Update treeview
    tree.delete(*tree.get_children())
    # adjust the column's width to the header string since adding data will only make it grow
    for col in list_headers:
        tree.column(col, width=tkFont.Font().measure(col.title()))
    for ch in s:
        c = ord(ch)
        item = (f'U+{c:04X}', UniData.GetName(c), UniData.GetScript(c),
                UniData.GetCategory(c), UniData.GetUTF16String(c), UniData.GetUTF8String(c))
        tree.insert('', 'end', values=item)
        # adjust column's width if necessary to fit each value
        for ix, val in enumerate(item):
            col_w = int(tkFont.Font().measure(val)*0.8) # No idea why I've to multiply by 0.8 else it's way too large
            if tree.column(list_headers[ix], width=None) < col_w:   # type: ignore
                tree.column(list_headers[ix], width=col_w)


# Events processing

# Support to implement inputText modification handler
ignore_event = False

def inputText_Modified(t: ScrolledText):
    global ignore_event
    if ignore_event:
        ignore_event = False
        return
    Transform()
    ignore_event = True
    t.edit_modified(False)

def Treeview_Select(evt):
    #w = evt.widget
    for selection in tree.selection():
        item = tree.item(selection)
        values = item["values"]
        print('Selected:', values)

def SamplesCombobox_Select(evt) -> None:
    key:str = SamplesCombobox.get()
    sample = SamplesCollection[key]
    inputText.delete('1.0', 'end')
    inputText.insert('1.0', sample)

def about_click():
    s = APP_TITLE + " version " + APP_VERSION + "\r\n" + APP_DESCRIPTION + "\r\n\n" + APP_PRODUCT + "\r\n" + APP_COPYRIGHT
    s += "\n\nUnicode Data: " + UniData.UnicodeVersion
    tkmb.showinfo("About "+APP_TITLE, s)


# Helper for Treeview
def sortby(tree, col, descending):
    """sort tree contents when a column header is clicked on"""
    # grab values to sort
    data = [(tree.set(child, col), child) for child in tree.get_children('')]
    # if the data to be sorted is numeric change to float
    if col == 'index':
        # Change data to numeric for some columns
        data2 = []
        for (value, internalkey) in data:
            data2.append((float(value), internalkey))
        data = data2
    # now sort the data in place
    data.sort(reverse=descending)
    for ix, item in enumerate(data):
        tree.move(item[1], '', ix)
    # switch the heading so it will sort in the opposite direction
    tree.heading(col, command=lambda col=col: sortby(tree, col, int(not descending)))

# Build form
tk.Label(root, text='Text', font=lf).grid(row=0, column=0, sticky='nw', padx=6, pady=6)
inputFrame = tk.Frame(root, padx=6, pady=6)
inputFrame.grid(row=0, column=1, sticky='we')
inputText = ScrolledText(inputFrame, height=4, width=80, wrap=tk.WORD, font=tf)
inputText.pack(side=tk.TOP, fill=tk.X)
inputText.bind('<<Modified>>', lambda event: inputText_Modified(inputText))
ttk.Label(inputFrame, text='Text samples', font=lf).pack(side=tk.LEFT, pady=6)
SamplesCombobox = ttk.Combobox(inputFrame, width=30)
SamplesCombobox.pack(side=tk.LEFT, padx=6, pady=6)
SamplesCombobox['values'] = list(SamplesCollection.keys()) 
SamplesCombobox.bind("<<ComboboxSelected>>", SamplesCombobox_Select)
SamplesCombobox.config(state='readonly')

buttonsFrame = tk.Frame(root, padx=6, pady=6)
buttonsFrame.grid(row=0, column=2, sticky='n')
aboutButton = ttk.Button(buttonsFrame, text='About', command=about_click)
aboutButton.pack(fill=tk.X)
root.bind('<F1>', lambda e: about_click())
aboutButton_ttp = TkToolTip(aboutButton, '[F1] Show application information')
ttk.Button(buttonsFrame, text='Close', command=lambda: tk.Frame.quit(root)).pack(fill=tk.X, pady=6)     # type: ignore

tk.Label(root, text='Transformations', font=lf).grid(row=1, column=0, sticky='nw', padx=6, pady=6)
optionsFrame = tk.Frame(root, padx=6, pady=6)
optionsFrame.grid(row=1, column=1, sticky='nw')
caseFrame = tk.LabelFrame(optionsFrame, text='Case', font=lf)
caseFrame.pack(side=tk.LEFT, anchor='n')
ttk.Radiobutton(caseFrame, text='Lowercase', variable=case, value=1).pack(anchor='w')
ttk.Radiobutton(caseFrame, text='Uppercase', variable=case, value=2).pack(anchor='w')
ttk.Radiobutton(caseFrame, text='None', variable=case, value=0).pack(anchor='w')

normFrame = tk.LabelFrame(optionsFrame, text='Normalization', font=lf)
normFrame.pack(side=tk.LEFT, anchor='n', padx=(6,0))
nfcOption = ttk.Radiobutton(normFrame, text='NFC', variable=norm, value='NFC')
TkToolTip(nfcOption, 'NFC: Normal Form C, canonical decomposition then canonical composition')
nfcOption.pack(anchor='w')
nfdOption = ttk.Radiobutton(normFrame, text='NFD', variable=norm, value='NFD')
TkToolTip(nfdOption, 'NFD: Normal Form D, canonical decomposition')
nfdOption.pack(anchor='w')
nfkcOption = ttk.Radiobutton(normFrame, text='NFKC', variable=norm, value='NFKC')
TkToolTip(nfkcOption, 'NFKC: Normal Form KC, compatibility decomposition then canonical composition')
nfkcOption.pack(anchor='w')
nfkdOption = ttk.Radiobutton(normFrame, text='NFKD', variable=norm, value='NFKD')
TkToolTip(nfkdOption, 'NFKD: Normal Form KD, compatibility decomposition')
nfkdOption.pack(anchor='w')
ttk.Radiobutton(normFrame, text='None', variable=norm, value='None').pack(anchor='w')

seqFrame = tk.LabelFrame(optionsFrame, text='Sequence', font=lf)
seqFrame.pack(side=tk.LEFT, anchor='n', padx=(6,0))
ttk.Radiobutton(seqFrame, text='Case then Normalization', variable=seq, value='CN').pack(anchor='w')
ttk.Radiobutton(seqFrame, text='Normalization then Case', variable=seq, value='NC').pack(anchor='w')

tk.Label(root, text='Result', font=lf).grid(row=2, column=0, sticky='nw', padx=6, pady=6)
resultText = ScrolledText(root, height=4, width=20, wrap=tk.WORD, font=tf)
resultText.grid(row=2, column=1, sticky='we', padx=6, pady=6)

tk.Label(root, text='Counts', font=lf).grid(row=3, column=0, sticky='nw', padx=6, pady=6)
countsFrame = tk.Frame(root, padx=6, pady=6)
countsFrame.grid(row=3, column=1, sticky='nw')
tk.Label(countsFrame, text='Chars', font=lf).pack(side=tk.LEFT, anchor='s')
tk.Label(countsFrame, textvariable=countChars, font=tf).pack(side=tk.LEFT, anchor='s')

tk.Label(root, text='Codepoints', font=lf).grid(row=4, column=0, sticky='nw', padx=6, pady=6)
container = tk.Frame(root, padx=6, pady=6)
container.grid(row=4, column=1, sticky='nsew')
# create a Treeview with dual scrollbars
tree = ttk.Treeview(container, columns=list_headers, show="headings")
tree.pack(expand=1)
tree.bind('<<TreeviewSelect>>', Treeview_Select)
vsb = ttk.Scrollbar(orient="vertical", command=tree.yview)
hsb = ttk.Scrollbar(orient="horizontal", command=tree.xview)
tree.configure(yscrollcommand=vsb.set, xscrollcommand=hsb.set)
tree.grid(column=0, row=0, sticky='nsew', in_=container)
vsb.grid(column=1, row=0, sticky='ns', in_=container)
hsb.grid(column=0, row=1, sticky='ew', in_=container)
container.grid_columnconfigure(0, weight=1)
container.grid_rowconfigure(0, weight=1)

# Build tree
for col in list_headers:
    tree.heading(col, text=col.title(), command=lambda c=col: sortby(tree, c, 0))   # type: ignore
    # adjust the column's width to the header string
    tree.column(col, width=tkFont.Font().measure(col.title()))

# Define row and colum that will automatically resize when form is resized
root.columnconfigure(1, weight=1)
root.rowconfigure(4, weight=1)

# Initial focus
inputText.focus()


# Init form values

# Emoji
# 🐗  Boar: U+1F417, UTF-8: 0xF0 0x9F 0x90 0x97, UTF-16: 0xD83D 0xDC17, UTF-32: 0x0001F417
# 🧔  Bearded Person: U+1F9D4
# 🧔🏻  Bearded Person+Light Skin Tone: U+1F9D4 U+1F3FB
# 🧝  Elf: U+1F9DD
# 🧝‍♂️  Man Elf: U+1F9DD(🧝) U+200D(ZWJ) U+2642(♂) U+FE0F(VS-16)
# 🧝‍♀️  Woman Elf:  U+1F9DD(🧝) U+200D(ZWJ) U+2640(♀) U+FE0F(VS-16)
# 🧝🏽  Elf: Medium Skin Tone, U+1F9DD (🧝) U+1F3FD (🏽)
# 🧝🏽‍♂️  Man Elf: Medium Skin Tone, U+1F9DD (🧝) U+1F3FD (🏽) U+200D(ZWJ) U+2642(♂) U+FE0F(VS-16)
# 🧝🏽‍♀️  Woman Elf: Medium Skin Tone U+1F9DD (🧝) U+1F3FD (🏽) U+200D(ZWJ) U+2640(♀) U+FE0F(VS-16)

# Can't use chars beyond BMP such as 𝄞 or 🐗: character U+1xxxx is above the range (U+0000-U+FFFF) allowed by Tcl
#inputText.insert('1.0', 'Aé♫山\r\nœæĳøß≤≠Ⅷﬁﬆ')
# 2020-09-03 update: Now characters beyond U+1xxxx are supported!!!   Complex emoji still are not...
#inputText.insert('1.0', 'Aé♫山𝄞🐗\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️')
inputText.insert('1.0', 'Aé♫山𝄞🐗🐻‍❄️\r\nœæĳøß≤≠Ⅷﬁﬆ\r\n🐱‍🏍 🐱‍👓 🐱‍🚀 🐱‍👤 🐱‍🐉 🐱‍💻\r\n🧝 🧝‍♂️ 🧝‍♀️ 🧝🏽 🧝🏽‍♂️ 🧝🏽‍♀️')

root.mainloop()
