#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-09 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzEmptyTrayInspector.Model;
using EzEmptyTrayInspector.Model.Aoi;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace EzEmptyTrayInspector.Ctrl
{
    /// <summary>
    /// 拉框 實時濾波 顯示效果
    /// </summary>
    public class CviFiltersBox : CvImageViewerInteractor
    {
        #region CONFIG
        //JetColorsMap _COLORS = new JetColorsMap(MAX_SHOW_NUMBER);
        static Color[] _COLORS = new Color[]
        {
            Color.Lime,
            Color.Cyan,
            Color.LightBlue,
            Color.Blue,
            Color.DarkCyan,
        };
        int MAX_SHOW_NUMBER => _COLORS.Length;
        #endregion

        #region PRIVATE_DATA
        Control _wndHost;
        List<RotatedRect> _rotatedRects;
        JxRotAngleSettings _rotSettings;
        IEzImage _imgSource;
        SideID _sideId;
        #endregion

        #region PRIVATE_RUNTIME_TAGS
        int _applyDelay = 500;
        bool _isApplying = false;
        DateTime _applyTime = DateTime.Now;
        int _autoClearDelay = 5000;
        bool _isAutoClearing = false;
        DateTime _autoClearTime = DateTime.Now;
        #endregion

        public CviFiltersBox(params object[] args)
        {
        }
        public void ApplyFilters(SideID sideId, IEzImage imgSource, JxRotAngleSettings rotSettings, Control wnd)
        {
            _wndHost = wnd;
            _sideId = sideId;
            _imgSource = imgSource;
            _rotSettings = rotSettings;
            
            if (_isApplying)
            {
                // 防止使用者一直連續改變參數設定值
                _applyTime = DateTime.Now;
                return;
            }

            _isApplying = true;
            ThreadPool.QueueUserWorkItem(delegate
            {
                int ms = _applyDelay;
                while (ms > 0)
                {
                    Thread.Sleep(ms);
                    var ts = DateTime.Now - _applyTime;
                    ms = _applyDelay - (int)ts.TotalMilliseconds;
                }
                apply_filters(_sideId, _imgSource, _rotSettings, _wndHost);
                _isApplying = false;
            });
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (Visible && Enabled)
            {
                draw_rotated_rects(viewer, gxView);
            }
        }
        #endregion

        #region PRIVATE_DRAW_FUNCTIONS
        private void draw_rotated_rects(CvImageViewer viewer, Graphics gxView)
        {
            if (_rotatedRects == null || _rotatedRects.Count == 0)
                return;

            bool isWorld = viewer.IsInWorldCoordinate();

            if (isWorld)
                viewer.SwitchToViewportCoordinate(gxView);

            int N = Math.Min(_rotatedRects.Count, MAX_SHOW_NUMBER);

            for (int id = 0; id < N; id++)
            {
                var pts = Cv2.BoxPoints(_rotatedRects[id]);

                int len = pts.Length;
                for (int i = 0; i < len; i++)
                    viewer.TransWorldToViewport(ref pts[i].X, ref pts[i].Y);

                using (var pen = new Pen(_COLORS[id], 5f))
                {
                    for (int i = 0; i < len; i++)
                    {
                        int j = (i + 1) % len;
                        gxView.DrawLine(pen, pts[i].X, pts[i].Y, pts[j].X, pts[j].Y);
                    }
                }
            }

            int id0 = 0;
            using (Font font = new Font(FontFamily.GenericMonospace, 16f, FontStyle.Bold))
            using (Brush br = new SolidBrush(_COLORS[id0]))
            {
                var rotRect = _rotatedRects[id0];
                var bound = rotRect.BoundingRect();
                float tx = bound.X;
                float ty = bound.Y;
                viewer.TransWorldToViewport(ref tx, ref ty);

                double angle = EzAoiBaseUtil.GetNormalizedAngle(rotRect);
                string txt = $"Angle = {angle:0.00}°";
                gxView.DrawString(txt, font, br, tx, ty);

                //// DEBUG
                //txt = rotRect.Size.Width > rotRect.Size.Height ? " (W > H)" : " (W <= H)";
                //gxView.DrawString(txt, font, br, tx, ty+18);
            }

            if (isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        private void draw_feature_image(CvImageViewer viewer, Graphics gxView)
        {
            // Reserved
        }
        #endregion

        #region PRIVATE_MAJOR_FUNCTIONS
        void apply_filters(SideID sideId, IEzImage imgSource, JxRotAngleSettings rotSettings, Control wnd)
        {
            _wndHost = wnd;

            var model = Global.AoiModel;
            if (model != null && imgSource != null && rotSettings != null && rotSettings.Enabled)
            {
                model.TryApplyFilters(sideId, imgSource, rotSettings, out object result);
                if (result is List<RotatedRect> rotRects)
                {
                    _rotatedRects = rotRects;

                    Visible = true;
                    _wndHost?.Invalidate();

                    auto_clear(5000);
                    return;
                }
            }

            if (_rotatedRects != null)
            {
                _rotatedRects = null;
                _wndHost?.Invalidate();
            }
        }
        void auto_clear(int delay)
        {
            _autoClearTime = DateTime.Now;
            _autoClearDelay = delay;
            if (_isAutoClearing)
                return;

            _isAutoClearing = true;
            ThreadPool.QueueUserWorkItem((arg) =>
            {
                int ms = _autoClearDelay;
                while (ms > 0)
                {
                    Thread.Sleep(ms);
                    var ts = DateTime.Now - _autoClearTime;
                    ms = _autoClearDelay - (int)ts.TotalMilliseconds;
                }
                _rotatedRects = null;
                _wndHost?.Invoke((Action)_wndHost.Invalidate);
                _isAutoClearing = false;
            }, null);
        }
        #endregion
    }
}
