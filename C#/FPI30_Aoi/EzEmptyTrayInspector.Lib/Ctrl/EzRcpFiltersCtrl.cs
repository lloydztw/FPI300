#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Gui;
using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace EzAoiEmptyTrayInspector.Ctrl
{
    internal class EzRcpFiltersCtrl : BaseUtil, IDisposable
    {
        #region GLOBAL_DATA
        IxEmptyTrayInspector _model => Global.AoiModel;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Control _wndRcpHostPanel;
        IvSingleMatchView _filteredViewPanel;
        Timer _restoreTimer;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        IEzImage _imgSrcOrg;
        JxPreFilterSettings _jxImagePreSettings;
        bool _isRcpEdittingMode = false;
        /// <summary>
        /// 新增 CancellationTokenSource 用於取消過期的運算 Task
        /// </summary>
        CancellationTokenSource _cts;
        #endregion

        public EzRcpFiltersCtrl(Control wndHost, JxPreFilterSettings jxImagePreSettings, IvSingleMatchView filteredViewPanel)
        {
            _wndRcpHostPanel = wndHost;
            _jxImagePreSettings = jxImagePreSettings;
            _filteredViewPanel = filteredViewPanel;
            init_event_handlers();
        }
        public void Dispose()
        {
            CleanUp();
        }

        public void AttachImageSource(IEzImage imgSrc)
        {
            _imgSrcOrg = imgSrc;
        }
        public void Enable(bool editting)
        {
            if(_isRcpEdittingMode != editting)
            {
                _isRcpEdittingMode=editting;
                if (!editting)
                    restoreToOriginalView(0);
            }
        }

        #region EVENT_HANDLERS
        void init_event_handlers()
        {
            connect_recipe_prop_handlers();
            _wndRcpHostPanel.HandleDestroyed += (s,e) => CleanUp();
        }
        void connect_recipe_prop_handlers()
        {
            if(_jxImagePreSettings!=null)
            {
                _jxImagePreSettings.OnModified += _jxImagePreSettings_OnModified;
            }
        }
        void disconnect_recipe_prop_handlers()
        {
            if (_jxImagePreSettings != null)
            {
                _jxImagePreSettings.OnModified -= _jxImagePreSettings_OnModified;
                _jxImagePreSettings = null;
            }
        }
        void _jxImagePreSettings_OnModified(object sender, EventArgs e)
        {
            if (!_isRcpEdittingMode)
                return;

            AsyncApplyFilters(_imgSrcOrg?.Image as Mat);
        }
        #endregion

        void CleanUp()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            disconnect_recipe_prop_handlers();
            restoreToOriginalView(0);
            disposeRestoreTimer();
        }
        void AsyncApplyFilters(Mat imgSrc)
        {
            // 1. 取消並釋放前一次未完成的 CTS
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (!_isRcpEdittingMode)
                return;

            if (imgSrc == null || imgSrc.IsDisposed || imgSrc.Empty())
            {
                restoreToOriginalView(0);
                return;
            }

            // 2. 建立新 CTS
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // 在 UI 主線程深拷貝一份獨立的 Mat 給背景線程使用
            var imgWork = imgSrc.Clone();

            // 3. 異步執行影像處理
            Task.Run(async () =>
            {
                Mat imgFiltered = null;
                try
                {
                    // 防抖延遲 50ms
                    await Task.Delay(50, token);
                    token.ThrowIfCancellationRequested();

                    // 在背景線程進行 OpenCV 影像濾鏡運算 (不佔用 UI)
                    imgFiltered = _model?.TryApplyPreFilters(imgWork);

                    token.ThrowIfCancellationRequested();

                    // 4. 計算 completed，切回 UI 主線程更新畫面
                    if (imgFiltered != null && _wndRcpHostPanel.IsHandleCreated)
                    {
                        _wndRcpHostPanel.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    bool disposeSrc = (imgFiltered != imgWork);
                                    showFilteredView(imgFiltered, "Filtered Image", disposeSrc);
                                }
                                else
                                {
                                    // 切回 UI 後若已被取消，釋放濾鏡圖
                                    if (imgFiltered != imgWork)
                                        imgFiltered?.Dispose();
                                }
                            }
                            catch
                            {
                                if (imgFiltered != imgWork)
                                    imgFiltered?.Dispose();
                            }
                        }));
                    }
                }
                catch (OperationCanceledException)
                {
                    // 中途取消時，若有產生新 Mat 則立即釋放
                    if (imgFiltered != null && imgFiltered != imgWork)
                    {
                        imgFiltered.Dispose();
                    }
                }
                catch
                {
                    if (imgFiltered != null && imgFiltered != imgWork)
                    {
                        imgFiltered.Dispose();
                    }
                    throw;
                }
                finally
                {
                    // imgWork 生命週期僅限於本次背景 Task，結束後必定釋放
                    imgWork?.Dispose();
                    imgWork = null;
                }
            }, token);
        }

        #region PRIVATE_FUNCTIONS
        void showFilteredView(Mat img, string name, bool disposeSrc, int autoCloseDelay = 5000)
        {
            if (img == null)
            {
                restoreToOriginalView(0);
            }
            else
            {
                _filteredViewPanel.UpdateImage(img, "Filtered Image", true);
                _filteredViewPanel.Window.Visible = true;
                restoreToOriginalView(autoCloseDelay);
            }
        }
        void restoreToOriginalView(int delay = 5000)
        {
            var viewer = _filteredViewPanel.Window;
            if (delay <= 0)
            {
                viewer.Visible = false;
                disposeRestoreTimer();
                return;
            }

            if (_restoreTimer == null)
            {
                _restoreTimer = new Timer();
                _restoreTimer.Tick += (s, e) =>
                {
                    _restoreTimer.Stop();
                    viewer.Visible = false;
                };
            }

            _restoreTimer?.Stop();
            _restoreTimer.Interval = delay;
            _restoreTimer?.Start();
        }
        void disposeRestoreTimer()
        {
            _restoreTimer?.Stop();
            _restoreTimer?.Dispose();
            _restoreTimer = null;
        }
        #endregion
    }
}
