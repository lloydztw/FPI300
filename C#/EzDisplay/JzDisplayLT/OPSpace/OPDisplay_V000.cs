using JetEazy;
using JetEazy.BasicSpace;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WorldOfMoveableObjects;


namespace JzDisplay.OPSpace.V0
{
    class OPDisplay
    {
        enum MappingDirectionEnum
        {
            ToMovingObject,
            FromMovingObject,
        }

        Point ptMouse_Down, ptMouse_Up, ptMouse_Move;
        RectangleF rectFMouseDown;

        PictureBox picDisplay;
        Label lblInformation;

        double MaxRatio = 10;                //可放最大的比例
        double MinRatio = 0.1f;              //可縮最小的比例
        double RatioNow = 1f;                //現在的比例

        Bitmap bmpOrg = new Bitmap(1, 1);   //原來的的圖形
        Bitmap bmpPaint = new Bitmap(1, 1); //要畫上去的圖形

        Bitmap bmpBackup = new Bitmap(1, 1);

        RectangleF rectFPaintFrom;          //從要畫上去的範圍
        RectangleF rectFPaintTo;            //畫上去的範圍

        Pen penSelect = new Pen(Color.Orange, 2);       //選擇框的畫筆
        RectangleF rectfSelect = new RectangleF(-10000, -10000, 0, 0);      //選擇框的大小
        bool IsMouseDown = false;                       //鼠標是否已被按下，防止後來移動到圖形上還被觸發
        bool IsLeftMouseDown = false;                   //鼠標是否左鍵被按下，確定框選被啟動
        List<int> SelectList = new List<int>();         //選擇的List
        List<int> SelectBackupList = new List<int>();   //選擇的備份List
        bool IsMouseEnter = false;                      //鼠標是否在

        public DisplayTypeEnum DISPLAYTYPE = DisplayTypeEnum.SHOW;
        public int iMode = 0;
        
        public bool IsHoldForSelct = false;             //檢查是否有Ctrl被按下

        public bool ISMOUSEDOWN
        {
            get
            {
                return IsMouseDown;
            }
        }
        
        bool IsResizing = false;
        bool IsRotation = false;
        bool IsMove = false;

        JzTimes myTime = new JzTimes();
        Timer myDisplayTimer;

        Mover JzMover;
        Mover JzStaticMover;
        Object obj = new object();

        JzFindObjectClass JzFind = new JzFindObjectClass();
                
        /// <summary>
        /// 取得圖形最左上角位置的相對實際作標
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        PointF ptfMapping(Point pt)         
        {
            return new PointF(((float)pt.X - rectFPaintTo.X) / (float)RatioNow, ((float)pt.Y - rectFPaintTo.Y) / (float)RatioNow);
        }
        public OPDisplay(PictureBox pic,Label lbl,float maxratio,float minratio)
        {
          
             JzMover = new Mover();
            JzStaticMover = new Mover();

            MaxRatio = maxratio;
            MinRatio = minratio;

            picDisplay = pic;
            lblInformation = lbl;

            picDisplay.MouseDown += PicDisplay_MouseDown;
            picDisplay.MouseUp += PicDisplay_MouseUp;
            picDisplay.MouseMove += Pic_MouseMove;
            picDisplay.Paint += PicDisplay_Paint;
            picDisplay.DoubleClick += PicDisplay_DoubleClick;
            picDisplay.MouseEnter += PicDisplay_MouseEnter;
            picDisplay.MouseLeave += PicDisplay_MouseLeave;
            picDisplay.MouseWheel += PicDisplay_MouseWheel;

            penSelect.DashStyle = DashStyle.Dot;
            //picDisplay.MouseWheel += PicDisplay_MouseWheel;

            myDisplayTimer = new Timer();
            myDisplayTimer.Tick += MyDisplayTimer_Tick;
            myDisplayTimer.Interval = 20;
        }
        private void MyDisplayTimer_Tick(object sender, EventArgs e)
        {
            if (IsLeftMouseDown)
            {
                //CheckSelectedShape(false);
                //picDisplay.Invalidate();


            }
        }

        #region Normal Operation

        /// <summary>
        /// 取代顯示的圖型
        /// </summary>
        /// <param name="bmp"></param>
        public void ReplaceDisplayImage(Bitmap bmp) //取代顯示的圖形，不動畫面及畫出的框
        {
            if (bmpPaint.Width == 1)
                SetDisplayImage(bmp);
            else
            {
                bmpOrg.Dispose();
                bmpOrg = new Bitmap(bmp);

                bmpPaint.Dispose();
                bmpPaint = new Bitmap(bmpOrg);

                if (DISPLAYTYPE == DisplayTypeEnum.ADJUST)
                {
                    rectFPaintFrom = new RectangleF(0, 0, bmpPaint.Width, bmpPaint.Height);
                    float wratio = (float)picDisplay.Width / (float)bmpPaint.Width;
                    float hratio = (float)picDisplay.Height / (float)bmpPaint.Height;

                    RatioNow = Math.Min(wratio, hratio);

                    SizeF szpic = new SizeF((float)(RatioNow * (double)bmpPaint.Width), (float)(RatioNow * (double)bmpPaint.Height));
                    PointF ptf = new PointF(((float)picDisplay.Width - szpic.Width) / 2f, ((float)picDisplay.Height - szpic.Height) / 2f);

                    RectangleF oldrectF = rectFPaintTo;

                    rectFPaintTo = new RectangleF(ptf, szpic);

                    rectFPaintTo.Location = oldrectF.Location;
                }

                MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                picDisplay.Invalidate();
            }
        }
        public void RefreshDisplayShape()
        {
            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
            //picDisplay.Invalidate();
        }
        /// <summary>
        /// 重畫所有的東西
        /// </summary>
        public void ReDraw()
        {
            picDisplay.Invalidate();
        }

