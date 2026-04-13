using System;
using JetEazy.QMath;
using PasoMotorID = System.Int16;
using PasoActuatorID = System.Int32;


namespace Paso
{
    public class CxPasoMotorVector : CxPasoVector
    {
        public const int N_DIMENSIONS = 3;
        public double XB
        {
            get { return m_VD[0]; }
            set { m_VD[0] = value; }
        }
        public double Theta
        {
            get { return m_VD[1]; }
            set { m_VD[1] = value; }
        }
        public double Z
        {
            get { return m_VD[2]; }
            set { m_VD[2] = value; }
        }

        public void Enable(PasoMotorID id)
        {
            Enable(_getIndex(id));
        }
        public void Disable(PasoMotorID id)
        {
            Disable(_getIndex(id));
        }
        public bool IsEnabled(PasoMotorID id)
        {
            return IsEnabled(_getIndex(id)); ;
        }
        public CxPasoIoVector IO = new CxPasoIoVector();
        public object Tag;

        #region CONSTRUCTORS
            public CxPasoMotorVector() : base(N_DIMENSIONS) { }
            public CxPasoMotorVector(QVector src) : base(src) { }
            public CxPasoMotorVector(CxPasoMotorVector src) : base(src)
            {
                CopyFrom(src);
            }
        #endregion

        public void CopyFrom(CxPasoMotorVector src)
        {
            base.CopyFrom(src);

            if (src.IO != null)
                this.IO.CopyFrom(src.IO);
            else
                this.IO = null;

            this.Tag = src.Tag;
        }
        public void SmartCopyFrom(CxPasoMotorVector src)
        {
            base.SmartCopyFrom(src);

            if (src.IO != null)
                this.IO.SmartCopyFrom(src.IO);
        }
        public override string ToString()
        {
            //return string.Format("(motors={0:0.000},{1:0.000},{2:0.000}) {3}",
            //                        XB, YB, ZB, IO);

            if (N_DIMENSIONS != 0)
            {
                string str = "(motors=";

                str += this[0].ToString("0.000");
                for (int i = 1; i < N_DIMENSIONS; i++)
                {
                    str += ",";
                    str += this[i].ToString("0.000");
                }
                str += ") ";

                if (IO != null)
                    str += IO.ToString();

                return str;
            }

            return "";
        }

        #region PRIVATE_MEMBERS
            private int _getIndex(PasoMotorID id)
            {
                //return (int)(id - PasoMotorID.Min);
                return (int)id;
            }
        #endregion
    }

    public class CxPasoIoVector : CxPasoVector
    {
        public const int N_DIMENSIONS = 1;

        public void Enable(PasoActuatorID id)
        {
            Enable(_getIndex(id));
        }
        public void Disable(PasoActuatorID id)
        {
            Disable(_getIndex(id));
        }
        public bool IsEnabled(PasoActuatorID id)
        {
            return IsEnabled(_getIndex(id));
        }
        public bool this[PasoActuatorID id]
        {
            get
            {
                int i = _getIndex(id);
                return m_VD[i] > 0.0;
            }
            set
            {
                int i = _getIndex(id);
                m_VD[i] = value ? 1.0 : 0.0;
            }
        }

        #region CONSTRUCTORS
            public CxPasoIoVector() : base(N_DIMENSIONS) {}
            public CxPasoIoVector(QVector src) : base(src) {}
            public CxPasoIoVector(CxPasoVector src) : base(src) {}
        #endregion

        public override string ToString()
        {
            if (N_DIMENSIONS != 0)
            {
                string str = "(pneumatics=";

                //m_enableFlags
                str += m_enableFlags[0] ? "1" : "0";
                for (int i = 1; i < N_DIMENSIONS; i++)
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
                //return (int)(id - PasoActuatorID.ControlBox) - 1;
            return (id - 1000);
            }
        #endregion
    }

    public class CxPasoVector : QVector
    {
        protected bool[] m_enableFlags;

        public void EnableAll()
        {
            for (int i = 0; i < m_enableFlags.Length; i++)
                m_enableFlags[i] = true;
        }
        public void DisableAll()
        {
            for (int i = 0; i < m_enableFlags.Length; i++)
                m_enableFlags[i] = false;
        }
        public void Enable(int idx)
        {
            m_enableFlags[idx] = true;
        }
        public void Disable(int idx)
        {
            m_enableFlags[(int)idx] = false;
        }
        public bool IsEnabled(int idx)
        {
            return m_enableFlags[(int)idx];
        }

        #region CONSTRUCTORS

        public CxPasoVector(int iSize)
            : base(iSize)
        {
            m_enableFlags = new bool[iSize];
            EnableAll();
        }

        public CxPasoVector(QVector src)
            : base(src)
        {
            m_enableFlags = new bool[src.Size];
            EnableAll();
        }

        public CxPasoVector(CxPasoVector src)
            : base(src)
        {
            m_enableFlags = new bool[src.Size];
            CopyFrom(src);
        }

        public CxPasoVector(double[] src)
            : base(src.Length)
        {
            m_enableFlags = new bool[src.Length];
            Array.Copy(src, base.V, src.Length);
        }

        #endregion

        public void CopyFrom(CxPasoVector src)
        {
            for (int i = 0; i < src.Size; i++)
            {
                this.m_VD[i] = src.m_VD[i];
                this.m_enableFlags[i] = src.m_enableFlags[i];
            }
        }
        public void SmartCopyFrom(CxPasoVector src)
        {
            for (int i = 0; i < src.Size; i++)
            {
                if (src.IsEnabled(i))
                {
                    this.m_VD[i] = src.m_VD[i];
                    //> this.m_enableFlags[i] = src.m_enableFlags[i];
                }
            }
        }
    }
}
