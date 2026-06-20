#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2022-09-16 LeTian Chang: 重整
 * 2009-11-30 LeTian Chang: Creation.
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using EzIO.Mem.Utils;
using System;
using System.Collections.Generic;


namespace EzIO.Sim
{
    public class IoRandomSimulator : IDisposable
    {
        #region PRIVATE_DATA
        IEnumerable<IoPoint> _ioPoints;
        Random _rnd = new Random();
        EzThreadRunner _thread;
        #endregion

        public IoRandomSimulator(IEnumerable<IoPoint> ioPoints)
        {
            _ioPoints = ioPoints;
            _thread = new EzThreadRunner(this, simFunc);
        }
        public IoRandomSimulator(IoMemory ioMem)
            : this(new List<IoPoint>(ioMem.IterPoints()))
        {
        }
        public void Dispose()
        {
            _thread?.Stop();
            _thread?.Dispose();
        }
        public void Start(int interval = 250)
        {
            _thread.IntervalMs = interval;
            _thread.Start();
        }

        #region PRIVATE_FUNCTIONS
        void simFunc()
        {
            foreach (var pt in _ioPoints)
            {
                if (pt.Bits == 1)
                {
                    int data = _rnd.Next() % 2;
                    pt.Write((uint)data);
                }
                else if (pt.Bits <= 16)
                {
                    int data = _rnd.Next(5128);
                    pt.Write((uint)data);
                }
                else
                {
                    float value = (float)(_rnd.NextDouble() * 10000);
                    var u32 = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
                    pt.Write(u32);
                }
            }
        }
        #endregion
    }
}
