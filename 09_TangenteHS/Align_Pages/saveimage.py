import matplotlib.image as mpimg
import cropimage

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

img = cropimage.crop_image(img, inserttop, insertbottom, insertleft, insertright)
print('new size: w=', img.shape[1], ' h=', img.shape[0])

mpimg.imsave(newfile, img, format='jpg', dpi=300)
