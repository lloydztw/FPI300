using Common.RecipeSpace;
using JetEazy;
using JzDisplay;
using LaserAlignDX.GA.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.JxRecipesTool;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using VisionDesigner;
using VsCommon.ControlSpace.MachineSpace;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmTemplateX3 : Form
    {
        CMvdImage xMvdTestImage = new CMvdImage();
        //LineScanCalibrateClass LineScanCalibrate
        //{
        //    get { return Traveller106.Universal.LineScanCalibrateClasses[cboCaliIndex.SelectedIndex]; }
        //}
        Mover xMovers = new Mover();

        Mover xMoversDs1 = new Mover();

        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }

        RectangleF LeftTopRect
        {
            get { return xRecipe.xRegionTrain; }
        }
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

        //int xMoverIndex = 0;
        //Mover xMovers = new Mover();
        //bool bSelectRegion = false;
        //Button btnSelectRegion;
        //Button btnDeleteAllRegion;
        //Button btnDeleteRegion;

        Mover CodeMovers = new Mover();
        bool bSelectRegion = false;
        Button btnSelectRegion;
        Button btnCodeTest;
        Button btnCalRealPointF;
        Button btnWritePLCStage;


        Button btnAddRegion => button4;
        Button btnDeleteAllRegion => button7;
        Button btnDeleteRegion => button5;

        Button btnCreateImageTemplate => button6;
        Button btnSaveInspectPara => button3;

        //Button btnOpenImage;
        //Button btnTestImage;
        //Button btnCreateImageTemplate;
        //Button btnSaveInspectPara;

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

            btnSelectRegion = button8;
            btnCodeTest = button9;
            btnCalRealPointF = button1;
            btnWritePLCStage = button2;

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
            textBox1.Text = PointFtoStringSimple(LeftTopRectCenter);
            PointF ptworld = cali.ViewToWorld(LeftTopRectCenter);
            textBox2.Text = PointFtoStringSimple(ptworld);

            switch (cboCaliIndex.SelectedIndex)
            {
                case 0:
                    MACHINE.PLCIO.SetStage1(0, ptworld, new PointF());
                    break;
                case 1:
                    MACHINE.PLCIO.SetStage1(1, new PointF(), ptworld);
                    break;
                case 2:
                    MACHINE.PLCIO.SetStage2(0, ptworld, new PointF());
                    break;
                case 3:
                    MACHINE.PLCIO.SetStage2(1, new PointF(), ptworld);
                    break;
            }

            label5.Text = $"{DateTime.Now.ToString()}{cboCaliIndex.Text}操作完成";
        }

        private void BtnCalRealPointF_Click(object sender, EventArgs e)
        {

            ////记录模板左上角实际点 用来阵列实际的位置
            //PointF ptCenter = new PointF(xRecipe.xRectRegionPrint.X + xRecipe.xRectRegionPrint.Width / 2,
            //    xRecipe.xRectRegionPrint.Y + xRecipe.xRectRegionPrint.Height / 2);
            LineScanCalibrateClass cali = Traveller106.Universal.LineScanCalibrateClasses[0];
            PointF ptworld = cali.ViewToWorld(LeftTopRectCenter);
            xRecipe.xRealLeftX = ptworld.X;
            xRecipe.xRealLeftY = ptworld.Y;

            //textBox1.Text = PointFtoStringSimple(ptCenter);
            //PointF ptworld = LineScanCalibrate.ViewToWorld(ptCenter);
            //textBox2.Text = PointFtoStringSimple(ptworld);

            //if (cboCaliIndex.SelectedIndex == 0)
            //{
            //    xRecipe.xRealLeftX = ptworld.X;
            //    xRecipe.xRealLeftY = ptworld.Y;
            //}
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
        private void DS_CaptureAction(RectangleF rectf)
        {
            if (!bSelectRegion)
                return;
            BoundRect(ref rectf, xRecipe.bmpprinttemplate.Size);
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


                if (radioButton1.Checked)//template
                {
                    xRecipe.xRegionTrain = rectf;
                    xRecipe.bmpDefectTemplate?.Dispose();
                    xRecipe.bmpDefectTemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    xRecipe.SavePrintTemplateRegionTrain();
                    DS2.ReplaceDisplayImage(xRecipe.bmpDefectTemplate);
                }
                else if (radioButton2.Checked)//code
                {
                    xRecipe.xRectCodeRegion = rectf;
                    xRecipe.bmpcodetemplate?.Dispose();
                    xRecipe.bmpcodetemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    xRecipe.SaveCodeTemplate();
                }
                else if (radioButton3.Checked)//left
                {
                    xRecipe.xLineLeft = rectf;
                    xRecipe.SaveLinesRegion();

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
                }
                else if (radioButton4.Checked)//top
                {
                    xRecipe.xLineTop = rectf;
                    xRecipe.SaveLinesRegion();


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

                }
                else if (radioButton5.Checked)//right
                {
                    xRecipe.xLineRight = rectf;
                    xRecipe.SaveLinesRegion();

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

                }
                else if (radioButton6.Checked)//bottom
                {
                    xRecipe.xLineBottom = rectf;
                    xRecipe.SaveLinesRegion();

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

        #region TOOLS

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

        #endregion
    }
}
