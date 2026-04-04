#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.Mvc.Gui
{
    public class CviRuler : CvImageViewerInteractor
    {
        #region PRIVATE_STATIONARY_DATA
        Color _color;
        int _thinkness;
        PointF _p1;
        PointF _p2;
        double _dist;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        static int MAJOR_TICK_INTERVAL = 10;            // 每隔多少mm畫一個主刻度
        static float MAJOR_TICK_LENGTH = 0.85f;         // 主刻度長度 (mm)
        static float MINOR_TICK_LENGTH = 0.50f;         // 次刻度長度 (mm)

        // 預存所有刻度的影像座標
        private struct TickPoint
        {
            public PointF P1;           // 刻度線起點 (影像座標)
            public PointF P2;           // 刻度線終點 (影像座標)
            public PointF PT;           // 刻度線字位 (影像座標)
            public bool IsMajor;
            public string Label;
            public float Angle;
        }
        private float _angle;           // 所有 Tick 共用的角度 (Degrees)
        private PointF _summaryTextPt;  // 總距離文字顯示位置
        private List<TickPoint> _precomputedTicks = new List<TickPoint>();
        #endregion

        public CviRuler(Color c, int thinkness = 3)
        {
            _color = c;
            _thinkness = thinkness;
        }

        public void SetColor(Color c)
        {
            _color = c;
        }
        public void UpdateEndPoints(PointF pixelPt1, PointF pixelPt2, double dist)
        {
            bool isChanged = pixelPt1 != _p1 || pixelPt2 != _p2 || Math.Abs(dist - _dist) > 0.0005;
            if (!isChanged)
                return;

            _p2 = pixelPt1;
            _p1 = pixelPt2;
            _dist = dist;
            UpdateTicks(); // 重新計算
        }

        #region PRIVATE_FUNCTIONS_AND_OVERRIDES
        private void UpdateTicks()
        {
            _precomputedTicks.Clear();
            if (_p1.IsEmpty || _p2.IsEmpty || _dist <= 0)
                return;

            float dx = _p2.X - _p1.X;
            float dy = _p2.Y - _p1.Y;

            float totalPixelLen = (float)Math.Sqrt(dx * dx + dy * dy);
            float unitX = dx / totalPixelLen;
            float unitY = dy / totalPixelLen;
            float pixelPerMm = totalPixelLen / (float)_dist;
            float majorTickLenPixels = MAJOR_TICK_LENGTH * pixelPerMm;
            float minorTickLenPixels = MINOR_TICK_LENGTH * pixelPerMm;

            // 計算共用角度 (Degree)
            _angle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            bool inv = _angle < -90.1f || _angle > 90.1f;
            if (inv)
            {
                _angle += 180f;
                unitX = -unitX;
                unitY = -unitY;
            }

            // summaryTextPt
            _summaryTextPt = new PointF((_p1.X + _p2.X) / 2f, (_p1.Y + _p2.Y) / 2f);
            _summaryTextPt.X -= (-unitY * (majorTickLenPixels + minorTickLenPixels));
            _summaryTextPt.Y -= (unitX * (majorTickLenPixels + minorTickLenPixels));

            // 垂直向量 (相對於影像座標)
            // 這裡先固定一個基礎長度，OnDraw 時不用再算三角函數
            var inv_sign = inv ? -1f : 1;
            for (int mm = 0; mm <= ((int)_dist); mm++)
            {
                bool isMajor = (mm % MAJOR_TICK_INTERVAL == 0);
                float tickLen = isMajor ? majorTickLenPixels : minorTickLenPixels;

                float currentDist = mm * pixelPerMm;
                float centerX = _p1.X + unitX * currentDist * inv_sign;
                float centerY = _p1.Y + unitY * currentDist * inv_sign;

                // 計算垂直偏向量
                float vX = -unitY * tickLen;
                float vY = unitX * tickLen;
                float vtX = -unitY * (tickLen + minorTickLenPixels);
                float vtY = unitX * (tickLen + minorTickLenPixels);

                _precomputedTicks.Add(new TickPoint
                {
                    P1 = new PointF(centerX, centerY),
                    P2 = new PointF(centerX + vX, centerY + vY),
                    PT = new PointF(centerX + vtX, centerY + vtY),
                    IsMajor = isMajor,
                    Label = isMajor ? mm.ToString() : null
                });
            }
        }

        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_precomputedTicks.Count == 0) return;

            bool inWorld = viewer.IsInWorldCoordinate();

            if (inWorld)
                viewer.SwitchToViewportCoordinate(gxView);

            // 取得當前縮放下的 1mm 像素量，用來決定要畫多細
            // 這裡直接拿預算好的第一段距離來測算即可
            float vLen1mm = 0;
            if (_precomputedTicks.Count > 1)
            {
                PointF vStart = TransToView(viewer, _p1);
                PointF vNext = TransToView(viewer, new PointF(_p1.X + (_p2.X - _p1.X) / (float)_dist, _p1.Y + (_p2.Y - _p1.Y) / (float)_dist));
                vLen1mm = (float)Math.Sqrt(Math.Pow(vNext.X - vStart.X, 2) + Math.Pow(vNext.Y - vStart.Y, 2));
            }

            var font = viewer.Font;
            using (Pen pen = new Pen(_color, _thinkness))
            using (SolidBrush brush = new SolidBrush(_color))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                gxView.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 1. 畫主線
                var p1 = TransToView(viewer, _p1);
                var p2 = TransToView(viewer, _p2);
                gxView.DrawLine(pen, p1, p2);

                // 2. 畫預算好的刻度
                foreach (var tick in _precomputedTicks)
                {
                    // 動態隱藏邏輯：如果太小，跳過非 Major 刻度
                    if (vLen1mm < 3f && !tick.IsMajor)
                        continue;

                    PointF vpt1 = TransToView(viewer, tick.P1);
                    PointF vpt2 = TransToView(viewer, tick.P2);
                    gxView.DrawLine(pen, vpt1, vpt2);

                    // 繪製文字
                    if (tick.Label != null && vLen1mm >= 3f)
                    {
                        PointF vptT = TransToView(viewer, tick.PT);

                        // 保存目前狀態並平移旋轉
                        var state = gxView.Save();
                        gxView.TranslateTransform(vptT.X, vptT.Y);
                        gxView.RotateTransform(_angle);

                        // 座標 (0,0) 即為 vptT，配合 StringFormat 達成置中
                        gxView.DrawString(tick.Label, font, brush, 0, 0, sf);

                        gxView.Restore(state);
                    }

                    // 繪製總長度文字
                    if (true)
                    {
                        //var summaryText = $"L= {_dist:0.000} mm";
                        var summaryText = $"{_dist:0.000}";
                        var vptT = TransToView(viewer, _summaryTextPt);

                        // 保存目前狀態並平移旋轉
                        var state = gxView.Save();
                        gxView.TranslateTransform(vptT.X, vptT.Y);
                        gxView.RotateTransform(_angle);

                        // 座標 (0,0) 即為 vptT，配合 StringFormat 達成置中
                        gxView.DrawString(summaryText, font, brush, 0, 0, sf);

                        gxView.Restore(state);
                    }
                }
            }
        }

        private void drawTickLabel(CvImageViewer viewer, Graphics gxView, TickPoint tick, SolidBrush brush)
        {
            var font = viewer.Font;

            PointF vptT = TransToView(viewer, tick.PT);

            // 儲存目前的畫布狀態
            var state = gxView.Save();

            // 1. 移動原點到文字中心
            gxView.TranslateTransform(vptT.X, vptT.Y);

            // 2. 旋轉畫布
            // 注意：如果想要文字永遠朝上（不隨線段倒過來），可以加一個判斷
            float drawAngle = tick.Angle;
            //if (drawAngle > 90 || drawAngle < -90) drawAngle += 180;

            gxView.RotateTransform(drawAngle);

            // 3. 繪製文字 (以 0,0 為中心)
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                // 這裡 DrawString 的座標給 (0,0)
                gxView.DrawString(tick.Label, font, brush, 0, 0, sf);
            }

            // 4. 恢復畫布狀態
            gxView.Restore(state);
        }

        static PointF TransToView(CvImageViewer viewer, PointF pt)
        {
            float x = pt.X;
            float y = pt.Y;
            viewer.TransWorldToViewport(ref x, ref y);
            return new PointF(x, y);
        }
        #endregion
    }
}
