using JetEazy.QMath;
using PasoActuatorID = System.Int32;


namespace JetEazy.Drivers.Motor
{
    public class MotIoVector : MotVector
    {
        #region PRIVATE_DATA
        int _indexOffset = 0;
        #endregion

        public new bool this[int id]
        {
            get
            {
                int i = _getIndex(id);
                return m_VD[i] > 0;
            }
            set
            {
                int i = _getIndex(id);
                m_VD[i] = value ? 1.0 : 0.0;
            }
        }

        public MotIoVector(int dims)
            : base(dims)
        {
            m_enableFlags = new bool[dims];
            EnableAll();
        }
        public MotIoVector(QVector src)
            : base(src)
        {
            m_enableFlags = new bool[src.Size];
            if (src is MotVector vector)
                CopyFrom(vector);
            else
                EnableAll();
        }

        public override string ToString()
        {
            int dims = this.m_VD.Length;
            if (dims > 0)
            {
                string str = "(io=";
                str += m_enableFlags[0] ? "1" : "0";
                for (int i = 1; i < dims; i++)
                {
                    str += m_enableFlags[i] ? ",1" : ",0";
                }
                str += ")";
                return str;
            }
            return "";
        }

        #region PRIVATE_MEMBERS
        private int _getIndex(PasoActuatorID id)
        {
            return (id - _indexOffset);
        }
        #endregion
    }
}
