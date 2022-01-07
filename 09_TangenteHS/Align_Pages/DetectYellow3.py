# DetectYellow3
# Final processing of pages
#
# 2022-01-06    PV      Complete version

import math
import statistics
import os
from dataclasses import dataclass
from typing import List, Optional
import numpy as np
import matplotlib.image as mpimg    # type: ignore
import cropimage
from common_fs import *

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
    filefp: str
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


def veclength(v):
    return math.sqrt(v[0]**2+v[1]**2+v[2]**2)


def process(file: str, numfile: int):
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

    area = area-yellow
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


def p(file, numfile, width, height, colp, rowp):
    newpage = ScannedPage(file, numfile, width, height, colp, rowp)
    Pages.append(newpage)


# Step 1, determine sizes and position of yellow rect
source = r'D:\Scans\THS76\02Redresse'
dest = r'D:\Scans\THS76\03Crop'
prefix = 'THS76'

process_files = False

if process_files:
    for filefp in get_all_files(source):
        path, filename = os.path.split(filefp)
        basename, ext = os.path.splitext(filename)
        if ext.lower() == '.jpg':
            nf = int(basename[-3:])
            process(filefp, nf)
    print()
    print('rowpmax=', repr(rowpmax))
    print('colppairmax=', repr(colppairmax))
    print('colpimpairmax=', repr(colpimpairmax))
    print('margedmax=', repr(margedmax))
    print()
