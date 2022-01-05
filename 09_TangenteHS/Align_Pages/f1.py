import matplotlib.image as mpimg
import numpy as np

source1 = r'C:\Scans\THS23\2 Crop Hz\THS23-001.png'
source2 = r'C:\Scans\THS23\3 Crop Vt\THS23-001.png'

img1 = mpimg.imread(source1)
img1 = img1[:,:,:3]
print(img1.shape)

img2 = mpimg.imread(source2)
print(img2.shape)

