using JetEazy;
using JetEazy.ImageViewerEx;
using JetEazy.ImageViewerQx;
using JzDisplay.Interface;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WorldOfMoveableObjects;


namespace JzDisplay.OPSpace
{
    //--- LeTian 新的建構式  -----------------------------------------------------------------------
    partial class OPDisplay : CvImageViewerInteractor, IOpDisplayPriv
    {
        #region PRIVATE_GUI_COMPONENTS
        QzImageViewer picDisplay => _imgViewer;
        QzImageViewer _imgViewer = null;
        bool _isGeoFigInViewCoord = false;
        #endregion

        public OPDisplay(Control viewer, Label lblCoordInfo = null)
        {
            JzMover = new Mover();
            JzStaticMover = new Mover();

            lblInformation = lblCoordInfo;
            _imgViewer = viewer as QzImageViewer;

            viewer.DoubleClick += PicDisplay_DoubleClick;
            viewer.MouseEnter += PicDisplay_MouseEnter;
            viewer.MouseLeave += PicDisplay_MouseLeave;
            penSelect.DashStyle = DashStyle.Dot;

            //myDisplayTimer = new Timer();
            //myDisplayTimer.Tick += MyDisplayTimer_Tick;
            //myDisplayTimer.Interval = 20;

            if (_imgViewer != null)
            {
                var interactor = this as CvImageViewerInteractor;
                interactor.Enabled = true;
                interactor.Visible = true;
                _imgViewer.AddInteractor(interactor);
            }
        }
    }


    //--- LeTian 橋接碼 I (座標轉換)  ---------------------------------------------------------------
    partial class OPDisplay
    {
        #region ADAPTER_FOR_COORD_TRANSFORM

        private bool m_DisableRight = true;

        public float PenWidth = 3;
        public float FontSize = 10;

        /// <summary>
        /// 可放最大的比例
        /// </summary>
        double MaxRatio
        {
            get { return 8.5; }
            set { }
        }

        /// <summary>
        /// 可縮最小的比例
        /// </summary>
        /// 
        double MinRatio
        {
            get { return 0.1; }
            set { }
        }

        /// <summary>
        /// 現在的比例
        /// </summary>
        double RatioNow
        {
            get
            {
                return picDisplay != null ? picDisplay.GetZoomScale() : 1.0;
            }
            set
            {
                // Just do nothing (
            }
        }

        /// <summary>
        /// World coodinate (被瀏覽的Bitmap世界坐標)
        /// </summary>
        RectangleF rectFPaintFrom
        {
            get
            {
                // 橋接到 QzImageViewer (The Large Image Viewer)
                return picDisplay?.GetWorldRect() ?? new RectangleF(0, 0, 100, 100);
            }
            set
            {
                // Do Nothing
                // 座標全部由 QzCameraViewer 計算
            }
        }

        /// <summary>
        /// 畫上去的範圍 (ViewPointRect)
        /// </summary>
        RectangleF rectFPaintTo
        {
            get
            {
                if (_imgViewer == null)
                    return new RectangleF(0, 0, 100, 100);
                var rect = _imgViewer.GetWorldRect();
                _imgViewer.TransWorldToViewport(ref rect);
                return rect;
            }
            set
            {
                // Do Nothing
                // 座標全部由 QzCameraViewer 計算
            }
        }

        /// <summary>
        /// 取得圖形世界座標 (World coordinate)
        /// </summary>
        PointF ptfMapping(Point pt)
        {
            // 橋接到 QzImageViewer (The Large Image Viewer)
            //>>> return new PointF(((float)pt.X - rectFPaintTo.X) / (float)RatioNow, ((float)pt.Y - rectFPaintTo.Y) / (float)RatioNow);
            float x = pt.X;
            float y = pt.Y;
            _imgViewer?.TransViewportToWorld(ref x, ref y);
            return new PointF(x, y);
        }
        #endregion

