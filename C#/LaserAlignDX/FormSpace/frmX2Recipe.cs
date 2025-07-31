using AForge.Imaging.Filters;
using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using JzDisplay;
using LaserAlignDX.BasicSpace;
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
using TravellerMINIX6.OPSpace;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmX2Recipe : Form
    {
        protected IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        protected RecipeMainX2Class xRecipe
        {
            get { return RecipeMainX2Class.Instance; }
        }
        //Mover xMover = new Mover();
        Mover xMovers = new Mover();
        bool bSelectRegion = false;
        //RectangleF xRectGoodImage = new RectangleF(0, 0, 1, 1);
        RegionName xRegionNameCurrent // = RegionName.BASE0;
        {
            get
            {
                RegionName regionName = RegionName.BASE0;
                if (rdoBase0.Checked)
                    regionName = RegionName.BASE0;
                else if (rdoBase1.Checked)
                    regionName = RegionName.BASE1;
                else if (rdoTemplate.Checked)
                    regionName = RegionName.GOODIMAGE;
                return regionName;
            }
        }
        Timer xTimer = null;
        Button btnOK;
        Button btnCancel;

        Button btnLoadImage;
        Button btnRegionBase;
        Button btnRegionBaseForm;
        Button btnGetRealImage;

        Button btnCreateRegions;
        //Button btnSelectTemplate;
        Button btnSelectTemplateForm;

        public frmX2Recipe()
        {
            InitializeComponent();
            this.Load += FrmX2Recipe_Load;
            this.FormClosed += FrmX2Recipe_FormClosed;
            this.SizeChanged += FrmX2Recipe_SizeChanged;
        }

        private void FrmX2Recipe_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        private void FrmX2Recipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void FrmX2Recipe_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            btnOK = button1;
            btnCancel = button2;

            btnLoadImage = button7;
            btnRegionBase = button3;
            btnRegionBaseForm = button4;
            btnCreateRegions = button9;
            btnGetRealImage = button5;
            btnSelectTemplateForm = button6;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            btnLoadImage.Click += BtnLoadImage_Click;
            btnRegionBase.Click += BtnRegionBase_Click;
            btnRegionBaseForm.Click += BtnRegionBaseForm_Click;
            btnCreateRegions.Click += BtnCreateRegions_Click;
            //btnSelectTemplate.Click += BtnSelectTemplate_Click;
            btnSelectTemplateForm.Click += BtnSelectTemplateForm_Click;
            btnGetRealImage.Click += BtnGetRealImage_Click;

            xTimer = new Timer();
            xTimer.Interval = 50;
            xTimer.Enabled = true;
            xTimer.Tick += XTimer_Tick;

            DS.ReplaceDisplayImage(xRecipe.bmpOrg);

            propertyGrid1.SelectedObject = RecipeParaGridClass.Instance;

            this.Text = "参数设定窗口";
            this.FormBorderStyle = FormBorderStyle.None;

            LanguageExClass.Instance.EnumControls(this);

#if OPT_LETIAN_AUTO_LAYOUT
            // To fit into my screen for debug.
#if DEBUG
            this.FormBorderStyle = FormBorderStyle.Sizable;
#endif
            this.WindowState = FormWindowState.Maximized;
#endif
        }

        private void BtnGetRealImage_Click(object sender, EventArgs e)
        {
            if (IScanCam.GetFreeImageBitmap() != null)
            {
                xRecipe.bmpOrg.Dispose();
                xRecipe.bmpOrg = IScanCam.GetFreeImageBitmap().ToBitmap();
                //using (FreeImageBitmap image = new FreeImageBitmap(IScanCam.GetFreeImageBitmap()))
                //{
                //    xRecipe.bmpOrg.Dispose();
                //    xRecipe.bmpOrg = image.ToBitmap();
                //}
                ////xRecipe.bmpOrg = new Bitmap(IScanCam.GetFreeImageBitmap().ToBitmap());
                DS.ReplaceDisplayImage(xRecipe.bmpOrg);
            }
        }

        private void BtnSelectTemplateForm_Click(object sender, EventArgs e)
        {
            showTemplateX2DialogWindow();
        }
        frmTemplateX2  FrmTemplateX2 = null;
        void showTemplateX2DialogWindow()
        {
            FrmTemplateX2 = new frmTemplateX2();
            FrmTemplateX2.ShowDialog();
            FrmTemplateX2.Dispose();
            FrmTemplateX2 = null;
        }

        //private void BtnSelectTemplate_Click(object sender, EventArgs e)
        //{
        //    bSelectRegion = !bSelectRegion;
        //}

        private void BtnCreateRegions_Click(object sender, EventArgs e)
        {
            _autoRowCol();
        }

        private void XTimer_Tick(object sender, EventArgs e)
        {
            btnRegionBase.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnSelectTemplate.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
        }
       
        frmBaseX2  FrmBaseX2 = null;
        void showBaseX2DialogWindow()
        {
            FrmBaseX2 = new frmBaseX2(xRegionNameCurrent);
            FrmBaseX2.ShowDialog();
            FrmBaseX2.Dispose();
            FrmBaseX2 = null;
        }
        private void BtnRegionBaseForm_Click(object sender, EventArgs e)
        {
            showBaseX2DialogWindow();
        }

        private void BtnRegionBase_Click(object sender, EventArgs e)
        {
            //xRegionNameCurrent = RegionName.BASE0;
            bSelectRegion = !bSelectRegion;
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_filename))
            {
                FreeImageBitmap freeImageBitmap = new FreeImageBitmap(_filename);
                if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    xRecipe.bmpOrg.Dispose();
                    xRecipe.bmpOrg = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                    //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                    //xRecipe.bmpOrg.Dispose();
                    //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                    //b1.Dispose();
                    DS.ReplaceDisplayImage(xRecipe.bmpOrg);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                    xRecipe.bmpOrg.Dispose();
                    xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                    b1.Dispose();
                    DS.ReplaceDisplayImage(xRecipe.bmpOrg);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    xRecipe.bmpOrg.Dispose();
                    xRecipe.bmpOrg = freeImageBitmap.ToBitmap();

                    DS.ReplaceDisplayImage(xRecipe.bmpOrg);
                }
                else
                {
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                }

                freeImageBitmap.Dispose();
            }
        }

        Bitmap Convert32bppTo8bpp(Bitmap original)
        {
            // 创建一个新的8bpp位图
            Bitmap newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format8bppIndexed);

            // 设置调色板（这里使用灰度调色板）
            ColorPalette palette = newBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            newBitmap.Palette = palette;

            // 锁定位图数据
            BitmapData originalData = original.LockBits(
                new Rectangle(0, 0, original.Width, original.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            BitmapData newData = newBitmap.LockBits(
                new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // 转换像素数据
            unsafe
            {
                byte* originalPtr = (byte*)originalData.Scan0;
                byte* newPtr = (byte*)newData.Scan0;

                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        // 获取32bpp像素值
                        byte b = originalPtr[y * originalData.Stride + x * 4];
                        byte g = originalPtr[y * originalData.Stride + x * 4 + 1];
                        byte r = originalPtr[y * originalData.Stride + x * 4 + 2];
                        byte a = originalPtr[y * originalData.Stride + x * 4 + 3];

                        // 转换为灰度值（8bpp）
                        byte gray = (byte)((r * 0.299 + g * 0.587 + b * 0.114) * (a / 255.0));

                        // 写入8bpp位图
                        newPtr[y * newData.Stride + x] = gray;
                    }
                }
            }

            // 解锁位图
            original.UnlockBits(originalData);
            newBitmap.UnlockBits(newData);

            return newBitmap;
        }
        Bitmap Convert24bppTo8bpp(Bitmap original)
        {
            //if (original.PixelFormat != PixelFormat.Format24bppRgb)
            //    throw new ArgumentException("源图像必须是24位位图");

            // 创建新的8位位图
            Bitmap newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format8bppIndexed);

            // 设置灰度调色板
            ColorPalette palette = newBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            newBitmap.Palette = palette;

            // 锁定位图数据进行操作
            BitmapData originalData = original.LockBits(
                new Rectangle(0, 0, original.Width, original.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData newData = newBitmap.LockBits(
                new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* originalPtr = (byte*)originalData.Scan0;
                byte* newPtr = (byte*)newData.Scan0;

                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        // 获取24bpp像素值
                        byte b = originalPtr[y * originalData.Stride + x * 3];
                        byte g = originalPtr[y * originalData.Stride + x * 3 + 1];
                        byte r = originalPtr[y * originalData.Stride + x * 3 + 2];

                        // 转换为灰度值（8bpp）
                        byte gray = (byte)(r * 0.299 + g * 0.587 + b * 0.114);

                        // 写入8bpp位图
                        newPtr[y * newData.Stride + x] = gray;
                    }
                }
            }

            // 解锁位图
            original.UnlockBits(originalData);
            newBitmap.UnlockBits(newData);

            return newBitmap;
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            xRecipe.Load();
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            //CoarsePositioningClass.Instance.sCoarsePosList = ctlPosClasses[0].GetPositionList();
            //ModelPositioningClass.Instance.sModelPosList = ctlPosClasses[1].GetPositionList();

            //GraphicalObject grobj = myMover[0].Source;
            //myRecipe.rect_start = (grobj as JzRectEAG).GetRect;
            //GraphicalObject grobj1 = myMover[1].Source;
            //myRecipe.rect_end = (grobj1 as JzRectEAG).GetRect;
            this.DialogResult = DialogResult.OK;
        }
        void init_Display()
        {
            //DS = dispUI1;
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS.CaptureAction += DS_CaptureAction;
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;

            //DS2.Initial(100, 0.01f);
            //DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS2.CaptureAction += DS_CaptureAction2;
        }
        void update_Display(bool eChangeToDefault = true)
        {
            DS.Refresh();
            if (eChangeToDefault)
                DS.DefaultView();

            //DS2.Refresh();
            //if (eChangeToDefault)
            //    DS2.DefaultView();
        }
        private void DS_CaptureAction(RectangleF rectf)
        {
            if (!bSelectRegion)
                return;
            BoundRect(ref rectf, xRecipe.bmpOrg.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                Bitmap bmpx = new Bitmap(xRecipe.bmpOrg);
                Graphics g = Graphics.FromImage(bmpx);
                switch (xRegionNameCurrent)
                {
                    case RegionName.BASE0:
                        xRecipe.xRectRegionBase0 = rectf;
                        xRecipe.bmpbase0 = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        xRecipe.SaveBase();
                        break;
                    case RegionName.BASE1:
                        xRecipe.xRectRegionBase1 = rectf;
                        xRecipe.bmpbase1 = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        xRecipe.SaveBase();
                        break;
                    case RegionName.GOODIMAGE:
                        xRecipe.xRectRegionPrint = rectf;
                        xRecipe.bmpprinttemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        xRecipe.SavePrintTemplate();
                        //foreach (var cell in xRecipe.xRegionCells)
                        //{
                        //    if (cell.viewRectF.IntersectsWith(rectf))
                        //    {
                        //        xRectGoodImage = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                        //        break;
                        //    }
                        //}

                        break;
                }
                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectf });
                g.Dispose();
                DS.ReplaceDisplayImage(bmpx);
                bmpx.Dispose();

                //xRecipe.SaveBase();
            }
            bSelectRegion = false;
        }

        private void _autoRowCol()
        {
            xRecipe.CreateViews();

            DS.ClearStaticMover();
            xMovers.Clear();

            int i = 0;
            while (i < xRecipe.xRegionCells.Count)
            {
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), xRecipe.xRegionCells[i].viewRectF);
                _rect.RelateLevel = 2;
                _rect.RelateNo = i;
                _rect.RelatePosition = 0;
                xMovers.Add(_rect);

                i++;
            }

            DS.SetStaticMover(xMovers);
            DS.RefreshDisplayShape();
            DS.MappingSelect();

            update_Display(false);
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


        #endregion
    }
}
