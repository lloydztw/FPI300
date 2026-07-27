"""
Module         : PlotUtils
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2023/12/21
"""
__version__ = '1.0.1'


from typing import List
import numpy as np
from PIL import Image, ImageDraw, ImageFont

from . import PlotUtils as PU
from .CvzUtils import *


#Global
_PIL_FONT = ImageFont.truetype("arial.ttf", 28)


def matp_init_chinese_display(figsize = (12,5)):    
    '''
    讓 matplotlib.pyplot 可以顯示中文
    '''
    PU.init_matplotlib_for_chinese(figsize)

def set_pil_font_size(size: int =  28):
    global _PIL_FONT
    _PIL_FONT = ImageFont.truetype("arial.ttf", size)

def trans_toPIL(img: np.ndarray):
    pil_image = Image.fromarray(img)
    return pil_image

def draw_text_in_pil(img: np.ndarray, text: str, color, pt = None):
    img_pil = trans_toPIL(img)
    draw = ImageDraw.Draw(img_pil)
    # color = (0,25,0) if ok else (100,0,0)
    if pt is None:
        pt = (2,1)
    draw.text(pt, text, font=_PIL_FONT, fill=color)
    # write back to np image with the new pil image
    if len(img.shape) < 3:
        img[:,:] = np.asarray(img_pil)
    else:
        img[:,:,:] = np.asarray(img_pil)
    pass

def plot_col_images(imgs: List[np.ndarray], title: str = None):
    if imgs is not None and len(imgs)>0:
        imgShow = concate_images_horiz(imgs)
        # imgShow = cv2.cvtColor(imgShow, cv2.COLOR_RGB2BGR)
        # plt.imshow(imgShow)
        # if title is not None:
        #     plt.title(title)
        # plt.show()
        
        # NOTE: PU.smart_imshow 內部 會自動調用 cv2.COLOR_RGB2BGR
        PU.smart_imshow(None, imgShow, title)
    pass

def display_image(name: str, image: np.ndarray):
    if not PU.HAS_MATPLOT_LIB:
        from PIL import Image
        pil_image = Image.fromarray(image)
        pil_image.show(name)
    else:
        import matplotlib.pyplot as plt
        plt.imshow(image)
        plt.title(name)
        plt.pause(1)
    pass
