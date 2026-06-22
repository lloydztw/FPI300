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
using System.Windows.Forms;

namespace JetEazy.Actuactor
{
    /// <summary>
    /// Not Thread Safe !!!
    /// </summary>
    public class EzGuiTimerTickSource : IxTickSource
    {
        #region PRIVATE_KERNEL_DATA
        List<IxTickDriven> _tickees = new List<IxTickDriven>();
        Timer _timer;
        #endregion

        public EzGuiTimerTickSource(int interval = 100)
        {
            _timer = new Timer();
            _timer.Interval = interval;
            _timer.Enabled = false;
            _timer.Tick += _timer_Tick;
        }
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Dispose();
                _timer = null;
            }
        }

        public Timer Timer => _timer;
        public void Connect(params IxTickDriven[] tickees)
        {
            if (tickees == null || tickees.Length == 0)
                return;

            bool flag = Enabled;
            Enabled = false;

            foreach (var t in tickees)
            {
                if (t != null && !_tickees.Contains(t))
                    _tickees.Add(t);
            }

            Enabled = flag;
        }
        public void Disconnect(params IxTickDriven[] tickees)
        {
            if (tickees == null || tickees.Length == 0)
                return;

            bool flag = Enabled;
            Enabled = false;

            foreach (var t in tickees)
            {
                if (t != null)
                    _tickees.Remove(t);
            }

            Enabled = flag;
        }
        public bool Enabled
        {
            get
            {
                return _timer != null && _timer.Enabled;
            }
            set
            {
                if (_timer != null) _timer.Enabled = value;
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            foreach (var t in _tickees)
                t?.Tick();
        }
    }
}
