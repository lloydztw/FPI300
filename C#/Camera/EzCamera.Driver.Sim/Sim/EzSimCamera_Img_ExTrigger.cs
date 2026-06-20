#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using EzCamera.Interface;
using System;
using System.Threading;

namespace EzCamera.Driver.Sim
{
    /// <summary>
    /// 模擬硬體觸發相機
    /// </summary>
    public class EzSimCameraExTrigger : EzSimImageCamera
    {
        #region DEFAULT
        public static new EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("Sim", "模擬硬體觸發相機", index: camID);
        }
        #endregion

        #region PRIVATE_DATA
        //volatile bool _inTrigger = false;
        bool _simTriggerRunFlag = false;
        Action _simTriggerAction = null;
        ManualResetEvent _isFree = new ManualResetEvent(false);
        #endregion

        public EzSimCameraExTrigger(int camID) 
            : this(DefaultDeviceInfo(camID))
        {
            //停用 polling thread
            OPT_USE_POLLING_THREAD = false;
        }
        public EzSimCameraExTrigger(IEzDeviceInfo info)
            : base(info)
        {
            //停用 polling thread
            OPT_USE_POLLING_THREAD = false;
        }

        public override bool IsLiveMode()
        {
            return _simTriggerRunFlag || _simTriggerAction != null;
        }
        public override void StartLiveMode()
        {
            if (IsLiveMode())
                return;

            //開始模擬 硬體觸發 
            _simTriggerRunFlag = true;
            _simTriggerAction = one_shot_trigger;
            _simTriggerAction.BeginInvoke(null, null);

            // Event
            fireLiveModeChanged();
        }
        public override void StopLiveMode()
        {
            //停止模擬 硬體觸發 
            _simTriggerRunFlag = false;
            _simTriggerAction = null;


            base.unlockBufs();

            //等待 完全終止
            _isFree.WaitOne(3000);

            // Event
            //fireLiveModeChanged();
        }

        #region 模擬硬體觸發
        private void one_shot_trigger()
        {
            //_inTrigger = true;
            _isFree.Reset();

            if (_simTriggerRunFlag)
            {
                var liveBmp = base.drvCaptureImage();

                pushOneLiveImage(liveBmp);
            }

            if (_simTriggerRunFlag)
            {
                _simTriggerAction?.BeginInvoke(null, null);
            }

            if (!_simTriggerRunFlag)
            {
                _simTriggerAction = null;
                fireLiveModeChanged();
            }

            _isFree.Set();
        }
        #endregion
    }
}
