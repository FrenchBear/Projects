# CrooPath solver
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
0	Centre de réserve d’extension épuisé
.	libre
N	extension vers le haut (Nord)
S	extension vers le bas (Sud)
W	extension vers la gauche (West)
E	extension vers la droite (Est)
X	cellule bloquée

Stratégie
- Tant qu'il existe des cellules joignables par une seule extension, mettre en oeuvre cette extension
- S'il existe des cellules extensibles d'une seule manière, mettre en oeuvre cette extension
(Ne pas tenir compte des extensions qui pourraient poser problème)
"""

p = puzzle([
".6....2",
".....X.",
"..1.4..",
"...4.X.",
"3.4....",
".4....4",
"X...3.X"])

print(str(p))
print()
p.solve()
print(str(p))

