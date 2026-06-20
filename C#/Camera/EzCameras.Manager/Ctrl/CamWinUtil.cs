#region AUTHOR
/*
 * EzCameras.Manager
 * Copyright (C) 2025
 * 2025-09-01 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Interface;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzCamera.Manager
{
    public static class CamWinUtil
    {
        /// <summary>
        /// 目前為配合 DirectShow 內部視窗, 需要用一些非同步的技巧, 才能安全關閉主視窗
        /// </summary>
        public static void SafeClosingForm(IEzCamera camera, Form frmOwner, FormClosingEventArgs e, int delay = 100)
        {
            //if (camera != null && camera.IsLiveMode())
            //{
            //    // 非同步 停止 live Mode
            //    ((Action)camera.StopLiveMode).BeginInvoke(
            //            ar => DelayCloseForm(frmOwner, delay),
            //            null);

            //    // 延遲關閉主視窗
            //    e.Cancel = true;
            //}

            SafeClosingForm(new[] { camera }, frmOwner, e, delay);
        }
        /// <summary>
        /// 目前為配合 DirectShow 內部視窗, 需要用一些非同步的技巧, 才能安全關閉主視窗
        /// </summary>
        public static void SafeClosingForm(IEnumerable<IEzCamera> cameras, Form frmOwner, FormClosingEventArgs e, int delay = 100)
        {
            IEzCamera lastOneLiveCamera = null;
            foreach (var camera in cameras)
            {
                if (camera != null && camera.IsLiveMode())
                    lastOneLiveCamera = camera;
            }

            // 沒有任何 live camera, 可以直接安全關窗
            if (lastOneLiveCamera == null)
            {
                e.Cancel = false;
                return;
            }

            foreach (var camera in cameras)
            {
                // 延遲關閉主視窗
                e.Cancel = true;

                if (camera != null && camera.IsLiveMode())
                {
                    // 非同步 停止 live Mode
                    if (camera != lastOneLiveCamera)
                        ((Action)camera.StopLiveMode).BeginInvoke(null, null);
                    else
                        ((Action)camera.StopLiveMode).BeginInvoke(
                                ar => DelayCloseForm(frmOwner, delay),
                                null);
                }
            }
        }
        /// <summary>
        /// 目前為配合 DirectShow 內部視窗, 需要用一些非同步的技巧, 才能安全 開啟 相機選擇器
        /// </summary>
        public static void SafeBrowseCameraDevice(IEzCamera camera, Form frmOwner, Action<IEzCamera> callback)
        {
            System.Diagnostics.Trace.Assert(frmOwner != null);
            System.Diagnostics.Trace.Assert(callback != null);

            new Action(() =>
            {
                if (camera != null)
                {
                    camera.StopLiveMode();
                    for (int t = 0; t < 10 && camera.IsLiveMode(); t++)
                        System.Threading.Thread.Sleep(100);
                }

                frmOwner.BeginInvoke(new Action<IEzCamera>((oldCamera) =>
                {
                    var mgr = AppCamerasManager.Instance;
                    var newCamera = mgr.BrowseCameraConfig();
                    if (newCamera != null && newCamera != oldCamera)
                    {
                        callback(newCamera);
                    }
                }), camera);

            }).BeginInvoke(null, null);
        }
        /// <summary>
        /// 延後 delay (ms) 進入 live mode,
        /// 讓 GUI 所有視窗, 有時間進行第一次完整重畫.
        /// </summary>
        public static void StartFirstTimeLiveMode(IEzCamera camera, int delay)
        {
            // 延後 delay (ms) 進入 live mode,
            // 讓 GUI 所有視窗, 有時間進行第一次完整重畫.

            if (camera == null)
                return;

            new Action(() =>
            {
                System.Threading.Thread.Sleep(delay);
                if (camera != null)
                {
                    if (!camera.IsLarge())
                        camera.StartLiveMode();
                    else
                        camera.TriggerOneFrame();
                }
            }).BeginInvoke(null, null);
        }
        /// <summary>
        /// 非同步延遲關閉 Form
        /// </summary>
        public static void DelayCloseForm(Form frm, int delay)
        {
            try
            {
                if (frm == null)
                    return;

                if (frm.InvokeRequired)
                {
                    frm.Invoke(new Action(() => { DelayCloseForm(frm, delay); }));
                }
                else
                {
                    if (delay <= 0)
                    {
                        frm?.Close();
                    }
                    else
                    {
                        new Action(() =>
                        {
                            System.Threading.Thread.Sleep(delay);
                            DelayCloseForm(frm, 0);

                        }).BeginInvoke(null, null);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