        #region EVENT_HANDLERS
        private void PicDisplay_MouseLeave(object sender, EventArgs e)
        {
            IsMouseEnter = false;

            OnDebug("MOUSE LEAVE");
        }
        private void PicDisplay_MouseEnter(object sender, EventArgs e)
        {
            IsMouseEnter = true;

            OnDebug("MOUSE ENTER");
        }
        private void PicDisplay_DoubleClick(object sender, EventArgs e)
        {
            DefaultView();
        }
        #endregion

        #region OVERRIDES_重新利用舊有的_EVENT_HANDLERS
#if (OPT_OLD_VICTOR)
        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {

            if (iMode == -1)
                return;

            Graphics grfx = e.Graphics;

            grfx.DrawImage(bmpPaint, rectFPaintTo, rectFPaintFrom, GraphicsUnit.Pixel);
            DrawShapes(grfx);

            if (IsLeftMouseDown)
            {
                switch (DISPLAYTYPE)
                {
                    case DisplayTypeEnum.ADJUST:
                    case DisplayTypeEnum.SHOW:
                        break;
                    case DisplayTypeEnum.NORMAL:
                    case DisplayTypeEnum.CAPTRUE:
                        grfx.DrawRectangle(penSelect, RectTwoPoint(ptMouse_Down, ptMouse_Move, ref rectfSelect));
                        break;
                }
            }
        }
#endif
        private void PicDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            ptMouse_Move = e.Location;

            PointF ptfOffset = new PointF(ptMouse_Move.X - ptMouse_Down.X, ptMouse_Move.Y - ptMouse_Down.Y);

            if (IsLeftMouseDown)
            {
                //if (DISPLAYTYPE == DisplayTypeEnum.ADJUST)
                //{
                //    OnAdjustAction(ptfOffset);
                //}
                //else
                //{
                //    CheckSelectedShape(false);
                //    picDisplay.Invalidate();
                //}

                switch (DISPLAYTYPE)
                {
                    case DisplayTypeEnum.ADJUST:
                        OnAdjustAction(ptfOffset);
                        break;
                    case DisplayTypeEnum.CAPTRUE:
                        break;
                    default:
                        CheckSelectedShape(false);
                        picDisplay.Invalidate();
                        break;
                }
            }
            else
            {
                if (JzMover.Move(e.Location) && e.Button == MouseButtons.Left && DISPLAYTYPE == DisplayTypeEnum.NORMAL)
                {
                    int caughtindex = JzMover.CaughtNode;

                    if (!IsResizing && !IsRotation)
                    {
                        RestoreShape();

                        MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
                        StartMove(new Point((int)ptfOffset.X, (int)ptfOffset.Y));
                        IsMove = true;
                    }

                    picDisplay.Invalidate();
                    return;
                }

                switch (e.Button)
                {
                    case MouseButtons.Right:
                        //if (!m_DisableRight)
                        {
                            rectFPaintTo = rectFMouseDown;
                            rectFPaintTo.Offset(ptfOffset);
                            RestoreShape();

                            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
                            picDisplay.Invalidate();
                        }
                        break;
                }
            }
            updateInfoLabel();

        }
        private void PicDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            if (iMode == -1)
                return;

            IsMouseDown = false;

            ptMouse_Up = e.Location;

