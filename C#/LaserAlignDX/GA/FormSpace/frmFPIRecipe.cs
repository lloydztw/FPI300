using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx.Interactors;
using JetEazy.Interface;
using JetEazy.Match;
using JetEazy.Utils;
using JzDisplay;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using MoveGraphLibrary;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Traveller106;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmFPIRecipe : Form
    {
        #region PRIVATE_MEMBERS
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
        #endregion

        #region PRIVATE_GUI_MEMBERS
        int xTabIndex
        {
            get
            {
                return tabControl2.SelectedIndex;
            }
        }
        Timer xTimer = null;
        #endregion

        #region GUI_LINKS
        Button btnOK => button1;
        Button btnCancel => button2;
        Button btnLoadImage => button7;
        Button btnSelectRegion => button3;
        //Button btnRegionBaseForm;
        Button btnGetRealImage => button5;
        Button btnCreateRegions => button9;
        //Button btnSelectTemplate;
        Button btnSelectTemplateForm => button6;
        Button btnNoTrayTemplateForm => button4;
        Button btnFlyTemplateForm => button10;
        Button btnControlLight => button8;
        Button btnSaveImage => button11;
        #endregion


        public frmFPIRecipe()
        {
            InitializeComponent();
            this.Load += FrmFPIRecipe_Load;
            this.FormClosed += FrmFPIRecipe_FormClosed;
            this.SizeChanged += FrmFPIRecipe_SizeChanged;

            LtAoiFactory.OnLineScanRequested += LtAoi_OnLineScanRequested;
        }



        private void FrmFPIRecipe_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        private void FrmFPIRecipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 注意: xTimer 不用之後, 必須調用 Dispose() !!!
            xTimer?.Dispose();
            xTimer = null;

            // 卸載 EventHandler
            LtAoiFactory.OnLineScanRequested -= LtAoi_OnLineScanRequested;
        }

        private void FrmFPIRecipe_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            //btnOK = button1;
            //btnCancel = button2;
            //btnLoadImage = button7;
            //btnSelectRegion = button3;
            //btnNoTrayTemplateForm = button4;
            //btnCreateRegions = button9;
            //btnGetRealImage = button5;
            //btnSelectTemplateForm = button6;
            //btnControlLight = button8;
            //btnFlyTemplateForm = button10;
            //btnSaveImage = button11;

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
            

            // 注意: xTimer 不用之後, 必須調用 Dispose() !!!
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



        //frmNoTrayX3 frmNoTrayX3x = null;
        private void BtnNoTrayTemplateForm_Click(object sender, EventArgs e)
        {
            openEmptyTrayInspectorTool();

            //frmNoTrayX3x = new frmNoTrayX3();
            //frmNoTrayX3x.ShowDialog();
            //frmNoTrayX3x.Dispose();
            //frmNoTrayX3x = null;
            //using (var frmNoTrayX3x = new frmNoTrayX3())
            //{
            //    frmNoTrayX3x.ShowDialog();
            //}
        }

        private void BtnSaveImage_Click(object sender, EventArgs e)
        {
            string _filepath = JetEazy.BasicSpace.JzToolsClass.SaveFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_filepath))
            {
                using (IEzImage ezImage = new EzFreeBitmap(xRecipe.bmpOrg, true))
                {
                    ezImage.Save(_filepath);
                }
                //IEzImage ezImage = new EzFreeBitmap(xRecipe.bmpOrg, true);
                //ezImage.Save(_filepath);
                //ezImage.Dispose();
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("图片保存完成.路径:")}{Environment.NewLine + _filepath}", false);
            }
        }

        //>>> 沒有必要的話, 不需要將 frmFlySetup 提升為 member data !!!
        //>>> frmFlySetup frmFlySetupX = null;// new frmFlySetup();

        private void BtnFlyTemplateForm_Click(object sender, EventArgs e)
        {
            //>>> 沒有必要的話, 不需要將 frmFlySetup 提升為 member data !!!

            using (var frmFlySetupX = new frmFlySetup())
            {
                frmFlySetupX.ShowDialog();
                //frmFlySetupX.Dispose();
                //frmFlySetupX = null;
            }
        }

        //>>> 沒有必要的話, 不需要將 frmLightControl 提升為 member data !!!
        //>>> frmLightControl frmLight = null;

        private void BtnControlLight_Click(object sender, EventArgs e)
        {
            //>>> 沒有必要的話, 不需要將 frmLightControl 提升為 member data !!!

            using (var frmLight = new frmLightControl())
            {
                frmLight.ShowDialog();
                //frmLight.Dispose();
                //frmLight = null;
            }
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

        //>>> 沒有必要的話, 不需要將 FrmTemplateX3 提升為 member data !!!
        //>>> frmTemplateX3 FrmTemplateX3 = null;

        void showTemplateX3DialogWindow()
        {
            using (var frm = new frmTemplateX3())
            {
                frm.ShowDialog();
                //FrmTemplateX3.Dispose();
                //FrmTemplateX3 = null;
            }

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
        //private void BtnRegionBaseForm_Click(object sender, EventArgs e)
        //{
        //    //showBaseX2DialogWindow();
        //}

        private void BtnSelectRegion_Click(object sender, EventArgs e)
        {
            //xRegionNameCurrent = RegionName.BASE0;
            bSelectRegion = !bSelectRegion;
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            string fileName = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");

            if (!string.IsNullOrEmpty(fileName))
            {
#if (OPT_LEGACY)
                FreeImageBitmap freeImageBitmap = new FreeImageBitmap(fileName);

                switch (xTabIndex)
                {
                    case 0:
                        if (freeImageBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            xRecipe.bmpOrg.Dispose();
                            xRecipe.bmpOrg = EzMvdImageConvertor.Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                            //xRecipe.bmpOrg.Dispose();
                            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            //b1.Dispose();
                            DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                        {
                            Bitmap b1 = EzMvdImageConvertor.Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
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
                            xRecipe.bmpOrgNoTray = EzMvdImageConvertor.Convert32bppTo8bpp(freeImageBitmap.ToBitmap());

                            //Bitmap b1 = Convert32bppTo8bpp(freeImageBitmap.ToBitmap());
                            //xRecipe.bmpOrg.Dispose();
                            //xRecipe.bmpOrg = b1.Clone(new Rectangle(0, 0, b1.Width, b1.Height), PixelFormat.Format8bppIndexed);
                            //b1.Dispose();
                            DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
                        }
                        else if (freeImageBitmap.PixelFormat == PixelFormat.Format24bppRgb)
                        {
                            Bitmap b1 = EzMvdImageConvertor.Convert24bppTo8bpp(freeImageBitmap.ToBitmap());
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
#endif
                switch (xTabIndex)
                {
                    case 0: loadBigImage_for_Measurement(fileName); break;
                    case 1: loadBigImage_for_EmptyTray(fileName); break;
                }
            }
        }

        #region BIG_IMAGE_LOADING_FUNCTIONS
        void loadBigImage_for_Measurement(string fileName)
        {
            try
            {
                var newBmp = GaImageUtil.LoadBigImage(fileName);
                if (newBmp != null)
                {
                    xRecipe.bmpOrg?.Dispose();
                    xRecipe.bmpOrg = newBmp;
                    DS1.ReplaceDisplayImage(newBmp);
                }
            }
            catch (Exception ex)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning(ex.Message);
            }
        }
        void loadBigImage_for_EmptyTray(string fileName)
        {
            try
            {
                var newBmp = GaImageUtil.LoadBigImage(fileName);
                if (newBmp != null)
                {
                    xRecipe.bmpOrgNoTray?.Dispose();
                    xRecipe.bmpOrgNoTray = newBmp;
                    DS2.ReplaceDisplayImage(newBmp);
                }
            }
            catch (Exception ex)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning(ex.Message);
            }
        }
        #endregion

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

        private void LtAoi_OnLineScanRequested(object sender, EventArgs e)
        {
            new Action(() =>
            {
                //System.Threading.Thread.Sleep(2000);
                LtAoiFactory.PushBitmap(xRecipe.bmpOrg, "[參數] bmpOrg");
            }).BeginInvoke(null, null);
        }

        void openEmptyTrayInspectorTool()
        {
            var frmOwner = FindForm();
            var tool = LtAoiFactory.OpenEmptyTrayInspectorTool(frmOwner, bmpToShow: xRecipe.bmpOrg);
        }

        void init_Display()
        {
            //DS = dispUI1;
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS1.CaptureAction += DS_CaptureAction;
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;
            CviCross cviCross1 = new CviCross(Color.Yellow);    // 建議: 每一個 Viewer 擁有 獨立的 CviCross
            DS1.ImageViewer.AddInteractor(cviCross1);
            cviCross1.Visible = true;

            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS2.CaptureAction += DS_CaptureAction2;
            CviCross cviCross2 = new CviCross(Color.Yellow);    // 建議: 每一個 Viewer 擁有 獨立的 CviCross
            DS2.ImageViewer.AddInteractor(cviCross2);
            cviCross2.Visible = true;
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
            xRecipe.CreateViews(writeback: true);
            propertyGrid1.SelectedObject = RecipeParaGridClass.Instance;

            DS1.ClearStaticMover();
            DS2.ClearStaticMover();
            xMovers.Clear();

            int i = 0;
            while (i < xRecipe.xRegionCells.Count)
            {
                var cell = xRecipe.xRegionCells[i];
                //EzBloc bloc = grid.Get(cell.CellRow, cell.CellCol);
                //if (bloc != null)
                //{
                //    JetEazy.Qcvt.SetCenter(ref cell.viewRectF, bloc.CenterX, bloc.CenterY);
                //}
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), cell.viewRectF);
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
