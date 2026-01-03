# Json parser using lark
# https://lark-parser.readthedocs.io/en/stable/json_tutorial.html
#
# 2025-12-31    PV

from lark import Lark, Transformer

json_parser = Lark(r"""
    ?value: dict
          | list
          | string
          | SIGNED_NUMBER      -> number
          | "true"             -> true
          | "false"            -> false
          | "null"             -> null

    list : "[" [value ("," value)*] "]"

    dict : "{" [pair ("," pair)*] "}"
    pair : string ":" value

    string : ESCAPED_STRING

    %import common.ESCAPED_STRING
    %import common.SIGNED_NUMBER
    %import common.WS
    %ignore WS

    """, start='value', parser='lalr')

text = '{"key": ["item0", "item1", true, 3.14]}'

tree = json_parser.parse(text)
print(tree.pretty())
print()

class TreeToJson(Transformer):
    def string(self, s):
        (s,) = s
        return s[1:-1]
    def number(self, n):
        (n,) = n
        return float(n)

    list = list
    pair = tuple
    dict = dict

    #true = lambda self, _: True
    def true(self, _):
        return True

    null = lambda self, _: None
    false = lambda self, _: False

j = TreeToJson().transform(tree)
print(j)
