using Common.RecipeSpace;
using FreeImageAPI;
using JetEazy.ControlSpace;
using JetEazy.Interface;
using JzDisplay;
using LaserAlignDX.BasicSpace.ParaSpace;
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
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace.FPI30Form
{
    public partial class frmCalibration : Form
    {
        Mover xMovers = new Mover();
        Bitmap bmpOperate = new Bitmap(1, 1);
        const int BTNCOUNT = 4;

        protected IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }

        Button btnGetImage;
        Button btnLoadImage;
        Button btnOK;
        Button btnCancel;
        Button btnReCalibration;

        Button[] btnPointPos = new Button[BTNCOUNT];
        CalibrationUI[] calibrationUIs = new CalibrationUI[BTNCOUNT];
        bool[] btnSelect = new bool[BTNCOUNT];
        int m_CurrentIndex = 0;

        Button btnSelectCurrent
        {
            get { return btnPointPos[m_CurrentIndex]; }
        }
        bool IsSelectCurrent
        {
            get { return btnSelect[m_CurrentIndex]; }
            set { btnSelect[m_CurrentIndex] = value; }
        }
        //CalibrationUI CalibrationCurrent
        //{
        //    get { return calibrationUIs[tabControl3.SelectedIndex]; }
        //}

        public frmCalibration()
        {
            InitializeComponent();
            this.Load += FrmCalibration_Load;
            this.SizeChanged += FrmCalibration_SizeChanged;
        }

        private void FrmCalibration_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        private void FrmCalibration_Load(object sender, EventArgs e)
        {
            this.Text = $"载台校正页面";

            init_Display();
            update_Display();

            btnPointPos = new Button[BTNCOUNT] { button1, button2, button3, button4 };
            calibrationUIs = new CalibrationUI[BTNCOUNT] { calibrationUI1, calibrationUI2, calibrationUI3, calibrationUI4 };
            btnSelect = new bool[BTNCOUNT] { false, false, false, false };

            btnGetImage = button5;
            btnLoadImage = button6;
            btnOK = button7;
            btnCancel = button8;
            btnReCalibration = button9;

            btnGetImage.Click += BtnGetImage_Click;
            btnLoadImage.Click += BtnLoadImage_Click;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnReCalibration.Click += BtnReCalibration_Click;

            pgPara.SelectedObject = MvdFindCircleClass.Instance;
            cboStage.SelectedIndex = 0;

            int i = 0;
            while (i < BTNCOUNT)
            {
                btnPointPos[i].Tag = i;
                btnPointPos[i].Click += FrmCalibrationBtnPos_Click;

                calibrationUIs[i].Init(tabControl3.TabPages[i].Text, i);

                i++;
            }

        }

        private void BtnReCalibration_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (i < BTNCOUNT)
            {
                //LineScanCalibrateClasses[i] = new LineScanCalibrateClass();
                //LineScanCalibrateClasses[i].Initial(WORKPATH, 0, $"Calibrate_default_info{i}.ini");
                Traveller106.Universal.LineScanCalibrateClasses[i].Save();
                i++;
            }
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"重新校正成功", false);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            MvdFindCircleClass.Instance.Save();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"保存成功", false);
        }

        private void FrmCalibrationBtnPos_Click(object sender, EventArgs e)
        {
            m_CurrentIndex = int.Parse(((Button)sender).Tag.ToString());
            IsSelectCurrent = !IsSelectCurrent;
            btnSelectCurrent.BackColor = IsSelectCurrent ? Color.Red : Color.FromArgb(192, 255, 192);

        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_filename))
            {
                FreeImageBitmap freeImageBitmap = new FreeImageBitmap(_filename);
                if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    bmpOperate.Dispose();
                    bmpOperate = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                    DS.ReplaceDisplayImage(bmpOperate);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    bmpOperate.Dispose();
                    bmpOperate = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                    DS.ReplaceDisplayImage(bmpOperate);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    bmpOperate.Dispose();
                    bmpOperate = freeImageBitmap.ToBitmap();
                    DS.ReplaceDisplayImage(bmpOperate);
                }
                else
                {
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                }


                freeImageBitmap.Dispose();
            }
        }

        private void BtnGetImage_Click(object sender, EventArgs e)
        {
            if (IScanCam.GetFreeImageBitmap() != null)
            {
                bmpOperate.Dispose();
                bmpOperate = IScanCam.GetFreeImageBitmap().ToBitmap();
                DS.ReplaceDisplayImage(bmpOperate);

            }
        }

        void init_Display()
        {
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS2.Initial(100, 0.01f);
            //DS2.SetDisplayType(DisplayTypeEnum.SHOW);
            //DS3.Initial(100, 0.01f);
            //DS3.SetDisplayType(DisplayTypeEnum.SHOW);
            DS.CaptureAction += DS_CaptureAction;
        }

        private void DS_CaptureAction(RectangleF rectf)
        {
            RectangleF rectf_org = new RectangleF(10, 10, 10, 10);
            RectangleF rectf_des = new RectangleF(10, 10, 10, 10);

            if (IsSelectCurrent)
            {
                BoundRect(ref rectf, bmpOperate.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    DS.ClearStaticMover();
                    xMovers.Clear();
                    JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), rectf);
                    _rect.RelateLevel = 2;
                    //_rect.RelateNo = i;
                    _rect.RelatePosition = 0;
                    xMovers.Add(_rect);

                    IsSelectCurrent = false;

                    Bitmap bmp0 = bmpOperate.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    PointF ptf1 = MvdFindCircleClass.Instance.GetImageMarkCenter(
                                        bmp0,
                                        true,
                                        out RectangleF maxrecttemp,
                                        out Bitmap bmpouputtemp,
                                        out int maxareatemp);

                    rectf_des = new RectangleF(maxrecttemp.X + rectf.X,
                                                       maxrecttemp.Y + rectf.Y,
                                                       maxrecttemp.Width,
                                                       maxrecttemp.Height);

                    ptf1.X += rectf.X;
                    ptf1.Y += rectf.Y;

                    switch (cboStage.SelectedIndex)
                    {
                        case 0:
                            calibrationUIs[0].SetViewPoints(m_CurrentIndex, ptf1);
                            calibrationUIs[1].SetViewPoints(m_CurrentIndex, ptf1);
                            break;
                        case 1:
                            calibrationUIs[2].SetViewPoints(m_CurrentIndex, ptf1);
                            calibrationUIs[3].SetViewPoints(m_CurrentIndex, ptf1);
                            break;
                    }

                    //CalibrationCurrent.SetViewPoints(m_CurrentIndex, ptf1);

                    switch (MvdFindCircleClass.Instance.mFindType)
                    {
                        case OPSpace.FindType.CIRCLE:

                            JzCircleEAG jzCircleEAG = new JzCircleEAG(Color.FromArgb(0, Color.Blue), rectf_des);
                            jzCircleEAG.RelateLevel = 3;
                            //_rect.RelateNo = i;
                            jzCircleEAG.RelatePosition = 0;
                            xMovers.Add(jzCircleEAG);

                            break;
                        case OPSpace.FindType.BLOB:
                        default:

                            _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), rectf_des);
                            _rect.RelateLevel = 3;
                            //_rect.RelateNo = i;
                            _rect.RelatePosition = 0;
                            xMovers.Add(_rect);

                            break;
                    }

                    btnSelectCurrent.BackColor = (IsSelectCurrent ? Color.Red : Color.FromArgb(192, 255, 192));

                    bmp0.Dispose();

                    DS.SetStaticMover(xMovers);
                    DS.RefreshDisplayShape();
                    DS.MappingSelect();

                    update_Display(false);
                }
            }
        }

        void update_Display(bool eRefresh = true)
        {
            DS.Refresh();
            if (eRefresh)
                DS.DefaultView();
            //DS2.Refresh();
            //DS2.DefaultView();
            //DS3.Refresh();
            //DS3.DefaultView();
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

        public void BoundRect(ref Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
    }
}
