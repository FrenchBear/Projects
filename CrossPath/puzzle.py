# puzzle.py - Class to handle a crosspath puzzle and its resolution
# Internally puzzle is handled as a list of list of characters in p
# 2017-09-14    PV

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

        # Pretty output for str()
        self.dicmap = { 'N':'▲', 'S':'▼', 'E':'►', 'W':'◄', '.':'·', 'X':'▒', '0':'◌'}
        self.dicmap.update({str(n):str(n) for n in range(1, 10)})

    def __str__(self):
        return '\r\n'.join(' '.join(self.dicmap[c] for c in l) for l in self.p)

    def __repr__(self):
        return self.__str__()

    def GetExtensionsToCell(self, rTarget, cTarget):
        assert self.p[rTarget][cTarget]=="."
        l = []

        trace = rTarget==0 and cTarget==3

        # search for horizontal extensions
        for c in range(self.columns):
            v = self.p[rTarget][c] 
            if v>="1" and v<="9":
                vnum = int(v)
                if c<cTarget:
                    delta = 1
                    previousExtension="E"
                else:
                    delta = -1
                    previousExtension="W"
                c1 = c
                while vnum>0:
                    c1 += delta
                    if c1==cTarget:
                        l.append((rTarget, c, 0, delta, previousExtension))
                        break
                    if c1>=self.columns: break
                    if self.p[rTarget][c1]!="." and self.p[rTarget][c1]!=previousExtension: break
                    if self.p[rTarget][c1]==".": vnum -= 1

        # search for vertical extensions
        for r in range(self.rows):
            v = self.p[r][cTarget] 
            if v>="1" and v<="9":
                vnum = int(v)
                if r<rTarget:
                    delta = 1
                    previousExtension="S"
                else:
                    delta = -1
                    previousExtension="N"
                r1 = r
                while vnum>0:
                    r1 += delta
                    if r1==rTarget:
                        l.append((r, cTarget, delta, 0, previousExtension))
                        break
                    if r1>=self.rows: break
                    if self.p[r1][cTarget]!="." and self.p[r1][cTarget]!=previousExtension: break
                    if self.p[r1][cTarget]==".": vnum -= 1
            
        return l


    def solve(self, showSteps):
        doAgain = True
        while doAgain:
            doAgain = False
            for r in range(self.rows):
                for c in range(self.columns):
                    if self.p[r][c]==".":
                        l = self.GetExtensionsToCell(r, c)
                        if len(l)==1:
                            # Apply extension l[0]
                            if showSteps:
                                print((r, c), ": ", l[0])
                            rSource = l[0][0]
                            cSource = l[0][1]
                            rDelta = l[0][2]
                            cDelta = l[0][3]
                            extChar = l[0][4]
                            v = int(self.p[rSource][cSource])
                            while True:
                                rSource += rDelta
                                cSource += cDelta
                                if self.p[rSource][cSource]==".":
                                    self.p[rSource][cSource] = extChar
                                    v -= 1
                                if rSource==r and cSource==c:
                                    rSource = l[0][0]
                                    cSource = l[0][1]
                                    self.p[rSource][cSource] = str(v)
                                    break
                            if showSteps:
                                print(str(self))
                                print()
                            doAgain = True
                            break
                if doAgain: break