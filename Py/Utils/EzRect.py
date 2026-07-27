"""
Module         : EzRect
@Author        : LeTian Chang
@Email         : lloydz.tw@gmail.com
@Creation      : 2023/11/19
"""

class EzRect:
    """ Rectangle
    """
    def __init__(self, *args, **kwds):
        # [x,y,w,h]
        self._rect = [0,0,0,0]
        self.setValues(*args, **kwds)
    def __call__(self, *args, **kwds):
        self.setValues(*args, **kwds)
    def __repr__(self):
        x,y,w,h = self._rect
        return f"p1={(x,y)}, p2={(x+w,y+h)}, sz={(w,h)}"
    def __str__(self):
        return f"{self._rect}"
    def __iter__(self):
        return iter(self._rect)
            
    def setValues(self, *args, **kwds):
        if self._rect == None:
            self._rect = [0,0,0,0]
        argsN = len(args)
        if argsN >= 4:
            self._rect = list(args[:4])
        elif argsN >= 2:
            x , y = args[0]
            x2, y2 = args[1]
            self._rect = [x, y, x2-x, y2-y]
        elif kwds:
            if "pt1" in kwds and "pt2" in kwds:
                self.pt1 = kwds["pt1"]
                self.pt2 = kwds["pt2"]
            elif "x" in kwds and "y" in kwds and "w" in kwds and "h" in kwds:
                self._rect = [kwds["x"], kwds["y"], kwds["w"], kwds["h"]]
            elif "x1" in kwds and "y1" in kwds and "x2" in kwds and "y2" in kwds:
                self.xyxy = [kwds["x1"], kwds["y1"], kwds["x2"], kwds["y2"]]
        pass
    
    def inflate(self, dw, dh=None, inplace=False):
        if dh is None:
            dh = dw
        x,y,w,h = self._rect
        rect = [x-dw, y-dh, w+dw+dw, h+dh+dh]
        if inplace:
            self._rect = rect
            return self
        return EzRect(*rect)
    
    def zoom(self, zoomX, zoomY=None, inplace=False):
        if zoomY is None:
            zoomY = zoomX
        x,y,w,h = self._rect
        rect = [x*zoomX, y*zoomY, w*zoomX, h*zoomY]
        if inplace:
            self._rect = rect
            return self
        return EzRect(*rect)

    def clip(self, boundary: list, inplace=False):
        xmin, ymin, bw, bh = boundary
        xmax = xmin + bw
        ymax = ymin + bh
        x1, y1, x2, y2 = self.xyxy
        xx1 = min(max(x1, xmin), xmax)
        xx2 = min(max(x2, xmin), xmax)
        yy1 = min(max(y1, ymin), ymax)
        yy2 = min(max(y2, ymin), ymax)
        if xx1==x1 and xx2==x2 and yy1==y1 and yy2==y2:
            return self
        if inplace:
            self.xyxy = [xx1, yy1, xx2, yy2]
            return self
        return EzRect((xx1,yy1),(xx2,yy2))

    def round(self, inplace=False):
        if self.isEmpty:
            return self if inplace else self.copy()
        rect = [int(round(v)) for v in self._rect]
        if inplace:
            self._rect = rect
            return self
        else:
            return EzRect(*rect)

    def copy(self):
        return EzRect(*self._rect)

    @staticmethod
    def empty():
        return EzRect()
    
    @property
    def isEmpty(self):
        return self._rect is None or self._rect[2]==0 or self._rect[3]==0
    
    @property
    def xywh(self):
        return self._rect
    @xywh.setter
    def xywh(self, value):
        x,y,w,h = value
        self._rect = [x,y,w,h]
        pass
    
    @property
    def x(self):
        return self._rect[0]
    @x.setter
    def x(self, value):
        self._rect[0] = value
    @property
    def y(self):
        return self._rect[1]
    @y.setter
    def y(self, value):
        self._rect[1] = value
    @property
    def width(self):
        return self._rect[2]
    @width.setter
    def width(self, value):
        self._rect[2]=value
    @property
    def height(self):
        return self._rect[3]
    @height.setter
    def height(self, value):
        self._rect[3]=value
    @property
    def size(self):
        return tuple(self._rect[-2:])
    @size.setter
    def size(self, value):
        w,h = value
        self._rect[3]=w
        self._rect[4]=h
    @property
    def center(self):
        x1,y1,x2,y2 = self.xyxy
        return (x1+x2)//2, (y1+y2)//2
    
    @property
    def xyxy(self):
        if self._rect is not None:
            x,y,w,h = self._rect
            return [x, y, x+w, y+h]
        return [0,0,0,0]
    @xyxy.setter
    def xyxy(self, value):
        x,y,x2,y2 = value
        self._rect = [x, y, x2-x, y2-y]
        pass
    
    @property
    def pt1(self):
        if self._rect is not None:
            x,y = self._rect[:2]
            return (x,y)
        return (0,0)
    @pt1.setter
    def pt1(self, value: tuple):
        self._rect[0] = value[0]
        self._rect[1] = value[1]
    @property
    def pt2(self):
        if self._rect is not None:
            x,y,w,h = self._rect
            return (x+w,y+h)
        return (0,0)
    @pt2.setter
    def pt2(self, value):
        x2, y2 = value
        x, y = self.pt1
        self._rect = [x, y, x2-x, y2-y]
        return self


if __name__ == "__main__":
    # 詳細測試請用 tests / test_EzRect.py
    rect = EzRect(10,20,50,30)
    print(rect)
    rect(1,2,3,4)
    print(rect)
    print(rect.inflate(10))
    pass
