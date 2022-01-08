# DetectYellow3
# Final processing of pages
#
# 2022-01-06    PV      Complete version
# 2022-01-07    PV      Yellow autocalibration
# 2022-01-08    PV      Multiple values for num_backcolor

import math
import statistics
import os
import sys
from dataclasses import dataclass
from typing import List, Optional
import numpy as np
import matplotlib.image as mpimg    # type: ignore
import cropimage
from common_fs import *

(rowmin, rowmax) = (2680, 2800)
(colminpair, colmaxpair) = (130, 330)
(colminimpair, colmaximpair) = (1640, 1820)

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


def veclength(v):
    return math.sqrt(v[0]**2+v[1]**2+v[2]**2)


def process(file: str, numfile: int, num_backcolor: np.ndarray):
    print(file, ';', numfile, ';', sep='', end='')

    img = mpimg.imread(file)[:, :, :3]
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

    area = area-num_backcolor
    area = area.reshape(areawidth*areaheight, 3)
    area = np.apply_along_axis(veclength, 1, area)
    area = area.reshape(areaheight, areawidth)

    colp: Optional[int] = None
    rowp: Optional[int] = None

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
    Pages.append(newpage)


def p(file, numfile, width, height, colp, rowp):
    newpage = ScannedPage(file, numfile, width, height, colp, rowp)
    Pages.append(newpage)


# Step 1, determine sizes and position of yellow rect
root = 'D:\Scans\THS38'
source = os.path.join(root, '02Redresse')
dest =os.path.join(root, '03Crop')

if not os.path.isdir(dest):
    os.mkdir(dest)
for d in [dest+'\\p', dest+'\\i']:
    if not os.path.isdir(d):
        os.mkdir(d)

# Calibration
calibration_root = os.path.join(root, 'Calibration')
colors_dic: dict[str, np.ndarray] = {}
for calibration_file in get_files(calibration_root):
    color_name = basename(calibration_file).casefold()
    img = mpimg.imread(os.path.join(calibration_root, calibration_file))
    color = np.median(img, axis=(0,1))
    print(f'{color_name}={color}')
    colors_dic[color_name] = color

def get_color(page: int) -> np.ndarray:
    return colors_dic['jaune']

process_files = True
if process_files:
    for filefp in get_all_files(source):
        path, filename = os.path.split(filefp)
        basename, ext = os.path.splitext(filename)
        if ext.lower() == '.jpg':
            nf = int(basename[-3:])
            process(filefp, nf, get_color(nf))
    print()
    for page in Pages:
        print(repr(page).replace('ScannedPage', 'p'))
    #sys.exit(0)
   
