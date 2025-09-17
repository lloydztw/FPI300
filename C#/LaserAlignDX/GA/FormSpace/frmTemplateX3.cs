using JetEazy.BasicSpace;
using JetEazy.Utils;
using JzDisplay;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace.RecipeSpace;
using MoveGraphLibrary;
using System;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;
using VisionDesigner.PairLineFind;
using VsCommon.ControlSpace.MachineSpace;
using WorldOfMoveableObjects;


namespace LaserAlignDX.FormSpace
{
    public partial class frmTemplateX3 : Form
    {
        //CMvdImage xMvdTestImage = new CMvdImage();
        //LineScanCalibrateClass LineScanCalibrate
        //{
        //    get { return Traveller106.Universal.LineScanCalibrateClasses[cboCaliIndex.SelectedIndex]; }
        //}

        #region INTERACTORS
        Mover xMovers = new Mover();
        Mover xMoversDs1 = new Mover();
        #endregion

        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }

        /// <summary>
        /// Golden Chip Template Rect
        /// 優化後, 已經不用於 PLC 座標數據之計算
        /// </summary>
        RectangleF LeftTopRect
        {
            get { return xRecipe.xRegionTrain; }
        }

        /// <summary>
        /// 目前 Gaara 定義為 Camera Grid 左上 [row=0, col=0] 中心點
        /// 優化後, 已經不用於 PLC 座標數據之計算
        /// </summary>
        PointF LeftTopRectCenter
        {
            get
            {
                PointF ptCenter
                    = new PointF(LeftTopRect.X + LeftTopRect.Width / 2 + xRecipe.xRectRegionPrint.X,
                                 LeftTopRect.Y + LeftTopRect.Height / 2 + xRecipe.xRectRegionPrint.Y);
                return ptCenter;
            }
        }

        #region GUI_MEMBERS
        //int xMoverIndex = 0;
        //Mover xMovers = new Mover();
        //bool bSelectRegion = false;
        //Button btnSelectRegion;
        //Button btnDeleteAllRegion;
        //Button btnDeleteRegion;

        //Mover CodeMovers = new Mover();
        bool bSelectRegion = false;
        Button btnSelectRegion => button8;
        Button btnCodeTest => button9;
        Button btnCalRealPointF => button1;
        Button btnWritePLCStage => button2;


        Button btnAddRegion => button4;
        Button btnDeleteAllRegion => button7;
        Button btnDeleteRegion => button5;

        Button btnCreateImageTemplate => button6;
        Button btnSaveInspectPara => button3;

        //Button btnOpenImage;
        //Button btnTestImage;
        //Button btnCreateImageTemplate;
        //Button btnSaveInspectPara;
        #endregion

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        protected InspectX3ParaClass xInspectX3
        {
            get { return InspectX3ParaClass.Instance; }
        }

        public frmTemplateX3()
        {
            InitializeComponent();
            this.Load += FrmTemplateX3_Load;
        }

        #region EVENT_HANDLERS
        private void FrmTemplateX3_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            //xBmpTemplate.Dispose();
            //xBmpTemplate = xRecipe.bmpOrg.Clone(xRect_Image, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            DS1.ReplaceDisplayImage(xRecipe.bmpprinttemplate);
            DS2.ReplaceDisplayImage(xRecipe.bmpDefectTemplate);

            propertyGrid1.SelectedObject = InspectX3ParaClass.Instance;

            this.Text = "设定模板界面";

            btnCalRealPointF.Visible = false;

            //btnSelectRegion = button8;
            //btnCodeTest = button9;
            //btnCalRealPointF = button1;
            //btnWritePLCStage = button2;

            btnSelectRegion.Click += BtnSelectCodeRegion_Click;
            btnCodeTest.Click += BtnCodeTest_Click;
            btnCalRealPointF.Click += BtnCalRealPointF_Click;
            btnWritePLCStage.Click += BtnWritePLCStage_Click;

