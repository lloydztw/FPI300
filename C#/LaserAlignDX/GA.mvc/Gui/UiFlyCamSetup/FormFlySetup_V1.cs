#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-17 重整萬子的飛拍相機控制 以防止 GUI thread 的時間被吃掉造成卡機 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.GA.FormSpace.FPI30Form;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Traveller106;
using VisionDesigner.BlobFind;
using VsCommon.ControlSpace.MachineSpace;

using Timer = System.Windows.Forms.Timer;
using VsLight = JetEazy.ControlSpace.PLCSpace.VsLight;

namespace LaserAlignDX.FormSpace
{
    public partial class FormFlySetup : Form
    {
        #region INTERACTORS
        CviFlySetupOverlay _flyOverlay;
        CviFlySetupOverlay _resultOverlay;
        #endregion

        #region GLOBAL_MESS
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        #endregion

        #region GLOBAL_MESS_MACHINE
        protected MainFPIX3MachineClass MACHINEx3
        {
            get { return (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE; }
        }
        protected VsLight LightCtrl
        {
            get
            {
                var ligthCtrl = (MACHINEx3.LightCollection.Length > 1) ? MACHINEx3.LightCollection[1] : null;
                return ligthCtrl;
            }
        }
        #endregion

        #region GLOBAL_MESS_FLY_CAMERA
        IxLineScanCam IxFlyAreaCam
        {
            get { return Universal.IxFlyAreaCam; }
        }
        #endregion

        #region GUI_LINKS

        //btnGetLocalImage = button7;
        //btnOK = button1;
        //btnCancel = button2;
        //btnSelectRegion = button3;
        //btnOpenFly = button4;
        //btnLightTrigger = button5;
        //btnSpecialCal = button6;
        Button btnGetLocalImage => buttonFly3;
        //Button btnOK => btnOK;
        //Button btnCancel => btnCancel;
        Button btnSelectRegion => buttonFly4;
        Button btnOpenFly => buttonFly1;
        Button btnLightTrigger => buttonFly2;
        Button btnSpecialCalc => buttonFly6;
        Button btnCodetest => buttonFly5;
        RichTextBox rtbCodeContent => richTextBox1;

        FlyOffsetUI flyOffsetUI => flyOffsetUI1;
        FlyOffsetUI flyOffset2UI => flyOffsetUI2;
        Label lblExpo => label2;
        Label lblGain => label3;
        
        Button btnOpenMotorJogWindow => buttonFly9;

        TabControl tabMainPages => tabControl2;
        #endregion

        #region WINDOW_TIMER
        /// <summary>
        /// 如果是自己 new 的 Timer 記得退出前要調用 Dispose() !!!
        /// </summary>
        Timer xTimer = null;
        #endregion

        #region FLY_CAMERA_RUNTIME_STATE_DATA
        volatile bool _isFlyCameraLiveMode = false;
        volatile bool _isFlyCameraOneshotCaptureMode = false;
        #endregion

        #region PRIVATE_FLAGS
        bool _bSelectRegion = false;
        bool _isPropertyModified = false;
        #endregion

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

            //Load += (s, e) => QMSG.Dump(this);
            Load += (s, e) => QMSG.Translate(this);
            this.WindowState = FormWindowState.Maximized;
        }

        #region WINDOW_EVENT_HANDLERS
        private void FrmFlySetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!MakeSureNotLiveMode())
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

            showFlyImage(xRecipe.bmpOrgFly);

            IxFlyAreaCam.LineTriggerAction += IxFlyAreaCam_LineTriggerAction;
            