else:
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-001.jpg', numfile=1, width=1984, height=2849, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-002.jpg', numfile=2, width=1960, height=2832, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-003.jpg', numfile=3, width=1972, height=2841, colp=1800, rowp=2754)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-004.jpg', numfile=4, width=1972, height=2841, colp=172, rowp=2765)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-005.jpg', numfile=5, width=1963, height=2835, colp=1792, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-006.jpg', numfile=6, width=1968, height=2838, colp=172, rowp=2762)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-007.jpg', numfile=7, width=1966, height=2837, colp=1796, rowp=2756)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-008.jpg', numfile=8, width=1965, height=2836, colp=171, rowp=2757)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-009.jpg', numfile=9, width=1963, height=2835, colp=1793, rowp=2742)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-010.jpg', numfile=10, width=1968, height=2838, colp=171, rowp=2747)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-011.jpg', numfile=11, width=1962, height=2834, colp=1797, rowp=2734)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-012.jpg', numfile=12, width=1962, height=2834, colp=168, rowp=2738)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-013.jpg', numfile=13, width=1966, height=2837, colp=1806, rowp=2731)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-014.jpg', numfile=14, width=1976, height=2844, colp=167, rowp=2745)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-015.jpg', numfile=15, width=1963, height=2835, colp=1804, rowp=2736)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-016.jpg', numfile=16, width=1965, height=2836, colp=157, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-017.jpg', numfile=17, width=1963, height=2835, colp=1790, rowp=2734)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-018.jpg', numfile=18, width=1965, height=2836, colp=171, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-019.jpg', numfile=19, width=1962, height=2834, colp=1791, rowp=2734)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-020.jpg', numfile=20, width=1968, height=2838, colp=177, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-021.jpg', numfile=21, width=1962, height=2834, colp=1795, rowp=2730)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-022.jpg', numfile=22, width=1965, height=2836, colp=168, rowp=2741)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-023.jpg', numfile=23, width=1963, height=2835, colp=1794, rowp=2733)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-024.jpg', numfile=24, width=1962, height=2834, colp=169, rowp=2744)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-025.jpg', numfile=25, width=1963, height=2835, colp=1790, rowp=2748)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-026.jpg', numfile=26, width=1968, height=2838, colp=175, rowp=2763)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-027.jpg', numfile=27, width=1962, height=2834, colp=1787, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-028.jpg', numfile=28, width=1965, height=2836, colp=176, rowp=2760)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-029.jpg', numfile=29, width=1965, height=2836, colp=1785, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-030.jpg', numfile=30, width=1962, height=2834, colp=177, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-031.jpg', numfile=31, width=1963, height=2835, colp=1781, rowp=2745)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-032.jpg', numfile=32, width=1965, height=2836, colp=186, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-033.jpg', numfile=33, width=1966, height=2837, colp=1788, rowp=2748)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-034.jpg', numfile=34, width=1970, height=2840, colp=180, rowp=2761)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-035.jpg', numfile=35, width=1963, height=2835, colp=1782, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-036.jpg', numfile=36, width=1972, height=2841, colp=182, rowp=2763)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-037.jpg', numfile=37, width=1963, height=2835, colp=1779, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-038.jpg', numfile=38, width=1965, height=2836, colp=189, rowp=2758)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-039.jpg', numfile=39, width=1963, height=2835, colp=1778, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-040.jpg', numfile=40, width=1970, height=2840, colp=187, rowp=2758)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-041.jpg', numfile=41, width=1966, height=2837, colp=1784, rowp=2748)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-042.jpg', numfile=42, width=1970, height=2840, colp=185, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-043.jpg', numfile=43, width=1963, height=2835, colp=1779, rowp=2748)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-044.jpg', numfile=44, width=1968, height=2838, colp=188, rowp=2753)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-045.jpg', numfile=45, width=1966, height=2837, colp=1792, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-046.jpg', numfile=46, width=1965, height=2836, colp=177, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-047.jpg', numfile=47, width=1966, height=2837, colp=1792, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-048.jpg', numfile=48, width=1970, height=2840, colp=176, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-049.jpg', numfile=49, width=1963, height=2835, colp=1793, rowp=2730)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-050.jpg', numfile=50, width=1963, height=2835, colp=166, rowp=2732)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-051.jpg', numfile=51, width=1962, height=2834, colp=1794, rowp=2728)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-052.jpg', numfile=52, width=1963, height=2835, colp=169, rowp=2732)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-053.jpg', numfile=53, width=1966, height=2837, colp=1797, rowp=2738)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-054.jpg', numfile=54, width=1973, height=2842, colp=174, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-055.jpg', numfile=55, width=1962, height=2834, colp=1793, rowp=2737)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-056.jpg', numfile=56, width=1966, height=2837, colp=172, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-057.jpg', numfile=57, width=1962, height=2834, colp=1794, rowp=2739)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-058.jpg', numfile=58, width=1963, height=2835, colp=170, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-059.jpg', numfile=59, width=1970, height=2840, colp=1801, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-060.jpg', numfile=60, width=1968, height=2838, colp=173, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-061.jpg', numfile=61, width=1973, height=2842, colp=1785, rowp=2753)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-062.jpg', numfile=62, width=1966, height=2837, colp=181, rowp=2756)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-063.jpg', numfile=63, width=1966, height=2837, colp=1782, rowp=2747)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-064.jpg', numfile=64, width=1966, height=2837, colp=187, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-065.jpg', numfile=65, width=1966, height=2837, colp=1789, rowp=2744)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-066.jpg', numfile=66, width=1966, height=2837, colp=177, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-067.jpg', numfile=67, width=1966, height=2837, colp=1790, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-068.jpg', numfile=68, width=1970, height=2840, colp=180, rowp=2758)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-069.jpg', numfile=69, width=1962, height=2834, colp=1787, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-070.jpg', numfile=70, width=1970, height=2840, colp=181, rowp=2757)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-071.jpg', numfile=71, width=1963, height=2835, colp=1791, rowp=2743)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-072.jpg', numfile=72, width=1965, height=2836, colp=174, rowp=2750)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-073.jpg', numfile=73, width=1962, height=2834, colp=1794, rowp=2745)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-074.jpg', numfile=74, width=1965, height=2836, colp=169, rowp=2753)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-075.jpg', numfile=75, width=1965, height=2836, colp=1798, rowp=2735)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-076.jpg', numfile=76, width=1962, height=2834, colp=165, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-077.jpg', numfile=77, width=1966, height=2837, colp=1798, rowp=2733)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-078.jpg', numfile=78, width=1970, height=2840, colp=168, rowp=2741)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-079.jpg', numfile=79, width=1966, height=2837, colp=1802, rowp=2739)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-080.jpg', numfile=80, width=1970, height=2840, colp=167, rowp=2747)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-081.jpg', numfile=81, width=1962, height=2834, colp=1788, rowp=2736)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-082.jpg', numfile=82, width=1968, height=2838, colp=176, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-083.jpg', numfile=83, width=1963, height=2835, colp=1789, rowp=2732)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-084.jpg', numfile=84, width=1962, height=2834, colp=175, rowp=2742)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-085.jpg', numfile=85, width=1966, height=2837, colp=1783, rowp=2746)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-086.jpg', numfile=86, width=1970, height=2840, colp=180, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-087.jpg', numfile=87, width=1962, height=2834, colp=1784, rowp=2750)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-088.jpg', numfile=88, width=1962, height=2834, colp=185, rowp=2760)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-089.jpg', numfile=89, width=1969, height=2839, colp=1792, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-090.jpg', numfile=90, width=1969, height=2839, colp=180, rowp=2760)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-091.jpg', numfile=91, width=1962, height=2834, colp=1787, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-092.jpg', numfile=92, width=1962, height=2834, colp=180, rowp=2759)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-093.jpg', numfile=93, width=1966, height=2837, colp=1782, rowp=2754)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-094.jpg', numfile=94, width=1966, height=2837, colp=187, rowp=2766)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-095.jpg', numfile=95, width=1966, height=2837, colp=1775, rowp=2747)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-096.jpg', numfile=96, width=1966, height=2837, colp=192, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-097.jpg', numfile=97, width=1963, height=2835, colp=1785, rowp=2747)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-098.jpg', numfile=98, width=1969, height=2839, colp=179, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-099.jpg', numfile=99, width=1966, height=2837, colp=1793, rowp=2757)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-154.jpg', numfile=154, width=1965, height=2836, colp=159, rowp=2765)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-155.jpg', numfile=155, width=1974, height=2843, colp=1810, rowp=2751)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-156.jpg', numfile=156, width=1962, height=2834, colp=153, rowp=2754)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-157.jpg', numfile=157, width=1962, height=2834, colp=1801, rowp=2740)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-158.jpg', numfile=158, width=1965, height=2836, colp=156, rowp=2749)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-159.jpg', numfile=159, width=1962, height=2834, colp=1810, rowp=2743)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-160.jpg', numfile=160, width=1965, height=2836, colp=155, rowp=2752)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-161.jpg', numfile=161, width=1966, height=2837, colp=1797, rowp=2743)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-162.jpg', numfile=162, width=1963, height=2835, colp=168, rowp=2750)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-163.jpg', numfile=163, width=1965, height=2836, colp=1796, rowp=2737)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-164.jpg', numfile=164, width=1963, height=2835, colp=169, rowp=2744)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-165.jpg', numfile=165, width=1966, height=2837, colp=1794, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-166.jpg', numfile=166, width=1976, height=2844, colp=175, rowp=2767)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-167.jpg', numfile=167, width=1966, height=2837, colp=1791, rowp=2764)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-168.jpg', numfile=168, width=1963, height=2835, colp=172, rowp=2770)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-169.jpg', numfile=169, width=1968, height=2838, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-170.jpg', numfile=170, width=1963, height=2835, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-171.jpg', numfile=171, width=1968, height=2838, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-172.jpg', numfile=172, width=1963, height=2835, colp=166, rowp=2755)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-173.jpg', numfile=173, width=1966, height=2837, colp=1786, rowp=2754)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-174.jpg', numfile=174, width=1973, height=2842, colp=184, rowp=2766)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-175.jpg', numfile=175, width=1970, height=2840, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-176.jpg', numfile=176, width=1984, height=2849, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-177.jpg', numfile=177, width=1988, height=2815, colp=None, rowp=None)
    p(file='D:\\Scans\\THS75\\02Redresse\\THS75-178.jpg', numfile=178, width=1995, height=2813, colp=None, rowp=None)

