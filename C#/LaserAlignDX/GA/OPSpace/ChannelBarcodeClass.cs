using JetEazy.ControlSpace.PLCSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Traveller106.ControlSpace.IOSpace;
using VsCommon.ControlSpace.IOSpace;

namespace TravellerMINIX6.OPSpace
{
    public class ChannelBarcodeClass
    {
        MiniX6IOClass PLCIO;
        EzBarcodeM3DHelper ezBarcodeM3D;

        private int m_base = 0;
        private int m_channelindex = 0;
        private string m_channelname = string.Empty;
        private string m_barcodeCurrent = string.Empty;
        private int m_ReadCount = 3;
        private bool m_forcepass = false;//读码强制通过
        private List<string> m_barcodelist = new List<string>();
        private List<string> m_bindbarcodelist = new List<string>();

        public List<string> Bindbarcodelist
        {
            get { return m_bindbarcodelist; }
        }
        public List<string> Barcodelist
        {
            get { return m_barcodelist; }
        }
        public bool Forcepass
        {
            get { return m_forcepass; }
            set { m_forcepass = value; }
        }
        public int ReadCount
        {
            get { return m_ReadCount; }
            set { m_ReadCount = value; }
        }
        public string Barcode
        {
            get { return m_barcodeCurrent; }
            //set { m_barcodeCurrent = value; }
        }
        public string ChannelName
        {
            get { return m_channelname; }
            //set { m_channelname = value; }
        }
        public int ChannelIndex
        {
            get { return m_channelindex; }
            //set { m_channelindex = value; }
        }
        public int Base
        {
            get { return m_base; }
            //set { m_base = value; }
        }
        public bool IsGetBarcodeStart
        {
            get { return PLCIO.GetBit(m_base); }
            //set { PLCIO.SetQXQB(m_base, value); }
        }
        public bool GetBarcodeComplete
        {
            //get { return PLCIO.GetQXQB(m_base + 1); }
            set { PLCIO.SetBit(m_base + 1, value); }
        }
        public bool IsBindBarcodeStart
        {
            get { return PLCIO.GetBit(m_base + 2); }
            //set { PLCIO.SetQXQB(m_base, value); }
        }
        public bool BindBarcodeComplete
        {
            //get { return PLCIO.GetQXQB(m_base + 1); }
            set { PLCIO.SetBit(m_base + 3, value); }
        }
        public ChannelBarcodeClass(int echannelindex, int ebaseindex, MiniX6IOClass eplcio, EzBarcodeM3DHelper ebarcode)
        {
            m_channelindex = echannelindex;
            m_channelname = "Ch" + echannelindex.ToString();
            PLCIO = eplcio;
            m_base = ebaseindex;
            ezBarcodeM3D = ebarcode;
        }
        public void Tick()
        {
            IsGetBarcodeStartGo = IsGetBarcodeStart;
            //if (GetbarcodeStartTrigered)
            //{
            //    if (!is_thread_running())
            //    {
            //        start_scan_thread(new object());
            //    }
            //}
            IsBindBarcodeStartGo = IsBindBarcodeStart;
            //if (BindbarcodeStartTrigered)
            //{
            //    if (m_barcodelist.Count > 0)
            //    {
            //        string m_bartmp = m_barcodelist[0];
            //        m_barcodelist.RemoveAt(0);
            //        m_bindbarcodelist.Add(m_bartmp);
            //        BindBarcodeComplete = true;
            //    }
            //}
        }
        public void Reset()
        {
            m_barcodelist.Clear();
            m_bindbarcodelist.Clear();
        }
        public void Dispose()
        {
            stop_scan_thread();
        }
        public string GetBindBarcodeFrist()
        {
            if (m_bindbarcodelist.Count <= 0)
                return "ErrCode";

            string str = m_bindbarcodelist[0];
            m_bindbarcodelist.RemoveAt(0);
            return str;
        }

        bool GetbarcodeStartTrigered = false;
        bool GetbarcodeStartnow = false;
        bool IsGetBarcodeStartGo
        {
            get
            {
                return GetbarcodeStartnow;
            }
            set
            {
                if (GetbarcodeStartnow != value)
                {
                    if (value)
                        GetbarcodeStartTrigered = true;
                    else
                        GetbarcodeStartTrigered = false;

                    GetbarcodeStartnow = value;
                    if (GetbarcodeStartTrigered)
                    {
                        if (!is_thread_running())
                        {
                            start_scan_thread(new object());
                        }
                    }
                }
                else
                    GetbarcodeStartTrigered = false;
            }
        }