            try
            {
                EzPropertyGridTranslator.Register(FlyParaClass.Instance);
                propertyGrid1.SelectedObject = FlyParaClass.Instance;
            }
            catch (Exception ex)
            {
                string errMsg = "Translate(FlyParaClass) Error";
                errMsg += "\n\r\n\r" + ex.Message;
                errMsg += "\n\n" + ex.StackTrace;
                QMessageBox.Show(errMsg, GlobalConfig.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;

            this.Text = "Fly Camera Recipe Editor";

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
        private void BtnSpecialCal_Click(object sender, EventArgs e)
        {
            RectangleF rectf = xRecipe.xRectRegionPrintFly;
            BoundRect(ref rectf, xRecipe.bmpOrgFly.Size);

            var flyAoi = _sysModel?.AoiModel?.GetFlyCameraAoi();
            if (flyAoi != null && rectf.Width > 1 && rectf.Height > 1)
            {
                using (Bitmap bmpG = xRecipe.bmpOrgFly.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
                {
                    bool bOK = flyAoi.CheckSpecialAngle(bmpG, out List<CBlobInfo> list, out float angle, out System.Drawing.PointF Center);

                    if (bOK)
                    {
                        _resultOverlay.ClearResults();
                        showImage(DS2, xRecipe.bmpOrgFly, "Angle Result");

                        foreach (CBlobInfo cBlob in list)
                        {
                            // Use the detected rotated rectangle and the existing MVD conversion.
                            // Blob coordinates are local to bmpG; move the center to the full image.
                            var box = GaMvdExt.ToBox2D(cBlob.BoxInfo);
                            if (box == null)
                                continue;
                            box.SetCenter(cBlob.BoxInfo.CenterX + rectf.X,
                                cBlob.BoxInfo.CenterY + rectf.Y);
                            _resultOverlay.AddResultBox(box);
                        }

                        update_Display(false);

                        //MessageBox.Show($"计算角度:{angle}");
                        VsMessageBox.Info($"{QMSG.Text(Prompts.Info_FlyCam_Angle)} : {angle:0.00}");
                    }
                }
            }
        }
        private void BtnLightTrigger_Click(object sender, EventArgs e)
        {
            SnapshotOneFlyCameraFrame();
        }
        private void BtnOpenFly_Click(object sender, EventArgs e)
        {
            EnableFlyCameraLiveMode(!_isFlyCameraLiveMode);
        }
        private void XTimer_Tick(object sender, EventArgs e)
        {
            updateGuiStatus();
        }
        private void BtnSelectRegion_Click(object sender, EventArgs e)
        {
            _bSelectRegion = !_bSelectRegion;
            _flyOverlay.SelectionEnabled = _bSelectRegion;
            updateGuiStatus();
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //if(m_FlyRunning)
            //{
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");

            if (!MakeSureNotLiveMode())
                return;

            xRecipe.Load();
            applyCameraExposureAndGain();
            this.DialogResult = DialogResult.Cancel;
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            //if (m_FlyRunning)
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");

            if (!MakeSureNotLiveMode())
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
                    showFlyImage(xRecipe.bmpOrgFly);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                    b1.Dispose();
                    showFlyImage(xRecipe.bmpOrgFly);
                }
                else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    xRecipe.bmpOrgFly.Dispose();
                    xRecipe.bmpOrgFly = freeImageBitmap.ToBitmap();

                    showFlyImage(xRecipe.bmpOrgFly);
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
                //            showFlyImage(xRecipe.bmpOrg);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                //        {
                //            Bitmap b1 = Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
                //            xRecipe.bmpOrg.Dispose();
                //            xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                //            b1.Dispose();
                //            showFlyImage(xRecipe.bmpOrg);
                //        }
                //        else if (freeImageBitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                //        {
                //            xRecipe.bmpOrg.Dispose();
                //            xRecipe.bmpOrg = freeImageBitmap.ToBitmap();

                //            showFlyImage(xRecipe.bmpOrg);
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
                    showFlyImage(xRecipe.bmpOrgFly);
                }
                catch (Exception ex)
                {
                    VsMessageBox.Warning(ex.Message);
                }

                GaUtil.SetCursor(this, oldCursor);
            }
        }
        #endregion

        #region FLY_CAMERA_EVENT_HANDLER
        private void IxFlyAreaCam_LineTriggerAction(JetEazy.CCDSpace.CameraFrame cameraFrame, IntPtr pBuffer)
        {
            if (!_isFlyCameraLiveMode && !_isFlyCameraOneshotCaptureMode)
                return;

            if (cameraFrame.Format != PixelFormat.Format8bppIndexed)
                return;

            //(0) 將 pBuffer 轉換成 byte[]
            byte[] bmpBytes = new byte[cameraFrame.uBytes];
            Marshal.Copy(pBuffer, bmpBytes, 0, bmpBytes.Length);
            int iw = cameraFrame.iWidth;
            int ih = cameraFrame.iHeight;

            //(1) 單張取圖 模式
            if (_isFlyCameraOneshotCaptureMode)
            {
                xRecipe.bmpOrgFly?.Dispose();
                xRecipe.bmpOrgFly = ConvertFromMONO(bmpBytes, iw, ih);
                //showFlyImage(xRecipe.bmpOrgFly);
                Invoke((Action<Bitmap>)showFlyImage, xRecipe.bmpOrgFly);
                _isFlyCameraOneshotCaptureMode = false;
                return;
            }

            //(2) 連續實時影像 模式
            if (_isFlyCameraLiveMode)
            {
                using (Bitmap bmpNew = ConvertFromMONO(bmpBytes, iw, ih))
                {
                    Invoke((Action<Bitmap>)showFlyImage, bmpNew);
                }

                if (_isFlyCameraLiveMode)
                {
                    // 加入 sleep 以防止 GUI thread 的時間被吃掉造成卡機 !!!
                    System.Threading.Thread.Sleep(10);
                    triggerOneshotLight();
                }
            }
        }
        #endregion

