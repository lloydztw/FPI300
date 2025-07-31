using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionDesigner;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmTemplateX2 : Form
    {
        //RectangleF xRect_Image = new RectangleF();
        //Bitmap xBmpTestTemplate = new Bitmap(1, 1);

        CMvdImage xMvdTestImage = new CMvdImage();

        int xMoverIndex = 0;
        Mover xMovers = new Mover();
        bool bSelectRegion = false;
        Button btnSelectRegion;
        Button btnDeleteAllRegion;
        Button btnDeleteRegion;

        Mover CodeMovers = new Mover();
        bool bSelectCodeRegion = false;
        Button btnSelectCodeRegion;
        Button btnCodeTest;

        Button btnOpenImage;
        Button btnTestImage;
        Button btnCreateImageTemplate;
        Button btnSaveInspectPara;

        protected RecipeMainX2Class xRecipe
        {
            get { return RecipeMainX2Class.Instance; }
        }

        //public frmTemplateX2(RectangleF eRect_Image)
        //{
        //    //this.xRect_Image = eRect_Image;
        //    InitializeComponent();
        //    this.Load += FrmTemplateX2_Load;
        //}
        public frmTemplateX2()
        {
            //this.xRect_Image = eRect_Image;
            InitializeComponent();
            this.Load += FrmTemplateX2_Load;
        }

        private void FrmTemplateX2_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            //xBmpTemplate.Dispose();
            //xBmpTemplate = xRecipe.bmpOrg.Clone(xRect_Image, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            DS1.ReplaceDisplayImage(xRecipe.bmpprinttemplate);

            propertyGrid1.SelectedObject = InspectX2Class.Instance;

            this.Text = "设定模板界面";

            btnOpenImage = button6;
            btnTestImage = button1;
            btnCreateImageTemplate = button2;
            btnSaveInspectPara = button3;
            btnSelectRegion = button4;
            btnDeleteAllRegion = button5;
            btnDeleteRegion = button7;
            btnSelectCodeRegion = button8;
            btnCodeTest = button9;

            btnOpenImage.Click += BtnOpenImage_Click;
            btnTestImage.Click += BtnTestImage_Click;
            btnCreateImageTemplate.Click += BtnCreateImageTemplate_Click;
            btnSaveInspectPara.Click += BtnSaveInspectPara_Click;
            btnSelectRegion.Click += BtnSelectRegion_Click;
            btnDeleteAllRegion.Click += BtnDeleteAllRegion_Click;
            btnDeleteRegion.Click += BtnDeleteRegion_Click;
            btnSelectCodeRegion.Click += BtnSelectCodeRegion_Click;
            btnCodeTest.Click += BtnCodeTest_Click;

            _addCharRegion();
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
            DS1.ClearMover();
            InspectX2Class.Instance.rectangles.Clear();
            i = 0;
            while (i < xMoversTemp.Count)
            {
                GraphicalObject grobj = xMoversTemp[i].Source;
                xMovers.Add(grobj as JzRectEAG);
                i++;
            }

            DS1.SetMover(xMovers);
            DS1.RefreshDisplayShape();
            DS1.MappingSelect();
        }

        private void _addCharRegion()
        {
            DS1.ClearMover();
            xMovers.Clear();

            int i = 0;
            while (i < InspectX2Class.Instance.rectangles.Count)
            {
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), InspectX2Class.Instance.rectangles[i]);
                _rect.RelateLevel = 2;
                _rect.RelateNo = i;
                _rect.RelatePosition = 0;
                xMovers.Add(_rect);

                i++;
            }

            DS1.SetMover(xMovers);
            DS1.RefreshDisplayShape();
            DS1.MappingSelect();
        }

        private void BtnDeleteAllRegion_Click(object sender, EventArgs e)
        {
            xMovers.Clear();
            DS1.ClearMover();
            xMoverIndex = 0;
            InspectX2Class.Instance.rectangles.Clear();
        }

        private void BtnSelectRegion_Click(object sender, EventArgs e)
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

            DS1.SetMover(xMovers);
            DS1.RefreshDisplayShape();
            DS1.MappingSelect();
        }

        private void BtnSaveInspectPara_Click(object sender, EventArgs e)
        {
            xRecipe.bmpprintmask.Dispose();
            xRecipe.bmpprintmask = new Bitmap(xRecipe.bmpprinttemplate);
            Graphics graphics = Graphics.FromImage(xRecipe.bmpprintmask);
            graphics.Clear(Color.Black);

            InspectX2Class.Instance.rectangles.Clear();

            int i = 0;
            while (i < xMovers.Count)
            {
                GraphicalObject grobj = xMovers[i].Source;
                RectangleF rectF = (grobj as JzRectEAG).GetRectF;
                graphics.FillRectangle(Brushes.White, rectF);
                InspectX2Class.Instance.rectangles.Add(rectF);
                i++;
            }

            graphics.Dispose();
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            xRecipe.bmpprintmask = grayscale.Apply(xRecipe.bmpprintmask);

            DS3.ReplaceDisplayImage(xRecipe.bmpprintmask);
            xRecipe.SavePrintTemplate();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"保存成功", false);
        }

        private void BtnCreateImageTemplate_Click(object sender, EventArgs e)
        {
            int iOK = 0;
            iOK = xRecipe.PrintTempTrain();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(iOK == 0 ? "创建成功" : "创建失败")}", false);
        }

        private void BtnTestImage_Click(object sender, EventArgs e)
        {

        }

        private void BtnOpenImage_Click(object sender, EventArgs e)
        {
            //string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            //if (!string.IsNullOrEmpty(_filename))
            //{
            //    xMvdTestImage.InitImage(_filename);
            //    MVD_IMAGE_DATA_INFO _MvdImage = xMvdTestImage.GetImageData();
            //    MVD_DATA_CHANNEL_INFO ch0 = _MvdImage.stDataChannel[0];
            //    Bitmap _bmpFromMVD = ByteArrayToBitmap(ch0.arrDataBytes, (int)xMvdTestImage.Width, (int)xMvdTestImage.Height);
            //    DS2.ReplaceDisplayImage(_bmpFromMVD);

            //    _bmpFromMVD.Dispose();
            //}
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
