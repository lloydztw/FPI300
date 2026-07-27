"""
Module         : CvzUtils
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2023/10/03
@Revision      : 2024/07/04
"""
__version__ = '1.5.0'


import os
import cv2
import numpy as np
# import tempfile
# import shutil

PIC_EXTS = [".jpg", ".png", ".bmp"]


def _TRACE(*args):
    print(*args)

def convert_wechat_filenames(rootPath: str):
    """ 把含有微信中文檔名 更名 成英文檔名
    """
    search_words = ['圖片']
    result_dict = {}
    for dirpath, dnames, fnames in os.walk(rootPath):
        for fname in fnames:
            for word in search_words:
                if word in fname:
                    rawFile = os.path.join(dirpath,fname)
                    result_dict[rawFile] = word
                    break
    for rawFile, word in result_dict.items():
        newFile = rawFile.replace(word,"").replace(" ","")
        os.rename(rawFile, newFile)
        print("[更名]", newFile)
    pass


def cvz_smart_to_gray(img: np.ndarray, force_copy: bool = False) -> np.ndarray:
    if img is None:
        return None
    ch = img.shape[2] if len(img.shape)>=3 else 1
    if ch==4:
        return cv2.cvtColor(img, cv2.COLOR_RGBA2GRAY)
    elif ch==3:
        return cv2.cvtColor(img, cv2.COLOR_RGB2GRAY)
    else:
        return img.copy() if force_copy else img
    
def cvz_imread(filename: str, gray: bool = False, forceColor: bool = False) -> np.ndarray:
    """ 協助 cv2 處理中文檔名
    """
    image = cv2.imdecode(np.fromfile(filename, dtype=np.uint8), -1)
    if gray:
        # image = cv2.cvtColor(image, cv2.COLOR_BGRA2GRAY)
        image = cvz_smart_to_gray(image)
    else:
        ch = image.shape[2] if len(image.shape)>=3 else 1
        if ch==4:
            image = cv2.cvtColor(image, cv2.COLOR_BGRA2BGR)
        elif ch==3:
            pass
        elif forceColor:
            image = cv2.cvtColor(image, cv2.COLOR_GRAY2BGR)
            pass
    return image

def cvz_imwrite(filename: str, img: np.ndarray):
    """ 協助 cv2 處理中文檔名
    """
    cv2.imencode('.jpg', img)[1].tofile(filename) 


def cvz_get_image_zoom_info(imgDst: np.ndarray, imgSrc: np.ndarray):
    dstH, dstW = imgDst.shape[:2]
    srcH, srcW = imgSrc.shape[:2]
    dstSize = min(dstH, dstW)
    srcSize = min(srcH, srcW)
    isRotated90 = ((dstH>=dstW) != (srcH>=srcW))
    rotate = 90 if isRotated90 else 0
    # if dstSize==srcSize and rotate==0:
    #     return None
    zoom = float(dstSize) / srcSize if (dstSize!=srcSize) else 1
    return {'zoom':zoom, 'rotate':rotate}

def cvz_shrink_image(img: np.ndarray, minH=480, delete = False):
    h, w = img.shape[:2]
    if min(h,w) > minH:
        shrink = min(w/minH, h/minH)
        w = int(w/shrink)
        h = int(h/shrink)
        imgNew = cv2.resize(img, (w,h))
        if delete:
            del img
        # _TRACE(f"縮放至 {w} x {h}")
        return imgNew
    return img