        #region PRIVATE_FLY_CAMERA_HELPER_FUNCTIONS
        private bool IsFlyCameraBusy()
        {
            return _isFlyCameraLiveMode || _isFlyCameraOneshotCaptureMode;
        }
        private void SnapshotOneFlyCameraFrame()
        {
            if (IsFlyCameraBusy())
                return;

            _isFlyCameraOneshotCaptureMode = true;
            triggerOneshotLight();
            
            updateGuiStatus();
        }
        private void EnableFlyCameraLiveMode(bool enabled)
        {
            if (_isFlyCameraOneshotCaptureMode)
                return;

            if (enabled == _isFlyCameraLiveMode)
                return;

            if (enabled)
            {
                _isFlyCameraLiveMode = true;
                triggerOneshotLight();
            }
            else
            {
                _isFlyCameraLiveMode = false;
            }

            updateGuiStatus();
        }
        private void triggerOneshotLight()
        {
            LightCtrl?.Trigger();
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        bool MakeSureNotLiveMode()
        {
            if (_isFlyCameraLiveMode)
            {
                //JetEazy.BasicSpace.VsMSG.Instance.Warning($"请先停止实时画面!");
                VsMessageBox.Warning(QMSG.Text(Prompts.Waring_Please_Stop_Live_Mode));
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
        void updateGuiStatus()
        {
            if (_isFlyCameraLiveMode)
                tabMainPages.SelectedIndex = 0;

            bool isBusy = _isFlyCameraOneshotCaptureMode || _isFlyCameraLiveMode;
            if (isBusy && _flyOverlay != null)
            {
                _bSelectRegion = false;
                _flyOverlay.SelectionEnabled = false;
            }
            btnLightTrigger.Enabled = !isBusy;
            btnGetLocalImage.Enabled = !isBusy;
            btnSelectRegion.Enabled = !isBusy;
            btnSpecialCalc.Enabled = !isBusy;
            btnCodetest.Enabled = !isBusy;
            btnOpenMotorJogWindow.Enabled = !isBusy;

            updateColor(btnSelectRegion, _bSelectRegion);
            updateColor(btnOpenFly, _isFlyCameraLiveMode);
        }
        void updateColor(Control c, bool active)
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
        #endregion

        #region IMAGE_DISPLAY
        void init_Display()
        {
            // Hidden controls do not occupy their AutoSize title rows.
            DS1.TitleBar.Visible = false;
            DS2.TitleBar.Visible = false;

            _flyOverlay = new CviFlySetupOverlay(DS1);
            _flyOverlay.RegionSelected += DS_CaptureAction;
            _resultOverlay = new CviFlySetupOverlay(DS2);
        }
        void disposeDisplayInteractions()
        {
            _flyOverlay?.Dispose();
            _resultOverlay?.Dispose();
            _flyOverlay = null;
            _resultOverlay = null;
        }
        void showFlyImage(Bitmap image)
        {
            showImage(DS1, image, "Fly Camera");
        }
        static void showImage(JezTransImageViewPanel panel, Bitmap image, string title)
        {
            bool firstImage = panel.Image == null;
            // UpdateImage clones into an owned Mat; the caller retains the Bitmap.
            panel.UpdateImage(image, title, false);
            if (firstImage && panel.Image != null)
                panel.MatViewer.RebuildViewport();
            panel.MatViewer.Invalidate();
        }
        void update_Display(bool eChangeToDefault = true)
        {
            if (eChangeToDefault)
            {
                DS1.MatViewer.RebuildViewport();
                DS2.MatViewer.RebuildViewport();
            }
            DS1.MatViewer.Invalidate();
            DS2.MatViewer.Invalidate();
        }
        private void DS_CaptureAction(RectangleF rectf)
        {
            if (!_bSelectRegion)
                return;
            _bSelectRegion = false;
            _flyOverlay.SelectionEnabled = false;
            updateGuiStatus();
            if (xRecipe.bmpOrgFly == null)
                return;

            // Clamp in image coordinates, including drags outside the image.
            rectf = RectangleF.Intersect(rectf, new RectangleF(PointF.Empty, xRecipe.bmpOrgFly.Size));
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                xRecipe.xRectRegionPrintFly = rectf;
                xRecipe.bmpprintFlytemplate = xRecipe.bmpOrgFly.Clone(rectf, PixelFormat.Format8bppIndexed);
                xRecipe.SaveTemplate("FLY");

                using (var bitmap = new Bitmap(xRecipe.bmpOrgFly))
                using (var graphics = Graphics.FromImage(bitmap))
                using (var pen = new Pen(Color.Lime, 3))
                {
                    graphics.DrawRectangle(pen, rectf.X, rectf.Y, rectf.Width, rectf.Height);
                    showFlyImage(bitmap);
                }
            }
        }
        #endregion

        #region AOI_AND_CAMERA_FUNCTIONS
        //----------------------------------------------------------------------------
        // 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        //----------------------------------------------------------------------------
        void aoiDecodeCode(Bitmap srcBmp, out string text)
        {
#if (OPT_OLD_CODE)
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
#endif

            text = _sysModel?.AoiModel?.GetAoiQrDecoder()?.TryDecode(srcBmp);
            if (text == null)
                text = "";
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
        #endregion

        #region UTIL_FUNCTIONS
        void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        Bitmap ConvertFromMONO(byte[] rgbaData, int width, int height)
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
