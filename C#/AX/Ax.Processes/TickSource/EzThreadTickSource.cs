#region AUTHOR
/*
 * <FileName>
 * 
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * 2012-03-22 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * letian@jeteazy.com
 * lloydz.tw@gmail.com.tw
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Timer = System.Threading.Timer;

namespace JetEazy.Actuactor
{
    public class EzThreadTickSource : IxTickSource
    {
        #region PRIVATE_KERNEL_DATA
        //ConcurrentBag<IxTickDriven> _tickees = new ConcurrentBag<IxTickDriven>();
        List<IxTickDriven> _tickees = new List<IxTickDriven>();
        object _sync = new object();
        Timer _timer;
        int _interval;
        bool _enabled;
        #endregion

        #region RUNTIME_DATA
        volatile bool _entry = false;
        int _reEntryCount = 0;
        #endregion

        public EzThreadTickSource(int interval = 100)
        {
            _interval = interval;
            _timer = new Timer(tick);
        }
        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }
        
        public Timer Timer => _timer;
        public void Connect(params IxTickDriven[] tickees)
        {
            if (tickees == null || tickees.Length == 0)
                return;

            bool flag = Enabled;
            Enabled = false;

            lock (_sync)
            {
                foreach (var t in tickees)
                {
                    if (t != null && !_tickees.Contains(t))
                        _tickees.Add(t);
                }
            }

            Enabled = flag;
        }
        public void Disconnect(params IxTickDriven[] tickees)
        {
            if (tickees == null || tickees.Length == 0)
                return;

            bool flag = Enabled;
            Enabled = false;

            lock (_sync)
            {
                foreach (var t in tickees)
                {
                    if (t != null)
                        _tickees.Remove(t);
                }
            }

            Enabled = flag;
        }
        public bool Enabled
        {
            get
            {
                return _enabled && _timer != null;
            }
            set
            {
                _enabled = value;
                if (_timer != null)
                {
                    if (_enabled)
                    {
                        _timer.Change(0, _interval);
                    }
                    else
                    {
                        _timer.Change(int.MaxValue, int.MaxValue);
                        _entry = false;
                    }
                }
            }
        }

        void tick(object arg)
        {
            if (!_enabled)
                return;

            if (!_entry)
            {
                _entry = true;
                lock (_sync)
                {
                    foreach (var tickee in _tickees)
                    {
                        tickee.Tick();
                    }
                }
                _entry = false;
                _reEntryCount = 0;
            }
            else
            {
                Debug.WriteLine("{0}: Re-Entry = {1}", this, ++_reEntryCount);
            }
        }
    }
}
