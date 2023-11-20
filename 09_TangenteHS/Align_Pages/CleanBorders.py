import matplotlib.image as mpimg
import numpy as np

def clean_borders(pathfile: str, newname: str):
    img = mpimg.imread(pathfile)[:,:,:3]
    height = img.shape[0]
    width = img.shape[1]
    print(height, width)


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
            print(col, avg, cnt)
            if avg<220 or cnt>450:
                break
            cc += 1
        return cc

    bw = 30
    left = clean_boder(0, bw)
    right = clean_boder(width-1, width-1-bw)
    print(pathfile, ' left=',left,' right=', right)

    if left>0:
        img[:,:left+1] = [255,255,255]
    if right>0:
        img[:,-right-1:] = [255,255,255]
    
    mpimg.imsave(newname, img, format='jpg', dpi=300)


# for file in get_all_files(r'D:\Scans\ToDo\THS33B\03Crop'):
#     clean_borders(file, os.path.join(r'D:\Scans\Result', filepart(file)))

clean_borders(r'D:\Scans\ToDo\THS33B\03Crop\p\THS33-066.jpg', r'D:\Scans\Result\THS33-066.jpg')