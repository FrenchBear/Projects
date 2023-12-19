# puzzle.py - Class to handle a CrossPath puzzle and its resolution
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

        # Check that count of empty cells equals total of extensions
        ns = [e for r in self.p for e in r].count(".")
        te = sum([int(e) if e>="1" and e<="9" else 0 for r in self.p for e in r])
        assert ns==te


    def __str__(self):
        return '\r\n'.join(' '.join(self.dicmap[c] for c in l) for l in self.p)

    def __repr__(self):
        return self.__str__()

    def GetExtensionsToCell(self, rTarget, cTarget):
        assert self.p[rTarget][cTarget]=="."
        l = []

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


    def ExploreExt(self, rSource, cSource, rDelta, cDelta, extension):
        vuse = 0
        vnum = int(self.p[rSource][cSource])
        r,c = rSource,cSource
        while vnum>0:
            r += rDelta
            c += cDelta
            if r>=self.rows or r<0 or c>=self.columns or c<0: break
            if self.p[r][c]!="." and self.p[r][c]!=extension: break
            if self.p[r][c]==".": 
                vuse += 1
                vnum -= 1
        if vuse>0:
            return (rDelta, cDelta, extension, vuse)
        else:
            return None

    def GetExtensionsFromCell(self, rSource, cSource):
        v = self.p[rSource][cSource]
        assert v>="0" and v<="9"
        l = []

        # Look in all 4 directions
        ext = self.ExploreExt(rSource, cSource, -1, 0, "N")
        if ext: l.append(ext)
        ext = self.ExploreExt(rSource, cSource, 1, 0, "S")
        if ext: l.append(ext)
        ext = self.ExploreExt(rSource, cSource, 0, -1, "W")
        if ext: l.append(ext)
        ext = self.ExploreExt(rSource, cSource, 0, 1, "E")
        if ext: l.append(ext)
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
                            # Only one extension possible to cell (r,c): do it
                            if showSteps:
                                print("To ", (r, c), ": ", l[0])
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

                    if self.p[r][c]>="1" and self.p[r][c]<="9":
                        l = self.GetExtensionsFromCell(r, c)
                        if len(l)==1:
                            # Only one extension direction possible that exhaust remaining count: do it
                            if showSteps:
                                print("From ", (r, c), ": ", l[0])
                            rDelta = l[0][0]
                            cDelta = l[0][1]
                            extChar = l[0][2]
                            vuse = l[0][3]
                            assert int(self.p[r][c])==vuse
                            r1,c1 = r,c
                            while vuse>0:
                                r1 += rDelta
                                c1 += cDelta
                                if r>=self.rows or r<0 or c>=self.columns or c<0: break
                                if self.p[r1][c1]!="." and self.p[r1][c1]!=extChar: break
                                if self.p[r1][c1]==".": 
                                    vuse -= 1
                                    self.p[r1][c1] = extChar
                            assert vuse==0
                            self.p[r][c] = "0"
                            if showSteps:
                                print(str(self))
                                print()
                            doAgain = True
                            break

                if doAgain: break