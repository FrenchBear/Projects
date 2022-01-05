import matplotlib.image as mpimg
import matplotlib.pyplot as plt
import numpy as np
import math
import os
import sys
from dataclasses import dataclass
from typing import List, Optional


yellow = np.array([251., 211., 19.])
(rowmin, rowmax) = (2680, 2800)
(colminpair, colmaxpair) = (130, 330)
(colminimpair, colmaximpair) = (1640, 1820)

rowpmax = 0
margebmax = 0
colppairmax = 0
colpimpairmax = 0
margedmax = 0
margegmax = 0


@dataclass
class ScannedPage:
    file: str
    numfile: int
    width: int
    height: int
    colp: Optional[int]
    rowp: Optional[int]

    def is_pair(self) -> bool:
        return self.numfile % 2 == 0

    def is_impair(self) -> bool:
        return self.numfile % 2 == 1


Pages: List[ScannedPage] = []


def dotdist(p1, p2):
    return np.linalg.norm(p2-p1)


def veclength(p):
    return math.sqrt(p[0]**2+p[1]**2+p[2]**2)


def process(file: str, numfile: int):
    print(file, ';', numfile, ';', sep='', end='')

    img = mpimg.imread(file)[:,:,:3]
    width: int = img.shape[1]
    height: int = img.shape[0]
    print(width, ';', height, ';', sep='', end='')

    if numfile % 2 == 0:
        (colmin, colmax) = (colminpair, colmaxpair)
    else:
        (colmin, colmax) = (colminimpair, colmaximpair)

    area = img[rowmin:rowmax, colmin:colmax, :]
    areaheight: int = area.shape[0]
    areawidth: int = area.shape[1]

    area = area-yellow
    area = area.reshape(areawidth*areaheight, 3)
    area = np.apply_along_axis(veclength, 1, area)
    area = area.reshape(areaheight, areawidth)

    colp: Optional[int] = None
    rowp: Optional[int] = None

    #if 4<numfile<168 and numfile!=5 and numfile!=45 and numfile!=133:
    if numfile % 2 == 0:
        r = range(0, areawidth)
    else:
        r = range(areawidth-1, -1, -1)
    for col in r:
        n = (area[:, col] <= 50).sum()
        if n >= 30:
            colp = col+colmin
            break

    for row in range(areaheight-1, 0, -1):
        n = (area[row, :] <= 50).sum()
        if n >= 30:
            rowp = row+rowmin
            break

    print(colp, ';', rowp, sep='')

    newpage = ScannedPage(file, numfile, width, height, colp, rowp)
    global Pages
    Pages.append(newpage)

    global rowpmax, margebmax
    if rowp:
        if rowp > rowpmax:
            rowpmax = rowp
        if height-rowp > margebmax:
            margebmax = height-rowp

    global colppairmax, colpimpairmax, margedmax, margegmax
    if colp:
        if numfile % 2 == 0:
            if colp > colppairmax:
                colppairmax = colp
        else:
            if colp > colpimpairmax:
                colpimpairmax = colp
            if width-colp > margedmax:
                margedmax = width-colp


def files1(path):
    for root, subs, files in os.walk(path):
        for file in files:
            yield os.path.join(root, file)

# Step 1, determine sizes and position of yellow rect
source=r'D:\Scans\THS76\02Redresse'
dest=r'D:\Scans\THS76\03X'
prefix='THS76'

for pathfile in files1(source):
    path, file = os.path.split(pathfile)
    basename, ext = os.path.splitext(file)
    if ext.lower() == '.jpg':
        numfile = int(basename[-3:])
        process(pathfile, numfile)
print()
print('rowpmax=', repr(rowpmax))
print('colppairmax=', repr(colppairmax))
print('colpimpairmax=', repr(colpimpairmax))
print('margedmax=', repr(margedmax))
print()
for page in Pages:
    print("Pages.append("+repr(page)+")")
sys.exit(0)