        bool BindbarcodeStartTrigered = false;
        bool BindbarcodeStartnow = false;
        bool IsBindBarcodeStartGo
        {
            get
            {
                return BindbarcodeStartnow;
            }
            set
            {
                if (BindbarcodeStartnow != value)
                {
                    if (value)
                        BindbarcodeStartTrigered = true;
                    else
                        BindbarcodeStartTrigered = false;

                    BindbarcodeStartnow = value;

                    if (BindbarcodeStartTrigered)
                    {
                        if (m_barcodelist.Count > 0)
                        {
                            string m_bartmp = m_barcodelist[0];
                            m_barcodelist.RemoveAt(0);
                            m_bindbarcodelist.Add(m_bartmp);
                            BindBarcodeComplete = true;
                            FireChangeState("2$" + m_barcodeCurrent);
                        }
                    }
                }
                else
                    BindbarcodeStartTrigered = false;
            }
        }

        #region PRIVATE_THREAD_FUNCTIONS
        private Thread _thread = null;
        private bool _runFlag = false;
        private bool _isThreadStopping = false;

        protected bool is_thread_running()
        {
            return _runFlag || _thread != null;
        }
        protected void start_scan_thread(object phase)
        {
            if (!is_thread_running())
            {
                _runFlag = true;
                _thread = new Thread(thread_func);
                _thread.Name = this.m_channelname;
                _thread.Start(phase);
            }
            else
            {
                //GdxGlobal.LOG.Warn("有 Thread 尚未結束");
            }
        }
        protected void stop_scan_thread(int timeout = 3000)
        {
            if (is_thread_running())
            {
                _runFlag = false;
                var stopFunc = new Action<int>((tmout) =>
                {
                    if (!_isThreadStopping)
                    {
                        _isThreadStopping = true;
                        try
                        {
                            var t = _thread;
                            if (t != null)
                            {
                                if (!t.Join(tmout))
                                    t.Abort();
                                _thread = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            //GdxGlobal.LOG.Warn(ex, "Thread 終止異常!");
                        }
                        _isThreadStopping = false;
                    }
                });
                stopFunc.BeginInvoke(timeout, null, null);
            }
        }
        private void thread_func(object arg)
        {
            var phase = (object)arg;

            while (_runFlag)
            {
                try
                {
                    bool barcodecomplete = false;
                    int readindex = 0;
                    m_barcodeCurrent = string.Empty;

                    if (m_forcepass)
                    {
                        m_barcodeCurrent = m_channelname + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        barcodecomplete = true;
                    }
                    else
                    {
                        while (readindex < m_ReadCount)
                        {
                            m_barcodeCurrent = ezBarcodeM3D.Run();

                            //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
                            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                            m_barcodeCurrent = r.Replace(m_barcodeCurrent, "");
                            m_barcodeCurrent = m_barcodeCurrent.ToUpper();
                            m_barcodeCurrent = m_barcodeCurrent.Replace("Z", "");
                            if (!string.IsNullOrEmpty(m_barcodeCurrent))
                            {
                                barcodecomplete = true;
                                break;
                            }
                            readindex++;
                        }
                    }

                    if (barcodecomplete)
                    {
                        m_barcodelist.Add(m_barcodeCurrent);
                        GetBarcodeComplete = true;
                        FireChangeState("1$" + m_barcodeCurrent);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (_runFlag)
                    {
                        try
                        {
                            //_LOG(ex, "live compensating 異常!");
                            //SetNextState(9999);
                        }
                        catch
                        {
                        }
                    }
                    break;
                }
            }

            _runFlag = false;
            _thread = null;

            //int nextState = phase.ExitCode;
            //SetNextState(nextState);
            //base.IsOn = true;
        }
        #endregion

        public delegate void ChangeStateHandler(string statusstr);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(string statusstr)
        {
            if (OnChangeState != null)
            {
                OnChangeState(statusstr);
            }
        }
    }
}
