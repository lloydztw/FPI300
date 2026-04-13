#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using System;


namespace JetEazy.Drivers.Motor
{
    public class MotVector : QVector
    {
        #region PROTECTED_DATA
        protected bool[] m_enableFlags;
        #endregion

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
        public bool IsAnyEnabled()
        {
            foreach(bool enabled in m_enableFlags)
            {
                if (enabled) return true;
            }
            return false;
        }
        
        public void Enable(int id)
        {
            m_enableFlags[id] = true;
        }
        public void Disable(int id)
        {
            m_enableFlags[(int)id] = false;
        }
        public bool IsEnabled(int id)
        {
            return m_enableFlags[(int)id];
        }
        
        public MotVector(int dims)
            : base(dims)
        {
            m_enableFlags = new bool[dims];
            EnableAll();
        }
        public MotVector(QVector src)
            : base(src)
        {
            m_enableFlags = new bool[src.Size];
            if (src is MotVector vector)
                CopyFrom(vector);
            else
                EnableAll();
        }
        public MotVector(double[] src)
            : base(src.Length)
        {
            m_enableFlags = new bool[src.Length];
            Array.Copy(src, base.V, src.Length);
        }

        public bool IsOn(int id)
        {
            return (int)m_VD[id] > 0;
        }
        public bool IsOff(int id)
        {
            return (int)m_VD[id] == 0;
        }
        public void Set(int id, bool on)
        {
            m_VD[id] = on ? 1 : 0;
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        public void CopyFrom(MotVector src)
        {
            for (int i = 0; i < src.Size; i++)
            {
                this.m_VD[i] = src.m_VD[i];
                this.m_enableFlags[i] = src.m_enableFlags[i];
            }
        }
        
        /// <summary>
        /// 只複製 src 被 enabled 欄位 的數值 <br/>
        /// 原來的 enabled flags 維持不動 !!!
        /// </summary>
        public void SmartCopyFrom(MotVector src)
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