else:
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-001.jpg', numfile=1, width=1940, height=2833, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-002.jpg', numfile=2, width=1916, height=2818, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-003.jpg', numfile=3, width=1965, height=2824, colp=1783, rowp=2764)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-004.jpg', numfile=4, width=1966, height=2836, colp=175, rowp=2774)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-005.jpg', numfile=5, width=1960, height=2821, colp=1787, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-006.jpg', numfile=6, width=1964, height=2837, colp=170, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-007.jpg', numfile=7, width=1956, height=2822, colp=1790, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-008.jpg', numfile=8, width=1961, height=2837, colp=164, rowp=2784)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-009.jpg', numfile=9, width=1961, height=2826, colp=1801, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-010.jpg', numfile=10, width=1953, height=2833, colp=154, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-011.jpg', numfile=11, width=1959, height=2823, colp=1801, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-012.jpg', numfile=12, width=1959, height=2835, colp=157, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-013.jpg', numfile=13, width=1953, height=2831, colp=1792, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-014.jpg', numfile=14, width=1973, height=2838, colp=171, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-015.jpg', numfile=15, width=1953, height=2822, colp=1794, rowp=2757)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-016.jpg', numfile=16, width=1956, height=2818, colp=162, rowp=2755)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-017.jpg', numfile=17, width=1955, height=2824, colp=1794, rowp=2753)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-018.jpg', numfile=18, width=1973, height=2845, colp=169, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-019.jpg', numfile=19, width=1952, height=2823, colp=1791, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-020.jpg', numfile=20, width=1952, height=2834, colp=160, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-021.jpg', numfile=21, width=1955, height=2823, colp=1782, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-022.jpg', numfile=22, width=1965, height=2845, colp=177, rowp=2783)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-023.jpg', numfile=23, width=1953, height=2823, colp=1779, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-024.jpg', numfile=24, width=1959, height=2843, colp=174, rowp=2774)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-025.jpg', numfile=25, width=1982, height=2843, colp=1798, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-026.jpg', numfile=26, width=1961, height=2842, colp=171, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-027.jpg', numfile=27, width=1952, height=2825, colp=1785, rowp=2753)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-028.jpg', numfile=28, width=1956, height=2840, colp=166, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-029.jpg', numfile=29, width=1955, height=2833, colp=1787, rowp=2761)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-030.jpg', numfile=30, width=1951, height=2834, colp=161, rowp=2762)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-031.jpg', numfile=31, width=1949, height=2823, colp=1786, rowp=2755)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-032.jpg', numfile=32, width=1954, height=2834, colp=166, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-033.jpg', numfile=33, width=1951, height=2825, colp=1768, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-034.jpg', numfile=34, width=1953, height=2834, colp=183, rowp=2778)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-035.jpg', numfile=35, width=1955, height=2827, colp=1770, rowp=2768)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-036.jpg', numfile=36, width=1955, height=2838, colp=181, rowp=2779)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-037.jpg', numfile=37, width=1949, height=2831, colp=1778, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-038.jpg', numfile=38, width=1946, height=2838, colp=166, rowp=2783)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-039.jpg', numfile=39, width=1949, height=2826, colp=1785, rowp=2774)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-040.jpg', numfile=40, width=1949, height=2834, colp=166, rowp=2781)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-041.jpg', numfile=41, width=1951, height=2824, colp=1787, rowp=2762)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-042.jpg', numfile=42, width=1952, height=2836, colp=164, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-043.jpg', numfile=43, width=1953, height=2824, colp=1790, rowp=2763)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-044.jpg', numfile=44, width=1949, height=2837, colp=163, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-045.jpg', numfile=45, width=1949, height=2819, colp=1782, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-046.jpg', numfile=46, width=1942, height=2833, colp=167, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-047.jpg', numfile=47, width=1950, height=2818, colp=1783, rowp=2754)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-048.jpg', numfile=48, width=1950, height=2833, colp=165, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-049.jpg', numfile=49, width=1949, height=2830, colp=1789, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-050.jpg', numfile=50, width=1948, height=2831, colp=158, rowp=2763)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-051.jpg', numfile=51, width=1951, height=2824, colp=1792, rowp=2756)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-052.jpg', numfile=52, width=1946, height=2835, colp=157, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-053.jpg', numfile=53, width=1946, height=2820, colp=1777, rowp=2758)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-054.jpg', numfile=54, width=1954, height=2835, colp=174, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-055.jpg', numfile=55, width=1946, height=2822, colp=1776, rowp=2756)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-056.jpg', numfile=56, width=1953, height=2841, colp=173, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-057.jpg', numfile=57, width=1947, height=2822, colp=1775, rowp=2763)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-058.jpg', numfile=58, width=1950, height=2839, colp=172, rowp=2781)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-059.jpg', numfile=59, width=1945, height=2825, colp=1781, rowp=2760)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-060.jpg', numfile=60, width=1941, height=2831, colp=162, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-061.jpg', numfile=61, width=1935, height=2816, colp=1781, rowp=2748)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-062.jpg', numfile=62, width=1937, height=2829, colp=153, rowp=2762)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-063.jpg', numfile=63, width=1939, height=2818, colp=1785, rowp=2751)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-064.jpg', numfile=64, width=1945, height=2835, colp=161, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-065.jpg', numfile=65, width=1940, height=2817, colp=1769, rowp=2766)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-066.jpg', numfile=66, width=1942, height=2834, colp=167, rowp=2783)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-067.jpg', numfile=67, width=1941, height=2814, colp=1772, rowp=2757)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-068.jpg', numfile=68, width=1937, height=2832, colp=164, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-069.jpg', numfile=69, width=1943, height=2818, colp=1780, rowp=2761)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-070.jpg', numfile=70, width=1944, height=2834, colp=162, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-071.jpg', numfile=71, width=1947, height=2824, colp=1785, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-072.jpg', numfile=72, width=1948, height=2834, colp=160, rowp=2782)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-073.jpg', numfile=73, width=1945, height=2822, colp=1791, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-074.jpg', numfile=74, width=1946, height=2835, colp=154, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-075.jpg', numfile=75, width=1951, height=2822, colp=1794, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-076.jpg', numfile=76, width=1952, height=2836, colp=157, rowp=2782)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-077.jpg', numfile=77, width=1945, height=2824, colp=1788, rowp=2768)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-078.jpg', numfile=78, width=1950, height=2838, colp=158, rowp=2782)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-079.jpg', numfile=79, width=1946, height=2822, colp=1791, rowp=2760)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-080.jpg', numfile=80, width=1946, height=2835, colp=154, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-081.jpg', numfile=81, width=1947, height=2820, colp=1787, rowp=2751)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-082.jpg', numfile=82, width=1948, height=2837, colp=164, rowp=2763)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-083.jpg', numfile=83, width=1958, height=2829, colp=1792, rowp=2764)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-084.jpg', numfile=84, width=1958, height=2842, colp=168, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-085.jpg', numfile=85, width=1946, height=2825, colp=1775, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-086.jpg', numfile=86, width=1951, height=2838, colp=170, rowp=2779)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-087.jpg', numfile=87, width=1948, height=2828, colp=1772, rowp=2764)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-088.jpg', numfile=88, width=1954, height=2845, colp=176, rowp=2779)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-089.jpg', numfile=89, width=1967, height=2841, colp=1783, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-090.jpg', numfile=90, width=1947, height=2839, colp=169, rowp=2779)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-091.jpg', numfile=91, width=1954, height=2822, colp=1784, rowp=2752)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-092.jpg', numfile=92, width=1953, height=2836, colp=169, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-093.jpg', numfile=93, width=1952, height=2821, colp=1785, rowp=2751)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-094.jpg', numfile=94, width=1950, height=2831, colp=163, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-095.jpg', numfile=95, width=1951, height=2819, colp=1780, rowp=2757)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-096.jpg', numfile=96, width=1956, height=2837, colp=171, rowp=2769)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-097.jpg', numfile=97, width=1957, height=2825, colp=1779, rowp=2762)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-098.jpg', numfile=98, width=1950, height=2834, colp=172, rowp=2774)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-099.jpg', numfile=99, width=1948, height=2819, colp=1767, rowp=2742)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-100.jpg', numfile=100, width=1954, height=2836, colp=181, rowp=2758)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-101.jpg', numfile=101, width=1963, height=2832, colp=1799, rowp=2760)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-102.jpg', numfile=102, width=1958, height=2844, colp=158, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-103.jpg', numfile=103, width=1948, height=2822, colp=1789, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-104.jpg', numfile=104, width=1960, height=2842, colp=161, rowp=2786)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-105.jpg', numfile=105, width=1952, height=2820, colp=1795, rowp=2750)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-106.jpg', numfile=106, width=1955, height=2836, colp=159, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-107.jpg', numfile=107, width=1960, height=2822, colp=1799, rowp=2764)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-108.jpg', numfile=108, width=1951, height=2837, colp=155, rowp=2778)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-109.jpg', numfile=109, width=1956, height=2819, colp=1792, rowp=2768)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-110.jpg', numfile=110, width=1947, height=2835, colp=158, rowp=2782)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-111.jpg', numfile=111, width=1947, height=2822, colp=1795, rowp=2752)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-112.jpg', numfile=112, width=1955, height=2839, colp=156, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-113.jpg', numfile=113, width=1958, height=2823, colp=1792, rowp=2757)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-114.jpg', numfile=114, width=1948, height=2837, colp=160, rowp=2769)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-115.jpg', numfile=115, width=1953, height=2825, colp=1793, rowp=2779)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-116.jpg', numfile=116, width=1953, height=2838, colp=160, rowp=2794)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-117.jpg', numfile=117, width=1951, height=2824, colp=1781, rowp=2766)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-118.jpg', numfile=118, width=1955, height=2839, colp=168, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-119.jpg', numfile=119, width=1961, height=2828, colp=1787, rowp=2754)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-120.jpg', numfile=120, width=1960, height=2839, colp=170, rowp=2768)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-121.jpg', numfile=121, width=1955, height=2820, colp=1780, rowp=2770)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-122.jpg', numfile=122, width=1952, height=2834, colp=172, rowp=2787)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-123.jpg', numfile=123, width=1951, height=2822, colp=1773, rowp=2761)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-124.jpg', numfile=124, width=1957, height=2837, colp=178, rowp=2774)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-125.jpg', numfile=125, width=1955, height=2822, colp=1795, rowp=2738)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-126.jpg', numfile=126, width=1955, height=2840, colp=155, rowp=2754)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-127.jpg', numfile=127, width=1961, height=2824, colp=1790, rowp=2759)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-128.jpg', numfile=128, width=1954, height=2836, colp=166, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-129.jpg', numfile=129, width=1951, height=2819, colp=1778, rowp=2763)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-130.jpg', numfile=130, width=1958, height=2841, colp=176, rowp=2780)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-131.jpg', numfile=131, width=1954, height=2821, colp=1771, rowp=2754)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-132.jpg', numfile=132, width=1957, height=2838, colp=181, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-133.jpg', numfile=133, width=1954, height=2833, colp=1795, rowp=2769)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-134.jpg', numfile=134, width=1955, height=2838, colp=156, rowp=2773)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-135.jpg', numfile=135, width=1964, height=2826, colp=1799, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-136.jpg', numfile=136, width=1952, height=2834, colp=158, rowp=2781)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-137.jpg', numfile=137, width=1959, height=2819, colp=1800, rowp=2758)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-138.jpg', numfile=138, width=1954, height=2833, colp=157, rowp=2772)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-139.jpg', numfile=139, width=1956, height=2819, colp=1799, rowp=2767)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-140.jpg', numfile=140, width=1952, height=2836, colp=153, rowp=2782)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-141.jpg', numfile=141, width=1961, height=2829, colp=1796, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-142.jpg', numfile=142, width=1956, height=2840, colp=165, rowp=2789)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-143.jpg', numfile=143, width=1954, height=2822, colp=1796, rowp=2760)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-144.jpg', numfile=144, width=1953, height=2834, colp=156, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-145.jpg', numfile=145, width=1957, height=2826, colp=1798, rowp=2754)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-146.jpg', numfile=146, width=1956, height=2836, colp=160, rowp=2765)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-147.jpg', numfile=147, width=1953, height=2820, colp=1794, rowp=2771)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-148.jpg', numfile=148, width=1961, height=2842, colp=162, rowp=2786)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-149.jpg', numfile=149, width=1958, height=2821, colp=1783, rowp=2762)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-150.jpg', numfile=150, width=1963, height=2841, colp=176, rowp=2777)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-151.jpg', numfile=151, width=1955, height=2819, colp=1787, rowp=2757)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-152.jpg', numfile=152, width=1952, height=2832, colp=168, rowp=2769)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-153.jpg', numfile=153, width=1956, height=2830, colp=1785, rowp=2775)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-154.jpg', numfile=154, width=1960, height=2825, colp=171, rowp=2768)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-155.jpg', numfile=155, width=1960, height=2823, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-156.jpg', numfile=156, width=1964, height=2829, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-157.jpg', numfile=157, width=1958, height=2827, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-158.jpg', numfile=158, width=1959, height=2830, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-159.jpg', numfile=159, width=1964, height=2830, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-160.jpg', numfile=160, width=1960, height=2843, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-161.jpg', numfile=161, width=1994, height=2821, colp=None, rowp=None)
    p(file='D:\\Scans\\THS76\\02Redresse\\THS76-162.jpg', numfile=162, width=2020, height=2831, colp=None, rowp=None)


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

width_m = int(statistics.median(widths))
height_m = int(statistics.median(heights))
rowp_m = int(statistics.median(x for x in rowps if x))
colpp_m = int(statistics.median(x for x in colps_p if x))
colpi_m = int(statistics.median(x for x in colps_i if x))

print(f'{width_m=}')
print(f'{height_m=}')
print(f'{rowp_m=}')
print(f'{colpp_m=}')
print(f'{colpi_m=}')


# Step 2 process data
for page in Pages:
    folder, filename = os.path.split(page.filefp)
    print(filename)

    if page.numfile%2==0:
        target = os.path.join(dest, 'p', filename)
    else:
        target = os.path.join(dest, 'i', filename)

    # If the target already exist and is more recent than source, no need to do it again
    if os.path.isfile(target) and os.path.getmtime(target) > os.path.getmtime(page.filefp):
        continue

    img = mpimg.imread(page.filefp)[:, :, :3]
    width: int = img.shape[1]
    height: int = img.shape[0]

    if page.rowp:
        tins = rowp_m-page.rowp
    else:
        tins = (height_m-page.height)//2
    bins = height_m-page.height-tins

    if page.numfile==5 or page.numfile==9:
        breakpoint()

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