        /// <summary>
        /// 設定顯示的圖型
        /// </summary>
        /// <param name="bmp"></param>
        public void SetDisplayImage(Bitmap bmp)     //設定顯示的圖形，回到全景畫面
        {
            SetDisplayImage(bmp, false);
        }
        public void SetDisplayImage(Bitmap bmp, bool IsResetMover) //設定顯示的圖形，回到全景畫面，是否要清掉畫框
        {
            if (bmp != null)    //null 時為回到全影畫面
            {
                bmpOrg.Dispose();
                bmpOrg = new Bitmap(bmp);

                bmpPaint.Dispose();
                bmpPaint = new Bitmap(bmpOrg);
            }

            if (IsResetMover)
                JzMover.Clear();

            DefaultView();
        }
        public void SetDisplayImage()     //回到全景畫面
        {
            SetDisplayImage(null, false);
        }
        /// <summary>
        /// 設定顯示的形狀
        /// </summary>
        /// <param name="mover"></param>
        public void SetMover(Mover mover)
        {
            JzMover = mover;

            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).InitialCtrl(picDisplay, JzMover);
            }

        }
        /// <summary>
        /// 設定固定顯示的形狀
        /// </summary>
        /// <param name="mover"></param>
        public void SetStaticMover(Mover staticmover)
        {
            JzStaticMover = staticmover;

            GraphicalObject grobj;

            for (int i = JzStaticMover.Count - 1; i >= 0; i--)
            {
                grobj = JzStaticMover[i].Source;
            }

            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);
            picDisplay.Invalidate();
        }
        /// <summary>
        /// 清除固定顯示的形狀
        /// </summary>
        public void ClearStaticMover()
        {
            JzStaticMover.Clear();
            picDisplay.Invalidate();
        }
        /// <summary>
        /// 載入圖形後可見到整個畫面的預設位置及比例
        /// </summary>
        public void DefaultView()
        {
            rectFPaintFrom = new RectangleF(0, 0, bmpPaint.Width, bmpPaint.Height);
            
            float wratio = (float)picDisplay.Width / (float)bmpPaint.Width;
            float hratio = (float)picDisplay.Height / (float)bmpPaint.Height;

            RatioNow = Math.Min(wratio, hratio);
            
            SizeF szpic = new SizeF((float)(RatioNow * (double)bmpPaint.Width), (float)(RatioNow * (double)bmpPaint.Height));
            PointF ptf = new PointF(((float)picDisplay.Width - szpic.Width) / 2f, ((float)picDisplay.Height - szpic.Height) / 2f);

            rectFPaintTo = new RectangleF(ptf, szpic);

            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

            picDisplay.Invalidate();
        }
        /// <summary>
        /// 顯示資訊
        /// </summary>
        private void FillInformation()
        {
            string str = "";

            if (iMode == -1)
                return;

            PointF ptfmapping = ptfMapping(ptMouse_Move);

            str += "Position Now : (" + ptMouse_Move.X.ToString() + "," + ptMouse_Move.Y.ToString() + ") ";
            str += "Mapping Now : (" + ptfmapping.X.ToString("0") + "," + ptfmapping.Y.ToString("0") + ") ";

            if (ptfmapping.X >= 0 && ptfmapping.X < bmpOrg.Width && ptfmapping.Y >= 0 && ptfmapping.Y < bmpOrg.Height)
            {
                Color mcolor = bmpOrg.GetPixel((int)ptfmapping.X, (int)ptfmapping.Y);

                str += "RGB Now : (R=" + mcolor.R.ToString().PadLeft(3, ' ') +
                    ",G=" + mcolor.G.ToString().PadLeft(3, ' ') +
                    ",B=" + mcolor.B.ToString().PadLeft(3, ' ') + ") ";

                str += "Grayscale =" + GreyscaleInt(mcolor.R, mcolor.G, mcolor.B).ToString().PadLeft(3, ' ');
            }

            if(DISPLAYTYPE != DisplayTypeEnum.CAPTRUE)
                lblInformation.Text = str + "         " + ToSelectListString();


            lblInformation.Text = lblInformation.Text.Substring(0, Math.Min(lblInformation.Text.Length, 150));

            lblInformation.Invalidate();

        }
        /// <summary>
        /// Resize Object 的方式
        /// </summary>
        /// <param name="pt"></param>
        private void StartResizing(Point pt, ref bool isresizing)       //Need To Added For New Shape for isresizingvpn.
        {
            GraphicalObject grobj = JzMover.CaughtSource;
            int iNode = JzMover.CaughtNode;
            NodeShape shapePressed = JzMover.CaughtNodeShape;

            if (grobj is JzRectEAG)                                         // Rectangle_EAG
            {
                if (iNode < 4)
                {
                    isresizing = true;
                    (grobj as JzRectEAG).StartResizing(pt, iNode);
                }
            }
            #region Reserver
            //else if (grobj is RegularPolygon_EAG)                               // RegularPolygon_EAG
            //{
            //    (grobj as RegularPolygon_EAG).StartScaling(pt, iNode);
            //}
            //else if (grobj is ConvexPolygon_EAG)                                // ConvexPolygon_EAG
            //{
            //    if (shapePressed == NodeShape.Circle)
            //    {
            //        (grobj as ConvexPolygon_EAG).StartReconfiguring(iNode);
            //    }
            //    else if (shapePressed == NodeShape.Strip)
            //    {
            //        (grobj as ConvexPolygon_EAG).StartResizing(pt, iNode);
            //    }
            //}
            #endregion
            else if (grobj is JzIdentityHoleEAG)                        // RegPoly_IdenticalHole_EAG
            {
                if (iNode == 1 || iNode == 4)
                {
                    isresizing = true;
                    (grobj as JzIdentityHoleEAG).StartScaling(pt, iNode);
                }
            }
            else if (grobj is JzPolyEAG)                             // ChatoyantPolygon_EAG
            {
                if (shapePressed == NodeShape.Circle)
                {
                    isresizing = true;
                    (grobj as JzPolyEAG).StartReconfiguring(iNode);
                }
                //else if (shapePressed == NodeShape.Strip)
                //{
                //    isresizing = true;
                //    (grobj as JzPolyEAG).StartResizing(pt, iNode);
                //}
            }
            else if (grobj is JzStripEAG)                                        // Strip_EAG
            {
                if (iNode != 2)
                {
                    isresizing = true;
                    (grobj as JzStripEAG).StartResizing(pt, iNode);
                }
            }
            #region Reserve
            //else if (grobj is RegPoly_RegPolyHole_EAG)                          // RegPoly_RegPolyHole_EAG
            //{
            //    if (iNode == 1 || iNode == 3)
            //    {
            //        (grobj as RegPoly_RegPolyHole_EAG).StartResizing(pt, iNode);
            //    }
            //}
            //else if (grobj is ConvexPoly_RegPolyHole_EAG)                       // ConvexPoly_RegPolyHole_EAG
            //{
            //    if (shapePressed == NodeShape.Circle)
            //    {
            //        (grobj as ConvexPoly_RegPolyHole_EAG).StartReconfiguring(iNode);
            //    }
            //    else if (shapePressed == NodeShape.Strip)
            //    {
            //        (grobj as ConvexPoly_RegPolyHole_EAG).StartResizing_Outer(pt, iNode);
            //    }
            //    else                 //if (shapePressed == NodeShape .Strip)
            //    {
            //        ConvexPoly_RegPolyHole_EAG polyCPRPH = grobj as ConvexPoly_RegPolyHole_EAG;
            //        if (iNode == polyCPRPH.VerticesNumber_Outer * 2 + 1)
            //        {
            //            polyCPRPH.StartResizing_Inner(pt);
            //        }
            //    }
            //}
            #endregion

            else if (grobj is JzCircleHoleEAG)                                  // RegPoly_CircularHole_EAG
            {
                if (iNode == 1 || iNode == 4)
                {
                    isresizing = true;
                    (grobj as JzCircleHoleEAG).StartResizing(pt, iNode);
                }
            }
            else if (grobj is JzCircleEAG)                                       // Circle_EAG
            {
                JzCircleEAG circle = grobj as JzCircleEAG;

                if (shapePressed == NodeShape.Circle && iNode == circle.NodesCount - 3)
                {
                    isresizing = true;
                    circle.StartResizing(pt);
                }
            }
            else if (grobj is JzRingEAG)                                         // Ring_EAG
            {
                JzRingEAG ring = grobj as JzRingEAG;
                if (shapePressed == NodeShape.Strip)
                {
                    isresizing = true;
                    ring.StartResectoring(iNode);
                }
                else if (shapePressed == NodeShape.Circle && (iNode == 1 || iNode == ring.NodesCount - 1))
                {
                    isresizing = true;
                    ring.StartResizing(pt, iNode);
                }
            }
        }
        // -------------------------------------------------        StartRotation
        /// <summary>
        /// 旋轉 Object 的方式
        /// </summary>
        /// <param name="pt"></param>
        private void StartRotation(Point pt, ref bool isrotation)
        {
            GraphicalObject grobj = JzMover.CaughtSource;

            isrotation = true;

            #region Wait For Whole Rotation Revise By Victor Tsai
            //for (int i = JzMover.Count - 1; i >= 0; i--)
            //{
            //    grobj = JzMover[i].Source;

            //    if (!(grobj as GeoFigure).IsSelected)
            //        continue;

            //    isrotation = true;
            //    (grobj as GeoFigure).StartRotation(pt);
            //    //if (grobj is Rectangle_EAG)
            //    //{
            //    //    (grobj as Rectangle_EAG).Move(offsetpt.X, offsetpt.Y);
            //    //}
            //}
            #endregion

            if (grobj is GeoFigure)
            {
                #region Reserve
                //if (grobj is RegPoly_RegPolyHole_EAG)
                //{
                //    (grobj as RegPoly_RegPolyHole_EAG).StartRotation(pt, mover.CaughtNode);
                //}
                //else if (grobj is ConvexPoly_RegPolyHole_EAG)
                //{
                //    (grobj as ConvexPoly_RegPolyHole_EAG).StartRotation(pt, mover.CaughtNode);
                //}
                //else
                #endregion
                {
                    (grobj as GeoFigure).StartRotation(pt);
                }
            }
        }
        /// <summary>
        /// 全部一起移動
        /// </summary>
        /// <param name="offsetpt"></param>
        private void StartMove(Point offsetpt)  //Need To Added For New Shape
        { 
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                if (!(grobj as GeoFigure).IsSelected)
                    continue;

                (grobj as GeoFigure).Move(offsetpt.X, offsetpt.Y);

                //(grobj as GeoFigure).MappingToMovingObject(offsetpt, RatioNow);

                //(grobj as GeoFigure).MappingFromMovingObject(new PointF(0, 0), RatioNow);

                //if (grobj is JzRectEAG)
                //{
                //    (grobj as JzRectEAG).Move(offsetpt.X,offsetpt.Y) ;
                //}
                //else if(grobj is JzCircleEAG)
                //{
                //    (grobj as JzCircleEAG).Move(offsetpt.X, offsetpt.Y);
                //}
                //else if (grobj is JzPolyEAG)
                //{
                //    (grobj as JzPolyEAG).Move(offsetpt.X, offsetpt.Y);
                //}
                //else if (grobj is JzRingEAG)
                //{
                //    (grobj as JzRingEAG).Move(offsetpt.X, offsetpt.Y);
                //}
                //else if (grobj is JzStripEAG)
                //{
                //    (grobj as JzStripEAG).Move(offsetpt.X, offsetpt.Y);
                //}
                //else if (grobj is JzIdentityHoleEAG)
                //{
                //    (grobj as JzIdentityHoleEAG).Move(offsetpt.X, offsetpt.Y);
                //}
                //else if (grobj is JzCircleHoleEAG)
                //{
                //    (grobj as JzCircleHoleEAG).Move(offsetpt.X, offsetpt.Y);
                //}
            }

            //在按Control時候移動用的東西
            //MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.FromMovingObject);
        }
        /// <summary>
        /// 將 Moving Object 在畫布上畫出來
        /// </summary>
        /// <param name="grfx"></param>
        private void DrawShapes(Graphics grfx)
        {
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                (grobj as GeoFigure).Draw(grfx);
            }

            for (int i = JzStaticMover.Count - 1; i >= 0; i--)
            {
                grobj = JzStaticMover[i].Source;

                (grobj as GeoFigure).Draw(grfx);
            }
        }
        /// <summary>
        /// 鼠標在 Moving Obejct 上按下的操作
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool MouseDownShapes(MouseEventArgs e)
        {
            bool IsCatched = false;

            IsResizing = false;
            IsRotation = false;

            if (JzMover.Catch(e.Location, e.Button))
            {
                int caughtindex = JzMover.CaughtObject;
                GraphicalObject grobj = JzMover.CaughtSource;

                ReviseSelectList(caughtindex);

                if (e.Button == MouseButtons.Left)
                {
                    if (grobj is GeoFigure)
                    {
                        StartResizing(e.Location, ref IsResizing);
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    StartRotation(e.Location,ref IsRotation);
                }
                IsCatched = true;
            }

            OnMover(MoverOpEnum.SELECT, ToSelectListString(false));

            return IsCatched;
        }
        /// <summary>
        /// 鼠標在 Moving Obejct 上放開的操作
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool MouseUpShapes(MouseEventArgs e)
        {
            bool IsCatched = false;

            if (JzMover.Catch(e.Location))
            {
                if (JzMover.Release())
                {
                    GraphicalObject grobj = JzMover.ReleasedSource;
                    if (e.Button == MouseButtons.Left)
                    {
                        if (grobj is GeoFigure)
                        {
                            (grobj as GeoFigure).DefineCover();       // Redefine
                        }
                    }
                }
                IsCatched = true;

                //OnMover(MoverOpEnum.SELECT, ToSelectListString(false));
            }
            //else
            //{
            //    if (e.Button == MouseButtons.Right && Auxi_Geometry.Distance(ptMouse_Down, ptMouse_Up) <= 3)
            //    {
            //        //ContextMenuStrip = menuOnEmpty;
            //    }
            //}

            return IsCatched;
        }
        /// <summary>
        /// 備份 Moving Object
        /// </summary>
        private void BackupShape()  //Need To Added For New Shape
        {
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).Backup();
            }

            for (int i = JzStaticMover.Count - 1; i >= 0; i--)
            {
                grobj = JzStaticMover[i].Source;
                (grobj as GeoFigure).Backup();
            }

        }
        /// <summary>
        /// 回復 Moving Object
        /// </summary>
        private void RestoreShape() //Need To Added For New Shape
        {
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).Restore();
            }
            for (int i = JzStaticMover.Count - 1; i >= 0; i--)
            {
                grobj = JzStaticMover[i].Source;
                (grobj as GeoFigure).Restore();
            }

        }
        
        /// <summary>
        /// 備份原始圖形
        /// </summary>
        public void BackupImage()
        {
            bmpBackup.Dispose();
            bmpBackup = new Bitmap(bmpOrg);
        }
        public void RestoreImage()
        {
            bmpOrg.Dispose();
            bmpOrg = new Bitmap(bmpBackup);

            bmpPaint.Dispose();
            bmpPaint = new Bitmap(bmpOrg);

            bmpBackup.Dispose();
            bmpBackup = new Bitmap(1, 1);

            picDisplay.Invalidate();
        }

        /// <summary>
        /// 檢查是否有交界到的圖形
        /// </summary>
        private void CheckSelectedShape(bool isdirect)  //Need To Added For New Shape
        {

            if (myTime.msDuriation < 100 && !isdirect)
                return;

            myTime.Cut();

            // 若移動無超過3X3 則視為不小心點到的雜訊，過濾掉
            if (rectfSelect.Width < 3 && rectfSelect.Width < 3)
            {
                if (isdirect)
                    OnMover(MoverOpEnum.MOUSEUP, ToSelectListString(false));
                else
                    OnMover(MoverOpEnum.SELECT, ToSelectListString(false));

                FillInformation();

                return;
            }

            GraphicalObject grobj;
            bool IsCheckedInside = false;
            Rectangle rect = new Rectangle((int)rectfSelect.X, (int)rectfSelect.Y, (int)rectfSelect.Width, (int)rectfSelect.Height);
            
            //if (!IsHoldForSelct)
            //    ClearSelectShape();
            if (IsHoldForSelct)
            {
                if (SelectBackupList.Count > 0)
                    TransSelectList(SelectBackupList, SelectList);
            }

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                IsCheckedInside = false;
                grobj = JzMover[i].Source;

                if (!(grobj as GeoFigure).Visible)
                    continue;

                if (SelectBackupList.IndexOf(i) < 0)
                    IsCheckedInside = (grobj as GeoFigure).CheckIntersection(rect);
                else
                    IsCheckedInside = true;
                
                if (IsCheckedInside)
                {
                    if (SelectList.IndexOf(i) < 0)
                        SelectList.Add(i);

                    grobj = JzMover[SelectList[0]].Source;
                    (grobj as GeoFigure).IsFirstSelected = true;
                }
                else
                {
                    if (SelectList.Count > 0)
                    {
                        if (i == SelectList[0] && SelectList.Count > 1)
                        {
                            grobj = JzMover[SelectList[1]].Source;
                            (grobj as GeoFigure).IsFirstSelected = true;
                        }
                        SelectList.Remove(i);
                    }
                }
            }
            if(isdirect)
                OnMover(MoverOpEnum.MOUSEUP, ToSelectListString(false));
            else
                OnMover(MoverOpEnum.SELECT, ToSelectListString(false));
            FillInformation();

        }
        /// <summary>
        /// 清除所有圖形的選擇
        /// </summary>
        private void ClearSelectShape()
        {
            ClearSelectShape(true);
        }
        private void ClearSelectShape(bool isincludeselectlist) //Need To Added For New Shape
        {
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                (grobj as GeoFigure).IsSelected = false;
                (grobj as GeoFigure).IsFirstSelected = false;
            }

            if (isincludeselectlist)
            {
                SelectList.Clear();
                SelectBackupList.Clear();
            }
        }
        /// <summary>
        /// 對應原始圖形和畫布上圖形的位置
        /// </summary>
        /// <param name="neworgbias"></param>
        /// <param name="newratio"></param>
        /// <param name="mappingdir"></param>
        private void MappingShape(PointF neworgbias, double newratio, MappingDirectionEnum mappingdir) //Need To Added For New Shape
        {
            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                switch(mappingdir)
                {
                    case MappingDirectionEnum.ToMovingObject:
                        (grobj as GeoFigure).MappingToMovingObject(neworgbias, newratio);
                        break;
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
                    case MappingDirectionEnum.ToMovingObject:
                        (grobj as GeoFigure).MappingToMovingObject(neworgbias, newratio);
                        break;
                    case MappingDirectionEnum.FromMovingObject:
                        (grobj as GeoFigure).MappingFromMovingObject(neworgbias, newratio);
                        break;
                }
            }

        }

        public void MappingShape()
        {
            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.FromMovingObject);
        }


        /// <summary>
        /// 對應選擇圖形於 Select List
        /// </summary>
        /// <param name="selectlist"></param>
        private void MappingSelect(List<int> selectlist)
        {
            GraphicalObject grobj;

            if (selectlist.Count == 0)
                ClearSelectShape();
            else
            {
                for (int i = JzMover.Count - 1; i >= 0; i--)
                {
                    grobj = JzMover[i].Source;

                    if (selectlist.IndexOf(i) == 0)
                    {
                        (grobj as GeoFigure).IsFirstSelected = true;
                        (grobj as GeoFigure).IsSelected = true;
                    }
                    else if (selectlist.IndexOf(i) > 0)
                    {
                        (grobj as GeoFigure).IsFirstSelected = false;
                        (grobj as GeoFigure).IsSelected = true;
                    }
                    else
                    {
                        (grobj as GeoFigure).IsFirstSelected = false;
                        (grobj as GeoFigure).IsSelected = false;
                    }
                }
            }
        }
        private void MappingSelect(List<int> selectlist,bool istolist)
        {
            GraphicalObject grobj;

            if (!istolist)
            {
                MappingSelect(selectlist);
            }
            else
            {
                selectlist.Clear();

                for (int i = 0; i < JzMover.Count; i++)
                {
                    grobj = JzMover[i].Source;

                    if ((grobj as GeoFigure).IsFirstSelected)
                    {
                        selectlist.Insert(0, i);
                        //break;
                    }
                    else if((grobj as GeoFigure).IsSelected)
                    {
                        selectlist.Add(i);
                    }
                }
            }
        }
        public void MappingLsbSelect(List<int> lsbselectlist)
        {
            MappingSelect(lsbselectlist);

            MappingSelect(SelectBackupList, true);
            MappingSelect(SelectList, true);

            FillInformation();

            picDisplay.Invalidate();
        }
        /// <summary>
        /// 將Mover的資料對應到SlelectList去
        /// </summary>
        public void MappingSelect()
        {
            MappingSelect(SelectBackupList, true);
            MappingSelect(SelectList, true);

            picDisplay.Invalidate();
        }
        /// <summary>
        /// 回傳原始圖的Bitmap
        /// </summary>
        /// <returns></returns>
        public Bitmap GetPaintImage()
        {
            return bmpPaint;
        }
        /// <summary>
        /// 將兩個點合成一個矩型
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        private Rectangle RectTwoPoint(Point StartPoint, Point EndPoint,ref RectangleF rectf)
        {
            Point RecEndPoint = StartPoint;
            Point RecStartPoint = EndPoint;
            Rectangle Rect = new Rectangle();

            if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }
            else if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y <= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X <= RecStartPoint.X && RecEndPoint.Y <= RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X <= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }

            //if (Rect.X < 0)
            //    Rect.X = 0;
            if (Rect.Width <= 0)
                Rect.Width = 1;

            //if (Rect.Y < 0)
            //    Rect.Y = 0;

            if (Rect.Height <= 0)
                Rect.Height = 1;

            rectf = new RectangleF(Rect.X, Rect.Y, Rect.Width, Rect.Height);

            return Rect;

        }
        /// <summary>
        /// 將 From Selection List 複制到 To Selection List
        /// </summary>
        /// <param name="fromlist"></param>
        /// <param name="tolist"></param>
        private void TransSelectList(List<int> fromlist, List<int> tolist)
        {
            tolist.Clear();

            foreach(int index in fromlist)
            {
                tolist.Add(index);
            }
        }
        /// <summary>
        /// 根據Caught Index 改變 Select List 的對應
        /// </summary>
        /// <param name="caughtindex"></param>
        private void ReviseSelectList(int caughtindex)
        {
            if (SelectBackupList.IndexOf(caughtindex) > -1 && SelectBackupList[0] != caughtindex)
            {
                SelectBackupList.Remove(caughtindex);
                SelectBackupList.Insert(0, caughtindex);

                MappingSelect(SelectBackupList);
                TransSelectList(SelectBackupList, SelectList);

                //picDisplay.Invalidate();
            }
            else if (SelectBackupList.IndexOf(caughtindex) < 0)
            {
                if (IsHoldForSelct)
                {
                    SelectBackupList.Add(caughtindex);

                    MappingSelect(SelectBackupList);
                    TransSelectList(SelectBackupList, SelectList);
                }
                else
                {
                    ClearSelectShape();

                    SelectBackupList.Clear();
                    SelectBackupList.Add(caughtindex);

                    MappingSelect(SelectBackupList);
                    TransSelectList(SelectBackupList, SelectList);

                    //picDisplay.Invalidate();
                }
            }

        }
        /// <summary>
        /// 新增圖形
        /// </summary>
        /// <param name="shape"></param>
        public void AddShape(ShapeEnum shape)
        {
            int copyindex = 0;

            if(JzMover.Count == 0)
            {
                AddShape(ConvertShape(shape), false, copyindex);
            }
            else
            {
                if(SelectList.Count == 0)
                {
                    copyindex = JzMover.Count - 1;
                    AddShape(Figure_EAG.Any, true, copyindex);
                }
                else
                {
                    for (int i = JzMover.Count - 1; i >= 0; i--)
                    {
                        if (SelectBackupList.IndexOf(i) > -1)
                        {
                            AddShape(Figure_EAG.Any, true, i);
                        }
                    }
                }
            }

            MappingSelect(SelectBackupList, true);
            TransSelectList(SelectBackupList, SelectList);
            
            OnMover(MoverOpEnum.SELECT, ToSelectListString(false));


            picDisplay.Invalidate();
            FillInformation();
            
        }
        /// <summary>
        /// 刪除圖形 
        /// </summary>
        public void DelShape()
        {
            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                if (SelectBackupList.IndexOf(i) > -1)
                {
                    JzMover.RemoveAt(i);
                }
            }

            OnMover(MoverOpEnum.DEL, ToSelectListString(false));

            MappingSelect(SelectBackupList, true);
            TransSelectList(SelectBackupList, SelectList);

            picDisplay.Invalidate();
            FillInformation();

            
        }
        /// <summary>
        /// 新增圖形的副程式
        /// </summary>
        /// <param name="figure"></param>
        /// <param name="iscopy"></param>
        /// <param name="copyindex"></param>
        void AddShape(Figure_EAG figure,bool iscopy,int copyindex)  //Need To Added For New Shape
        {
            PointF ptC = new PointF();
            int nVertices = 0;
            PointF[] pts = new PointF[10];
            Color[] clrs = new Color[10];

            int polyvertices = 4;
            double polydegree = 45;

            if (!iscopy)
            {
                switch (figure)
                {
                    case Figure_EAG.Rectangle:
                        JzRectEAG jzrect = new JzRectEAG(picDisplay, JzMover, new PointF(100, 100), 100, 100, 0, Color.FromArgb(60, Color.Red), false);
                        jzrect.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzrect.IsFirstSelected = true;
                        jzrect.IsSelected = true;

                        JzMover.Add(jzrect);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzrect.Figure).ToString());

                        break;
                    case Figure_EAG.Circle:
                        JzCircleEAG jzcircle = new JzCircleEAG(picDisplay, JzMover, new PointF(100, 100), 100, Color.FromArgb(60, Color.Red), false);
                        jzcircle.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzcircle.IsFirstSelected = true;
                        jzcircle.IsSelected = true;

                        JzMover.Add(jzcircle);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzcircle.Figure).ToString());
                        break;
                    case Figure_EAG.ChatoyantPolygon:

                        ptC = new PointF(100, 100);
                        nVertices = 12;
                        pts = new PointF[] { new PointF(50, 50), new PointF(75, 50), new PointF(125, 50), new PointF(150, 50), new PointF(150, 75), new PointF(150, 125), new PointF(150, 150), new PointF(125, 150), new PointF(75, 150), new PointF(50, 150), new PointF(50, 125), new PointF(50, 75) };

                        //int nVertices = 4;
                        //PointF[] pts = new PointF[] { new PointF(50, 50), new PointF(150, 50), new PointF(150, 150), new PointF(50, 150)};

                        clrs = Auxi_Colours.SmoothChangedColors(nVertices, Color.FromArgb(60, Color.Red), Color.FromArgb(60, Color.Red));

                        JzPolyEAG jzpoly = new JzPolyEAG(picDisplay, JzMover, ptC, pts, Color.FromArgb(60, Color.Red), clrs, false);
                        jzpoly.AuxiLines = false;

                        jzpoly.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzpoly.IsFirstSelected = true;
                        jzpoly.IsSelected = true;

                        JzMover.Add(jzpoly);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzpoly.Figure).ToString());
                        break;
                    case Figure_EAG.Ring:
                    case Figure_EAG.ORing:

                        if (figure == Figure_EAG.Ring)
                        {
                            clrs = Auxi_Colours.SmoothChangedColors(2, Color.FromArgb(60, Color.Red), Color.FromArgb(60, Color.Red));
                            clrs[1] = Color.FromArgb(0, Color.Red);
                        }
                        else
                        {
                            clrs = new Color[1];
                            clrs[0] = Color.FromArgb(60, Color.Red);
                        }

                        JzRingEAG jzring = new JzRingEAG(picDisplay, JzMover, new PointF(100, 100), 50, 100, 1.0d, clrs, false, figure);
                        jzring.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzring.IsFirstSelected = true;
                        jzring.IsSelected = true;

                        JzMover.Add(jzring);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzring.Figure).ToString());

                        break;
                    case Figure_EAG.Strip:

                        JzStripEAG jzstrip = new JzStripEAG(picDisplay, JzMover, new PointF(100, 100), new PointF(200, 100), 50, Color.FromArgb(60, Color.Red), false);
                        jzstrip.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzstrip.IsFirstSelected = true;
                        jzstrip.IsSelected = true;

                        JzMover.Add(jzstrip);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzstrip.Figure).ToString());
                        break;
                    case Figure_EAG.HexHex:
                    case Figure_EAG.RectRect:

                        polyvertices = 4;
                        polydegree = 45;

                        if (figure == Figure_EAG.HexHex)
                        {
                            polyvertices = 6;
                            polydegree = 0;
                        }

                        JzIdentityHoleEAG jzpolypoly = new JzIdentityHoleEAG(picDisplay, JzMover, new PointF(100, 100), 50, 100, polyvertices, polydegree, Color.FromArgb(60, Color.Red), false, figure);
                        jzpolypoly.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzpolypoly.IsFirstSelected = true;
                        jzpolypoly.IsSelected = true;

                        JzMover.Add(jzpolypoly);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzpolypoly.Figure).ToString());
                        break;
                    case Figure_EAG.HexO:
                    case Figure_EAG.RectO:
                        
                        polyvertices = 4;
                        polydegree = 45;

                        if (figure == Figure_EAG.HexO)
                        {
                            polyvertices = 6;
                            polydegree = 0;
                        }

                        JzCircleHoleEAG jzpolyo = new JzCircleHoleEAG(picDisplay, JzMover, new PointF(100, 100), 50, 100, polyvertices, polydegree, Color.FromArgb(60, Color.Red), false, figure);
                        jzpolyo.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                        jzpolyo.IsFirstSelected = true;
                        jzpolyo.IsSelected = true;

                        JzMover.Add(jzpolyo);

                        OnMover(MoverOpEnum.ADD, ConvertShape(jzpolyo.Figure).ToString());

                        break;
                }
            }
            else  //Need To Added For New Shape
            {
                GraphicalObject grobj = JzMover[copyindex].Source;

                if (grobj is JzRectEAG)
                {
                    JzRectEAG jzrect = new JzRectEAG(picDisplay, JzMover, (grobj as JzRectEAG));
                    jzrect.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzrect);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzrect.Figure).ToString());
                }
                else if (grobj is JzCircleEAG)
                {
                    JzCircleEAG jzcircle = new JzCircleEAG(picDisplay, JzMover, (grobj as JzCircleEAG));
                    jzcircle.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzcircle);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzcircle.Figure).ToString());
                }
                else if (grobj is JzPolyEAG)
                {
                    JzPolyEAG jzpoly = new JzPolyEAG(picDisplay, JzMover, (grobj as JzPolyEAG));
                    jzpoly.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzpoly);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzpoly.Figure).ToString());
                }
                else if (grobj is JzRingEAG)
                {
                    JzRingEAG jzring = new JzRingEAG(picDisplay, JzMover, (grobj as JzRingEAG));
                    jzring.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzring);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzring.Figure).ToString());
                }
                else if (grobj is JzStripEAG)
                {
                    JzStripEAG jzstrip = new JzStripEAG(picDisplay, JzMover, (grobj as JzStripEAG));
                    jzstrip.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzstrip);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzstrip.Figure).ToString());
                }
                else if (grobj is JzIdentityHoleEAG)
                {
                    JzIdentityHoleEAG jzpolyid = new JzIdentityHoleEAG(picDisplay, JzMover, (grobj as JzIdentityHoleEAG));
                    jzpolyid.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzpolyid);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzpolyid.Figure).ToString());
                }
                else if (grobj is JzCircleHoleEAG)
                {
                    JzCircleHoleEAG jzpolyo = new JzCircleHoleEAG(picDisplay, JzMover, (grobj as JzCircleHoleEAG));
                    jzpolyo.MappingToMovingObject(rectFPaintTo.Location, RatioNow);

                    JzMover.Add(jzpolyo);

                    OnMover(MoverOpEnum.ADD, ConvertShape(jzpolyo.Figure).ToString());
                }
            }

        }
        /// <summary>
        /// Shape 對應 Figure 的轉換
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        Figure_EAG ConvertShape(ShapeEnum shape)    //Nedd To Added For New Shape    
        {
            Figure_EAG retfigure = Figure_EAG.Rectangle;

            switch(shape)
            {
                case ShapeEnum.RECT:
                    retfigure = Figure_EAG.Rectangle;
                    break;
                case ShapeEnum.CIRCLE:
                    retfigure = Figure_EAG.Circle;
                    break;
                case ShapeEnum.POLY:
                    retfigure = Figure_EAG.ChatoyantPolygon;
                    break;
                case ShapeEnum.RING:
                    retfigure = Figure_EAG.Ring;
                    break;
                case ShapeEnum.ORING:
                    retfigure = Figure_EAG.ORing;
                    break;
                case ShapeEnum.CAPSULE:
                    retfigure = Figure_EAG.Strip;
                    break;
                case ShapeEnum.RECTRECT:
                    retfigure = Figure_EAG.RectRect;
                    break;
                case ShapeEnum.HEXHEX:
                    retfigure = Figure_EAG.HexHex;
                    break;
                case ShapeEnum.RECTO:
                    retfigure = Figure_EAG.RectO;
                    break;
                case ShapeEnum.HEXO:
                    retfigure = Figure_EAG.HexO;
                    break;
            }

            return retfigure;
        }
        /// <summary>
        /// Figure 對應 Shape 的轉換
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        ShapeEnum ConvertShape(Figure_EAG shape)    //Nedd To Added For New Shape
        {
            ShapeEnum retshape = ShapeEnum.RECT;

            switch (shape)
            {
                case Figure_EAG.Rectangle:
                    retshape = ShapeEnum.RECT;
                    break;
                case Figure_EAG.Circle:
                    retshape = ShapeEnum.CIRCLE;
                    break;
                case Figure_EAG.ChatoyantPolygon:
                    retshape = ShapeEnum.POLY;
                    break;
                case Figure_EAG.Ring:
                    retshape = ShapeEnum.RING;
                    break;
                case Figure_EAG.ORing:
                    retshape = ShapeEnum.ORING;
                    break;
                case Figure_EAG.Strip:
                    retshape = ShapeEnum.CAPSULE;
                    break;
                case Figure_EAG.RectRect:
                    retshape = ShapeEnum.RECTRECT;
                    break;
                case Figure_EAG.HexHex:
                    retshape = ShapeEnum.HEXHEX;
                    break;
                case Figure_EAG.RectO:
                    retshape = ShapeEnum.RECTO;
                    break;
                case Figure_EAG.HexO:
                    retshape = ShapeEnum.HEXO;
                    break;
            }

            return retshape;
        }
        /// <summary>
        /// 移動選擇的資料
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveMover(int x,int y)
        {
            StartMove(new Point(x, y));
            picDisplay.Invalidate();
        }
        /// <summary>
        /// 改變尺吋 <-== To Be Continue
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SizeMover(int x,int y)          //Havn't Complete
        {
            for (int i = 0; i < JzMover.Count; i++)
            {
                GraphicalObject grobj = JzMover[i].Source;

                if ((grobj as GeoFigure).IsSelected)
                {
                    if (grobj is JzRectEAG)
                    {
                        //(grobj as JzRectEAG).MoveNode(1, x, y, MouseButtons.Left);
                    }
                    else if (grobj is JzCircleEAG)
                    {
                        //(grobj as JzCircleEAG).StartResizing(new Point(x, y));
                    }
                }
            }

            picDisplay.Invalidate();
        }
        /// <summary>
        /// 允許動的層
        /// </summary>
        /// <param name="level"></param>
        public void Lock(int level,bool isonly)
        {
            for (int i = 0; i < JzMover.Count; i++)
            {
                GraphicalObject grobj = JzMover[i].Source;

                bool isfigureshouldhide = false;

                if(isonly)
                {
                    isfigureshouldhide = (grobj as GeoFigure).RelateLevel != level && level != 0;
                }
                else
                {
                    isfigureshouldhide = (grobj as GeoFigure).RelateLevel < level && level != 0;
                }
                
                //if((grobj as GeoFigure).RelateLevel != level && level != 0)

                if(isfigureshouldhide)
                {
                    (grobj as GeoFigure).Visible = false;
                    (grobj as GeoFigure).TransparentForMover = true;
                }
                else
                {
                    (grobj as GeoFigure).Visible = true;
                    (grobj as GeoFigure).TransparentForMover = false;
                }
                
                (grobj as GeoFigure).IsFirstSelected = false;
                (grobj as GeoFigure).IsSelected= false;
            }

            MappingSelect();

            OnMover(MoverOpEnum.SELECT, ToSelectListString(false));
            //picDisplay.Invalidate();
        }
        /// <summary>
        /// 設定Matching 的方式
        /// </summary>
        /// <param name="bmpmatching"></param>
        /// <param name="matchmethod"></param>
        public void SetMatching(Bitmap bmpmatching,MatchMethodEnum matchmethod)
        {
            lock(obj)
            {
                bmpPaint.Dispose();
                bmpPaint = new Bitmap(bmpOrg);

                switch(matchmethod)
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
            picDisplay.Invalidate();
        }
        public void SaveScreen()
        {
            Bitmap bmp = new Bitmap(bmpOrg);

            BackupShape();

            MappingShape(new PointF(0, 0), 1d, MappingDirectionEnum.ToMovingObject);

            Graphics gfx = Graphics.FromImage(bmp);

            int i = 0;

            GraphicalObject grobj;

            i = 0;
            while(i < JzMover.Count)
            {
                grobj = JzMover[i].Source;
                (grobj as GeoFigure).Draw(gfx);

                i++;
            }
            i = 0;
            while (i < JzStaticMover.Count)
            {
                grobj = JzStaticMover[i].Source;
                (grobj as GeoFigure).Draw(gfx);

                i++;
            }
            gfx.Dispose();

            RestoreShape();

            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

            picDisplay.Invalidate();

            bmp.Save(Universal.TESTPATH + "\\PRTSCREEN" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);
            bmp.Dispose();
        }
        public Bitmap GetScreen()
        {
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
        }

        public Bitmap GetOrgBMP()
        {
            return bmpOrg;
        }
        Point PointFConvert(PointF PTF)
        {
            return new Point((int)PTF.X, (int)PTF.Y);
        }

        /// <summary>
        /// 賜死!
        /// </summary>
        public void Suicide()
        {
            JzMover.Clear();

            bmpOrg.Dispose();
            bmpPaint.Dispose();
            bmpBackup.Dispose();
        }
        #endregion

        #region Event Operation
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
        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (iMode == -1)
                return;

            Graphics grfx = e.Graphics;

            grfx.DrawImage(bmpPaint, rectFPaintTo, rectFPaintFrom, GraphicsUnit.Pixel);
            DrawShapes(grfx);

            if (IsLeftMouseDown)
            {
                switch(DISPLAYTYPE)
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
        private void Pic_MouseMove(object sender, MouseEventArgs e)
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

                switch(DISPLAYTYPE)
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
                if (JzMover.Move(e.Location))
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

                        rectFPaintTo = rectFMouseDown;
                        rectFPaintTo.Offset(ptfOffset);

                        RestoreShape();

                        MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                        picDisplay.Invalidate();

                        break;
                }
            }
            FillInformation();

        }
        private void PicDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            if (iMode == -1)
                return;

            IsMouseDown = false;

            ptMouse_Up = e.Location;

            if(MouseUpShapes(e))
            {
                MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.FromMovingObject);
            }

            switch(DISPLAYTYPE)
            {
                case DisplayTypeEnum.CAPTRUE:
                    RectangleF rectftmp = new RectangleF();

                    RectTwoPoint(PointFConvert(ptfMapping(ptMouse_Down)), PointFConvert(ptfMapping(ptMouse_Up)), ref rectftmp);

                    OnCapture(rectftmp);
                    break;
            }

            switch(e.Button)
            {
                case MouseButtons.Left:

                    IsLeftMouseDown = false;
                    myDisplayTimer.Stop();
                    CheckSelectedShape(true);
                    TransSelectList(SelectList, SelectBackupList);

                    if(IsMove)
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
            if (DISPLAYTYPE == DisplayTypeEnum.SHOW  && e.Button == MouseButtons.Left)
                return;
            
            IsMouseDown = true;

            ptMouse_Down = e.Location;
            rectFMouseDown = rectFPaintTo;

            BackupShape();

            //If Catched Then Get Out
            if(MouseDownShapes(e))
            {
                FillInformation();
                return;
            }

            switch(e.Button)
            {
                case MouseButtons.Left:
                    IsLeftMouseDown = true;

                    myDisplayTimer.Start();

                    if (!IsHoldForSelct)
                        ClearSelectShape();

                    switch(DISPLAYTYPE)
                    {
                        case DisplayTypeEnum.ADJUST:
                            OnMover(MoverOpEnum.READYTOMOVE, "");
                            break;
                    }


                    break;
                case MouseButtons.Right:

                    Cursor.Current = Cursors.SizeAll;

                    break;
            }

            picDisplay.Invalidate();

        }
        private void PicDisplay_DoubleClick(object sender, EventArgs e)
        {
            DefaultView();
        }
        private void PicDisplay_MouseWheel(object sender, MouseEventArgs e)
        {
            if (myTime.msDuriation < 100)
                return;

            myTime.Cut();

            if (IsMouseDown)
                return;

            float sizingratio = (float)Math.Pow(2d, (e.Delta > 0 ? 1d : -1d));
            double nextratio = RatioNow * (double)sizingratio;

            if (e.Delta < 0)
            {
                if (nextratio >= MinRatio)
                {
                    RatioNow = nextratio;

                    rectFPaintTo.X = ((ptMouse_Move.X - rectFPaintTo.X) / 2) + rectFPaintTo.X;
                    rectFPaintTo.Y = ((ptMouse_Move.Y - rectFPaintTo.Y) / 2) + rectFPaintTo.Y;

                    rectFPaintTo.Width = ((float)rectFPaintTo.Width / 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height / 2);

                    MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }
            else if (e.Delta > 0)
            {
                if (nextratio < MaxRatio)
                {
                    RatioNow = nextratio;

                    rectFPaintTo.X = (rectFPaintTo.X - ptMouse_Move.X) + rectFPaintTo.X;
                    rectFPaintTo.Y = (rectFPaintTo.Y - ptMouse_Move.Y) + rectFPaintTo.Y;

                    rectFPaintTo.Width = ((float)rectFPaintTo.Width * 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height * 2);

                    MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }

            FillInformation();
        }
        public bool IsPointInsideDisplay(Point pt)
        {
            bool ret = false;

            OnDebug("IS CHECKING");

            if (IsMouseEnter)
            {
                OnDebug("IS ENTER"); 

                Rectangle rect = new Rectangle(picDisplay.PointToScreen(picDisplay.Location), picDisplay.Size);
                Rectangle simplerect = new Rectangle(pt, new Size(1, 1));

                ret = rect.IntersectsWith(simplerect);
            }

            return ret;
        }
        
        public string ToSelectListString()
        {
            return ToSelectListString(true);
        }
        public string ToSelectListString(bool iswithquote)
        {
            string retstr = "";

            if (SelectList.Count > 0)
            {
                if(iswithquote)
                    retstr += "[";

                foreach (int index in SelectList)
                {
                    //retstr += index.ToString() + ",";

                    GraphicalObject grobj = JzMover[index].Source;
                    retstr += (grobj as GeoFigure).RelateNo.ToString() + ":" + (grobj as GeoFigure).RelatePosition.ToString() + ",";

                }

                retstr = retstr.Remove(retstr.Length - 1, 1);

                if (iswithquote)
                    retstr += "]";
            }

            return retstr;
        }

        public void GetSerachImage(ref Bitmap bmp)
        {
            Bitmap bmpx = new Bitmap(1, 1);

            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;

                (grobj as GeoFigure).GenSearchImage(0, 0, bmpOrg, ref bmp, ref bmpx);
            }

            bmpx.Dispose();
        }

        int GreyscaleInt(byte R, byte G, byte B)
        {
            return (int)((double)R * 0.3 + (double)G * 0.59 + (double)B * 0.11);
        }

        #endregion

        #region Experiment Functions
        public void SimWheel(int delta)
        {
            float sizingratio = (float)Math.Pow(2d, (delta > 0 ? 1d : -1d));
            double nextratio = RatioNow * (double) sizingratio;

            PointF ptMouse_Move = new PointF(40, 67);

            if (delta < 0)
            {
                if (nextratio >= MinRatio)
                {
                    RatioNow = nextratio;

                    rectFPaintTo.X = ((ptMouse_Move.X - rectFPaintTo.X) / 2) + rectFPaintTo.X;
                    rectFPaintTo.Y = ((ptMouse_Move.Y - rectFPaintTo.Y) / 2) + rectFPaintTo.Y;

                    rectFPaintTo.Width = ((float)rectFPaintTo.Width / 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height / 2);

                    MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }
            else if (delta > 0)
            {
                if (nextratio < MaxRatio)
                {
                    RatioNow = nextratio;

                    rectFPaintTo.X = (rectFPaintTo.X - ptMouse_Move.X) + rectFPaintTo.X;
                    rectFPaintTo.Y = (rectFPaintTo.Y - ptMouse_Move.Y) + rectFPaintTo.Y;

                    rectFPaintTo.Width = ((float)rectFPaintTo.Width * 2);
                    rectFPaintTo.Height = ((float)rectFPaintTo.Height * 2);

                    MappingShape(rectFPaintTo.Location, RatioNow,MappingDirectionEnum.ToMovingObject);

                    picDisplay.Invalidate();
                }
            }

            FillInformation();


        }
        public void AddRect()
        {
            //RectangleF rectF 

            int i = 0;

            while (i < 1)
            {
                JzRectEAG jzrect = new JzRectEAG(picDisplay, JzMover, new PointF(100 + i * 30, 100 + i *30), 100, 100, 0, Color.FromArgb(60, Color.Red), false);
                jzrect.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                JzMover.Add(jzrect);

                //JzMover.CatchMover()

                i++;
            }

            picDisplay.Invalidate();
        }
        public string ToStatusString()      //Nedd To Added For New Shape
        {
            string retstr = "";

            GraphicalObject grobj;

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                
                retstr += (grobj as GeoFigure).ToString() + Environment.NewLine;

                //if (grobj is JzRectEAG)
                //{
                //    retstr += (grobj as JzRectEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzCircleEAG)
                //{
                //    retstr += (grobj as JzCircleEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzPolyEAG)
                //{
                //    retstr += (grobj as JzPolyEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzRingEAG)
                //{
                //    retstr += (grobj as JzRingEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzStripEAG)
                //{
                //    retstr += (grobj as JzStripEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzIdentityHoleEAG)
                //{
                //    retstr += (grobj as JzIdentityHoleEAG).ToString() + Environment.NewLine;
                //}
                //else if (grobj is JzCircleHoleEAG)
                //{
                //    retstr += (grobj as JzCircleHoleEAG).ToString() + Environment.NewLine;
                //}

            }
            retstr = retstr.Remove(retstr.Length - 2);

            return retstr;
        }
        public void FromStatusString(string statusstr)       //Nedd To Added For New Shape
        {
            string[] strs = statusstr.Replace(Environment.NewLine, "x").Split('x');

            JzMover.Clear();

            foreach (string str in strs)
            {
                if (str.IndexOf(Figure_EAG.Rectangle.ToString()) > -1)
                {
                    JzRectEAG jzrect = new JzRectEAG(picDisplay, JzMover, str, Color.FromArgb(0, Color.Red));
                    jzrect.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    JzMover.Add(jzrect);
                }
                else if (str.IndexOf(Figure_EAG.Circle.ToString()) > -1)
                {
                    JzCircleEAG jzcircle = new JzCircleEAG(picDisplay, JzMover, str, Color.FromArgb(0, Color.Red));
                    jzcircle.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    JzMover.Add(jzcircle);
                }
                else if (str.IndexOf(Figure_EAG.ChatoyantPolygon.ToString()) > -1)
                {
                    JzPolyEAG jzpoly = new JzPolyEAG(picDisplay, JzMover, str, Color.FromArgb(0, Color.Red));
                    jzpoly.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    JzMover.Add(jzpoly);
                }
                else if (str.IndexOf(Figure_EAG.Ring.ToString()) > -1 || str.IndexOf(Figure_EAG.ORing.ToString()) > -1)
                {
                    JzRingEAG jzring = new JzRingEAG(picDisplay, JzMover, str, Color.FromArgb(0, Color.Red));
                    jzring.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    JzMover.Add(jzring);
                }
                else if (str.IndexOf(Figure_EAG.Strip.ToString()) > -1)
                {
                    JzStripEAG jzstrip = new JzStripEAG(picDisplay, JzMover, str, Color.FromArgb(0, Color.Red));
                    jzstrip.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    JzMover.Add(jzstrip);
                }
                else if (str.IndexOf(Figure_EAG.RectRect.ToString()) > -1 || str.IndexOf(Figure_EAG.HexHex.ToString()) > -1)
                {
                    //Ring_EAG jzring = new Ring_EAG(picDisplay, JzMover, str, Color.FromArgb(60, Color.Red));
                    //jzring.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    //JzMover.Add(jzring);
                }
                else if (str.IndexOf(Figure_EAG.RectO.ToString()) > -1 || str.IndexOf(Figure_EAG.HexO.ToString()) > -1)
                {
                    //Ring_EAG jzring = new Ring_EAG(picDisplay, JzMover, str, Color.FromArgb(60, Color.Red));
                    //jzring.MappingToMovingObject(rectFPaintTo.Location, RatioNow);
                    //JzMover.Add(jzring);
                }
            }

            //picDisplay.Invalidate();
        }
        public void ClearDisplay()
        {
            bmpOrg.Dispose();
            bmpOrg = new Bitmap(1, 1);

            bmpPaint.Dispose();
            bmpPaint = new Bitmap(1, 1);

            SelectBackupList.Clear();
            SelectList.Clear();
            
            JzMover.Clear();

            picDisplay.Invalidate();
        }
        public void ClearMover()
        {
            SelectBackupList.Clear();
            SelectList.Clear();

            JzMover.Clear();

            picDisplay.Invalidate();
        }
        //private bool IsRealIntersect(ref bool isintersected, GraphicalObject grobj)
        //{
        //    bool ret = false;

        //    if (grobj is Rectangle_EAG)
        //    {
        //        for (int i = 0; i < (int)rectf.Width; i++)
        //        {
        //            for (int j = 0; j < (int)rectf.Height; j++)
        //            {
        //                if((grobj as Rectangle_EAG).IsInside(new Point((int)rectf.X + i, (int)rectf.Y + j)))
        //                {
        //                    if (isintersected)
        //                        (grobj as Rectangle_EAG).IsFirstSelected = false;
        //                    else
        //                    {
        //                        isintersected = true;
        //                        (grobj as Rectangle_EAG).IsFirstSelected = true;
        //                    }

        //                    (grobj as Rectangle_EAG).IsSelected = true;

        //                    i = 100000;
        //                    j = 100000;

        //                }
        //            }
        //        }
        //    }
        //    //JzMover.Release();
        //    return ret;
        //}
        /// <summary>
        /// No Use Code
        /// </summary>
        /// <param name="rectf"></param>
        /// <param name="isintersected"></param>
        /// <returns></returns>
        private bool IsRealIntersect(RectangleF rectf, bool isintersected)
        {
            bool ret = false;

            //for (int i = 0; i < (int)rectf.Width; i++)
            //{
            //    for (int j = 0; j < (int)rectf.Height; j++)
            //    {
            //        if (JzMover.CatchMover(new Point((int)rectf.X + i, (int)rectf.Y + j)))
            //        {
            //            GraphicalObject grobj = JzMover.CaughtSource;

            //            if (grobj is Rectangle_EAG)
            //            {
            //                if (isintersected)
            //                    (grobj as Rectangle_EAG).IsFirstSelected = false;
            //                else
            //                {
            //                    isintersected = true;
            //                    (grobj as Rectangle_EAG).IsFirstSelected = true;
            //                }

            //                (grobj as Rectangle_EAG).IsSelected = true;

            //                i = 100000;
            //                j = 100000;
            //            }
            //        }
            //    }
            //}

            //JzMover.Release();

            return ret;
        }
        public void GetMask(int outrangex, int outrangey)       //Nedd To Added For New Shape
        {
            GraphicalObject grobj;

            Bitmap bmpfind = new Bitmap(1, 1);
            Bitmap bmpmask = new Bitmap(1, 1);

            for (int i = JzMover.Count - 1; i >= 0; i--)
            {
                grobj = JzMover[i].Source;
                if (grobj is JzRectEAG)
                {
                    (grobj as JzRectEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind,ref bmpmask);
                    
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    //bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);
                    
                    (grobj as JzRectEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);
                    
                    (grobj as JzRectEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    //bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzRectEAG).DigImage(0, 0, bmpfind);

                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzCircleEAG)
                {
                    (grobj as JzCircleEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzCircleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzCircleEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzCircleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzPolyEAG)
                {
                    (grobj as JzPolyEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzPolyEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzPolyEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzPolyEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzRingEAG)
                {
                    (grobj as JzRingEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzRingEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzRingEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzRingEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzStripEAG)
                {
                    (grobj as JzStripEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzStripEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzStripEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzStripEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzIdentityHoleEAG)
                {
                    (grobj as JzIdentityHoleEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzIdentityHoleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzIdentityHoleEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzIdentityHoleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
                else if (grobj is JzCircleHoleEAG)
                {
                    (grobj as JzCircleHoleEAG).GenSearchImage(outrangex, outrangey, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "L.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M.png", ImageFormat.Png);

                    (grobj as JzCircleHoleEAG).GetMaskedImage(bmpfind, bmpmask, Color.Black, Color.Red, false);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "M1.png", ImageFormat.Png);

                    //(grobj as Rectangle_EAG).DigImage(outrangex, outrangey, bmpfind);
                    //bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "N.png", ImageFormat.Png);


                    (grobj as JzCircleHoleEAG).GenSearchImage(0, 0, bmpPaint, ref bmpfind, ref bmpmask);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "O.png", ImageFormat.Png);
                    bmpmask.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "P.png", ImageFormat.Png);

                    (grobj as JzCircleHoleEAG).DigImage(0, 0, bmpfind);

                    bmpfind.Save(@"D:\\DISPLAYTEST\\" + i.ToString("00") + "Q.png", ImageFormat.Png);
                }
            }

            bmpfind.Dispose();
            bmpmask.Dispose();
        }
        #endregion

        public delegate void MoverHandler(MoverOpEnum moverop,string opstring);
        public event MoverHandler MoverAction;
        public void OnMover(MoverOpEnum moverop, string opstring)
        {
            if (MoverAction != null)
            {
                MoverAction(moverop, opstring);
            }
        }

        public delegate void AdjustHandler(PointF ptfoffset);
        public event AdjustHandler AdjustAction;
        public void OnAdjustAction(PointF ptfoffset)
        {
            if (AdjustAction != null)
            {
                AdjustAction(ptfoffset);
            }
        }

        public delegate void DebugHandler(string opstring);
        public event DebugHandler DebugAction;
        public void OnDebug(string opstring)
        {
            if (DebugAction != null)
            {
                DebugAction(opstring);
            }
        }

        public delegate void CaputreHandler(RectangleF rectf);
        public event CaputreHandler CaptureAction;
        public void OnCapture(RectangleF rectf)
        {
            if (CaptureAction != null)
            {
                CaptureAction(rectf);
            }
        }

        #region Old Code
        //public void PicDisplay_MouseWheel(float delta)
        //{
        //    if (myTime.msDuriation < 100)
        //        return;

        //    myTime.Cut();

        //    if (IsMouseDown)
        //        return;

        //    float sizingratio = (float)Math.Pow(2d, (delta > 0 ? 1d : -1d));
        //    double nextratio = RatioNow * (double)sizingratio;

        //    if (delta < 0)
        //    {
        //        if (nextratio >= MinRatio)
        //        {
        //            RatioNow = nextratio;

        //            rectFPaintTo.X = ((ptMouse_Move.X - rectFPaintTo.X) / 2) + rectFPaintTo.X;
        //            rectFPaintTo.Y = ((ptMouse_Move.Y - rectFPaintTo.Y) / 2) + rectFPaintTo.Y;

        //            rectFPaintTo.Width = ((float)rectFPaintTo.Width / 2);
        //            rectFPaintTo.Height = ((float)rectFPaintTo.Height / 2);

        //            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

        //            picDisplay.Invalidate();
        //        }
        //    }
        //    else if (delta > 0)
        //    {
        //        if (nextratio < MaxRatio)
        //        {
        //            RatioNow = nextratio;

        //            rectFPaintTo.X = (rectFPaintTo.X - ptMouse_Move.X) + rectFPaintTo.X;
        //            rectFPaintTo.Y = (rectFPaintTo.Y - ptMouse_Move.Y) + rectFPaintTo.Y;

        //            rectFPaintTo.Width = ((float)rectFPaintTo.Width * 2);
        //            rectFPaintTo.Height = ((float)rectFPaintTo.Height * 2);

        //            MappingShape(rectFPaintTo.Location, RatioNow, MappingDirectionEnum.ToMovingObject);

        //            picDisplay.Invalidate();
        //        }
        //    }

        //    FillInformation();
        //}

        #endregion
    }
}
