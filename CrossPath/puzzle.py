# puzzle.py - Class to handle a crosspath puzzle and its resolution
# Internally puzzle is handled as a list of list of characters in p

"""
Cell values:
1-9	Extension center 
0	Exhausted extension center
.	Free
N	North extension (up)
S	South extension (down)
W	West extension (left)
E	East extension (right)
X	Blocked cell

Stratégie
- Tant qu'il existe des cellules joignables par une seule extension, mettre en oeuvre cette extension
- S'il existe des cellules extensibles d'une seule manière, mettre en oeuvre cette extension
(Ne pas tenir compte des extensions qui pourraient poser problème)
"""

class puzzle(object):
    """description of class"""

    def __init__(self, ts):
        self.p = [list(s) for s in ts]
        self.rows = len(ts)
        self.columns = len(ts[0])

    def __str__(self):
        return '\r\n'.join(' '.join(c for c in l) for l in self.p)

    def __repr__(self):
        return self.__str__()

    def solve(self):
        for r in range(rows):
            for c in range(columns):
                if self.p[r][c]=="."
