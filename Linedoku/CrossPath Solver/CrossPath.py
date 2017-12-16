# CrossPath solver
# 2017-09-14    PV
# 2017-11-12    PV      Added TestExploreAll for brute force exploration part (to come)

from puzzle import *

# Les puzzles sont définis comme une liste de chaînes, une par ligne, chaque
# caractère de la chaîne étant:
# 1-9 Centre de réserve d’extension
# .  libre
# X cellule bloquée
medium_10 = [".6....2",
".....X.",
"..1.4..",
"...4.X.",
"3.4....",
".4....4",
"X...3.X"]


master_1 = ["...5....XXX",
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

test_4 = ["2.X1",
"..X.",
".1.2",
"3...",]

medium_36 = ["X.1.X..",
".2...3.",
"..2.X.4",
"...7...",
".XX.2..",
"3.....5",
".3...1.",]

def TestExploreAll():
    N = 3
    S = 0
    E = 4
    W = 2
    c = 4
    nl = 0
    ns = 0
    # Find all moves combinations that use exactly c points
    for iN in range(1 + min(N, c)):
        for iS in range(1 + min(c - iN, S)):
            for iE in range(1 + min(c - iN - iS, E)):
                for iW in range(1 + min(c - iN - iS - iE, W)):
                    nl += 1
                    if iN + iS + iE + iW == c:
                        ns += 1
                        print('N%d S%d E%d W%d' % (iN, iS, iE, iW))
    print(nl,' loops, ',ns, ' solutions')
#TestExploreAll()

#p = puzzle(medium_10)
#p = puzzle(master_1)
#p = puzzle(test_4)
p = puzzle(medium_36)

print(str(p))
print()
p.solve(False)
print(str(p))
