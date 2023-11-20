# DetectYellow3
# Final processing of pages
#
# 2022-01-06    PV      Complete version
# 2022-01-07    PV      Yellow autocalibration
# 2022-01-08    PV      Multiple values for num_backcolor

import math
import statistics
import os
from dataclasses import dataclass
from typing import List, Optional
import numpy as np
import matplotlib.image as mpimg    # type: ignore
import cropimage
from common_fs import get_all_files, get_files, basename

(rowmin, rowmax) = (2680, 2900)
(colminpair, colmaxpair) = (130, 330)
(colminimpair, colmaximpair) = (1640, 1920)

@dataclass
class ScannedPage:
    file: str
    numfile: int
    width: int
    height: int
    colp: Optional[int]
    rowp: Optional[int]
    colback: Optional[str]

    def is_pair(self) -> bool:
        return self.numfile % 2 == 0

    def is_impair(self) -> bool:
        return self.numfile % 2 == 1


Pages: List[ScannedPage] = []

def veclength(v):
    return math.sqrt(v[0]**2+v[1]**2+v[2]**2)


# Calibration
calibration_root = 'D:\Scans\Calibration'
colors_dic: dict[str, np.ndarray] = {}
for calibration_file in get_files(calibration_root):
    color_name = basename(calibration_file).casefold()
    img = mpimg.imread(os.path.join(calibration_root, calibration_file))
    color = np.median(img, axis=(0,1))
    print(f'{color_name}={color}')
    colors_dic[color_name] = color


def process(file: str, numfile: int):
    print(file, '\t', sep='', end='')

    img = mpimg.imread(file)[:, :, :3]
    width: int = img.shape[1]
    height: int = img.shape[0]
    print(width, 'x', height, '\t', sep='', end='')

    colp: Optional[int] = None
    rowp: Optional[int] = None
    colback: Optional[str] = None

    if numfile>=3:
        if numfile % 2 == 0:
            (colmin, colmax) = (colminpair, colmaxpair)
        else:
            (colmin, colmax) = (colminimpair, colmaximpair)

        rm = min(rowmax, height-2)
        area = img[rowmin:rm, colmin:colmax, :]
        areaheight: int = area.shape[0]
        areawidth: int = area.shape[1]

        # Find the best backcolor
        lback = []
        for name, backcolor in colors_dic.items():
            areac = area-backcolor
            areac = areac.reshape(areawidth*areaheight, 3)
            areac = np.apply_along_axis(veclength, 1, areac)
            areac = areac.reshape(areaheight, areawidth)
            n = (areac[:, :] <= 50).sum()
            lback.append((n, name, areac))
        lback.sort(reverse=True)
        area = lback[0][2]

        if lback[0][0]>500:
            colback = lback[0][1]
            print(lback[0][1], '\t', sep='', end='')
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
        else:
            print('None', '\t', sep='', end='')
            
    else:
        print('None', '\t', sep='', end='')

    print(colp, ';', rowp, sep='')

    newpage = ScannedPage(file, numfile, width, height, colp, rowp, colback)
    Pages.append(newpage)


def p(file, numfile, width, height, colp, rowp, colback):
    newpage = ScannedPage(file, numfile, width, height, colp, rowp, colback)
    Pages.append(newpage)

def clean_borders(img: np.ndarray):
    #height = img.shape[0]
    width = img.shape[1]

    def rgb2gray1(rgb):
        return np.dot(rgb[...,:3], [0.2989, 0.5870, 0.1140])

    def rgb2gray2(rgb):
        return np.dot(rgb[...,:3], [0.2125, 0.7154, 0.0721])

    gray = rgb2gray2(img)

    def clean_boder(x1, x2):
        step = 1 if x2>x1 else -1
        cc = -1
        for col in range(x1, x2, step):
            avg = np.average(gray[:,col])
            cnt = (gray[:, col] < 215).sum()
            #print(col, avg, cnt)
            if avg<220 or cnt>450:
                break
            cc += 1
        return cc

    bw = 30
    left = clean_boder(0, bw)
    right = clean_boder(width-1, width-1-bw)

    if left>0:
        img[:,:left+1] = [255,255,255]
    if right>0:
        img[:,-right-1:] = [255,255,255]


# Step 1, determine sizes and position of yellow rect
def process_root(root:str):
    global Pages
    
    Pages = []
    source = os.path.join(root, '02Redresse')
    dest =os.path.join(root, '03Crop')

    if not os.path.isdir(dest):
        os.mkdir(dest)
    for d in [dest+'\\p', dest+'\\i']:
        if not os.path.isdir(d):
            os.mkdir(d)

    process_files = True
    if process_files:
        for filefp in get_all_files(source):
            path, filename = os.path.split(filefp)
            base, ext = os.path.splitext(filename)
            if ext.lower() == '.jpg':
                nf = int(base[-3:])
                process(filefp, nf)
        print()
    
    else:
        p(file='D:\\Scans\\ToDo\\THS33B\\02Redresse\\THS33-001.jpg', numfile=1, width=1941, height=2826, colp=None, rowp=None, colback=None)
        # ...
        p(file='D:\\Scans\\ToDo\\THS33B\\02Redresse\\THS33-160.jpg', numfile=160, width=1943, height=2834, colp=None, rowp=None, colback=None)

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

    # Finally fixed the value, identical for all books
    width_m = 1950
    height_m = 2830

    print(f'{width_m=}')
    print(f'{height_m=}')
    print(f'{rowp_m=}')
    print(f'{colpp_m=}')
    print(f'{colpi_m=}')

    with open(os.path.join(root, 'Pages.py'), 'w', encoding='utf-8') as fout:
        for page in Pages:
            print(repr(page).replace('ScannedPage', 'p'))
            fout.write(repr(page).replace('ScannedPage', 'p')+'\n')
        fout.write('\n')
        fout.write(f'{width_m=}\n')
        fout.write(f'{height_m=}\n')
        fout.write(f'{rowp_m=}\n')
        fout.write(f'{colpp_m=}\n')
        fout.write(f'{colpi_m=}\n')

    # Step 2 process data
    for page in Pages:
        folder, filename = os.path.split(page.file)
        print(filename)

        if page.numfile%2==0:
            target = os.path.join(dest, 'p', filename)
        else:
            target = os.path.join(dest, 'i', filename)

        # If the target already exist and is more recent than source, no need to do it again
        # if os.path.isfile(target) and os.path.getmtime(target) > os.path.getmtime(page.file):
        #     continue

        img = mpimg.imread(page.file)[:, :, :3]
        #width: int = img.shape[1]
        #height: int = img.shape[0]

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
        clean_borders(img)
        mpimg.imsave(target, img, format='jpg', dpi=300)


#process_root("D:\Scans\ToDo\THS33B")
process_root("D:\Scans\ToDo\THS70")
process_root("D:\Scans\ToDo\THS71")
process_root("D:\Scans\ToDo\THS72")
process_root("D:\Scans\ToDo\THS73")
process_root("D:\Scans\ToDo\THS74")
process_root("D:\Scans\ToDo\THS75")
process_root("D:\Scans\ToDo\THS76")
