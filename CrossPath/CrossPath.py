# CrossPath solver
# 2017-09-14    PV

from puzzle import *

"""
.6....2
.....X.
..1.4..
...4.X.
3.4....
.4....4
X...3.X

Cellule
1-9	Centre de réserve d’extension
.	libre
X	cellule bloquée
"""

medium_10 = [
".6....2",
".....X.",
"..1.4..",
"...4.X.",
"3.4....",
".4....4",
"X...3.X"]


master_1 = [
"...5....XXX",
"..6.....3..",
"2..1.X.....",
".2....3..3.",
"X...5..6X.3",
"3....2...5.",
".XX.X.5...X",
"...9...XX..",
"4...3..3...",
".X...X...1.",
"..2X.XX...6"]


#p = puzzle(medium_10)
p = puzzle(master_1)

print(str(p))
print()
p.solve(False)
print(str(p))

