# cropimage.py
# Subroutine resizing a pic
#
# ToDo: Insert missing rows in one command instead of a loop...
#
# 2022-01-06    PV

import numpy as np

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
