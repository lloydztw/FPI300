import math

class QVector:
    precision: int = 6

    def __init__(self, *args):
        """
        支援多種建構方式:
        1. QVector(v1, v2, v3, ...) -> 傳入數值列表/可變參數
        2. QVector(dimensions: int) -> 指定維度，預設補 0.0
        3. QVector(src: QVector)    -> 複製另一個 QVector
        """
        if len(args) == 1:
            arg = args[0]
            if isinstance(arg, int):
                # QVector(int dimensions)
                self._m_VD = [0.0] * arg
            elif isinstance(arg, QVector):
                # QVector(QVector src)
                self._m_VD = [self._fix(val) for val in arg._m_VD]
            elif isinstance(arg, (list, tuple)):
                # QVector([1.0, 2.0, 3.0])
                self._m_VD = [self._fix(val) for val in arg]
            else:
                # QVector(1.0) 情況
                self._m_VD = [self._fix(float(arg))]
        else:
            # QVector(double, double, ...)
            self._m_VD = [self._fix(float(val)) for val in args]

    # ------------------------------------------------------------------
    # 私有輔助函數
    # ------------------------------------------------------------------
    @classmethod
    def _fix(cls, v: float) -> float:
        return round(float(v), cls.precision)

    @classmethod
    def _check_dimensions(cls, v1: 'QVector', v2: 'QVector') -> bool:
        if v1 is None or v2 is None:
            return False
        if v1.size != v2.size:
            raise ValueError("QVectors must be in the same Dimensions for Vector Operation !")
        return True

    # ------------------------------------------------------------------
    # 屬性 (Properties)
    # ------------------------------------------------------------------
    @property
    def dimensions(self) -> int:
        return len(self._m_VD)

    @property
    def length(self) -> int:
        return len(self._m_VD)

    @property
    def size(self) -> int:
        return len(self._m_VD)

    @property
    def V(self) -> list:
        return self._m_VD

    # X, Y, Z / x, y, z 座標 Getter & Setter
    @property
    def x(self) -> float:
        return self[0]

    @x.setter
    def x(self, value: float):
        self[0] = value

    @property
    def X(self) -> float:
        return self[0]

    @X.setter
    def X(self, value: float):
        self[0] = value

    @property
    def y(self) -> float:
        return self[1]

    @y.setter
    def y(self, value: float):
        self[1] = value

    @property
    def Y(self) -> float:
        return self[1]

    @Y.setter
    def Y(self, value: float):
        self[1] = value

    @property
    def z(self) -> float:
        return self[2]

    @z.setter
    def z(self, value: float):
        self[2] = value

    @property
    def Z(self) -> float:
        return self[2]

    @Z.setter
    def Z(self, value: float):
        self[2] = value

    # 長度與長度平方
    @property
    def norm_length(self) -> float:
        d_len_sq = self * self
        return self._fix(math.sqrt(d_len_sq))

    @property
    def norm_length_sq(self) -> float:
        return self * self

    # ------------------------------------------------------------------
    # 索引器 (Indexer)
    # ------------------------------------------------------------------
    def __getitem__(self, idx: int) -> float:
        if 0 <= idx < len(self._m_VD):
            return self._m_VD[idx]
        return 0.0

    def __setitem__(self, idx: int, value: float):
        if 0 <= idx < len(self._m_VD):
            self._m_VD[idx] = self._fix(value)

    # ------------------------------------------------------------------
    # 向量操作方法
    # ------------------------------------------------------------------
    def slice(self, start: int, length: int = 0) -> 'QVector':
        end = self.dimensions if length <= 0 else min(start + length, self.dimensions)
        n = max(0, end - start)
        v = QVector(n)
        for k, i in enumerate(range(start, end)):
            v[k] = self[i]
        return v

    def expand(self, dimensions: int) -> 'QVector':
        if dimensions > self.dimensions:
            dst = QVector(dimensions)
            for i in range(len(self._m_VD)):
                dst._m_VD[i] = self._m_VD[i]
            return dst
        elif dimensions < self.dimensions:
            return self.slice(0, dimensions)
        else:
            return QVector(self)

    def offset(self, *values) -> 'QVector':
        n = min(self.dimensions, len(values))
        for i in range(n):
            self[i] += values[i]  # Setter 會自動觸發 _fix
        return self

    # ------------------------------------------------------------------
    # 比較與解析方法
    # ------------------------------------------------------------------
    @classmethod
    def are_equal(cls, v1: 'QVector', v2: 'QVector', precision: int = -1) -> bool:
        if v1 is v2:
            return True
        if v1 is None or v2 is None:
            return False
        if v1.dimensions != v2.dimensions:
            return False

        if precision < 0:
            for i in range(v1.dimensions):
                if v1[i] != v2[i]:
                    return False
        else:
            for i in range(v1.dimensions):
                if round(v1[i], precision) != round(v2[i], precision):
                    return False

        return True

    def is_equal_to(self, v: 'QVector') -> bool:
        return QVector.are_equal(self, v)

    @classmethod
    def parse(cls, s: str) -> 'QVector':
        s_clean = s.replace("{", "").replace("}", "")
        strs = s_clean.split(',')
        n = len(strs)
        v = QVector(n)
        ok = False
        for i in range(n):
            try:
                d = float(strs[i].strip())
                v[i] = d
                ok = True
            except ValueError:
                pass
        return v if ok else None

    def from_string(self, s: str):
        v = QVector.parse(s)
        if v is not None:
            n = min(self.dimensions, v.dimensions)
            for i in range(n):
                self[i] = v[i]

    # ------------------------------------------------------------------
    # 運算子多載 (Operators)
    # ------------------------------------------------------------------
    def __mul__(self, other):
        # 內積: QVector * QVector
        if isinstance(other, QVector):
            if not self._check_dimensions(self, other):
                return 0.0
            ret = 0.0
            for i in range(self.dimensions):
                ret += self._fix(self[i] * other[i])
            return self._fix(ret)
        # 純量乘法: QVector * double
        elif isinstance(other, (int, float)):
            s = self._fix(other)
            v = QVector(self.dimensions)
            for i in range(self.dimensions):
                v[i] = self[i] * s
            return v
        return NotImplemented

    def __rmul__(self, other):
        # 支援純量左乘: double * QVector
        return self.__mul__(other)

    def __add__(self, other: 'QVector') -> 'QVector':
        if not self._check_dimensions(self, other):
            return None
        v = QVector(self.dimensions)
        for i in range(self.dimensions):
            v[i] = self[i] + other[i]
        return v

    def __sub__(self, other: 'QVector') -> 'QVector':
        if not self._check_dimensions(self, other):
            return None
        v = QVector(self.dimensions)
        for i in range(self.dimensions):
            v[i] = self[i] - other[i]
        return v

    def __truediv__(self, other: float) -> 'QVector':
        s = self._fix(other)
        v = QVector(self.dimensions)
        for i in range(self.dimensions):
            v[i] = self[i] / s
        return v

    # ------------------------------------------------------------------
    # 字串輸出 (ToString)
    # ------------------------------------------------------------------
    def to_string(self, fmt: str = "0.0000", compact: bool = False) -> str:
        # 解析 C# 浮點數格式 (例如 "0.0000" -> Python 的 ".4f")
        py_fmt = ".4f"
        if "." in fmt:
            decimals = len(fmt.split(".")[1])
            py_fmt = f".{decimals}f"

        sb = []
        if not compact:
            sb.append("{")

        for i in range(self.size):
            val = self._m_VD[i]
            val_str = f"{val:{py_fmt}}"

            if not compact:
                if val >= 0.0:
                    sb.append(" ")
                sb.append(" ")

            sb.append(val_str)

            if i != self.size - 1:
                sb.append(",")

        if not compact:
            sb.append(" }")

        return "".join(sb)

    def __str__(self) -> str:
        return self.to_string("0.0000", False)

    def __repr__(self) -> str:
        return f"QVector({self._m_VD})"