"""
# Precomputed in step 1
rowpmax= 5533
colppairmax= 379
colpimpairmax= 3565
margedmax= 336

Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-001.png', numfile=1, width=3809, height=5616, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-002.png', numfile=2, width=3746, height=5616, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-003.png', numfile=3, width=3711, height=5616, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-004.png', numfile=4, width=3680, height=5616, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-005.png', numfile=5, width=3854, height=5616, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-006.png', numfile=6, width=3746, height=5616, colp=218, rowp=5487))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-007.png', numfile=7, width=3854, height=5616, colp=3544, rowp=5504))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-008.png', numfile=8, width=3746, height=5616, colp=200, rowp=5510))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-009.png', numfile=9, width=3854, height=5616, colp=3534, rowp=5512))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-010.png', numfile=10, width=3746, height=5616, colp=200, rowp=5496))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-011.png', numfile=11, width=3854, height=5616, colp=3565, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-012.png', numfile=12, width=3746, height=5616, colp=200, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-013.png', numfile=13, width=3711, height=5616, colp=3519, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-014.png', numfile=14, width=3746, height=5616, colp=200, rowp=5499))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-015.png', numfile=15, width=3854, height=5616, colp=3551, rowp=5508))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-016.png', numfile=16, width=3746, height=5616, colp=212, rowp=5508))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-017.png', numfile=17, width=3727, height=5632, colp=3511, rowp=5515))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-018.png', numfile=18, width=3762, height=5632, colp=250, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-019.png', numfile=19, width=3727, height=5632, colp=3518, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-020.png', numfile=20, width=3762, height=5632, colp=218, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-021.png', numfile=21, width=3727, height=5632, colp=3504, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-022.png', numfile=22, width=3762, height=5632, colp=252, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-023.png', numfile=23, width=3727, height=5632, colp=3500, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-024.png', numfile=24, width=3762, height=5632, colp=225, rowp=5514))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-025.png', numfile=25, width=3727, height=5632, colp=3496, rowp=5515))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-026.png', numfile=26, width=3762, height=5632, colp=242, rowp=5506))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-027.png', numfile=27, width=3727, height=5632, colp=3491, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-028.png', numfile=28, width=3762, height=5632, colp=257, rowp=5515))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-029.png', numfile=29, width=3727, height=5632, colp=3483, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-030.png', numfile=30, width=3762, height=5632, colp=277, rowp=5511))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-031.png', numfile=31, width=3727, height=5632, colp=3486, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-032.png', numfile=32, width=3762, height=5632, colp=252, rowp=5510))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-033.png', numfile=33, width=3727, height=5632, colp=3474, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-034.png', numfile=34, width=3762, height=5632, colp=290, rowp=5504))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-035.png', numfile=35, width=3727, height=5632, colp=3473, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-036.png', numfile=36, width=3762, height=5632, colp=267, rowp=5506))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-037.png', numfile=37, width=3727, height=5632, colp=3473, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-038.png', numfile=38, width=3762, height=5632, colp=288, rowp=5506))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-039.png', numfile=39, width=3727, height=5632, colp=3474, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-040.png', numfile=40, width=3762, height=5632, colp=276, rowp=5511))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-041.png', numfile=41, width=3727, height=5632, colp=3462, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-042.png', numfile=42, width=3762, height=5632, colp=294, rowp=5505))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-043.png', numfile=43, width=3727, height=5632, colp=3469, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-044.png', numfile=44, width=3762, height=5632, colp=281, rowp=5503))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-045.png', numfile=45, width=3727, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-046.png', numfile=46, width=3762, height=5632, colp=293, rowp=5505))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-047.png', numfile=47, width=3727, height=5632, colp=3461, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-048.png', numfile=48, width=3762, height=5632, colp=365, rowp=5502))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-049.png', numfile=49, width=3727, height=5632, colp=3449, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-050.png', numfile=50, width=3762, height=5632, colp=369, rowp=5505))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-051.png', numfile=51, width=3727, height=5632, colp=3455, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-052.png', numfile=52, width=3762, height=5632, colp=291, rowp=5510))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-053.png', numfile=53, width=3708, height=5632, colp=3451, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-054.png', numfile=54, width=3762, height=5632, colp=318, rowp=5513))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-055.png', numfile=55, width=3727, height=5632, colp=3450, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-056.png', numfile=56, width=3806, height=5632, colp=298, rowp=5507))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-057.png', numfile=57, width=3727, height=5632, colp=3450, rowp=5522))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-058.png', numfile=58, width=3786, height=5632, colp=323, rowp=5496))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-059.png', numfile=59, width=3727, height=5632, colp=3447, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-060.png', numfile=60, width=3762, height=5632, colp=296, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-061.png', numfile=61, width=3727, height=5632, colp=3446, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-062.png', numfile=62, width=3762, height=5632, colp=322, rowp=5491))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-063.png', numfile=63, width=3727, height=5632, colp=3446, rowp=5511))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-064.png', numfile=64, width=3762, height=5632, colp=310, rowp=5494))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-065.png', numfile=65, width=3727, height=5632, colp=3437, rowp=5514))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-066.png', numfile=66, width=3762, height=5632, colp=328, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-067.png', numfile=67, width=3727, height=5632, colp=3436, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-068.png', numfile=68, width=3762, height=5632, colp=307, rowp=5512))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-069.png', numfile=69, width=3727, height=5632, colp=3426, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-070.png', numfile=70, width=3762, height=5632, colp=335, rowp=5500))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-071.png', numfile=71, width=3727, height=5632, colp=3437, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-072.png', numfile=72, width=3762, height=5632, colp=326, rowp=5492))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-073.png', numfile=73, width=3727, height=5632, colp=3433, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-074.png', numfile=74, width=3762, height=5632, colp=341, rowp=5494))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-075.png', numfile=75, width=3727, height=5632, colp=3428, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-076.png', numfile=76, width=3762, height=5632, colp=310, rowp=5507))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-077.png', numfile=77, width=3727, height=5632, colp=3427, rowp=5504))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-078.png', numfile=78, width=3762, height=5632, colp=335, rowp=5503))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-079.png', numfile=79, width=3727, height=5632, colp=3425, rowp=5526))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-080.png', numfile=80, width=3762, height=5632, colp=320, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-081.png', numfile=81, width=3727, height=5632, colp=3422, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-082.png', numfile=82, width=3762, height=5632, colp=337, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-083.png', numfile=83, width=3727, height=5632, colp=3422, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-084.png', numfile=84, width=3762, height=5632, colp=320, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-085.png', numfile=85, width=3744, height=5632, colp=3433, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-086.png', numfile=86, width=3797, height=5632, colp=341, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-087.png', numfile=87, width=3754, height=5632, colp=3435, rowp=5529))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-088.png', numfile=88, width=3769, height=5632, colp=328, rowp=5528))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-089.png', numfile=89, width=3723, height=5632, colp=3416, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-090.png', numfile=90, width=3798, height=5632, colp=354, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-091.png', numfile=91, width=3755, height=5632, colp=3419, rowp=5522))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-092.png', numfile=92, width=3789, height=5632, colp=299, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-093.png', numfile=93, width=3729, height=5632, colp=3456, rowp=5507))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-094.png', numfile=94, width=3832, height=5632, colp=379, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-095.png', numfile=95, width=3727, height=5632, colp=3439, rowp=5526))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-096.png', numfile=96, width=3762, height=5632, colp=306, rowp=5528))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-097.png', numfile=97, width=3727, height=5632, colp=3435, rowp=5517))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-098.png', numfile=98, width=3762, height=5632, colp=320, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-099.png', numfile=99, width=3727, height=5632, colp=3429, rowp=5531))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-100.png', numfile=100, width=3762, height=5632, colp=311, rowp=5530))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-101.png', numfile=101, width=3727, height=5632, colp=3425, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-102.png', numfile=102, width=3806, height=5632, colp=376, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-103.png', numfile=103, width=3727, height=5632, colp=3422, rowp=5530))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-104.png', numfile=104, width=3762, height=5632, colp=314, rowp=5532))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-105.png', numfile=105, width=3727, height=5632, colp=3419, rowp=5525))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-106.png', numfile=106, width=3762, height=5632, colp=327, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-107.png', numfile=107, width=3727, height=5632, colp=3422, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-108.png', numfile=108, width=3798, height=5632, colp=311, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-109.png', numfile=109, width=3727, height=5632, colp=3417, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-110.png', numfile=110, width=3762, height=5632, colp=336, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-111.png', numfile=111, width=3727, height=5632, colp=3425, rowp=5532))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-112.png', numfile=112, width=3762, height=5632, colp=314, rowp=5532))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-113.png', numfile=113, width=3743, height=5632, colp=3433, rowp=5522))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-114.png', numfile=114, width=3778, height=5632, colp=338, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-115.png', numfile=115, width=3743, height=5632, colp=3445, rowp=5526))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-116.png', numfile=116, width=3778, height=5632, colp=318, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-117.png', numfile=117, width=3743, height=5632, colp=3443, rowp=5513))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-118.png', numfile=118, width=3778, height=5632, colp=334, rowp=5515))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-119.png', numfile=119, width=3743, height=5632, colp=3444, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-120.png', numfile=120, width=3778, height=5632, colp=323, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-121.png', numfile=121, width=3743, height=5632, colp=3440, rowp=5522))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-122.png', numfile=122, width=3778, height=5632, colp=344, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-123.png', numfile=123, width=3743, height=5632, colp=3439, rowp=5529))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-124.png', numfile=124, width=3778, height=5632, colp=317, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-125.png', numfile=125, width=3743, height=5632, colp=3449, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-126.png', numfile=126, width=3778, height=5632, colp=338, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-127.png', numfile=127, width=3743, height=5632, colp=3448, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-128.png', numfile=128, width=3778, height=5632, colp=321, rowp=5508))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-129.png', numfile=129, width=3743, height=5632, colp=3451, rowp=5510))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-130.png', numfile=130, width=3778, height=5632, colp=335, rowp=5501))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-131.png', numfile=131, width=3743, height=5632, colp=3459, rowp=5520))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-132.png', numfile=132, width=3760, height=5632, colp=322, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-133.png', numfile=133, width=3743, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-134.png', numfile=134, width=3778, height=5632, colp=322, rowp=5510))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-135.png', numfile=135, width=3743, height=5632, colp=3457, rowp=5532))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-136.png', numfile=136, width=3778, height=5632, colp=305, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-137.png', numfile=137, width=3743, height=5632, colp=3453, rowp=5517))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-138.png', numfile=138, width=3778, height=5632, colp=329, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-139.png', numfile=139, width=3743, height=5632, colp=3456, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-140.png', numfile=140, width=3778, height=5632, colp=302, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-141.png', numfile=141, width=3743, height=5632, colp=3462, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-142.png', numfile=142, width=3778, height=5632, colp=331, rowp=5504))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-143.png', numfile=143, width=3743, height=5632, colp=3467, rowp=5527))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-144.png', numfile=144, width=3757, height=5632, colp=309, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-145.png', numfile=145, width=3743, height=5632, colp=3459, rowp=5519))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-146.png', numfile=146, width=3778, height=5632, colp=313, rowp=5490))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-147.png', numfile=147, width=3743, height=5632, colp=3463, rowp=5513))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-148.png', numfile=148, width=3748, height=5446, colp=310, rowp=5445))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-149.png', numfile=149, width=3743, height=5632, colp=3469, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-150.png', numfile=150, width=3757, height=5632, colp=328, rowp=5498))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-151.png', numfile=151, width=3743, height=5632, colp=3473, rowp=5514))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-152.png', numfile=152, width=3746, height=5632, colp=296, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-153.png', numfile=153, width=3743, height=5632, colp=3476, rowp=5524))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-154.png', numfile=154, width=3729, height=5632, colp=295, rowp=5515))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-155.png', numfile=155, width=3743, height=5632, colp=3490, rowp=5523))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-156.png', numfile=156, width=3703, height=5632, colp=258, rowp=5509))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-157.png', numfile=157, width=3743, height=5632, colp=3494, rowp=5504))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-158.png', numfile=158, width=3746, height=5632, colp=297, rowp=5507))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-159.png', numfile=159, width=3743, height=5632, colp=3483, rowp=5516))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-160.png', numfile=160, width=3714, height=5632, colp=276, rowp=5505))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-161.png', numfile=161, width=3743, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-162.png', numfile=162, width=3778, height=5632, colp=302, rowp=5514))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-163.png', numfile=163, width=3743, height=5632, colp=3491, rowp=5533))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-164.png', numfile=164, width=3778, height=5632, colp=273, rowp=5518))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-165.png', numfile=165, width=3743, height=5632, colp=3500, rowp=5521))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-166.png', numfile=166, width=3748, height=5632, colp=290, rowp=5517))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-167.png', numfile=167, width=3743, height=5632, colp=3506, rowp=5526))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-168.png', numfile=168, width=3778, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-169.png', numfile=169, width=3743, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-170.png', numfile=170, width=3778, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-171.png', numfile=171, width=3743, height=5632, colp=None, rowp=None))
Pages.append(ScannedPage(file='T:\\Scans\\THS32\\3 BCI\\THS32-172.png', numfile=172, width=3714, height=5632, colp=None, rowp=None))


# Step 2 process data
finalwidthimpair = colpimpairmax+margedmax
margebmax = max(page.height-page.rowp for page in Pages if page.rowp)
finalheight = rowpmax+margebmax
margegmax = max(colppairmax-page.colp for page in Pages if page.colp and page.numfile % 2 == 0)
finalwidthpair = max(page.width+colppairmax-page.colp for page in Pages if page.colp and page.numfile % 2 == 0)
finalwidth = max(finalwidthpair, finalwidthimpair)
print("finalwidth:", finalwidth)
print("finalheight:", finalheight)

# pG = []
# pD = []
# colpG = []
# colpD = []
# for page in Pages:
#     if page.numfile % 2 == 1:
#         pD.append(page.numfile)
#         colpD.append(page.colp)
#     else:
#         pG.append(page.numfile)
#         colpG.append(page.colp)
# plt.plot(pD, colpD)
# plt.show()

for page in Pages:
    if page.numfile<=200:
        #print(page)
        if page.numfile % 2 == 1:
            if page.colp:
                addmargeg = colpimpairmax-page.colp
                addmarged = margedmax-(page.width-page.colp)
            else:
                addmargeg = (finalwidth-page.width)//2
                addmarged = finalwidth-page.width-addmargeg
            newwidth = page.width+addmargeg+addmarged
            addmargeg += (finalwidth-newwidth)//2
            addmarged += (finalwidth-page.width-addmargeg-addmarged)
            #print('addmargeg', addmargeg, '  addmarged', addmarged, '  newwidth', newwidth)
        else:
            if page.colp:
                addmargeg = colppairmax-page.colp
                addmarged = finalwidth-page.width-addmargeg
            else:
                addmargeg = (finalwidth-page.width)//2
                addmarged = finalwidth-page.width-addmargeg
            newwidth = page.width+addmargeg+addmarged
            addmargeg += (finalwidth-newwidth)//2
            addmarged += (finalwidth-page.width-addmargeg-addmarged)
            #print('addmargeg', addmargeg, '  addmarged', addmarged, '  newwidth', newwidth)

        if page.rowp:
            addmargeh = rowpmax-page.rowp
            addmargeb = margebmax-(page.height-page.rowp)
        else:
            addmargeh = (finalheight-page.height)//2
            addmargeb = finalheight-page.height-addmargeh
        newheight = page.height+addmargeh+addmargeb
        #print('addmargeh', addmargeh, '  addmargeb', addmargeb, '  newheight', newheight)

        print(f'convert "{page.file}" -background white -extent {finalwidth}x{finalheight}-{addmargeg}-{addmargeh} "{dest}\\{prefix}-{page.numfile:0>3}.png"')
"""