# Talos messages parser using lark
#
# 2025-12-31    PV

from lark import Lark
from lark.visitors import Interpreter
from talos_decoder import talos_decode

with open('talos.lark', 'r', encoding='utf-8-sig') as g:
    grammar = g.read()

json_parser = Lark(grammar, start='start', parser='lalr')

with open('FoundTexts.txt', 'r', encoding='utf-8-sig') as f:
    text = f.read()

tree = json_parser.parse(text)

class MyTopDownVisitor(Interpreter):
    def blockname(self, tree):
        file = tree.children[0].value
        print('-'*50)
        print(file)
        print()

    def body(self, tree):
        talos_decode(tree.children[0].value)

MyTopDownVisitor().visit(tree)