            if (MouseUpShapes(e))
            {
                MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.FromMovingObject);   //>>> to World?
            }

            switch (DISPLAYTYPE)
            {
                //case DisplayTypeEnum.SELECT:
                case DisplayTypeEnum.NORMAL:
                case DisplayTypeEnum.CAPTRUE:
                    RectangleF rectInWorld = new RectangleF();

                    RectTwoPoint(
                        PointFConvert(ptfMapping(ptMouse_Down)),
                        PointFConvert(ptfMapping(ptMouse_Up)),
                        ref rectInWorld);

                    OnCapture(rectInWorld);
                    break;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    IsLeftMouseDown = false;
                    myDisplayTimer?.Stop();
                    CheckSelectedShape(true);
                    TransSelectList(SelectList, SelectBackupList);

                    if (IsMove)
                    {
                        IsMove = false;

                        for (int i = 0; i < JzMover.Count; i++)
                        {
                            GraphicalObject grobj = JzMover[i].Source;

                            if ((grobj as GeoFigure).IsSelected && (grobj as GeoFigure).RelateNo >= 1)
                            {
                                OnDebug("RELEASMOVE");
                                break;
                            }
                        }
                    }

                    break;
                case MouseButtons.Right:
                    //if (!m_DisableRight)
                        Cursor.Current = Cursors.Default;
                    break;
            }

            rectfSelect = new RectangleF(-10000, -10000, 0, 0);
            picDisplay.Invalidate();
        }
        private void PicDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            if (iMode == -1)
                return;
            if (DISPLAYTYPE == DisplayTypeEnum.SHOW && e.Button == MouseButtons.Left)
                return;

            IsMouseDown = true;
            ptMouse_Down = e.Location;
            rectFMouseDown = rectFPaintTo;

            BackupShape();

            //If Catched Then Get Out
            if (MouseDownShapes(e))
            {
                updateInfoLabel();
                return;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    IsLeftMouseDown = true;
                    myDisplayTimer?.Start();
                    if (!IsHoldForSelct)
                        ClearSelectShape();

                    switch (DISPLAYTYPE)
                    {
                        case DisplayTypeEnum.ADJUST:
                            OnMover(MoverOpEnum.READYTOMOVE, "");
                            break;
                    }
                    break;

                case MouseButtons.Right:
                    //if (!m_DisableRight)
                        Cursor.Current = Cursors.SizeAll;
                    break;
            }

            picDisplay.Invalidate();

        }
        private void PicDisplay_MouseWheel(object sender, MouseEventArgs e)
        {
#if (OPT_OLD)
            if (myTime.msDuriation < 100)
                return;
            myTime.Cut();
#endif

            if (IsMouseDown)
                return;

#if (OPT_OLD)
            float sizingratio = (float)Math.Pow(2d, (e.Delta > 0 ? 1d : -1d));
            double nextratio = RatioNow * (double)sizingratio;

            if (e.Delta < 0)
            {
                if (nextratio >= MinRatio)
                {
                    RatioNow = nextratio;

                    var rectFPaintTo = this.rectFPaintTo;
                    rectFPaintTo.X = ((ptMouse_Move.X - rectFPaintTo.X) / 2) + rectFPaintTo.X;
                    rectFPaintTo.Y = ((ptMouse_Move.Y - rectFPaintTo.Y) / 2) + rectFPaintTo.Y;
                    rectFPaintTo.Width = ((float)rectFPaintTo.Width / 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height / 2);
                    this.rectFPaintTo = rectFPaintTo;

                    MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }
            else if (e.Delta > 0)
            {
                if (nextratio < MaxRatio)
                {
                    RatioNow = nextratio;

                    var rectFPaintTo = this.rectFPaintTo;
                    rectFPaintTo.X = (rectFPaintTo.X - ptMouse_Move.X) + rectFPaintTo.X;
                    rectFPaintTo.Y = (rectFPaintTo.Y - ptMouse_Move.Y) + rectFPaintTo.Y;
                    rectFPaintTo.Width = ((float)rectFPaintTo.Width * 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height * 2);
                    this.rectFPaintTo = rectFPaintTo;

                    MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }
#endif

            updateInfoLabel();

            if (e.Delta != 0)
            {
                // 設定 _isGeoFigInViewCoord = false
                // 可讓 OnDraw 重新計算 GeoFig 位置
                _isGeoFigInViewCoord = false;
                picDisplay.Refresh();
            }
        }
        #endregion

        #region OVERRIDES_主要覆寫
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (iMode == -1)
                return;

            if (!Visible)
                return;

            bool isInWorld = viewer.IsInWorldCoordinate();
            if (isInWorld)
                viewer.SwitchToViewportCoordinate(gxView);

            if (!_isGeoFigInViewCoord)
                transGeoFigToViewportCoord();

            drawGeoFigures(gxView);
            drawSelecionDotLines(gxView);

            if (isInWorld)
                viewer.SwitchToWorldCoordinate(gxView);
        }
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            if (Enabled)
                PicDisplay_MouseDown(viewer, e);
            return false;
        }
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if (Enabled)
                PicDisplay_MouseMove(viewer, e);
            return false;
        }
        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            if (Enabled)
                PicDisplay_MouseUp(viewer, e);
            return false;
        }
        public override bool OnMouseWheel(CvImageViewer viewer, MouseEventArgs e)
        {
            PicDisplay_MouseWheel(viewer, e);
            return false;
        }
        #endregion

        #region PRIVATE_ITERATION_FUNCTIONS
        /// <summary>
        /// 迭代尋訪 指定 JzMover 與 JzStaticMover 內所有的 GeoFigure
        /// </summary>
        IEnumerable<GeoFigure> iterAllGeoFigures(bool reverse = false)
        {
            foreach (var geo in iterGeoFigures(JzMover, reverse))
                yield return geo;
            foreach (var geo in iterGeoFigures(JzStaticMover, reverse))
                yield return geo;
        }
        /// <summary>
        /// 迭代尋訪 指定 Mover 內所有的 GeoFigure
        /// </summary>
        IEnumerable<GeoFigure> iterGeoFigures(Mover mover, bool reverse = false)
        {
            if (mover != null)
            {
                if (reverse)
                {
                    for (int i = mover.Count - 1; i >= 0; i--)
                        if (mover[i].Source is GeoFigure geof)
                            yield return geof;
                        else
                            yield return null;
                }
                else
                {
                    for (int i = 0, N = mover.Count; i < N; i++)
                        if (mover[i].Source is GeoFigure geof)
                            yield return geof;
                        else
                            yield return null;
                }
            }
        }
        #endregion

        #region PRIVATE_RENDER_FUNCTIONS
        /// <summary>
        /// 將 Moving Object 在畫布上畫出來
        /// </summary>
        private void drawGeoFigures(Graphics gx)
        {
            foreach (GeoFigure geof in iterAllGeoFigures(false))
                geof?.Draw(gx);
        }
        private void drawSelecionDotLines(Graphics gx)
        {
            if (IsLeftMouseDown)
            {
                switch (DISPLAYTYPE)
                {
                    case DisplayTypeEnum.ADJUST:
                    case DisplayTypeEnum.SHOW:
                        break;
                    //case DisplayTypeEnum.SELECT:
                    case DisplayTypeEnum.NORMAL:
                    case DisplayTypeEnum.CAPTRUE:
                        {
                            //>>> gx.DrawRectangle(penSelect, RectTwoPoint(ptMouse_Down, ptMouse_Move, ref rectfSelect));
                            var rectV = RectTwoPoint(ptMouse_Down, ptMouse_Move, ref rectfSelect);
                            gx.DrawRectangle(penSelect, rectV);
                        }
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// 載入圖形後可見到整個畫面的預設位置及比例
        /// </summary>
        public void DefaultView()
        {
#if(false)
            rectFPaintFrom = new RectangleF(0, 0, bmpPaint.Width, bmpPaint.Height);

            float wratio = (float)picDisplay.Width / (float)bmpPaint.Width;
            float hratio = (float)picDisplay.Height / (float)bmpPaint.Height;

            RatioNow = Math.Min(wratio, hratio);

            SizeF szpic = new SizeF((float)(RatioNow * (double)bmpPaint.Width), (float)(RatioNow * (double)bmpPaint.Height));
            PointF ptf = new PointF(((float)picDisplay.Width - szpic.Width) / 2f, ((float)picDisplay.Height - szpic.Height) / 2f);

            rectFPaintTo = new RectangleF(ptf, szpic);
#endif
            picDisplay.RebuildViewport();
            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
            picDisplay.Invalidate();
        }

        /// <summary>
        /// 將所有的 GeoFigures 的座標還原到 World Coordinate ?
        /// </summary>
        public void MappingShape()
        {
            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.FromMovingObject);
        }

        /// <summary>
        /// 對應原始圖形和畫布上圖形的位置
        /// 將所有的 GeoFigure 進行座標轉換:
        /// <br/> .ToMovingObject : 轉換到 Viewport Coordinate (順)
        /// <br/> .FromMovingObject : 轉換到 World Coordinate (逆)
        /// </summary>
        private void MappingShape(PointF neworgbias, double newratio, MappingDirectionEnum mappingdir) //Need To Added For New Shape
        {
#if (OPT_OLD_VICTOR)
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                switch (mappingdir)
                {
                    // To viewport coordinate
                    case MappingDirectionEnum.ToMovingObject:
                        (grobj as GeoFigure).MappingToMovingObject(neworgbias, newratio);
                        break;

                    // To content coordinate
                    case MappingDirectionEnum.FromMovingObject:
                        (grobj as GeoFigure).MappingFromMovingObject(neworgbias, newratio);
                        break;
                }
            }

            //Add For Static Mover
            for (int i = JzStaticMover.Count - 1; i >= 0; i--)
            {
                grobj = JzStaticMover[i].Source;

                switch (mappingdir)
                {
                    // To viewport coordinate
                    case MappingDirectionEnum.ToMovingObject:
                        (grobj as GeoFigure).MappingToMovingObject(neworgbias, newratio);
                        break;

                    // To content coordinate
                    case MappingDirectionEnum.FromMovingObject:
                        (grobj as GeoFigure).MappingFromMovingObject(neworgbias, newratio);
                        break;
                }
            }
#else

            if (mappingdir == MappingDirectionEnum.ToMovingObject)
            {
                var worldRect = _imgViewer.GetWorldRect();
                if (worldRect.Width < 5 || worldRect.Height < 5)
                    return;

                //float x = worldRect.X;
                //float y = worldRect.Y;
                //imgViewer.TransWorldToViewport(ref x, ref y);
                //neworgbias = new PointF(x, y);
                //newratio = imgViewer.GetZoomScale();

                foreach (GeoFigure geof in iterAllGeoFigures(true))
                    geof?.MappingToMovingObject(neworgbias, newratio);

                _isGeoFigInViewCoord = true;
            }
            else
            {
                foreach (GeoFigure geof in iterAllGeoFigures(true))
                    geof?.MappingFromMovingObject(neworgbias, newratio);

                _isGeoFigInViewCoord = false;
            }
#endif
        }

        #region PRIVATE_LETIAN_FUNCTIONS_座標轉換
        void transGeoFigToViewportCoord()
        {
            getViewBiasAndZoom(out PointF biasPt, out float zoom);
            MappingShape(biasPt, zoom, MappingDirectionEnum.ToMovingObject);
            _isGeoFigInViewCoord = true;
        }
        void getViewBiasAndZoom(out PointF biasPt, out float zoom)
        {
            if (_imgViewer == null)
            {
                biasPt = new PointF(0, 0);
                zoom = 1;
                return;
            }

            var worldRect = _imgViewer.GetWorldRect();
            float x = worldRect.X;
            float y = worldRect.Y;
            _imgViewer.TransWorldToViewport(ref x, ref y);
            biasPt = new PointF(x, y);
            zoom = _imgViewer.GetZoomScale();
        }
        #endregion
    }


    //--- LeTian 橋接碼 II (Bitmaps)  ---------------------------------------------------------------
    partial class OPDisplay
    {
        #region RUNTIME_BITMAP_BUFs
        Bitmap bmpBackup = new Bitmap(1, 1);
        //Bitmap bmpPaint = new Bitmap(1, 1);  // 暫時把 bmpPaint 優化掉不使用!
        #endregion

        /// <summary>
        /// bmpOrg 由 OPDisplay 負責 Life Cycle
        /// </summary>
        Bitmap bmpOrg
        {
            get { return _imgViewer.Image; }
            set { _imgViewer.Image = value; }
        }

        void disposeBmps(bool keepSmallOnes = false)
        {
            //bmpPaint?.Dispose();
            //bmpPaint = null;
            bmpBackup?.Dispose();
            bmpBackup = null;

            if (keepSmallOnes)
            {
                //bmpPaint = new Bitmap(1, 1);
                bmpBackup = new Bitmap(1, 1);
            }
        }

        /// <summary>
        /// 取代顯示的圖型
        /// </summary>
        public void ReplaceDisplayImage(Bitmap bmp) //取代顯示的圖形，不動畫面及畫出的框
        {
            if (bmpOrg != null && bmpOrg.Width == 1)
            {
                ReplaceDisplayImage(bmp, IsResetMover: false);
            }
            else
            {
                bmpOrg?.Dispose();
                bmpOrg = (Bitmap)bmp.Clone();

                //bmpPaint?.Dispose();
                //bmpPaint = new Bitmap(bmpOrg);

                if (DISPLAYTYPE == DisplayTypeEnum.ADJUST)
                {
                    // 以下舊代碼 貌似 要把影像置中
#if (OPT_OLD)
                    var worldSize = bmpPaint.Size;
                    // rectFPaintFrom = new RectangleF(0, 0, worldSize.Width, worldSize.Height);
                    float wratio = (float)picDisplay.Width / (float)worldSize.Width;
                    float hratio = (float)picDisplay.Height / (float)worldSize.Height;
                    float RatioNow = Math.Min(wratio, hratio);

                    SizeF viewSize = new SizeF((float)(RatioNow * (double)worldSize.Width), (float)(RatioNow * (double)worldSize.Height));
                    PointF ptf = new PointF(((float)picDisplay.Width - viewSize.Width) / 2f, ((float)picDisplay.Height - viewSize.Height) / 2f);

                    RectangleF oldrectF = rectFPaintTo;
                    //rectFPaintTo = new RectangleF(ptf, szpic);
                    //rectFPaintTo.Location = oldrectF.Location;
                    var loc = oldrectF.Location;
                    rectFPaintTo = new RectangleF(loc, viewSize);
#endif
                    _imgViewer.RebuildViewport();
                }

                MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
                picDisplay.Invalidate();
            }
        }        
        
        /// <summary>
        /// 設定顯示的圖型
        /// </summary>
        /// <param name="bmp"></param>
        public void ReplaceDisplayImage(Bitmap bmp, bool IsResetMover) //設定顯示的圖形，回到全景畫面，是否要清掉畫框
        {
            if (bmp != null)    //null 時為回到全影畫面
            {
                bmpOrg?.Dispose();
                bmpOrg = (Bitmap)bmp.Clone();

                //bmpPaint?.Dispose();
                //bmpPaint = (Bitmap)bmpOrg.Clone();
            }

            if (IsResetMover)
                JzMover.Clear();

            DefaultView();
        }

        /// <summary>
        /// 此函式會把 GeoFigure也畫入圖像, 生成【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        public Bitmap GetScreen()
        {
            //int penWidth = 5;

#if (OPT_OLD)
            Bitmap bmp = new Bitmap(bmpOrg);

            BackupShape();

            MappingShape(new PointF(0, 0), 1d, MappingDirectionEnum.ToMovingObject);

            Graphics gfx = Graphics.FromImage(bmp);

            int i = 0;

            GraphicalObject grobj;

            i = 0;
            while (i < JzMover.Count)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).MainShowPen.Width = 5;
                (grobj as GeoFigure).Draw(gfx);

                i++;
            }
            i = 0;
            while (i < JzStaticMover.Count)
            {
                grobj = JzStaticMover[i].Source;
                (grobj as GeoFigure).MainShowPen.Width = 5;
                (grobj as GeoFigure).Draw(gfx);

                i++;
            }
            gfx.Dispose();

            i = 0;
            while (i < JzMover.Count)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).MainShowPen.Width = 1;
                //(grobj as GeoFigure).Draw(gfx);

                i++;
            }
            i = 0;
            while (i < JzStaticMover.Count)
            {
                grobj = JzStaticMover[i].Source;
                (grobj as GeoFigure).MainShowPen.Width = 1;
                //(grobj as GeoFigure).Draw(gfx);

                i++;
            }

            RestoreShape();

            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

            picDisplay.Invalidate();

            //bmp.Save(Universal.TESTPATH + "\\PRTSCREEN" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);
            //bmp.Dispose();

            return bmp;
