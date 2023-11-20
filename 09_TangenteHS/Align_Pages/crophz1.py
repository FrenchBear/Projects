import os
import matplotlib.image as mpimg
from common_fs import get_all_files

# source = r'C:\Scans\THS32\1 Scans\1'
# dest = r'C:\Scans\THS32\2 Crop HZ\1'

source = r'C:\Scans\THS32\1 Scans\2'
dest = r'C:\Scans\THS32\2 Crop HZ\2'


def process(file: str, picout: str, numfile: int):
    print(file, ';', numfile, ';', sep='', end='')

    img = mpimg.imread(file)[:, :, :3]
    width: int = img.shape[1]
    height: int = img.shape[0]
    print(width, ';', height, ';', sep='', end='')

    if numfile%2==1:
        if numfile >= 17 or numfile == 3 or numfile == 13:
            img = img[:, :-143, :]
        img = img[:, 42:, :]
    else:
        img = img[:, 100:-50, :]

    mpimg.imsave(picout, img, dpi=600)
    print('->', picout)


for pathfile in get_all_files(source):
    path, file = os.path.split(pathfile)
    basename, ext = os.path.splitext(file)
    if ext.lower() == '.png':
        numfile = int(basename[-3:])
        picout = os.path.join(dest, file)
        process(pathfile, picout, numfile)
