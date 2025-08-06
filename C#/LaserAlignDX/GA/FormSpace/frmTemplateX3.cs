using Common.RecipeSpace;
using JetEazy;
using JzDisplay;
using LaserAlignDX.OPSpace.RecipeSpace;
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

namespace LaserAlignDX.FormSpace
{
    public partial class frmTemplateX3 : Form
    {
        CMvdImage xMvdTestImage = new CMvdImage();
        //LineScanCalibrateClass LineScanCalibrate
        //{
        //    get { return Traveller106.Universal.LineScanCalibrateClasses[cboCaliIndex.SelectedIndex]; }
        //}

        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }

        //int xMoverIndex = 0;
        //Mover xMovers = new Mover();
        //bool bSelectRegion = false;
        //Button btnSelectRegion;
        //Button btnDeleteAllRegion;
        //Button btnDeleteRegion;

        Mover CodeMovers = new Mover();
        bool bSelectCodeRegion = false;
        Button btnSelectCodeRegion;
        Button btnCodeTest;
        Button btnCalRealPointF;
        Button btnWritePLCStage;

        //Button btnOpenImage;
        //Button btnTestImage;
        //Button btnCreateImageTemplate;
        //Button btnSaveInspectPara;

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
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

            propertyGrid1.SelectedObject = InspectX3ParaClass.Instance;

            this.Text = "设定模板界面";

            //btnOpenImage = button6;
            //btnTestImage = button1;
            //btnCreateImageTemplate = button2;
            //btnSaveInspectPara = button3;
            //btnSelectRegion = button4;
            //btnDeleteAllRegion = button5;
            //btnDeleteRegion = button7;
            btnSelectCodeRegion = button8;
            btnCodeTest = button9;
            btnCalRealPointF = button1;
            btnWritePLCStage = button2;

            //btnOpenImage.Click += BtnOpenImage_Click;
            //btnTestImage.Click += BtnTestImage_Click;
            //btnCreateImageTemplate.Click += BtnCreateImageTemplate_Click;
            //btnSaveInspectPara.Click += BtnSaveInspectPara_Click;
            //btnSelectRegion.Click += BtnSelectRegion_Click;
            //btnDeleteAllRegion.Click += BtnDeleteAllRegion_Click;
            //btnDeleteRegion.Click += BtnDeleteRegion_Click;
            btnSelectCodeRegion.Click += BtnSelectCodeRegion_Click;
            btnCodeTest.Click += BtnCodeTest_Click;
            btnCalRealPointF.Click += BtnCalRealPointF_Click;
            btnWritePLCStage.Click += BtnWritePLCStage_Click;

            cboCaliIndex.SelectedIndex = 0;

            //_addCharRegion();
        }

        private void BtnWritePLCStage_Click(object sender, EventArgs e)
        {
            PointF ptCenter = new PointF(xRecipe.xRectRegionPrint.X + xRecipe.xRectRegionPrint.Width / 2,
                xRecipe.xRectRegionPrint.Y + xRecipe.xRectRegionPrint.Height / 2);
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
            textBox1.Text = PointFtoStringSimple(ptCenter);
            PointF ptworld = cali.ViewToWorld(ptCenter);
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

            //记录模板左上角实际点 用来阵列实际的位置
            PointF ptCenter = new PointF(xRecipe.xRectRegionPrint.X + xRecipe.xRectRegionPrint.Width / 2,
                xRecipe.xRectRegionPrint.Y + xRecipe.xRectRegionPrint.Height / 2);
            LineScanCalibrateClass cali = Traveller106.Universal.LineScanCalibrateClasses[0];
            PointF ptworld = cali.ViewToWorld(ptCenter);
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
            bSelectCodeRegion = !bSelectCodeRegion;
            btnSelectCodeRegion.BackColor = (bSelectCodeRegion ? Color.Red : Color.FromArgb(192, 255, 192));
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
            if (!bSelectCodeRegion)
                return;
            BoundRect(ref rectf, xRecipe.bmpprinttemplate.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                Bitmap bmpx = new Bitmap(xRecipe.bmpprinttemplate);
                Graphics g = Graphics.FromImage(bmpx);
                xRecipe.xRectCodeRegion = rectf;
                xRecipe.bmpcodetemplate = xRecipe.bmpprinttemplate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                xRecipe.SaveCodeTemplate();

                g.DrawRectangles(new Pen(Color.Yellow, 3), new RectangleF[] { rectf });
                g.Dispose();
                DS1.ReplaceDisplayImage(bmpx);
                bmpx.Dispose();
            }
            bSelectCodeRegion = false;
            btnSelectCodeRegion.BackColor = (bSelectCodeRegion ? Color.Red : Color.FromArgb(192, 255, 192));
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
