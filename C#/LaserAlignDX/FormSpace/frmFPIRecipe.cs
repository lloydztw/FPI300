using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.ImageViewerEx.Interactors;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmFPIRecipe : Form
    {
        protected IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        //Mover xMover = new Mover();
        Mover xMovers = new Mover();
        bool bSelectRegion = false;
        //RectangleF xRectGoodImage = new RectangleF(0, 0, 1, 1);
        MeasureType xRegionNameCurrent // = RegionName.BASE0;
        {
            get
            {
                MeasureType measureType = MeasureType.MeasureAOI;
                if (rdoMeasureAOI.Checked)
                    measureType = MeasureType.MeasureAOI;
                else if (rdoMeasureBarcode.Checked)
                    measureType = MeasureType.MeasureBarcode;
                else if (rdoMeasureNoTray.Checked)
                    measureType = MeasureType.MeasureNoTray;
                return measureType;
            }
        }

        int xTabIndex
        {
            get
            {
                return tabControl2.SelectedIndex;
            }
        }

        Timer xTimer = null;
        Button btnOK;
        Button btnCancel;

        Button btnLoadImage;
        Button btnSelectRegion;
        //Button btnRegionBaseForm;
        Button btnGetRealImage;

        Button btnCreateRegions;
        //Button btnSelectTemplate;
        Button btnSelectTemplateForm;
        Button btnNoTrayTemplateForm;
        Button btnFlyTemplateForm;

        Button btnControlLight;

        Button btnSaveImage;

        public frmFPIRecipe()
        {
            InitializeComponent();
            this.Load += FrmFPIRecipe_Load;
            this.FormClosed += FrmFPIRecipe_FormClosed;
            this.SizeChanged += FrmFPIRecipe_SizeChanged;
        }

        private void FrmFPIRecipe_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        private void FrmFPIRecipe_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void FrmFPIRecipe_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            btnOK = button1;
            btnCancel = button2;

            btnLoadImage = button7;
            btnSelectRegion = button3;
            btnNoTrayTemplateForm = button4;
            btnCreateRegions = button9;
            btnGetRealImage = button5;
            btnSelectTemplateForm = button6;
            btnControlLight = button8;
            btnFlyTemplateForm = button10;
            btnSaveImage = button11;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            btnLoadImage.Click += BtnLoadImage_Click;
            btnSelectRegion.Click += BtnSelectRegion_Click;
            //btnRegionBaseForm.Click += BtnRegionBaseForm_Click;
            btnCreateRegions.Click += BtnCreateRegions_Click;
            //btnSelectTemplate.Click += BtnSelectTemplate_Click;
            btnSelectTemplateForm.Click += BtnSelectTemplateForm_Click;
            btnGetRealImage.Click += BtnGetRealImage_Click;
            btnControlLight.Click += BtnControlLight_Click;
            btnFlyTemplateForm.Click += BtnFlyTemplateForm_Click;
            btnSaveImage.Click += BtnSaveImage_Click;
            btnNoTrayTemplateForm.Click += BtnNoTrayTemplateForm_Click;

            xTimer = new Timer();
            xTimer.Interval = 50;
            xTimer.Enabled = true;
            xTimer.Tick += XTimer_Tick;

            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);

            propertyGrid1.SelectedObject = RecipeParaGridClass.Instance;

            this.Text = "参数设定窗口";
            this.FormBorderStyle = FormBorderStyle.None;

            LanguageExClass.Instance.EnumControls(this);


            #region 隐藏一些不需要的控件

            rdoMeasureBarcode.Visible = false;
            tabControl2.Controls.RemoveAt(1);

            #endregion


#if OPT_LETIAN_AUTO_LAYOUT
            // To fit into my screen for debug.
#if DEBUG
            this.FormBorderStyle = FormBorderStyle.Sizable;
#endif
            this.WindowState = FormWindowState.Maximized;