# Determinate median value for cropping
widths = []
heights = []
rowps = []
colps_p = []
colps_i = []
for page in Pages:
    widths.append(page.width)
    heights.append(page.height)
    rowps.append(page.rowp)
    if page.numfile % 2 == 0:
        colps_p.append(page.colp)
    else:
        colps_i.append(page.colp)

width_m = int(statistics.median(widths))-40
height_m = int(statistics.median(heights))-50
rowp_m = int(statistics.median(x for x in rowps if x))-20
colpp_m = int(statistics.median(x for x in colps_p if x))-20
colpi_m = int(statistics.median(x for x in colps_i if x))+20

print(f'{width_m=}')
print(f'{height_m=}')
print(f'{rowp_m=}')
print(f'{colpp_m=}')
print(f'{colpi_m=}')


# Step 2 process data
for page in Pages:
    folder, filename = os.path.split(page.file)
    print(filename)

    if page.numfile%2==0:
        target = os.path.join(dest, 'p', filename)
    else:
        target = os.path.join(dest, 'i', filename)

    # If the target already exist and is more recent than source, no need to do it again
    if os.path.isfile(target) and os.path.getmtime(target) > os.path.getmtime(page.file):
        continue

    img = mpimg.imread(page.file)[:, :, :3]
    width: int = img.shape[1]
    height: int = img.shape[0]

    if page.rowp:
        tins = rowp_m-page.rowp
    else:
        tins = (height_m-page.height)//2
    bins = height_m-page.height-tins

    if page.numfile%2==0:
        if page.colp:
            lins = colpp_m-page.colp
        else:
            lins = (width_m-page.width)//2
        rins = width_m-page.width-lins
    else:
        if page.colp:
            lins = colpi_m-page.colp
        else:
            lins = (width_m-page.width)//2
        rins = width_m-page.width-lins

    img = cropimage.crop_image(img, tins, bins, lins, rins)
    mpimg.imsave(target, img, format='jpg', dpi=300)
