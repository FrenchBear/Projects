import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import numpy as np

img = mpimg.imread('THS76-004.jpg')
width = img.shape[1]
height = img.shape[0]
print(width, height)

yellow = np.array([251., 211., 19.])


def dotdist(p1, p2):
    return np.linalg.norm(p2-p1)


def moving_average(a, n=3):
    ret = np.cumsum(a, dtype=float)
    ret[n:] = ret[n:] - ret[:-n]
    return ret[n - 1:] / n


dcol = []
for col in range(width):
    rowdist = [dotdist(img[row, col], yellow) for row in range(height-122, height)]
    n = sum(1 if d < 15 else 0 for d in rowdist)
    dcol.append(n)
#dcol = moving_average(dcol, 20)
plt.plot(dcol)
cmin = -1
cmax = 0
for i in range(1, len(dcol)):
    if cmin < 0 and dcol[i] > 30:
        cmin = i-2
    if cmin > 0 and dcol[i] < 5:
        cmax = i+1
        break
print(f'Col: {cmin}..{cmax}')

drow = []
for row in range(height-122, height):
    coldist = [dotdist(img[row, col], yellow) for col in range(width)]
    n = sum(1 if d < 15 else 0 for d in coldist)
    drow.append(n)
plt.plot(drow)
rmin = -1
rmax = 0
for i in range(1, len(drow)):
    if rmin < 0 and drow[i] > 30:
        rmin = i+height-122-1
    if rmin > 0 and drow[i] < 5:
        rmax = i+height-122-1
        break
print(f'Row: {rmin}..{rmax}')


plt.show()
