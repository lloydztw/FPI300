"""
Module         : PlotUtils
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2023/08/12
@Revision      : 2024/07/07
"""
__version__ = '1.2.0'


from typing import List
import numpy as np
import math
import cv2

try: 
    from matplotlib import pyplot as plt
    HAS_MATPLOT_LIB = True
except:
    HAS_MATPLOT_LIB = False

AUTO_PAUSE = 0


def init_matplotlib_for_chinese(figsize = (12,5)):
    '''
    讓 matplotlib.pyplot 可以顯示中文
    '''
    if not HAS_MATPLOT_LIB:
        return
    try:
        plt.rcParams['font.sans-serif'] = ['Microsoft JhengHei']    # 正黑體
        plt.rcParams['font.sans-serif'] = ['Microsoft YaHei']       # 雅黑體
        plt.rcParams['axes.unicode_minus'] = False
        if figsize is not None:
            plt.figure(figsize=figsize)
    except:
        pass
      
def trans_matp_color_to_opencv(mpl_color):
    return tuple(int(c * 255) for c in mpl_color[:3])

def get_colors_map(colorsNumber: int, name='jet', to_opencv= True):
    cmap = plt.get_cmap(name)
    mpl_colors = [cmap(i) for i in np.linspace(0, 1, colorsNumber)]
    if to_opencv:
        colors = [trans_matp_color_to_opencv(c) for c in mpl_colors]
        return colors
    return mpl_colors

def build_color_map(namesDict: dict):
    cmapArr = get_colors_map(len(namesDict))
    cmapDict = {}
    idx = 0
    for key in namesDict:
        cmapDict[key] = cmapArr[idx]
        idx += 1
    return cmapDict


def _plt_show():
    if HAS_MATPLOT_LIB:
        if AUTO_PAUSE:
            plt.pause(float(AUTO_PAUSE))
        else:
            plt.show()
    pass

def smart_plot_one(axp, img: np.ndarray, title: str = None):
    # (0) 判定是否有 axp
    isImmediately = axp is None
    if isImmediately:
        axp = plt

    # (1) 調用 imshow
    if len(img.shape)>=3 and img.shape[2]>1:
        # 轉換 RGB -> BGR
        # img_p = img[:,:,::-1] # 非常耗記憶體
        # axp.imshow(img_p)
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        axp.imshow(img)
    else:
        # GRAY 1.0 to 255
        axp.imshow(img, cmap='gray', vmin=0, vmax=255)

    # (2) 設定 title
    if title != None:
        if hasattr(axp,'set_title'):
            axp.set_title(title)
        else:
            axp.title(title)

    # (3) 立即顯示
    if isImmediately:
        # plt.show()
        _plt_show()
    pass

def smart_imshow(*args, **kwargs):
    """ 輸入參數: 
        - (1) img, [title]
        - (2) axp, img, [title]
    """

    # 分析參數 (明確指名)
    axp = kwargs.get('axp')
    img = kwargs.get('img')
    title = kwargs.get('title')

    # 分析參數 (依序)
    for arg in args:
        if img is None and isinstance(arg, np.ndarray):
            img = arg
        elif title is None and isinstance(arg, str):
            title = arg
        elif axp is None:
            # className = str(type(arg))
            axp = arg 
        
    smart_plot_one(axp, img, title)
    pass

def smart_imshow_imgs(imgs: List[np.ndarray], titles=None, roi=None, sharey=False, rows=1):
    N = len(imgs)
    cols = math.ceil(N / rows)
    _ , axs = plt.subplots(rows, cols, sharey=sharey)

    # 將 rows cols 一維化
    if N == 1:
        axsArr = [axs]
    elif rows <= 1:
        axsArr = axs
    else:
        axsArr = []
        i = 0
        for row in range(rows):
            for col in range(cols):
                if i < N:
                    axsArr.append(axs[row, col])
                    i += 1
        pass

    for i in range(N):
        img = imgs[i]
        title = titles[i] if titles is not None and i < len(titles) else None
        if roi is not None:
            sx, sy, sw, sh = roi
            img = img[sy:sy+sh, sx:sx+sw]
        smart_plot_one(axsArr[i], img, title)

    # plt.show()
    _plt_show()
    pass


def test():
    img = np.zeros((100,100,3), dtype=np.uint8)
    img2 = np.zeros((100,100,3), dtype=np.uint8)
    img[:,:,1] = 128
    img2[:,:,0] = 128

    # INIT for chinese charactors
    init_matplotlib_for_chinese()
    
    if True:
        # TEST 1
        smart_imshow(img, "綠")
        # TEST 2
        smart_imshow(plt, img2, "藍")
        plt.show()
    if True:
        # TEST 3
        _, axs = plt.subplots(2,2)
        smart_imshow(axs[0,0], img, "GREEN")
        smart_imshow(axs[0,1], img2, "BLUE")
        smart_imshow(axs[1,1], img, "GREEN")
        smart_imshow(axs[1,0], img2, "BLUE")    
        plt.show()
        # TEST 4
        smart_imshow_imgs([img, img2], ["GREEN", "BLUE"])
    if True:
        # TEST 5
        smart_imshow_imgs([img, img2, img], ["GREEN", "BLUE"], rows=2)
    pass


if __name__ == "__main__":
    import os
    print(os.path.split(__file__)[-1], "=", __version__)    
    test()