#endif
        }

        frmNoTrayX3 frmNoTrayX3x = null;
        private void BtnNoTrayTemplateForm_Click(object sender, EventArgs e)
        {
            frmNoTrayX3x = new frmNoTrayX3();
            frmNoTrayX3x.ShowDialog();
            frmNoTrayX3x.Dispose();
            frmNoTrayX3x = null;
        }

        private void BtnSaveImage_Click(object sender, EventArgs e)
        {

            string _filepath = JetEazy.BasicSpace.JzToolsClass.SaveFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_filepath))
            {
                Bitmap bmpfuckyou = new Bitmap(xRecipe.bmpOrg,
                    new Size(xRecipe.bmpOrg.Width >> 1, xRecipe.bmpOrg.Height >> 1));
                bmpfuckyou.Save(_filepath, ImageFormat.Jpeg);

                //FreeImageBitmap image = new FreeImageBitmap(xRecipe.bmpOrg,
                //    new Size(xRecipe.bmpOrg.Width >> 1, xRecipe.bmpOrg.Height >> 1));
                ////image.Rescale(new Size(xRecipe.bmpOrg.Width >> 1, xRecipe.bmpOrg.Height >> 1), FREE_IMAGE_FILTER.FILTER_BILINEAR);
                //image.Save(_filepath, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_JPEG);
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("图片保存完成.路径:")}{Environment.NewLine + _filepath}", false);
            }

        }

        frmFlySetup frmFlySetupX = null;// new frmFlySetup();
        private void BtnFlyTemplateForm_Click(object sender, EventArgs e)
        {
            frmFlySetupX = new frmFlySetup();
            frmFlySetupX.ShowDialog();
            frmFlySetupX.Dispose();
            frmFlySetupX = null;
        }

        frmLightControl frmLight = null;
        private void BtnControlLight_Click(object sender, EventArgs e)
        {
            frmLight = new frmLightControl();
            frmLight.ShowDialog();
            frmLight.Dispose();
            frmLight = null;
        }

        private void BtnGetRealImage_Click(object sender, EventArgs e)
        {
            if (IScanCam.GetFreeImageBitmap() != null)
            {
                switch (xTabIndex)
                {
                    case 0:
                        xRecipe.bmpOrg.Dispose();
                        xRecipe.bmpOrg = IScanCam.GetFreeImageBitmap().ToBitmap();
                        DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                        break;
                    case 1:
                        xRecipe.bmpOrgNoTray.Dispose();
                        xRecipe.bmpOrgNoTray = IScanCam.GetFreeImageBitmap().ToBitmap();
                        DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                        break;
                }

            }
        }

        private void BtnSelectTemplateForm_Click(object sender, EventArgs e)
        {
            showTemplateX3DialogWindow();
        }
        frmTemplateX3 FrmTemplateX3 = null;
        void showTemplateX3DialogWindow()
        {
            FrmTemplateX3 = new frmTemplateX3();
            FrmTemplateX3.ShowDialog();
            FrmTemplateX3.Dispose();
            FrmTemplateX3 = null;


            propertyGrid1.SelectedObject = RecipeParaGridClass.Instance;
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
            btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnSelectTemplate.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
        }

        //frmBaseX2 FrmBaseX2 = null;
        //void showBaseX2DialogWindow()
        //{
        //    FrmBaseX2 = new frmBaseX2(xRegionNameCurrent);
        //    FrmBaseX2.ShowDialog();
        //    FrmBaseX2.Dispose();
        //    FrmBaseX2 = null;
        //}
        private void BtnRegionBaseForm_Click(object sender, EventArgs e)
        {
            //showBaseX2DialogWindow();
        }

        private void BtnSelectRegion_Click(object sender, EventArgs e)
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

                switch (xTabIndex)
                {
                    case 0:
                        if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            xRecipe.bmpOrg.Dispose();
                            xRecipe.bmpOrg = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                            //xRecipe.bmpOrg.Dispose();
                            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            //b1.Dispose();
                            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                        {
                            Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                            xRecipe.bmpOrg.Dispose();
                            xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            b1.Dispose();
                            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                        {
                            xRecipe.bmpOrg.Dispose();
                            xRecipe.bmpOrg = freeImageBitmap.ToBitmap();

                            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                        }
                        else
                        {
                            JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                        }
                        break;
                    case 1:
                        if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            xRecipe.bmpOrgNoTray.Dispose();
                            xRecipe.bmpOrgNoTray = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                            //xRecipe.bmpOrg.Dispose();
                            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            //b1.Dispose();
                            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                        {
                            Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                            xRecipe.bmpOrgNoTray.Dispose();
                            xRecipe.bmpOrgNoTray = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            b1.Dispose();
                            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                        {
                            xRecipe.bmpOrgNoTray.Dispose();
                            xRecipe.bmpOrgNoTray = freeImageBitmap.ToBitmap();

                            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                        }
                        else
                        {
                            JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                        }
                        break;
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

        CviCross _cviCross = new CviCross(Color.Yellow);

        void init_Display()
        {
            //DS = dispUI1;
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS1.CaptureAction += DS_CaptureAction;
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
            DS1.ImageViewer.AddInteractor(_cviCross);
            _cviCross.Visible = true;

            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS2.CaptureAction += DS_CaptureAction2;
            DS2.ImageViewer.AddInteractor(_cviCross);
            _cviCross.Visible = true;
        }
        void update_Display(bool eChangeToDefault = true)
        {
            DS1.Refresh();
            if (eChangeToDefault)
                DS1.DefaultView();

            DS2.Refresh();
            if (eChangeToDefault)
                DS2.DefaultView();
        }
        private void DS_CaptureAction(RectangleF rectf)
        {
            if (!bSelectRegion)
                return;
            BoundRect(ref rectf, xRecipe.bmpOrg.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                //防止大图的问题 不能new图  通过控件的框显示
                //Bitmap bmpx = new Bitmap(xRecipe.bmpOrg);
                //Graphics g = Graphics.FromImage(bmpx);

                DS1.ClearStaticMover();
                //DS2.ClearStaticMover();
                xMovers.Clear();
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), rectf);
                _rect.RelateLevel = 2;
                //_rect.RelateNo = i;
                _rect.RelatePosition = 0;
                xMovers.Add(_rect);

                switch (xRegionNameCurrent)
                {
                    //case RegionName.BASE0:
                    //    xRecipe.xRectRegionBase0 = rectf;
                    //    xRecipe.bmpbase0 = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    //    xRecipe.SaveBase();
                    //    break;
                    //case RegionName.BASE1:
                    //    xRecipe.xRectRegionBase1 = rectf;
                    //    xRecipe.bmpbase1 = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    //    xRecipe.SaveBase();
                    //    break;
                    //case RegionName.GOODIMAGE:
                    //    xRecipe.xRectRegionPrint = rectf;
                    //    xRecipe.bmpprinttemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    //    xRecipe.SavePrintTemplate();

                    //    break;
                    case MeasureType.MeasureAOI:
                        xRecipe.xRectRegionPrint = rectf;
                        xRecipe.bmpprinttemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        xRecipe.SavePrintTemplate();
                        break;
                    case MeasureType.MeasureNoTray:
                        xRecipe.xRectRegionPrintNoTray = rectf;
                        xRecipe.bmpprintNoTraytemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        xRecipe.SavePrintNoTrayTemplate();
                        break;
                }

                DS1.SetStaticMover(xMovers);
                DS1.RefreshDisplayShape();
                DS1.MappingSelect();

                update_Display(false);

                //g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectf });
                //g.Dispose();
                //DS1.ReplaceDisplayImage(bmpx);
                //bmpx.Dispose();

                //xRecipe.SaveBase();
            }
            bSelectRegion = false;
        }
        private void DS_CaptureAction2(RectangleF rectf)
        {
            //if (!bSelectRegion)
            //    return;
            //BoundRect(ref rectf, xRecipe.bmpOrgNoTray.Size);
            //if (rectf.Width > 1 && rectf.Height > 1)
            //{
            //    Bitmap bmpx = new Bitmap(xRecipe.bmpOrgNoTray);
            //    Graphics g = Graphics.FromImage(bmpx);
            //    switch (xRegionNameCurrent)
            //    {
            //        case MeasureType.MeasureNoTray:
            //            xRecipe.xRectRegionPrintNoTray = rectf;
            //            xRecipe.bmpprintNoTraytemplate = xRecipe.bmpOrgNoTray.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //            xRecipe.SavePrintNoTrayTemplate();
            //            break;
            //    }
            //    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectf });
            //    g.Dispose();
            //    DS2.ReplaceDisplayImage(bmpx);
            //    bmpx.Dispose();

            //    //xRecipe.SaveBase();
            //}
            //bSelectRegion = false;
        }
        private void _autoRowCol()
        {
            xRecipe.CreateViews();

            DS1.ClearStaticMover();
            DS2.ClearStaticMover();
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

            //switch (xTabIndex)
            //{
            //    case 0:
            //        DS1.SetStaticMover(xMovers);
            //        DS1.RefreshDisplayShape();
            //        DS1.MappingSelect();
            //        break;
            //    case 1:
            //        DS2.SetStaticMover(xMovers);
            //        DS2.RefreshDisplayShape();
            //        DS2.MappingSelect();
            //        break;
            //}

            //DS1.SetStaticMover(xMovers);
            //DS1.RefreshDisplayShape();
            //DS1.MappingSelect();

            DS2.SetStaticMover(xMovers);
            DS2.RefreshDisplayShape();
            DS2.MappingSelect();

            DS1.SetStaticMover(xMovers);
            DS1.RefreshDisplayShape();
            DS1.MappingSelect();

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

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

    }
}
