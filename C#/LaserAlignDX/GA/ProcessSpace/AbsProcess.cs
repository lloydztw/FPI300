#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Threading;

namespace NeedleX.ProcessSpace
{
    /// <summary>
    /// IxProcess <br/>
    /// 最通用廣義的 Interface <br/>
    /// 為避免不熟悉者, 可以使用 AbsProcess 當基礎 <br/>
    /// 將來慢慢將共通的 Methods 收集至此處 <br/>
    /// @LETIAN: 20220619 First Creation
    /// </summary>
    public interface IxProcess
    {
        //event EventHandler OnStateChanged;
        //event EventHandler<ProcessEventArgs> OnError;
        event EventHandler<ProcessEventArgs> OnMessage;
        event EventHandler<ProcessEventArgs> OnCompleted;
        void Tick();
    }


    /// <summary>
    /// AbsProcess <br/>
    /// ---------------------------------------------------------------- <br/>
    /// (1) 跨 project 通用之 abstrace class <br/>
    /// (2) 暫時 Reuse ProcessClass (for backward compatibility) <br/>
    /// (3) ProcessClass 以後有待 優化. <br/>
    /// ---------------------------------------------------------------- <br/>
    /// @LETIAN: 20220619 First Creation
    /// </summary>
    public abstract class AbsProcess : ProcessClass, IxProcess
    {
        #region PROTECTED_DATA_MEMBERS
        protected int _defaultDuration = 100;
        #endregion

        //>>> public event EventHandler OnStateChanged;
        public event EventHandler<ProcessEventArgs> OnNG;
        public event EventHandler<ProcessEventArgs> OnMessage;
        public event EventHandler<ProcessEventArgs> OnStarted;
        public event EventHandler<ProcessEventArgs> OnCompleted;
        public event EventHandler<ProcessEventArgs> OnLiveImage;

        public virtual string Name
        {
            get { return GetType().Name; }
        }
        public virtual void Start(params object[] args)
        {
            LastNG = null;
            if (args.Length > 0)
                base.Start(args[0]?.ToString());
            else
                base.Start();
        }
        public new virtual void Stop()
        {
            base.Stop();
            //FireCompleted();
        }
        public abstract void Tick();

        protected abstract void InvalidatePlcScanned();
        protected void SetNextState(int id, int nextDuration = -1)
        {
            if (nextDuration >= 0)
                base.NextDuriation = nextDuration;
            else
                base.NextDuriation = _defaultDuration;
            _changeNextState(id); ;
        }

        public new int ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                _changeNextState(value);
            }
        }
        public string LastNG
        {
            get;
            protected set;
        }

        protected void FireNG(string message)
        {
            var e = new ProcessEventArgs() { Message = message };
            OnNG?.Invoke(this, e);
        }
        protected void FireMessage(string message)
        {
            var e = new ProcessEventArgs() { Message = message };
            OnMessage?.Invoke(this, e);
        }
        protected void FireMessage(ProcessEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
        protected void FireStarted(ProcessEventArgs e = null)
        {
            OnStarted?.Invoke(this, e);
        }
        protected void FireCompleted(ProcessEventArgs e = null)
        {
            if (e == null)
            {
                e = new ProcessEventArgs()
                {
                    Message = string.Format("Completed!")
                };
            }
            OnCompleted?.Invoke(this, e);
        }
        /// <summary>
        /// Caller 必須負責 bmp 的 life-cycle !!!
        /// </summary>
        protected void FireLiveImaging(Bitmap bmp)
        {
            try
            {
                OnLiveImage?.Invoke(this, new ProcessEventArgs("live.image", bmp));
            }
            catch (Exception ex)
            {
                //_LOG(ex, "FireLiveImaging 異常!");
            }
        }

        #region PRIVATE_FUNCTIONS
        private void _changeNextState(int id)
        {
            bool isChanged = (base.ID != id);
            base.ID = id;

            if (isChanged || true)
            {
                // 2022/10/22
                // 改變狀態, 代表 需要等待 plc 更新.
                // 自動清除 plc scanned 標記. 
                InvalidatePlcScanned();
            }

            //if (isChanged)
            //    OnStateChanged?.Invoke(this, null);

            //// TRACE Delay Time
            //if (false && NextDuriation >= 300)
            //{
            //    GdxGlobal.LOG.Log(GetType().Name, "Delay", NextDuriation, Color.DarkCyan);
            //}
        }
        #endregion
    }
}