#else

            var rect = new Rectangle(0, 0, bmpOrg.Width, bmpOrg.Height);
            Bitmap bmpScreen = bmpOrg.Clone(rect, PixelFormat.Format24bppRgb);

            //BackupShape();
            MappingShape(new PointF(0, 0), 1d, MappingDirectionEnum.ToMovingObject);

            using (Graphics gfx = Graphics.FromImage(bmpScreen))
            {
                foreach (var geof in iterAllGeoFigures(false))
                {
                    if (geof != null)
                    {
                        geof.PenWidth = 5;
                        geof.FontSize = 100;
                        geof.MainShowPen.Width = 5;
                        geof.Draw(gfx);
                        geof.MainShowPen.Width = 1;
                        geof.PenWidth = PenWidth;
                        geof.FontSize = FontSize;
                    }
                }
            }

            //RestoreShape();
            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
            picDisplay.Invalidate();

            return bmpScreen;
#endif
        }
        public void SaveScreen(string saveFileName = null)
        {
            using (Bitmap bmp = GetScreen())
            {
                bool useDefaultFormat = false;

                if (string.IsNullOrEmpty(saveFileName))
                {
                    saveFileName = GlobalConfig.SCREEN_DUMP_FILE;
                    useDefaultFormat = true;
                }
                
                string path = System.IO.Path.GetDirectoryName(saveFileName);
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                if (useDefaultFormat)
                    bmp.Save(saveFileName, GlobalConfig.SCREEN_DUMP_IMG_FORMAT);
                else
                    bmp.Save(saveFileName);
            }
        }

        /// <summary>
        /// 在Capture時使用的抓圖方式
        /// <br/> 此函式會生成 【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        public Bitmap GetOrgBMP(RectangleF cropRect)
        {
            Bitmap bmp = bmpOrg;

            if (bmp == null || cropRect.Width == 0 || cropRect.Height == 0)
                return new Bitmap(1, 1);

            RectangleF roi = new RectangleF(0, 0, bmp.Width, bmp.Height);
            roi.Intersect(cropRect);

            //return bmpOrg.Clone(roi, PixelFormat.Format32bppArgb);
            return bmp.Clone(roi, PixelFormat.Format24bppRgb);
        }
        /// <summary>
        /// 此函式直接傳回 bmpOrg 的參考 (reference)
        /// <br/> 調用者不可以將其 Dispose() !!!
        /// </summary>
        public Bitmap GetOrgBMP()
        {
            return bmpOrg;
        }

