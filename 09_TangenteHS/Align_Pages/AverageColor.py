import matplotlib.image as mpimg
import numpy as np

#img = mpimg.imread('YellowRect.png')
img = mpimg.imread('YellowRect75.jpg')
print(img)

avg = np.median(img, axis=(0,1))
print(avg)

# For png
# yellow = np.array([0.7921569, 0.6392157, 0.27058825])
# print(yellow)

yellow = np.array([251., 211., 19.])
print(yellow)
