using System;
using JetEazy.QMath;

namespace Paso
{
    public class CxPasoMotorVector : QVector
    {
        public const int N_DIMENSIONS = 7;

        public bool[] EnableFlags = new bool[N_DIMENSIONS];

        public double X
        {
            get { return m_VD[0]; }
            set { m_VD[0] = value; }
        }
        public double Y
        {
            get { return m_VD[1]; }
            set { m_VD[1] = value; }
        }
        public double Z
        {
            get { return m_VD[2]; }
            set { m_VD[2] = value; }
        }
        public double XB
        {
            get { return m_VD[3]; }
            set { m_VD[3] = value; }
        }
        public double YB
        {
            get { return m_VD[4]; }
            set { m_VD[4] = value; }
        }
        public double ZB
        {
            get { return m_VD[5]; }
            set { m_VD[5] = value; }
        }
        public double Theta
        {
            get { return m_VD[6]; }
            set { m_VD[6] = value; }
        }

        /*
        public bool ActionFrameLoader;
        public bool ActionLoaderClip;
        public bool ActionThetaClip;
        public bool ActionStamp;
        */ 

        public object Tag;

        public void EnableAll()
        {
            for (int i = 0; i < N_DIMENSIONS; i++)
                EnableFlags[i] = true;
        }
        public void DisableAll()
        {
            for (int i = 0; i < N_DIMENSIONS; i++)
                EnableFlags[i] = false;
        }
        public void Enable(PasoMotorID id)
        {
            EnableFlags[(int)id] = true;
        }
        public void Disable(PasoMotorID id)
        {
            EnableFlags[(int)id] = false;
        }
        public bool IsEnabled(PasoMotorID id)
        {
            return EnableFlags[(int)id];
        }

        public CxPasoMotorVector() : base(N_DIMENSIONS)
        {
            EnableAll();
        }
        public CxPasoMotorVector(QVector src) : base(src)
        {
            EnableAll();
        }
        public CxPasoMotorVector(CxPasoMotorVector src) : base(N_DIMENSIONS)
        {
            CopyFrom(src);
        }
        public void CopyFrom(CxPasoMotorVector src)
        {
            for (int i = 0; i < N_DIMENSIONS; i++)
            {
                this.m_VD[i] = src.m_VD[i];
                this.EnableFlags[i] = src.EnableFlags[i];
            }

            /*
            this.ActionFrameLoader = src.ActionFrameLoader;
            this.ActionLoaderClip = src.ActionLoaderClip;
            this.ActionThetaClip = src.ActionThetaClip;
            this.ActionStamp = src.ActionStamp;
            */ 
            
            this.Tag = src.Tag;
        }

        public override string ToString()
        {
            /*
            return string.Format("x={0:0.000}, y={1:0.000}, z={2:0.000}\n\rxb={3:0.000},yb={4:0.000},zb={5:0.000}",
                                    X, Y, Z, XB, YB, ZB);
            */
            return string.Format("t=({0:0.000},{1:0.000},{2:0.000}) b=({3:0.000},{4:0.000},{5:0.000}) zb={6:0.000}",
                                    X, Y, Z, XB, YB, Theta, ZB);
        }
    }
}
