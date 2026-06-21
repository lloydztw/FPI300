#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework.Gui;
using System;
using System.Windows.Forms;


namespace EzAoiChipLocQC.Ctrl
{
    /// <summary>
    /// 常用的 GUI Functions
    /// </summary>
    internal partial class BaseUtil
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        protected NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region PRIVATE_DATA
        private Form _frmMain;
        #endregion

        internal Form frmMain
        {
            get
            {
                if (_frmMain == null)
                    _frmMain = SearchAwMainForm();
                return _frmMain;
            }
            set
            {
                _frmMain = value;
            }
        }
        
        public static void setEnable(Control wnd, bool enable)
        {
            if (wnd != null)
                wnd.Enabled = enable;
        }
        public static void setVisible(Control wnd, bool enable)
        {
            if (wnd != null)
                wnd.Visible = enable;
        }
        public static void setNum(NumericUpDown num, decimal value)
        {
            if (num != null)
            {
                if (value > num.Maximum)
                    value = num.Maximum;
                else if (value < num.Minimum)
                    value = num.Minimum;
                else { }
                num.Value = value;
            }
        }
        public static void setNum(NumericUpDown num, double value)
        {
            setNum(num, (decimal)value);
        }
        public static void setNum(NumericUpDown num, int value)
        {
            setNum(num, (decimal)value);
        }
        public static void setIndex(ComboBox cbo, int index)
        {
            if (cbo != null)
                cbo.SelectedIndex = Math.Min(index, cbo.Items.Count - 1);
        }

        public static Cursor setCursor(Control wnd, Cursor cur)
        {
            if (wnd != null)
            {
                var old = wnd.Cursor;
                wnd.Cursor = cur;
                return old;
            }
            return Cursors.Default;
        }
        public Cursor setCursor(Cursor cur)
        {
            //if (frmMain != null)
            //{
            //    var old = frmMain.Cursor;
            //    frmMain.Cursor = cur;
            //    return old;
            //}
            return Cursors.Default;
        }
    }

    partial class BaseUtil
    {
        #region PRIVATE_DATA
        static FormAwMain _frmAwMain;
        #endregion

        public static FormAwMain SearchAwMainForm()
        {
            if (_frmAwMain != null)
                return _frmAwMain;
            foreach (var frm in Application.OpenForms)
                if (frm is FormAwMain frmMain)
                    return (_frmAwMain = frmMain);
            return null;
        }
        public static void PostAsyncInvoke(int delay, Action action)
        {
            if (_frmAwMain == null)
                SearchAwMainForm();

            new Action<Action>((func) =>
            {
                System.Threading.Thread.Sleep(delay);
                _frmAwMain?.BeginInvoke(func);
            }).BeginInvoke(action, null, null);
        }
        public static void PostAsyncInvoke(int delay, Action<object> action, object arg)
        {
            if (_frmAwMain == null)
                SearchAwMainForm();

            new Action<Action<object>, object>((func, a) =>
            {
                System.Threading.Thread.Sleep(delay);
                _frmAwMain?.BeginInvoke(func, a);
            }).BeginInvoke(action, arg, null, null);
        }
    }
}
