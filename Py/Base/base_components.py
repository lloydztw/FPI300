import math


class Vector:
    X: float
    Y: float
    
    def __init__(self, x, y):
        self.X = x
        self.Y = y

    def NormSQ(self):
        x = self.X
        y = self.Y
        return x*x + y*y
    
    def Norm(self):
        return math.sqrt(self.NormSQ)
    
    # 實現相減運算 (self - other)
    def __sub__(self, other):
        if isinstance(other, Vector):
            return Vector(self.X - other.X, self.Y - other.Y)
        return NotImplemented


def Dist(p1, p2):
    dx = p1[0] - p2[0]
    dy = p1[1] - p2[1]
    return math.sqrt(dx*dx + dy*dy)