#if (OPT_OLD_VICTOR)
        public Bitmap GetPaintImage()
        {
            return bmpPaint;
        }
#endif

        /// <summary>
        /// 備份原始圖形
        /// </summary>
        public void BackupImage()
        {
            bmpBackup?.Dispose();
            bmpBackup = new Bitmap(bmpOrg);
        }
        public void RestoreImage()
        {
            if (bmpBackup == null)
                bmpBackup = new Bitmap(1, 1);

            bmpOrg?.Dispose();
            bmpOrg = (Bitmap)bmpBackup.Clone();

            //bmpPaint.Dispose();
            //bmpPaint = new Bitmap(bmpOrg);

            bmpBackup?.Dispose();
            bmpBackup = null;

            picDisplay.Invalidate();
        }

        /// <summary>
        /// 設定Matching 的方式
        /// </summary>
        public void SetMatching(Bitmap bmpmatching, MatchMethodEnum matchmethod)
        {
            lock (obj)
            {
                using (Bitmap bmpPaint = (Bitmap)bmpOrg.Clone())
                {
                    //bmpPaint?.Dispose();
                    //bmpPaint = new Bitmap(bmpOrg);

                    switch (matchmethod)
                    {
                        case MatchMethodEnum.NONE:
                            break;
                        case MatchMethodEnum.OFF30:
                            JzFind.SetDiff(bmpPaint, bmpmatching, 60);
                            break;
                        case MatchMethodEnum.OFF50:
                            JzFind.SetDiff(bmpPaint, bmpmatching, 120);
                            break;
                        case MatchMethodEnum.OFF100:
                            JzFind.SetDiff(bmpPaint, bmpmatching, 180);
                            break;
                        case MatchMethodEnum.LUMINA:
                            JzFind.SetDiff(bmpPaint, bmpmatching);
                            break;
                    }
                }
            }
            picDisplay.Invalidate();
        }
        public void GenSearchImage(ref Bitmap bmp)
        {
            //Bitmap bmpx = new Bitmap(1, 1);
            //GraphicalObject grobj;
            //for (int i = JzMover.Count - 1; i >= 0; i--)
            //{
            //    grobj = JzMover[i].Source;
            //    (grobj as GeoFigure).GenSearchImage(0, 0, bmpOrg, ref bmp, ref bmpx);
            //}
            //bmpx.Dispose();

            Bitmap tmp = new Bitmap(1, 1);
            foreach (var geof in iterGeoFigures(JzMover, true))
            {
                geof?.GenSearchImage(0, 0, bmpOrg, ref bmp, ref tmp);
            }
            bmp?.Dispose();
        }
        public void GetMask(int outRangeX, int outRangeY)       //Nedd To Added For New Shape
        {
            var bmpWork = this.bmpOrg;

            GraphicalObject grobj;

            Bitmap bmpfind = new Bitmap(1, 1);
            Bitmap bmpmask = new Bitmap(1, 1);

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                if (grobj is JzRectEAG)
                {
                    (grobj as JzRectEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    //bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzRectEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);

                    (grobj as JzRectEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    //bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzRectEAG).DigImage(0, 0, bmpfind);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzCircleEAG)
                {
                    (grobj as JzCircleEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzCircleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzCircleEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzCircleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzPolyEAG)
                {
                    (grobj as JzPolyEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzPolyEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzPolyEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzPolyEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzRingEAG)
                {
                    (grobj as JzRingEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzRingEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzRingEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzRingEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzStripEAG)
                {
                    (grobj as JzStripEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzStripEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzStripEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzStripEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzIdentityHoleEAG)
                {
                    (grobj as JzIdentityHoleEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzIdentityHoleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzIdentityHoleEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzIdentityHoleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzCircleHoleEAG)
                {
                    (grobj as JzCircleHoleEAG).GenSearchImage(outRangeX, outRangeY, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzCircleHoleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzCircleHoleEAG).GenSearchImage(0, 0, bmpWork, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzCircleHoleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
            }

            bmpfind.Dispose();
            bmpmask.Dispose();
        }

        void IOpDisplayNormalB.GenSearchImage(ref Bitmap bmp)
        {
            // DispUI 與 OPDisplay 拼字不一致 !!!
            GenSearchImage(ref bmp);
        }
        void IOpDisplayNormalB.SaveOrgBMP(string savefilename)
        {
            var bmp = GetOrgBMP();
            bmp?.Save(savefilename);
        }
    }


    //--- LeTian 橋接碼 III (統合其他接口)  ---------------------------------------------------------------
    partial class OPDisplay
    {
        public void Dispose()
        {
            Suicide();
        }
        void IOpDisplayNormal.SetDisplayType(DisplayTypeEnum displaytype)
        {
            this.DISPLAYTYPE = displaytype;
        }
        void IOpDisplayNormal.SetImode(int iMode)
        {
            this.iMode = iMode;
        }
        void IOpDisplayExp.HoldSelect()
        {
            this.IsHoldForSelct = true;
        }
        void IOpDisplayExp.ReleaseSelect()
        {
            this.IsHoldForSelct = false;
        }
    }
}