def cvz_expand_image_repeat_borders(img: np.ndarray, size):
    """ size 是 (w,h) 或 inflate
    """
    c = img.shape[2] if len(img.shape) >=3 else 1
    h, w = img.shape[:2]

    dw, dh = size[:2] if isinstance(size, tuple) else size, size
    dw = max(dw, 0)
    dh = max(dh, 0)
    if dw==0 and dh==0:
        return img
    
    H = h + dh * 2
    W = w + dw * 2
    new_shape = (H, W) if c==1 else  (H, W, c)

    # Create a new image initialized with zeros (black)
    img_new = np.zeros(new_shape, dtype=img.dtype)
    
    rewind = 0

    if c == 1:
        # Vertical Lines
        for x in range(0, dw):
            # LEFT
            xs = 0 + min(x, rewind)
            left_line = img[:, xs:xs+1]
            xd = (dw-1) - x
            img_new[dh:dh+h, xd:xd+1] = left_line
            # RIGHT
            xs = (w-1) - min(x, rewind)
            right_line = img[:, xs:xs+1]
            xd = (dw+w) + x
            img_new[dh:dh+h, xd:xd+1] = right_line

        # Horizontal Lines:
        for y in range(0, dh):
            # TOP
            ys = 0 + min(y, rewind)
            top_line = img[ys:ys+1, :]
            yd = (dh-1) - y
            img_new[yd:yd+1, dw:dw+w] = top_line
            # BOTTOM
            ys = (h-1) - min(y, rewind)
            bottom_line = img[ys:ys+1, :]
            yd = (dh+h) + y
            img_new[yd:yd+1, dw:dw+w] = bottom_line            
        
        # Corners
        if True:
            block = np.ones((dh, dw), dtype = img.dtype)
            img_new[0:dh, 0:dw] = block * img[0,0]        # left-top
            img_new[0:dh, -dw:W] = block * img[0,-1]      # right-top
            img_new[-dh:H, 0:dw] = block * img[-1,0]      # left-bottom
            img_new[-dh:H, -dw:W] = block * img[-1,-1]    # right-bottom

        # ORIGINAL
        img_new[dh:dh+h, dw:dw+w] = img
    else:
        # Vertical Lines
        for x in range(0, dw):
            # LEFT
            xs = 0 + min(x, rewind)
            left_line = img[:, xs:xs+1, :]
            xd = (dw-1) - x
            img_new[dh:dh+h, xd:xd+1] = left_line
            # RIGHT
            xs = (w-1) - min(x, rewind)
            right_line = img[:, xs:xs+1, :]
            xd = (dw+w) + x
            img_new[dh:dh+h, xd:xd+1, :] = right_line

        # Horizontal Lines:
        for y in range(0, dh):
            # TOP
            ys = 0 + min(y, rewind)
            top_line = img[ys:ys+1, :, :]
            yd = (dh-1) - y
            img_new[yd:yd+1, dw:dw+w, :] = top_line
            # BOTTOM
            ys = (h-1) - min(y, rewind)
            bottom_line = img[ys:ys+1, :, :]
            yd = (dh+h) + y
            img_new[yd:yd+1, dw:dw+w] = bottom_line            
        
        # Corners
        if True:
            block = np.ones((dh, dw, c), dtype = img.dtype)
            img_new[0:dh, 0:dw, :] = block * img[0,0,:]        # left-top
            img_new[0:dh, -dw:W, :] = block * img[0,-1,:]      # right-top
            img_new[-dh:H, 0:dw, :] = block * img[-1,0,:]      # left-bottom
            img_new[-dh:H, -dw:W, :] = block * img[-1,-1,:]    # right-bottom

        # ORIGINAL
        img_new[dh:dh+h, dw:dw+w, :] = img
    
    return img_new
    
def cvz_expand_image(img: np.ndarray, size, fill_color = 'mean'):
    """ size 是 (w,h) 或 inflate
    """
    if fill_color=='repeat':
        return cvz_expand_image_repeat_borders(img, size)
    
    c = img.shape[2] if len(img.shape) >=3 else 1
    h, w = img.shape[:2]

    dw, dh = size[:2] if isinstance(size, tuple) else size, size
    dw = max(dw, 0)
    dh = max(dh, 0)
    if dw==0 and dh==0:
        return img
    
    new_h = h + dh * 2
    new_w = w + dw * 2
    new_shape = (new_h, new_w) if c==1 else  (new_h, new_w, c)

    if fill_color == 'gaussian':
        mean = int(img.mean())
        sigma = int(img.std())
        img_new = np.random.normal(mean, sigma, new_shape).astype(img.dtype)
    elif fill_color == 'mean' or fill_color is None:
        fill_color = int(img.mean())
        img_new = (np.ones(new_shape, img.dtype) * fill_color).astype(img.dtype)
    else:
        img_new = (np.ones(new_shape, img.dtype) * fill_color).astype(img.dtype)

    if c==1:
        img_new[dh:dh+h, dw:dw+w] = img
    else:
        img_new[dh:dh+h, dw:dw+w, :] = img

    return img_new


