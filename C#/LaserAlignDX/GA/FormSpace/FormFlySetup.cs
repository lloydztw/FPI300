using Eazy_Project_III;
using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.ImageViewerEx.Interactors;
using JetEazy.Interface;
using JetEazy.Utils;
using JzDisplay;
using LaserAlignDX.GA.FormSpace.FPI30Form;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Traveller106;
using VisionDesigner.BlobFind;
using VsCommon.ControlSpace.MachineSpace;
using WorldOfMoveableObjects;
using Timer = System.Windows.Forms.Timer;

namespace LaserAlignDX.FormSpace
{
    public partial class FormFlySetup : Form
    {
        #region INTERACTORS
        CviCross _cviCross = new CviCross(Color.Yellow);
        Mover xMovers = new Mover();
        #endregion

        #region GLOBAL_MESS_RECIPE
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        #endregion

        #region GLOBAL_MESS_MACHINE
        protected MainFPIX3MachineClass MACHINEx3
        {
            get { return (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        #endregion

        #region GLOBAL_MESS_FLY_CAMERA
        IxLineScanCam IxFlyAreaCam
        {
            get { return Universal.IxFlyAreaCam; }
        }
        #endregion

        #region PRIVATE_FLAGS
        bool _bSelectRegion = false;
        bool _bOpenFly = false;
        bool _bFlyComplete = false;
        bool _isPropertyModified = false;
        #endregion

        #region THREAD_DATA
        Thread _th_FlyLive = null;
        bool _isFlyRunning = false;
        //Bitmap bmpOperate = new Bitmap(1, 1);
        #endregion

        /// <summary>
        /// 如果是自己 new 的 Timer 記得退出前要調用 Dispose() !!!
        /// </summary>
        Timer xTimer = null;

        #region GUI_LINKS

        //btnGetLocalImage = button7;
        //btnOK = button1;
        //btnCancel = button2;
        //btnSelectRegion = button3;
        //btnOpenFly = button4;
        //btnLightTrigger = button5;
        //btnSpecialCal = button6;
        Button btnGetLocalImage => button7;
        Button btnOK => button1;
        Button btnCancel => button2;
        Button btnSelectRegion => button3;
        Button btnOpenFly => button4;
        Button btnLightTrigger => button5;
        Button btnSpecialCalc => button6;
        Button btnCodetest => button8;
        RichTextBox rtbCodeContent => richTextBox1;

        FlyOffsetUI flyOffsetUI => flyOffsetUI1;
        FlyOffsetUI flyOffset2UI => flyOffsetUI2;
        Label lblExpo => label2;
        Label lblGain => label3;
        
        Button btnOpenMotorJogWindow => button9;

        TabControl tabMainPages => tabControl2;
        #endregion

        /// <summary>
        /// 飞拍模式 0-取像 1-测试
        /// </summary>
        int iFlyMode
        {
            get
            {
                if (radioButton1.Checked)
                {
                    return 0;
                }
                else if (radioButton2.Checked)
                {
                    return 1;
                }

                return 0;
            }
        }

        public FormFlySetup()
        {
            Traveller106.Universal.IsOpenFlyForm = true;
            InitializeComponent();

            this.Load += FrmFlySetup_Load;
            this.FormClosing += FrmFlySetup_FormClosing;
            this.FormClosed += FrmFlySetup_FormClosed;

            // 保存原來的顏色
            btnSelectRegion.Tag = btnSelectRegion.BackColor;
            btnOpenFly.Tag = btnOpenFly.BackColor;
        }

        private void FrmFlySetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!makeSureNotLiveMode())
                e.Cancel = true;
        }
        private void FrmFlySetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            xTimer?.Stop();
            xTimer?.Dispose();
            xTimer = null;

            xRecipe.ReleaseBmpOrgFly(true);
            IxFlyAreaCam.LineTriggerAction -= IxFlyAreaCam_LineTriggerAction;
            Traveller106.Universal.IsOpenFlyForm = false;
        }

        private void FrmFlySetup_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            //btnGetLocalImage = button7;
            //btnOK = button1;
            //btnCancel = button2;
            //btnSelectRegion = button3;
            //btnOpenFly = button4;
            //btnLightTrigger = button5;
            //btnSpecialCal = button6;

            btnGetLocalImage.Click += BtnGetLocalImage_Click;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnSelectRegion.Click += BtnSelectRegion_Click;
            btnOpenFly.Click += BtnOpenFly_Click;
            btnLightTrigger.Click += BtnLightTrigger_Click;
            btnSpecialCalc.Click += BtnSpecialCal_Click;
            btnCodetest.Click += BtnCodetest_Click;
            btnOpenMotorJogWindow.Click += (s, ev) => OpenMotorJogWindow();

            DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);

            IxFlyAreaCam.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
            propertyGrid1.SelectedObject = FlyParaClass.Instance;
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;

            this.Text = "飞拍参数设定窗口";

            LanguageExClass.Instance.EnumControls(this);

            flyOffsetUI.Init(StageNumber.N0);
            flyOffset2UI.Init(StageNumber.N1);

            getCamDevParaAndUpdateUI();

            xTimer = new Timer();
            xTimer.Interval = 50;
            xTimer.Enabled = true;
            xTimer.Tick += XTimer_Tick;
        }

        private void BtnCodetest_Click(object sender, EventArgs e)
        {
            rtbCodeContent.Text = "";
            aoiDecodeCode(xRecipe.bmpprintFlytemplate, out string text);
            rtbCodeContent.Text = $"[{DateTime.Now.ToString("HH:mm:ss")}] {text}";
        }

        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            switch (e.ChangedItem.PropertyDescriptor.Name)
            {
                case "xCamExpo":
                case "xCamGain":
                    applyCameraExposureAndGain(true);
                    break;
            }
        }

        private void BtnGetCamPara_Click(object sender, EventArgs e)
        {
            //getCamDevParaAndUpdateUI();
        }

        private void BtnSpecialCal_Click(object sender, EventArgs e)
        {

            RectangleF rectf = xRecipe.xRectRegionPrintFly;
            BoundRect(ref rectf, xRecipe.bmpOrgFly.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                Bitmap bmp0 = xRecipe.bmpOrgFly.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                bool bOK = xRecipe.CheckSpecialAngle(bmp0, out List<CBlobInfo> list, out float angle, out System.Drawing.PointF Center);
                if (bOK)
                {

                    DS2.ClearStaticMover();
                    xMovers.Clear();
                    DS2.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                    foreach (CBlobInfo cBlob in list)
                    {
                        RectangleF x = new RectangleF(cBlob.RectInfo.CenterX - cBlob.RectInfo.Width / 2 + rectf.X,
                            cBlob.RectInfo.CenterY - cBlob.RectInfo.Height / 2 + rectf.Y,
                            cBlob.RectInfo.Width,
                            cBlob.RectInfo.Height);

                        JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), x);
                        _rect.RelateLevel = 2;
                        _rect.RelatePosition = 0;
                        _rect.SetAngle(-cBlob.BoxInfo.Angle + 90);
                        xMovers.Add(_rect);
                    }

                    //CviLabel cviLabel = new CviLabel();
                    //cviLabel.Text = $"计算角度:{angle}";
                    //cviLabel.Location = new Point((int)Center.X, (int)Center.Y);
                    //cviLabel.Visible = true;
                    //DS2.ImageViewer.AddInteractor(cviLabel);

                    DS2.SetStaticMover(xMovers);
                    DS2.RefreshDisplayShape();
                    DS2.MappingSelect();

                    update_Display(false);

                    MessageBox.Show($"计算角度:{angle}");
                }

            }
        }

        private void BtnLightTrigger_Click(object sender, EventArgs e)
        {
            _bOpenFly = true;
            if (MACHINEx3.LightCollection.Length > 1)
                MACHINEx3.LightCollection[1].Trigger();
        }

        private void BtnOpenFly_Click(object sender, EventArgs e)
        {
            //bOpenFly = !bOpenFly;
            //IxFlyAreaCam.TriggerMode(!IxFlyAreaCam.IsTriggerModeOn);

            if (!_isFlyRunning)
            {
                _isFlyRunning = true;
                _th_FlyLive = new Thread(new ThreadStart(flyRunning));
                _th_FlyLive.Start();
            }
            else
            {
                _isFlyRunning = false;
                System.Threading.Thread.Sleep(500);
                if (_th_FlyLive != null)
                {
                    _th_FlyLive.Abort();
                    _th_FlyLive = null;
                }
            }

            if (_isFlyRunning)
                tabMainPages.SelectedIndex = 0;

            btnLightTrigger.Enabled = !_isFlyRunning;
            btnGetLocalImage.Enabled = !_isFlyRunning;
            btnSelectRegion.Enabled = !_isFlyRunning;
            btnSpecialCalc.Enabled = !_isFlyRunning;
            btnCodetest.Enabled = !_isFlyRunning;
        }

        void flyRunning()
        {
            while (_isFlyRunning)
            {
                try
                {
                    if (!_bFlyComplete)
                    {
                        _bFlyComplete = true;
                        _bOpenFly = true;
                        if (MACHINEx3.LightCollection.Length > 1)
                            MACHINEx3.LightCollection[1].Trigger();
                    }

                    System.Threading.Thread.Sleep(50);

                }
                catch
                {

                }
            }
        }

        private void XTimer_Tick(object sender, EventArgs e)
        {
            //btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192);
            ////btnSelectTemplate.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
            //btnOpenFly.BackColor = (m_FlyRunning ? _activeColor : Color.FromArgb(192, 255, 192));
            updateColor(btnSelectRegion, _bSelectRegion);
            updateColor(btnOpenFly, _isFlyRunning);
        }

        private void BtnSelectRegion_Click(object sender, EventArgs e)
        {
            _bSelectRegion = !_bSelectRegion;
        }

        private void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            if (!_bOpenFly)
                return;

            if (cameraFrame.Format != PixelFormat.Format8bppIndexed)
                return;

            //转换图像
            byte[] bmpbytes = new byte[cameraFrame.uBytes];
            Marshal.Copy(pBuffer, bmpbytes, 0, bmpbytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;

            switch (iFlyMode)
            {
                case 0:

                    if (_bFlyComplete)
                    {
                        //using (Bitmap bmp = ConvertFromMONO(bmpbytes, iw, ih))
                        //{
                        //    DS1.ReplaceDisplayImage(bmp);
                        //}
                        Bitmap bmpNew = ConvertFromMONO(bmpbytes, iw, ih);
                        BeginInvoke(new Action<Bitmap>((bmp) =>
                        {
                            DS1.ReplaceDisplayImage(bmp); 
                            bmp?.Dispose();
                        }), bmpNew);
                        _bFlyComplete = false;
                    }
                    else
                    {
                        //取图
                        xRecipe.bmpOrgFly?.Dispose();
                        xRecipe.bmpOrgFly = ConvertFromMONO(bmpbytes, iw, ih);

                        //DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                        BeginInvoke((Action<Bitmap>)DS1.ReplaceDisplayImage, xRecipe.bmpOrgFly);
                    }

                    break;
                case 1:
                    //测试


                    break;
            }

            _bOpenFly = false;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //if(m_FlyRunning)
            //{
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");

            if (!makeSureNotLiveMode())
                return;

            xRecipe.Load();
            applyCameraExposureAndGain();
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            //if (m_FlyRunning)
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");

            if (!makeSureNotLiveMode())
                return;

            flyOffsetUI.GetPoints();
            flyOffset2UI.GetPoints();
            xRecipe.FlyAoiParams.Save();
            applyCameraExposureAndGain();
            this.DialogResult = DialogResult.OK;
        }

        private void BtnGetLocalImage_Click(object sender, EventArgs e)
        {
            string filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("JPG Files (*.jpg)|*.JPG|" + "All files (*.*)|*.*", "");
            
            if (!string.IsNullOrEmpty(filename))
            {
#if (OPT_OLD_CODE)
                FreeImageBitmap freeImageBitmap = new FreeImageBitmap(filename);
                if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                    //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                    //xRecipe.bmpOrg.Dispose();
                    //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                    //b1.Dispose();
                    DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                    b1.Dispose();
                    DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = freeImageBitmap.ToBitmap();

                    DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                }
                else
                {
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                }

                //switch (xTabIndex)
                //{
                //    case 0:
                //        if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                //        {
                //            xRecipe.bmpOrg.Dispose();
                //            xRecipe.bmpOrg = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                //            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                //            //xRecipe.bmpOrg.Dispose();
                //            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                //            //b1.Dispose();
                //            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                //        {
                //            Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                //            xRecipe.bmpOrg.Dispose();
                //            xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                //            b1.Dispose();
                //            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                //        {
                //            xRecipe.bmpOrg.Dispose();
                //            xRecipe.bmpOrg = freeImageBitmap.ToBitmap();

                //            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                //        }
                //        else
                //        {
                //            JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                //        }
                //        break;
                //    case 1:
                //        if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                //        {
                //            xRecipe.bmpOrgNoTray.Dispose();
                //            xRecipe.bmpOrgNoTray = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                //            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                //            //xRecipe.bmpOrg.Dispose();
                //            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                //            //b1.Dispose();
                //            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                //        {
                //            Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                //            xRecipe.bmpOrgNoTray.Dispose();
                //            xRecipe.bmpOrgNoTray = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                //            b1.Dispose();
                //            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                //        {
                //            xRecipe.bmpOrgNoTray.Dispose();
                //            xRecipe.bmpOrgNoTray = freeImageBitmap.ToBitmap();

                //            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                //        }
                //        else
                //        {
                //            JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载图片格式不支持！");
                //        }
                //        break;
                //}

                freeImageBitmap.Dispose();
#endif

                var oldCursor = GaUtil.SetCursor(this, Cursors.WaitCursor);

                try
                {
                    // 統一由 GaImageUtil.LoadBigImage 載入大圖檔 (自動轉成 8-bbp, 而且速度比 FreeBitmap 快)
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = GaImageUtil.LoadBigImage(filename);
                    DS1.ReplaceDisplayImage(xRecipe.bmpOrgFly);
                }
                catch (Exception ex)
                {
                    VsMessageBox.Warning(ex.Message);
                }

                GaUtil.SetCursor(this, oldCursor);
            }
        }
        
        bool makeSureNotLiveMode()
        {
            if (_isFlyRunning)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");
                return false;
            }
            return true;
        }

        void OpenMotorJogWindow()
        {
            using (var dlg = new FormMotors_FlyCam())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog();
            }
        }

        #region JzDisplay_FUNCTIONS
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
            DS2.ImageViewer.AddInteractor(_cviCross);
            _cviCross.Visible = true;
            //DS2.CaptureAction += DS_CaptureAction2;
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
            if (!_bSelectRegion)
                return;
            BoundRect(ref rectf, xRecipe.bmpOrgFly.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                Bitmap bmpx = new Bitmap(xRecipe.bmpOrgFly);
                Graphics g = Graphics.FromImage(bmpx);

                xRecipe.xRectRegionPrintFly = rectf;
                xRecipe.bmpprintFlytemplate = xRecipe.bmpOrgFly.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                xRecipe.SavePrintFlyTemplate();

                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectf });
                g.Dispose();
                DS1.ReplaceDisplayImage(bmpx);
                bmpx.Dispose();

                //xRecipe.SaveBase();
            }
            _bSelectRegion = false;
        }
        #endregion

        private void updateColor(Control c, bool active)
        {
            if (c == null)
                return;

            if (active)
            {
                c.BackColor = Color.Pink;
            }
            else if (c.Tag is Color color)
            {
                c.BackColor = color;
            }
            else
            {
                c.BackColor = Color.LightGray;
            }
        }

        #region TOOLS
        //----------------------------------------------------------------------------
        // 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        //----------------------------------------------------------------------------
        void aoiDecodeCode(Bitmap srcBmp, out string text)
        {
            if (srcBmp == null)
            {
                text = "";
                return;
            }

            //>>> xRecipe.mvd2DReader.Run(xRecipe.bmpcodetemplate,
            //>>>       new RectangleF(0, 0, xRecipe.bmpcodetemplate.Width, xRecipe.bmpcodetemplate.Height));

            var aoiTool = xRecipe.fly2DReader;
            var roi = new RectangleF(0, 0, srcBmp.Width, srcBmp.Height);

            aoiTool.Run(srcBmp, roi);

            var decodeInfo = aoiTool.DCodeInfo;
            text = decodeInfo != null ? decodeInfo.Content : "";
        }

        void getCamDevParaAndUpdateUI()
        {
            lblExpo.Text = $"{IxFlyAreaCam.GetExposure()} us";
            lblGain.Text = $"{IxFlyAreaCam.GetGain()} dB";
        }
        void applyCameraExposureAndGain(bool bUpdateUI = false)
        {
            try
            {
                if (FlyParaClass.Instance.GetCameraExpoAndGain(out float expo, out float gain))
                {
                    CommonLogClass.Instance.LogMessage($"設定 曝光時間= {expo} (us), 增益= {gain:0.0} (db)");
                    IxFlyAreaCam.SetExposure(expo);
                    IxFlyAreaCam.SetGain(gain);
                    if (bUpdateUI)
                        getCamDevParaAndUpdateUI();
                }
            }
            catch (Exception ex)
            {
                CommonLogClass.Instance.LogError($"無法設定 曝光時間與增益!");
            }
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
        private Bitmap ConvertFromMONO(byte[] rgbaData, int width, int height)
        {
            var pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                pixelFormat);

            IntPtr intPtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(rgbaData, 0, intPtr, rgbaData.Length);
            bitmap.UnlockBits(bitmapData);

            System.Drawing.Imaging.ColorPalette tempPalette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
            }
            bitmap.Palette = tempPalette;

            return bitmap;
        }

        #endregion
    }
}
