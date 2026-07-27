import numpy as np
from .EzRect import EzRect


class EzPolygon :
    """ EzPolygon 數據:
        - points 是 float
        - boundRect 是根據況會自動變 int
    """
    def __init__(self, points: list) -> None:
        self._points = points
        self._rect = EzPolygon.calcBoundaryRect(points)
        pass
    
    @staticmethod
    def calcBoundaryRect(points):
        # Convert point list to numpy array for easier calculations
        point_array = np.array(points)
        # Find the minimum and maximum x/y values from your point list
        min_x = np.min(point_array[:, 0])
        max_x = np.max(point_array[:, 0])
        min_y = np.min(point_array[:, 1])
        max_y = np.max(point_array[:, 1])
        w = max_x - min_x
        h = max_y - min_y
        if w>=1 or h>=1:
            # 一般的 pixel points
            rect = EzRect(min_x, min_y, w+1, h+1)
            rect.round(inplace=True)
        else:
            # yolo 或 labelme 比例點座標
            rect = EzRect(min_x, min_y, w, h)
        return rect
    
    @property
    def boundRect(self):
        return self._rect
    
    @property
    def points(self):
        return self._points
    
    def iterIntPoints(self):
        for x,y in self._points:
            yield int(x), int(y)
    def getIntPoint(self, idx: int):
        x, y = self._points[idx]
        return int(x), int(y)
    