using System;
using JetEazy.QMath;

namespace JetEazy.QMath
{
    public class QVector3 : QVector
    {
        public QVector3(double[] src)
            : base(src.Length)
        {
            Array.Copy(src, m_VD, src.Length);
        }

        public QVector3(double x, double y, double z)
            : base(3)
        {
            m_VD[0] = x;
            m_VD[1] = y;
            m_VD[2] = z;
        }
    }

    public class QVector2 : QVector
    {
        public QVector2(double[] src)
            : base(src.Length)
        {
            Array.Copy(src, m_VD, src.Length);
        }

        public QVector2(double x, double y)
            : base(2)
        {
            m_VD[0] = x;
            m_VD[1] = y;
        }
    }
}