def cvz_rotate_90n(img: np.ndarray, degree = 90):
    degree = int(degree)
    if degree==0:
        return img
    elif degree==90:
        return cv2.rotate(img, cv2.ROTATE_90_CLOCKWISE)
    elif degree==180:
        return cv2.rotate(img, cv2.ROTATE_180)
    elif degree==270:
        return cv2.rotate(img, cv2.ROTATE_90_COUNTERCLOCKWISE)
    else:
        return img

def cvz_rotate_any_degree(img: np.ndarray, degree):
    # 獲取圖像中心點
    height, width = img.shape[:2]
    center = (width // 2, height // 2)
    # 設置旋轉角度（逆時針為正）
    angle = degree
    # 獲取旋轉矩陣
    rotation_matrix = cv2.getRotationMatrix2D(center, angle, 1.0)
    # 對圖像進行旋轉
    rotated_image = cv2.warpAffine(img, rotation_matrix, (width, height))    
    return rotated_image

def cvz_rotate_90(img: np.ndarray, msg = None, delete = False):
    """ 使用 transpose
    """
    if msg is not None:
        _TRACE("[轉90度]", msg)
    imgNew = cv2.transpose(img)
    if delete:
        del img
    return imgNew
    
def cvz_rotate_image(img: np.ndarray, angle_degree):
    return cvz_rotate_any_degree(img, angle_degree)


def cvz_inflate(rect: list, dw: int, dh: int = None):
    if dh is None:
        dh = dw
    x,y,w,h = rect
    return [x-dw, y-dh, w+dw*2, h+dh*2]

def cvz_clip(rect: list, boundary: list):
    xmin, ymin, w, h = boundary
    xmax = xmin + w
    ymax = ymin + h

    isList = isinstance(rect, list)

    N = len(rect)
    if N == 4:
        x, y, w, h = rect
        x2 = x + w
        y2 = y + h
        x = max(x, xmin)
        y = max(y, ymin)
        x2 = min(x2, xmax)
        y2 = min(y2, ymax)
        return [x, y, x2-x, y2-y] if isList else (x, y, x2-x, y2-y)
    elif N==2:
        x, y = rect
        x = max(x, xmin)
        y = max(y, ymin)
        return [x,y] if isList else (x,y)
    else:
        assert False, "rect 格式有誤"
        return rect    

def cvz_clip_points(points, boundary: list, minusOne: bool):
    xmin, ymin, w, h = boundary
    xmax = xmin + w
    ymax = ymin + h
    if minusOne:
        xmax -= 1
        ymax -= 1
    N = len(points)
    for i in range(N):
        x, y = points[i]
        x = min(max(xmin, x), xmax)
        y = min(max(ymin, y), ymax)
        points[i] = (x,y)
    pass

def cvz_draw_rect(dst: np.ndarray, rect, color:tuple, thickness:int=1):
    x,y,w,h = rect
    cv2.rectangle(dst, (x,y), (x+w,y+h), color, thickness)
    pass

def cvz_inflate_rotated_rect(boxOrPts: tuple, dw: int, dh: int = None):
    """
    @boxOrPts: 格式為 ((cx, cy), (width, height), angle) 或 np.array
    """
    if isinstance(boxOrPts, tuple):
        center, (w, h), angle = boxOrPts
    else:
        center, (w, h), angle = cv2.minAreaRect(boxOrPts)

    if dh is None:
        dh = dw  
    w += (dw*2)
    h += (dh*2)
    return (center, (w,h), angle)

def cvz_draw_rotated_rect(dst: np.ndarray, box2D: tuple, color:tuple, thickness:int=1):
    """
    @rotatedRect: 格式 ((cx, cy), (width, height), angle)
    """    
    pts = cv2.boxPoints(box2D)
    # pts = np.int0(pts)
    pts = np.intp(pts)     
    cv2.drawContours(dst, [pts], 0, color, thickness)
    pass

def cvz_get_boundary_rect(box2D: tuple):
    """
    @rotatedRect: 格式 ((cx, cy), (width, height), angle)
    """
    pts = cv2.boxPoints(box2D)
    rect = cv2.boundingRect(pts)
    return rect


def cvz_concate_images_horiz(images):
    # is_pil = not isinstance(images[0], np.ndarray)
    # imgs = [np.asarray(tmp) for tmp in images] if is_pil else images
    # img = np.hstack(imgs)
    # return img
    is_pil = not isinstance(images[0], np.ndarray)
    images = [np.asarray(tmp) for tmp in images] if is_pil else images
    H = max([img.shape[0] for img in images])
    for i in range(len(images)):
        img = images[i]
        h, w = img.shape[:2]
        if h < H:
            dy = (H-h) // 2
            if len(img.shape) > 2 :
                chs = img.shape[2]
                extended_image = np.ones((H, w, chs), dtype=np.uint8) * int(img.mean())
                extended_image[dy:dy+h, :, :] = img
            else:
                extended_image = np.ones((H, w), dtype=np.uint8) * int(img.mean())                
                extended_image[dy:dy+h, :] = img
            images[i] = extended_image
    big_img = np.hstack(images)
    return big_img

def cvz_concate_images_vert(images):
    is_pil = not isinstance(images[0], np.ndarray)
    images = [np.asarray(tmp) for tmp in images] if is_pil else images
    W = max([img.shape[1] for img in images])
    for i in range(len(images)):
        img = images[i]
        h, w = img.shape[:2]
        if w < W:
            dx = (W-w) // 2
            if len(img.shape) > 2 :
                chs = img.shape[2]
                extended_image = np.ones((h, W, chs), dtype=np.uint8) * int(img.mean())
                extended_image[:, dx:dx+w, :] = img
            else:
                extended_image = np.ones((h, W), dtype=np.uint8) * int(img.mean())
                extended_image[:, dx:dx+w] = img
            images[i] = extended_image
    big_img = np.vstack(images)
    return big_img

def concate_images_horiz(images):
    return cvz_concate_images_horiz(images)

def concate_images_vert(images):
    return cvz_concate_images_vert(images)


def IterFilesRecursively(srcPath: str, ext: str):
    """
    Walk through all the directories and subdirectories using os.walk
    """
    for root, dirs, files in os.walk(srcPath):
        for file in files:
            if ext is None or file.endswith(ext):
                filePath = os.path.join(root, file)
                yield filePath
    pass

def FindBlobRects(img: np.ndarray, areaMinMax:tuple=None):    
    if len(img.shape)>2:
        tmp = cv2.cvtColor(img, cv2.COLOR_RGB2GRAY)
    else:
        tmp = img.copy()

    (numLabels, labels, stats, centroids) = cv2.connectedComponentsWithStats(tmp)
    
    H, W = img.shape[:2]
    if areaMinMax is None:
        aMin = 1
        aMax = W*H*2
    else:
        areaMinMax = np.array(areaMinMax)
        aMin = areaMinMax.min()
        aMax = areaMinMax.max() if len(areaMinMax)>1 else W*H*2        
    
    results = []
    for i in range(1, numLabels):
        x = stats[i, cv2.CC_STAT_LEFT]
        y = stats[i, cv2.CC_STAT_TOP]
        w = stats[i, cv2.CC_STAT_WIDTH]
        h = stats[i, cv2.CC_STAT_HEIGHT]
        area = stats[i, cv2.CC_STAT_AREA]
        if area > aMin and area < aMax:
            center= centroids[i]
            center = np.round(center).astype(int).tolist()
            result = {'area': area, 'rect':[x,y,w,h], 'center':center}
            results.append(result)
            
    if len(results)>1:
        sorted_results = sorted(results, key=lambda x: x['area'], reverse=True)
        results = sorted_results
    return results


def ENCRYPT(data: str, tag: str = '$'):
    if tag is not None and tag!='':
        ret = ""
        for c in data[::-1]:
            ret = ret + tag + c
        return ret
    else:
        return data[::-1]

def DECRYPT(data: str, tag: str = '$'):
    if tag is not None and tag!='':
        ret = data[1::2]
    else:
        ret = data
    return ret[::-1]



if __name__ == "__main__":
    print(os.path.split(__file__)[-1], "=", __version__)
    pass