            btnAddRegion.Click += BtnAddRegion_Click;
            btnDeleteAllRegion.Click += BtnDeleteAllRegion_Click;
            btnDeleteRegion.Click += BtnDeleteRegion_Click;
            btnCreateImageTemplate.Click += BtnCreateImageTemplate_Click;
            btnSaveInspectPara.Click += BtnSaveInspectPara_Click;

            cboCaliIndex.SelectedIndex = 0;

            _addCharRegion();
        }

        private void BtnWritePLCStage_Click(object sender, EventArgs e)
        {
            axWriteCalibDataToPlc_Gaara();
        }

        private void BtnCalRealPointF_Click(object sender, EventArgs e)
        {
            //这里不用了

            //////记录模板左上角实际点 用来阵列实际的位置
            ////PointF ptCenter = new PointF(xRecipe.xRectRegionPrint.X + xRecipe.xRectRegionPrint.Width / 2,
            ////    xRecipe.xRectRegionPrint.Y + xRecipe.xRectRegionPrint.Height / 2);
            //LineScanCalibrateClass cali = Traveller106.Universal.LineScanCalibrateClasses[0];
            //PointF ptworld = cali.ViewToWorld(LeftTopRectCenter);
            //xRecipe.xRealLeftX = ptworld.X;
            //xRecipe.xRealLeftY = ptworld.Y;

            ////textBox1.Text = PointFtoStringSimple(ptCenter);
            ////PointF ptworld = LineScanCalibrate.ViewToWorld(ptCenter);
            ////textBox2.Text = PointFtoStringSimple(ptworld);

            ////if (cboCaliIndex.SelectedIndex == 0)
            ////{
            ////    xRecipe.xRealLeftX = ptworld.X;
            ////    xRecipe.xRealLeftY = ptworld.Y;
            ////}
        }

        private void BtnCodeTest_Click(object sender, EventArgs e)
        {
            xRecipe.mvd2DReader.Run(xRecipe.bmpcodetemplate,
                new RectangleF(0, 0, xRecipe.bmpcodetemplate.Width, xRecipe.bmpcodetemplate.Height));
            if (xRecipe.mvd2DReader.DCodeInfo != null)
            {
                rtbCodeContent.Text = xRecipe.mvd2DReader.DCodeInfo.Content;
            }
            else
            {
                rtbCodeContent.Text = "";
            }
        }

        private void BtnSelectCodeRegion_Click(object sender, EventArgs e)
        {
            bSelectRegion = !bSelectRegion;
            btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
        }

        private void BtnAddRegion_Click(object sender, EventArgs e)
        {
            //bSelectRegion = !bSelectRegion;
            //btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));

            Rectangle rectangle = new Rectangle(0, 0, 50, 50);
            if (xMovers.Count == 0)
            {
                JzRectEAG RatioRectEAG = new JzRectEAG(Color.FromArgb(0, Color.Red), rectangle);
                RatioRectEAG.RelateNo = 2;
                xMovers.Add(RatioRectEAG);
            }
            else
            {
                bool bFound = false;
                int i = 0;
                while (i < xMovers.Count)
                {
                    GraphicalObject grobj_t0 = xMovers[i].Source;

                    if ((grobj_t0 as JzRectEAG).IsSelected)
                    {
                        bFound = true;
                        (grobj_t0 as JzRectEAG).IsSelected = false;
                        break;
                    }

                    i++;
                }

                if (!bFound)
                {
                    GraphicalObject grobj = xMovers[xMovers.Count - 1].Source;

                    JzRectEAG RatioRectEAG = new JzRectEAG(Color.FromArgb(0, Color.Red), (grobj as JzRectEAG).RealRectangleAround(0, 0));
                    RatioRectEAG.RelateNo = 2;
                    RatioRectEAG.SetOffset(new Point(20, 20));

                    xMovers.Add(RatioRectEAG);
                }
                else
                {
                    GraphicalObject grobj = xMovers[i].Source;

                    JzRectEAG RatioRectEAG = new JzRectEAG(Color.FromArgb(0, Color.Red), (grobj as JzRectEAG).RealRectangleAround(0, 0));
                    RatioRectEAG.RelateNo = 2;
                    RatioRectEAG.SetOffset(new Point(20, 20));
                    //RatioRectEAG.IsSelected = true;

                    xMovers.Add(RatioRectEAG);
                }
            }

            DS2.SetMover(xMovers);
            DS2.RefreshDisplayShape();
            DS2.MappingSelect();
        }
        private void _addCharRegion()
        {
            DS2.ClearMover();
            xMovers.Clear();

            int i = 0;
            while (i < xInspectX3.rectangles.Count)
            {
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), xInspectX3.rectangles[i]);
                _rect.RelateLevel = 2;
                _rect.RelateNo = i;
                _rect.RelatePosition = 0;
                xMovers.Add(_rect);

                i++;
            }

            DS2.SetMover(xMovers);
            DS2.RefreshDisplayShape();
            DS2.MappingSelect();
        }
        private void BtnDeleteRegion_Click(object sender, EventArgs e)
        {
            Mover xMoversTemp = new Mover();
            xMoversTemp.Clear();
            int i = 0;
            while (i < xMovers.Count)
            {
                GraphicalObject grobj = xMovers[i].Source;

                if (!(grobj as JzRectEAG).IsSelected)
                {
                    xMoversTemp.Add(grobj as JzRectEAG);
                }

                i++;
            }
            xMovers.Clear();
            DS2.ClearMover();
            xInspectX3.rectangles.Clear();
            i = 0;
            while (i < xMoversTemp.Count)
            {
                GraphicalObject grobj = xMoversTemp[i].Source;
                xMovers.Add(grobj as JzRectEAG);
                i++;
            }

            DS2.SetMover(xMovers);
            DS2.RefreshDisplayShape();
            DS2.MappingSelect();
        }

        private void BtnDeleteAllRegion_Click(object sender, EventArgs e)
        {
            xMovers.Clear();
            DS2.ClearMover();
            //xMoverIndex = 0;
            InspectX2Class.Instance.rectangles.Clear();
        }
        private void BtnCreateImageTemplate_Click(object sender, EventArgs e)
        {
            int iOK = 0;
            iOK = xRecipe.PrintTempTrain();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(iOK == 0 ? "创建成功" : "创建失败")}", false);
        }
        private void BtnSaveInspectPara_Click(object sender, EventArgs e)
        {
            xRecipe.bmpprintmask?.Dispose();
            xRecipe.bmpprintmask = new Bitmap(xRecipe.bmpDefectTemplate);
            Graphics graphics = Graphics.FromImage(xRecipe.bmpprintmask);
            graphics.Clear(Color.Black);

            xInspectX3.rectangles.Clear();

            int i = 0;
            while (i < xMovers.Count)
            {
                GraphicalObject grobj = xMovers[i].Source;
                RectangleF rectF = (grobj as JzRectEAG).GetRectF;
                graphics.FillRectangle(Brushes.White, rectF);
                xInspectX3.rectangles.Add(rectF);
                i++;
            }

            graphics.Dispose();
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            xRecipe.bmpprintmask = grayscale.Apply(xRecipe.bmpprintmask);

            DS3.ReplaceDisplayImage(xRecipe.bmpprintmask);
            xRecipe.SavePrintTemplate();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"保存成功", false);
        }
        #endregion

        void init_Display()
        {
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS1.CaptureAction += DS_CaptureAction;
            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS2.CaptureAction += DS_CaptureAction;
            DS3.Initial(100, 0.01f);
            DS3.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS3.CaptureAction += DS_CaptureAction;
        }
        void update_Display(bool eChangeToDefault = true)
        {
            DS1.Refresh();
            if (eChangeToDefault)
                DS1.DefaultView();
            DS2.Refresh();
            if (eChangeToDefault)
                DS2.DefaultView();
            DS3.Refresh();
            if (eChangeToDefault)
                DS3.DefaultView();
        }
        void DS_CaptureAction(RectangleF rectf)
        {
            if (!bSelectRegion)
                return;

            GaUtil.BoundRect(ref rectf, xRecipe.bmpprinttemplate.Size);

            if (rectf.Width > 1 && rectf.Height > 1)
            {
                DS1.ClearStaticMover();
                //DS2.ClearStaticMover();
                xMoversDs1.Clear();
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), rectf);
                _rect.RelateLevel = 1;
                //_rect.RelateNo = i;
                _rect.RelatePosition = 0;
                xMoversDs1.Add(_rect);


                if (radioButton1.Checked)       //template
                {
                    xRecipe.xRegionTrain = rectf;
                    xRecipe.bmpDefectTemplate?.Dispose();
                    xRecipe.bmpDefectTemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    xRecipe.SavePrintTemplateRegionTrain();
                    DS2.ReplaceDisplayImage(xRecipe.bmpDefectTemplate);
                }
                else if (radioButton2.Checked)  //code
                {
                    xRecipe.xRectCodeRegion = rectf;
                    xRecipe.bmpcodetemplate?.Dispose();
                    xRecipe.bmpcodetemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    xRecipe.SaveCodeTemplate();
                }
                else if (radioButton3.Checked)  //left
                {
#if (OLD_MESS_CODE)
                    xRecipe.xLineLeft = rectf;
                    xRecipe.SaveLinesRegion();

                    switch(xInspectX3.MFLType)
                    {
                        case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:

                            #region 找平行线

                            using (MvdPairLineClass pairLine = new MvdPairLineClass())
                            {
                                pairLine.bPositive = xInspectX3.bPositive0;
                                pairLine.bFindOrient = true;
                                CPairLineFindResult pair = pairLine.Run(xRecipe.bmpprinttemplate, rectf);
                                if (pair != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        pair.Line0.StartPoint.fX,
                                           pair.Line0.StartPoint.fY,
                                           pair.Line0.EndPoint.fX,
                                           pair.Line0.EndPoint.fY);

                                    g.DrawLine(new Pen(Color.Yellow, 3),
                                        pair.Line1.StartPoint.fX,
                                           pair.Line1.StartPoint.fY,
                                           pair.Line1.EndPoint.fX,
                                           pair.Line1.EndPoint.fY);

                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion

                            break;
                        default:
                            #region 找直线

                            using (MvdFindLineClass findline = new MvdFindLineClass())
                            {
                                findline.bPositive = xInspectX3.bPositive0;
                                findline.bFindOrient = true;
                                findline.bEdgePolarity = xInspectX3.bEdgePolarity0;
                                CMvdLineSegmentF lineSegmentF = findline.Run(xRecipe.bmpprinttemplate, rectf);
                                if (lineSegmentF != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        lineSegmentF.StartPoint.fX,
                                        lineSegmentF.StartPoint.fY,
                                        lineSegmentF.EndPoint.fX,
                                        lineSegmentF.EndPoint.fY);
                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion
                            break;
                    }
#endif
                    //@LETIAN: 2025-08-29 以後應該統一由 AoiModel 處理找線
                    tryRunFindLineSegment(EdgeBorder.Left, xRecipe.bmpprinttemplate, rectf);

                }
                else if (radioButton4.Checked)  //top
                {
#if (OLD_MESS_CODE)
                    xRecipe.xLineTop = rectf;
                    xRecipe.SaveLinesRegion();
                    switch (xInspectX3.MFLType)
                    {
                        case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:
                            #region 找平行线
                            using (MvdPairLineClass pairLine = new MvdPairLineClass())
                            {
                                pairLine.bPositive = xInspectX3.bPositive1;
                                pairLine.bFindOrient = false;
                                var pair = pairLine.Run(xRecipe.bmpprinttemplate, rectf);
                                if (pair != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        pair.Line0.StartPoint.fX,
                                           pair.Line0.StartPoint.fY,
                                           pair.Line0.EndPoint.fX,
                                           pair.Line0.EndPoint.fY);

                                    g.DrawLine(new Pen(Color.Yellow, 3),
                                        pair.Line1.StartPoint.fX,
                                           pair.Line1.StartPoint.fY,
                                           pair.Line1.EndPoint.fX,
                                           pair.Line1.EndPoint.fY);

                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion
                            break;
                        default:
                            #region 找直线

                            using (MvdFindLineClass findline = new MvdFindLineClass())
                            {
                                findline.bPositive = xInspectX3.bPositive1;
                                findline.bFindOrient = false;
                                findline.bEdgePolarity = xInspectX3.bEdgePolarity1;
                                CMvdLineSegmentF lineSegmentF = findline.Run(xRecipe.bmpprinttemplate, rectf);
                                if (lineSegmentF != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        lineSegmentF.StartPoint.fX,
                                        lineSegmentF.StartPoint.fY,
                                        lineSegmentF.EndPoint.fX,
                                        lineSegmentF.EndPoint.fY);
                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion
                            break;
                    }
#endif
                    //@LETIAN: 2025-08-29 以後應該統一由 AoiModel 處理找線
                    tryRunFindLineSegment(EdgeBorder.Top, xRecipe.bmpprinttemplate, rectf);
                }
                else if (radioButton5.Checked)  //right
                {
#if (OLD_MESS_CODE)
                    xRecipe.xLineRight = rectf;
                    xRecipe.SaveLinesRegion();
                    switch (xInspectX3.MFLType)
                    {
                        case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:

                            #region 找平行线

                            using (MvdPairLineClass pairLine = new MvdPairLineClass())
                            {
                                pairLine.bPositive = xInspectX3.bPositive2;
                                pairLine.bFindOrient = true;
                                CPairLineFindResult pair = pairLine.Run(xRecipe.bmpprinttemplate, rectf);
                                if (pair != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        pair.Line0.StartPoint.fX,
                                           pair.Line0.StartPoint.fY,
                                           pair.Line0.EndPoint.fX,
                                           pair.Line0.EndPoint.fY);

                                    g.DrawLine(new Pen(Color.Yellow, 3),
                                        pair.Line1.StartPoint.fX,
                                           pair.Line1.StartPoint.fY,
                                           pair.Line1.EndPoint.fX,
                                           pair.Line1.EndPoint.fY);

                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion

                            break;
                        default:
                            #region 找直线

                            using (MvdFindLineClass findline = new MvdFindLineClass())
                            {
                                findline.bPositive = xInspectX3.bPositive2;
                                findline.bFindOrient = true;
                                findline.bEdgePolarity = xInspectX3.bEdgePolarity2;
                                CMvdLineSegmentF lineSegmentF = findline.Run(xRecipe.bmpprinttemplate, rectf);
                                if (lineSegmentF != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        lineSegmentF.StartPoint.fX,
                                        lineSegmentF.StartPoint.fY,
                                        lineSegmentF.EndPoint.fX,
                                        lineSegmentF.EndPoint.fY);
                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }


                            #endregion
                            break;
                    }
#endif
                    //@LETIAN: 2025-08-29 以後應該統一由 AoiModel 處理找線
                    tryRunFindLineSegment(EdgeBorder.Right, xRecipe.bmpprinttemplate, rectf);
                }
                else if (radioButton6.Checked)//bottom
                {
#if (OLD_MESS_CODE)
                    xRecipe.xLineBottom = rectf;
                    xRecipe.SaveLinesRegion();
                    switch (xInspectX3.MFLType)
                    {
                        case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:

                            #region 找平行线

                            using (MvdPairLineClass pairLine = new MvdPairLineClass())
                            {
                                pairLine.bPositive = xInspectX3.bPositive3;
                                pairLine.bFindOrient = false;
                                CPairLineFindResult pair = pairLine.Run(xRecipe.bmpprinttemplate, rectf);
                                if (pair != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        pair.Line0.StartPoint.fX,
                                           pair.Line0.StartPoint.fY,
                                           pair.Line0.EndPoint.fX,
                                           pair.Line0.EndPoint.fY);

                                    g.DrawLine(new Pen(Color.Yellow, 3),
                                        pair.Line1.StartPoint.fX,
                                           pair.Line1.StartPoint.fY,
                                           pair.Line1.EndPoint.fX,
                                           pair.Line1.EndPoint.fY);

                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion

                            break;
                        default:
                            #region 找直线

                            using (MvdFindLineClass findline = new MvdFindLineClass())
                            {
                                findline.bPositive = xInspectX3.bPositive3;
                                findline.bFindOrient = false;
                                findline.bEdgePolarity = xInspectX3.bEdgePolarity3;
                                CMvdLineSegmentF lineSegmentF = findline.Run(xRecipe.bmpprinttemplate, rectf);
                                if (lineSegmentF != null)
                                {
                                    Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                                    Graphics g = Graphics.FromImage(bmpx);
                                    g.DrawLine(new Pen(Color.Lime, 3),
                                        lineSegmentF.StartPoint.fX,
                                        lineSegmentF.StartPoint.fY,
                                        lineSegmentF.EndPoint.fX,
                                        lineSegmentF.EndPoint.fY);
                                    g.Dispose();
                                    DS1.ReplaceDisplayImage(bmpx);
                                    bmpx.Dispose();
                                }
                            }

                            #endregion
                            break;
                    }
#endif
                    //@LETIAN: 2025-08-29 以後應該統一由 AoiModel 處理找線
                    tryRunFindLineSegment(EdgeBorder.Bottom, xRecipe.bmpprinttemplate, rectf);
                }

                DS1.SetStaticMover(xMoversDs1);
                DS1.RefreshDisplayShape();
                DS1.MappingSelect();

                update_Display(false);

                //Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                //Graphics g = Graphics.FromImage(bmpx);
                //xRecipe.xRectCodeRegion = rectf;
                //xRecipe.bmpcodetemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                //xRecipe.SaveCodeTemplate();

                //g.DrawRectangles(new Pen(Color.Yellow, 3), new RectangleF[] { rectf });
                //g.Dispose();
                //DS1.ReplaceDisplayImage(bmpx);
                //bmpx.Dispose();
            }

            bSelectRegion = false;
            btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
        }

        #region AOI_FUNCTIONS
        void tryRunFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF boxRect)
        {
            updateBorderBoxToRecipe(eBorder, boxRect, save: true);

            bool bPositive, bEdgePolarity, bFindOrient;

            #region 方向與極性
            switch (eBorder)
            {
                case EdgeBorder.Left:
                    bPositive = xInspectX3.bPositive0;
                    bEdgePolarity = xInspectX3.bEdgePolarity0;
                    bFindOrient = true;
                    break;
                case EdgeBorder.Right:
                    bPositive = xInspectX3.bPositive1;
                    bEdgePolarity = xInspectX3.bEdgePolarity1;
                    bFindOrient = false;
                    break;
                case EdgeBorder.Top:
                    bPositive = xInspectX3.bPositive2;
                    bEdgePolarity = xInspectX3.bEdgePolarity2;
                    bFindOrient = true;
                    break;
                case EdgeBorder.Bottom:
                    bPositive = xInspectX3.bPositive3;
                    bEdgePolarity = xInspectX3.bEdgePolarity3;
                    bFindOrient = false;
                    break;
                default:
                    return;
            }
            #endregion

            var mflType = xInspectX3.MFLType;

            // 目前黑色背景 暫時強制用 FindLineType_v1
            mflType = Eazy_Project_III.MeasureFindLineType.FindLineType_v1;

            switch (mflType)
            {
                case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:
                    #region 找平行线
                    using (MvdPairLineClass pairLine = new MvdPairLineClass())
                    {
                        pairLine.bPositive = bPositive;
                        pairLine.bFindOrient = bFindOrient;
                        CPairLineFindResult pair = pairLine.Run(bmpSrc, boxRect, (int)eBorder);
                        if (pair != null)
                            drawMvdLinesToDisp(bmpSrc, pair.Line0, pair.Line1);
                    }
                    #endregion
                    break;
                default:
                    #region 找直线
                    using (MvdFindLineClass finder = new MvdFindLineClass())
                    {
                        finder.bPositive = bPositive;
                        finder.bFindOrient = bFindOrient;
                        finder.bEdgePolarity = bEdgePolarity;
                        var mvdLine = finder.Run(bmpSrc, boxRect, (int)eBorder);
                        drawMvdLinesToDisp(bmpSrc, mvdLine);
                    }
                    #endregion
                    break;
            }
        }
        void updateBorderBoxToRecipe(EdgeBorder eBorder, RectangleF boxRect, bool save)
        {
            switch (eBorder)
            {
                case EdgeBorder.Left:
                    xRecipe.xLineLeft = boxRect;
                    break;
                case EdgeBorder.Top:
                    xRecipe.xLineTop = boxRect;
                    break;
                case EdgeBorder.Right:
                    xRecipe.xLineRight = boxRect;
                    break;
                case EdgeBorder.Bottom:
                    xRecipe.xLineBottom = boxRect;
                    break;
            }

            if (save)
                xRecipe.SaveLinesRegion();
        }
        void drawMvdLinesToDisp(Bitmap bmpSrc, params CMvdLineSegmentF[] mvdLines)
        {
            using (Bitmap bmpx = new Bitmap(bmpSrc))
            {
                using (Graphics gx = Graphics.FromImage(bmpx))
                {
                    foreach (var mvdLine in mvdLines)
                        if (mvdLine != null)
                            gx.DrawLine(new Pen(Color.Lime, 3),
                                mvdLine.StartPoint.fX,
                                mvdLine.StartPoint.fY,
                                mvdLine.EndPoint.fX,
                                mvdLine.EndPoint.fY);
                }
                DS1.ReplaceDisplayImage(bmpx);
            }
        }
        #endregion

        #region HELPER_FUNCTIONS
        public string PointFtoStringSimple(PointF ptf)
        {
            string Str = "";

            Str += ptf.X.ToString() + ",";
            Str += ptf.Y.ToString();

            return Str;
        }
        public PointF StringtoPointF(string ptfStr)
        {
            string[] strs = ptfStr.Split(',');
            PointF rectF = new PointF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);

            return rectF;


        }
#if (false)
        void BoundRect(ref Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        Bitmap bmpCopy(Bitmap eInput)
        {
            Bitmap bmp = eInput.Clone(new RectangleF(0, 0, eInput.Width, eInput.Height), eInput.PixelFormat);
            return bmp;
        }
        public Bitmap ByteArrayToBitmap(byte[] byteArray, int width, int height)
        {
            // 创建 Bitmap 对象
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // 设置调色板为灰度
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
            // 锁定 Bitmap 数据
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            // 将字节数组复制到 Bitmap 数据中
            System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bitmapData.Scan0, byteArray.Length);
            // 解锁 Bitmap 数据
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
#endif
        #endregion

        #region PLC_COORDS_WRITING_FUNCTOINS
        Control txtCamCoord => textBox1;
        Control txtWorldCoord => textBox2;
        Control lblCompletedInfo => label5;

        void axWriteCalibDataToPlc_Gaara()
        {
            ComboBox cboCaliIndex = this.cboCaliIndex;

            //int i = 0;
            //while (i < 4)
            //{
            //    LineScanCalibrateClass cali = Traveller106.Universal.LineScanCalibrateClasses[i];
            //    PointF ptworld = cali.ViewToWorld(ptCenter);
            //    switch (i)
            //    {
            //        case 0:
            //            MACHINE.PLCIO.SetStage1(0, ptworld, new PointF());
            //            break;
            //        case 1:
            //            MACHINE.PLCIO.SetStage1(1, new PointF(), ptworld);
            //            break;
            //        case 2:
            //            MACHINE.PLCIO.SetStage2(0, ptworld, new PointF());
            //            break;
            //        case 3:
            //            MACHINE.PLCIO.SetStage2(1, new PointF(), ptworld);
            //            break;
            //    }
            //    i++;
            //}
            //label5.Text = $"{DateTime.Now.ToString()}操作完成";

            LineScanCalibrateClass cali = Traveller106.Universal.LineScanCalibrateClasses[cboCaliIndex.SelectedIndex];
            PointF ptworld = cali.ViewToWorld(LeftTopRectCenter);
            //textBox1.Text = PointFtoStringSimple(LeftTopRectCenter);
            //textBox2.Text = PointFtoStringSimple(ptworld);
            updatePlcWritingStatus(null, Color.Black);

            switch (cboCaliIndex.SelectedIndex)
            {
                // 載台1 吸嘴排1
                case 0:
                    MACHINE.PLCIO.SetStage1(0, ptworld, new PointF());
                    updatePlcCoordsToGui(LeftTopRectCenter, ptworld);
                    break;

                // 載台1 吸嘴排2
                case 1:
                    LineScanCalibrateClass c0 = Traveller106.Universal.LineScanCalibrateClasses[0];
                    PointF ptOffset = new PointF(cali.ptsworld[0].X - c0.ptsworld[0].X,
                                                 cali.ptsworld[1].Y - c0.ptsworld[1].Y);
                    PointF ptworld1 = c0.ViewToWorld(LeftTopRectCenter);
                    PointF ptworld2 = new PointF(ptworld1.X + ptOffset.X, ptworld1.Y + ptOffset.Y);
                    textBox2.Text = PointFtoStringSimple(ptworld2);

                    MACHINE.PLCIO.SetStage1(1, new PointF(), ptworld2);
                    updatePlcCoordsToGui(LeftTopRectCenter, ptworld2);
                    break;

                // 載台2 吸嘴排1
                case 2:
                    MACHINE.PLCIO.SetStage2(0, ptworld, new PointF());
                    updatePlcCoordsToGui(LeftTopRectCenter, ptworld);
                    break;

                // 載台2 吸嘴排2
                case 3:
                    c0 = Traveller106.Universal.LineScanCalibrateClasses[2];
                    ptOffset = new PointF(cali.ptsworld[0].X - c0.ptsworld[0].X,
                                                 cali.ptsworld[1].Y - c0.ptsworld[1].Y);
                    ptworld1 = c0.ViewToWorld(LeftTopRectCenter);
                    ptworld2 = new PointF(ptworld1.X + ptOffset.X, ptworld1.Y + ptOffset.Y);
                    textBox2.Text = PointFtoStringSimple(ptworld2);

                    MACHINE.PLCIO.SetStage2(1, new PointF(), ptworld2);
                    updatePlcCoordsToGui(LeftTopRectCenter, ptworld2);
                    break;
            }

            int index = cboCaliIndex.SelectedIndex;
            CarrierEnum carrierID = (CarrierEnum)(index / 2);
            SuckerRowEnum suckerRowID = (SuckerRowEnum)(index % 2);
            updatePlcWritingStatus($"{DateTime.Now.ToString()} 載台 {carrierID} 吸嘴 {suckerRowID} 寫入PLC操作完成!", Color.Lime);
        }

        void updatePlcCoordsToGui(PointF camPt, PointF suckerWorldPt)
        {
            txtCamCoord.Text = $"({camPt.X:0.0}, {camPt.Y:0.0}) px";
            txtWorldCoord.Text = $"({suckerWorldPt.X:0.000}, {suckerWorldPt.Y:0.000}) mm";

        }
        void updatePlcWritingStatus(string status, Color color)
        {
            if (status == null)
            {
                lblCompletedInfo.Visible = false;
                return;
            }
            lblCompletedInfo.Text = status;
            lblCompletedInfo.ForeColor = color;
            lblCompletedInfo.BackColor = Color.Black;
            lblCompletedInfo.Visible = true;
        }
        #endregion
    }
}
