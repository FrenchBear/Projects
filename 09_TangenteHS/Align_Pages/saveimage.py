import matplotlib.image as mpimg
import matplotlib.pyplot as plt
import numpy as np
import math
import os
import sys
from dataclasses import dataclass
from typing import List, Optional

file = r'D:\Scans\THS76\02Redresse\THS76-002.jpg'
newfile = r'D:\Scans\THS76\03Crop\THS76-002.jpg'

img = mpimg.imread(file)[:, :, :3]
width: int = img.shape[1]
height: int = img.shape[0]
print(file, ': w=', width, ' h=', height)


newwidth = 2800
newheight = 3000
# newwidth = 1001
# newheight = 1201

inserttop = (newheight-height)//2
insertbottom = newheight-(height+inserttop)
assert newheight == height+inserttop+insertbottom

insertleft = (newwidth-width)//2
insertright = newwidth-(width+insertleft)
assert newwidth == width+insertleft+insertright


#from numpy import ndarray
def crop_image(img: np.ndarray, inserttop: int, insertbottom: int, insertleft: int, insertright: int) -> np.ndarray:
    # Insert/delete left column duplicating existing column 0 rep times
    rep = insertleft
    if rep > 0:
        for i in range(rep):
            img = np.insert(img, 0, img[:, 0, :], axis=1)
    elif rep < 0:
        img = np.delete(img, range(-rep), axis=1)

    # Insert right column duplicating last column rep times
    rep = insertright
    if rep > 0:
        for i in range(rep):
            img = np.insert(img, img.shape[1], img[:, -1, :], axis=1)
    elif rep < 0:
        img = np.delete(img, range(img.shape[1]+rep, img.shape[1]), axis=1)

    # Insert top row duplicating existing row 0 rep times
    rep = inserttop
    if rep > 0:
        for i in range(rep):
            img = np.insert(img, 0, img[0, :, :], axis=0)
    elif rep<0:
        img = np.delete(img, range(-rep), axis=0)

    # Insert bottom row duplicating last row rep times
    rep = insertbottom
    if rep > 0:
        for i in range(rep):
            img = np.insert(img, img.shape[0], img[-1, :, :], axis=0)
    elif rep<0:
        img = np.delete(img, range(img.shape[0]+rep, img.shape[0]), axis=0)

    return img


img = crop_image(img, inserttop, insertbottom, insertleft, insertright)
print('new size: w=', img.shape[1], ' h=', img.shape[0])

mpimg.imsave(newfile, img, format='jpg', dpi=300)
