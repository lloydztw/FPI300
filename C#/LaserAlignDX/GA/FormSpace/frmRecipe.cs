using AForge.Imaging.Filters;
using AForge.Math;
using Common.RecipeSpace;
using Eazy_Project_III;
using Eazy_Project_III.OPSpace;
using FreeImageAPI;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using JzDisplay;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.FormSpace.WX;
using MoveGraphLibrary;
using NeedleX.UISpace.UIMVC.Controler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.OPSpace;
using VsCommon.ControlSpace.MachineSpace;
using WorldOfMoveableObjects;

namespace NeedleX.FormSpace
{
    public partial class frmRecipe : Form
    {
        #region DEFINE WINDOWS MEMBERS

        XinliFindLineClass xinliFindLineClass = new XinliFindLineClass();
        LineClass lineClassxFindLine = null;
        protected RecipeMiniX6Class myRecipe
        {
            get { return RecipeMiniX6Class.Instance; }
        }

        Mover myMover = new Mover();
        //Bitmap bmpOperate = new Bitmap(1, 1);
        //Bitmap bmpOperateResult = new Bitmap(1, 1);

        //Mover myMover2 = new Mover();
        //Mover myMover3 = new Mover();

        Mover myMovers = new Mover();

        protected MiniX6MachineClass MACHINE
        {
            get { return (MiniX6MachineClass)Universal.MACHINECollection.MACHINE; }
        }

        CtlPosClass[] ctlPosClasses = new CtlPosClass[2];
        Bitmap[] bmpOperate;
        Timer mTimer = null;

        DataGridView dgv;

        bool IsNeedToChange = false;
        bool IsStartRoiRegion = false;

        bool IsStartMark0RoiRegion = false;
        bool IsStartMark1RoiRegion = false;
        bool IsStartMarkLineRoiRegion = false;

        bool IsML1Region = false;
        bool IsML2Region = false;
        //bool IsMLFitRegion = false;

        bool IsML1RegionOK = false;
        bool IsML2RegionOK = false;

        bool IsLeft = false;
        bool IsRight = false;
        bool IsTop = false;
        bool IsBottom = false;

        bool IsCheckDir = false;

        bool IsFourMark1 = false;
        bool IsFourMark2 = false;
        bool IsFourMark3 = false;
        bool IsFourMark4 = false;

        bool IsCheckWholeDir = false;
        bool IsCheckUseBarcode = false;

        Button btnOK;
        Button btnCancel;
        Button btnHandAxis;
        ComboBox cboCamList;
        NumericUpDown numExpo;
        NumericUpDown numGain;

        Button btnCamOneshot;
        Button btnCamContinue;
        Label lblCamName;
        Button btnLocalBmpLoad;
        Button btnAutoCreateRowCol;
        Button btnChangeStatic;
        Button btnRoiPattern;

        Button btnMark0;
        Button btnMark1;
        Button btnMarkLine;

        Button btnML1;
        Button btnML2;
        Button btnMLFit;

        Button btnFourPara;

        Button btnFitLeft;
        Button btnFitRight;
        Button btnFitTop;
        Button btnFitBottom;

        Button btnCheckDir;

        Button btnFourMark1;
        Button btnFourMark2;
        Button btnFourMark3;
        Button btnFourMark4;

        Button btnCheckWholeDir;

        Button btnFourMarkPara;
        Button btnReadBarcode;


        #endregion

        public frmRecipe()
        {
            InitializeComponent();
            this.Load += FrmRecipe_Load;
            this.FormClosed += FrmRecipe_FormClosed;
        }

        private void FrmRecipe_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmHandle.Instance.Hide();
        }

        #region WINDOWS EVENTS

        private void FrmRecipe_Load(object sender, EventArgs e)
        {
            //bmpOperate = new Bitmap[CameraConfig.Instance.COUNT];
            //for (int i = 0; i < CameraConfig.Instance.COUNT; i++)
            //{
            //    bmpOperate[i] = new Bitmap(1, 1);
            //}

            init_Display();
            update_Display();

            ctlPosClasses[0] = new CtlPosClass(posUI1);
            ctlPosClasses[1] = new CtlPosClass(posUI2);

            dgv = dataGridView1;

            btnOK = button1;
            btnCancel = button2;
            btnHandAxis = button4;
            cboCamList = comboBox1;
            numExpo = numericUpDown1;
            numGain = numericUpDown2;
            btnCamOneshot = button5;
            btnCamContinue = button6;
            lblCamName = label4;
            btnLocalBmpLoad = button7;
            btnAutoCreateRowCol = button8;
            btnChangeStatic = button9;
            btnRoiPattern = button10;
            btnMark0 = button11;
            btnMark1 = button12;
            btnMarkLine = button13;
            btnML1 = button14;
            btnML2 = button15;
            btnMLFit = button16;
            btnFourPara = button17;

            btnFitLeft = button21;
            btnFitRight = button20;
            btnFitTop = button19;
            btnFitBottom = button18;
            btnCheckDir = button22;

            btnFourMark4 = button23;
            btnFourMark3 = button24;
            btnFourMark2 = button25;
            btnFourMark1 = button26;
            btnCheckWholeDir = button27;
            btnFourMarkPara = button28;
            btnReadBarcode = button29;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnHandAxis.Click += BtnHandAxis_Click;
            btnCamOneshot.Click += BtnCamOneshot_Click;
            btnCamContinue.Click += BtnCamContinue_Click;
            btnLocalBmpLoad.Click += BtnLocalBmpLoad_Click;
            btnAutoCreateRowCol.Click += BtnAutoCreateRowCol_Click;
            btnChangeStatic.Click += BtnChangeStatic_Click;
            btnRoiPattern.Click += BtnRoiPattern_Click;

            btnMark0.Click += BtnMark0_Click;
            btnMark1.Click += BtnMark1_Click;
            btnMarkLine.Click += BtnMarkLine_Click;

            btnML1.Click += BtnML1_Click;
            btnML2.Click += BtnML2_Click;
            btnMLFit.Click += BtnMLFit_Click;

            btnFitLeft.Click += BtnFitLeft_Click;
            btnFitRight.Click += BtnFitRight_Click;
            btnFitTop.Click += BtnFitTop_Click;
            btnFitBottom.Click += BtnFitBottom_Click;
            btnCheckDir.Click += BtnCheckDir_Click;

            btnFourPara.Click += BtnFourPara_Click;

            btnFourMark1.Click += BtnFourMark1_Click;
            btnFourMark2.Click += BtnFourMark2_Click;
            btnFourMark3.Click += BtnFourMark3_Click;
            btnFourMark4.Click += BtnFourMark4_Click;

            btnCheckWholeDir.Click += BtnCheckWholeDir_Click;
            btnFourMarkPara.Click += BtnFourMarkPara_Click;
            btnReadBarcode.Click += BtnReadBarcode_Click;

            numExpo.ValueChanged += NumExpo_ValueChanged;
            numGain.ValueChanged += NumGain_ValueChanged;

            pgNormal.SelectedObject = myRecipe.XPropsRcp;
            pgNormal.PropertyValueChanged += PgNormal_PropertyValueChanged;

            //pictureBox1.Image = myRecipe.bmpORGPattern;
            DS2.ReplaceDisplayImage(myRecipe.bmpORGPattern);

            SizeChanged += FrmRecipe_SizeChanged;

            //ctlPosClasses[0].Init(MACHINE, CoarsePositioningClass.Instance.sCoarsePosList);
            //ctlPosClasses[1].Init(MACHINE, ModelPositioningClass.Instance.sModelPosList);

            init_CboList();

            FillDisplay();
            FillDisplay(true);
            _autoRowCol();
            //_autoRowColInit();

            mTimer = new Timer();
            mTimer.Interval = 50;
            mTimer.Enabled = true;
            mTimer.Tick += MTimer_Tick;

            //tabControl1.Controls.RemoveAt(0);
            tabControl1.Controls.RemoveAt(2);
            tabControl1.Controls.RemoveAt(2);
            //tabControl2.Controls.RemoveAt(1);
            btnCamOneshot.Visible = false;
            btnCamContinue.Visible = false;
            label4.Visible = false;
            btnHandAxis.Visible = false;

            btnChangeStatic.Visible = false;

            switch (Universal.FACTORYNAME)
            {
                case FactoryName.DONGGUAN:
                case Eazy_Project_III.FactoryName.DAGUI:
                    btnMark0.Visible = false;
                    btnMark1.Visible = false;
                    break;
            }

            this.Text = "参数设定窗口";
            this.FormBorderStyle = FormBorderStyle.Sizable;

            LanguageExClass.Instance.EnumControls(this);

#if OPT_LETIAN_AUTO_LAYOUT
            // To fit into my screen for debug.
#if DEBUG
            this.FormBorderStyle = FormBorderStyle.Sizable;
#endif
            this.WindowState = FormWindowState.Maximized;
#endif

        }

        private void BtnReadBarcode_Click(object sender, EventArgs e)
        {
            IsCheckUseBarcode = !IsCheckUseBarcode;
        }

        private void PgNormal_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //if (e.ChangedItem.PropertyDescriptor.Name == "LightValue")
            //{
            //    int _value = int.Parse(e.ChangedItem.Value.ToString());
            //    if (_value < 120 || _value > 255)
            //    {
            //        RecipeMiniX6Class.Instance.LightValue = 255;
            //        RecipeMiniX6Class.Instance.SaveIniSetup();
            //        VsMSG.Instance.Warning($"{ToChangeLanguage("超出设定范围")}[120,255]");
            //        pgNormal.SelectedObject = myRecipe.XPropsRcp;
            //        return;
            //    }
            //}
            //else
            //    RecipeMiniX6Class.Instance.SaveIniSetup();
            //pgNormal.SelectedObject = null;
            RecipeMiniX6Class.Instance.SaveIniSetup();
            //pgNormal.SelectedObject = myRecipe.XPropsRcp;
        }

        frmFourMark frmFourSetup = null;
        private void BtnFourMarkPara_Click(object sender, EventArgs e)
        {
            frmFourSetup = new frmFourMark();
            frmFourSetup.ShowDialog();
            frmFourSetup.Dispose();
            frmFourSetup = null;

            //using (var frm = new frmFourMark())
            //{
            //    frm.ShowDialog();
            //}
        }

        private void BtnCheckWholeDir_Click(object sender, EventArgs e)
        {
            IsCheckWholeDir = !IsCheckWholeDir;
        }

        private void BtnFourMark4_Click(object sender, EventArgs e)
        {
            IsFourMark4 = !IsFourMark4;
        }

        private void BtnFourMark3_Click(object sender, EventArgs e)
        {
            IsFourMark3 = !IsFourMark3;
        }

        private void BtnFourMark2_Click(object sender, EventArgs e)
        {
            IsFourMark2 = !IsFourMark2;
        }

        private void BtnFourMark1_Click(object sender, EventArgs e)
        {
            IsFourMark1 = !IsFourMark1;
        }

        private void BtnCheckDir_Click(object sender, EventArgs e)
        {
            IsCheckDir = !IsCheckDir;
        }

        private void BtnFitBottom_Click(object sender, EventArgs e)
        {
            //IsBottom = false;
            IsBottom = !IsBottom;
        }

        private void BtnFitTop_Click(object sender, EventArgs e)
        {
            //IsTop = false;
            IsTop = !IsTop;
        }

        private void BtnFitRight_Click(object sender, EventArgs e)
        {
            //IsRight = false;
            IsRight = !IsRight;
        }

        private void BtnFitLeft_Click(object sender, EventArgs e)
        {
            //IsLeft = false;
            IsLeft = !IsLeft;
        }

        private void BtnFourPara_Click(object sender, EventArgs e)
        {

        }

        LineClass lineClassx1 = null;
        LineClass lineClassx2 = null;
        private void BtnMLFit_Click(object sender, EventArgs e)
        {
            if (!IsML1RegionOK || lineClassx1 == null)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning("直线1未寻找！", true);
                return;
            }
            if (!IsML2RegionOK || lineClassx1 == null)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning("直线2未寻找！", true);
                return;
            }
            List<PointF> ptflines = new List<PointF>();
            ptflines.Add(lineClassx1.FirstPt);
            ptflines.Add(lineClassx1.SecondPt);
            ptflines.Add(lineClassx2.FirstPt);
            ptflines.Add(lineClassx2.SecondPt);

            LineClass fitline = getLineForPointF(ptflines.ToArray(), false, 1);
            Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
            Graphics g = Graphics.FromImage(bmpx);

            try
            {
                PointF p1 = fitline.FirstPt;// lineClass.GetPtFromY(0);
                PointF p2 = fitline.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                p1 = fitline.GetPtFromX(0);
                p2 = fitline.GetPtFromX(bmpx.Width);

                g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                myRecipe.MarkLineAngle = GetAngle(p1, p2);
            }
            catch (Exception ex)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("拟合直线失败")}{ex.Message}");
            }

            g.Dispose();
            DS2.ReplaceDisplayImage(bmpx);
            bmpx.Dispose();

        }

        private void BtnML2_Click(object sender, EventArgs e)
        {
            IsML2RegionOK = false;
            IsML2Region = !IsML2Region;
        }

        private void BtnML1_Click(object sender, EventArgs e)
        {
            IsML1RegionOK = false;
            IsML1Region = !IsML1Region;
        }

        private void BtnMarkLine_Click(object sender, EventArgs e)
        {
            IsStartMarkLineRoiRegion = !IsStartMarkLineRoiRegion;
        }

        private void BtnMark1_Click(object sender, EventArgs e)
        {
            IsStartMark1RoiRegion = !IsStartMark1RoiRegion;
        }

        private void BtnMark0_Click(object sender, EventArgs e)
        {
            IsStartMark0RoiRegion = !IsStartMark0RoiRegion;
        }

        private void BtnRoiPattern_Click(object sender, EventArgs e)
        {
            //IsStartRoiRegion = !IsStartRoiRegion;

            GraphicalObject grobj = myMover[0].Source;
            RectangleF rectf = (grobj as JzRectEAG).GetRect;

            myRecipe.bmpORGPattern.Dispose();
            Bitmap bmptemp = myRecipe.bmpORG.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            myRecipe.bmpORGPattern = preProcessImage(bmptemp);
            myRecipe.SavePattern();

            //pictureBox1.Image = myRecipe.bmpORGPattern;
            DS2.ReplaceDisplayImage(myRecipe.bmpORGPattern);
            bmptemp.Dispose();
        }

        private void FrmRecipe_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        bool isdy = true;
        private void BtnChangeStatic_Click(object sender, EventArgs e)
        {
            //DS.ClearStaticMover();
            //DS.ClearMover();
            if (isdy)
            {
                DS.SetStaticMover(myMovers);
                DS.SetMover(myMover);
                isdy = false;
            }
            else
            {
                DS.SetStaticMover(myMover);
                DS.SetMover(myMovers);
                isdy = true;
            }

            DS.RefreshDisplayShape();
            DS.MappingSelect();
        }

        private void BtnAutoCreateRowCol_Click(object sender, EventArgs e)
        {
            _autoRowCol();
        }

        private void BtnLocalBmpLoad_Click(object sender, EventArgs e)
        {
            string _filename = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_filename))
            {
                FreeImageBitmap freeImageBitmap = new FreeImageBitmap(_filename);
                myRecipe.bmpORG.Dispose();
                myRecipe.bmpORG = freeImageBitmap.ToBitmap();
                //Bitmap bmpinput = freeImageBitmap.ToBitmap();
                DS.ReplaceDisplayImage(myRecipe.bmpORG);
                freeImageBitmap.Dispose();
                //bmpinput.Dispose();
            }
        }

        private void MTimer_Tick(object sender, EventArgs e)
        {
            btnRoiPattern.BackColor = (IsStartRoiRegion ? Color.Red : Color.FromArgb(192, 255, 192));

            btnMark0.BackColor = (IsStartMark0RoiRegion ? Color.Red : Color.FromArgb(192, 255, 192));
            btnMark1.BackColor = (IsStartMark1RoiRegion ? Color.Red : Color.FromArgb(192, 255, 192));
            btnMarkLine.BackColor = (IsStartMarkLineRoiRegion ? Color.Red : Color.FromArgb(192, 255, 192));

            btnML1.BackColor = (IsML1Region ? Color.Red : Color.FromArgb(192, 255, 192));
            btnML2.BackColor = (IsML2Region ? Color.Red : Color.FromArgb(192, 255, 192));

            btnFitLeft.BackColor = (IsLeft ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFitRight.BackColor = (IsRight ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFitTop.BackColor = (IsTop ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFitBottom.BackColor = (IsBottom ? Color.Red : Color.FromArgb(192, 255, 192));

            btnCheckDir.BackColor = (IsCheckDir ? Color.Red : Color.FromArgb(192, 255, 192));

            btnFourMark1.BackColor = (IsFourMark1 ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFourMark2.BackColor = (IsFourMark2 ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFourMark3.BackColor = (IsFourMark3 ? Color.Red : Color.FromArgb(192, 255, 192));
            btnFourMark4.BackColor = (IsFourMark4 ? Color.Red : Color.FromArgb(192, 255, 192));

            btnCheckWholeDir.BackColor = (IsCheckWholeDir ? Color.Red : Color.FromArgb(192, 255, 192));
            btnReadBarcode.BackColor = (IsCheckUseBarcode ? Color.Red : Color.FromArgb(192, 255, 192));
        }

        private void BtnCamContinue_Click(object sender, EventArgs e)
        {

        }

        private void BtnCamOneshot_Click(object sender, EventArgs e)
        {
            if (cboCamList.SelectedIndex < 0)
                return;
            int index = cboCamList.SelectedIndex;

            bmpOperate[index].Dispose();
            GetCamera(index).Snap();
            bmpOperate[index] = new Bitmap(GetCamera(index).GetSnap());

            DS.ReplaceDisplayImage(bmpOperate[index]);
        }

        private void NumGain_ValueChanged(object sender, EventArgs e)
        {
            if (cboCamList.SelectedIndex < 0)
                return;

            int index = cboCamList.SelectedIndex;
            //RecipeNeedleClass.Instance.SetCamExpo(index, (float)numExpo.Value);
            //RecipeNeedleClass.Instance.SetCamGain(index, (float)numGain.Value);
            GetCamera(index).SetGain((float)numGain.Value);
        }

        private void NumExpo_ValueChanged(object sender, EventArgs e)
        {
            if (cboCamList.SelectedIndex < 0)
                return;

            int index = cboCamList.SelectedIndex;
            //RecipeNeedleClass.Instance.SetCamExpo(index, (float)numExpo.Value);
            //RecipeNeedleClass.Instance.SetCamGain(index, (float)numGain.Value);
            GetCamera(index).SetExposure((float)numExpo.Value);
        }

        //frmHandle FrmHandle = null;
        private void BtnHandAxis_Click(object sender, EventArgs e)
        {
            frmHandle.Instance.Show();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            //RecipeCHClass.Instance.Load();
            //RecipeTrayClass.Instance.Load();
            //VisionTrayClass.Instance.Load();
            myRecipe.Load();
            //frmHandle.Instance.Hide();
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            CoarsePositioningClass.Instance.sCoarsePosList = ctlPosClasses[0].GetPositionList();
            ModelPositioningClass.Instance.sModelPosList = ctlPosClasses[1].GetPositionList();

            GraphicalObject grobj = myMover[0].Source;
            myRecipe.rect_start = (grobj as JzRectEAG).GetRect;
            GraphicalObject grobj1 = myMover[1].Source;
            myRecipe.rect_end = (grobj1 as JzRectEAG).GetRect;
            //frmHandle.Instance.Hide();

            //myRecipe.rect_list.Clear();
            ////myRecipe.rect_list.Add(myRecipe.rect_start);
            ////myRecipe.rect_list.Add(myRecipe.rect_end);
            //int i = 0;
            //while (i < myMovers.Count)
            //{
            //    GraphicalObject grobj2 = myMovers[i].Source;
            //    RectangleF rx = (grobj2 as JzRectEAG).GetRect;

            //    myRecipe.rect_list.Add(rx);

            //    i++;
            //}
            myRecipe.MappingInspectStr = GetMappingStr();
            this.DialogResult = DialogResult.OK;
        }

        #endregion

        #region PRIVATE WINDOWS

        void FillDisplay()
        {
            //txttr1y.Text = RecipeTrayClass.Instance.Track_PosY_org[0].ToString();
            //txttr2y.Text = RecipeTrayClass.Instance.Track_PosY_org[1].ToString();
            //txttr3y.Text = RecipeTrayClass.Instance.Track_PosY_org[2].ToString();
            //txttr4y.Text = RecipeTrayClass.Instance.Track_PosY_org[3].ToString();
            //txttr1x.Text = RecipeTrayClass.Instance.Track_PosXL_org[0].ToString();
            //txttr2x.Text = RecipeTrayClass.Instance.Track_PosXL_org[1].ToString();
            //txttr2xR.Text = RecipeTrayClass.Instance.Track_PosXR_org[0].ToString();
            //txttr3xR.Text = RecipeTrayClass.Instance.Track_PosXR_org[1].ToString();
            //txttr4xR.Text = RecipeTrayClass.Instance.Track_PosXR_org[2].ToString();

            //txttr2ylastL.Text = RecipeTrayClass.Instance.Track_LenXL_TOP.ToString();
            //txttr2ylastR.Text = RecipeTrayClass.Instance.Track_LenXR_TOP.ToString();
            //txttr2xlast.Text = RecipeTrayClass.Instance.Track_LenXL_LEFT.ToString();
            //txttr2xRlast.Text = RecipeTrayClass.Instance.Track_LenXR_LEFT.ToString();
        }

        void FillDisplay(bool eChangeBaseBmp = false)
        {
            IsNeedToChange = false;

            _dynamicMover(tabControl1.SelectedIndex, eChangeBaseBmp);
            _staticMover(tabControl1.SelectedIndex);

            IsNeedToChange = true;
        }

        void _dynamicMover(int icamindex, bool eChangeBaseBmp = false)
        {
            icamindex = 0;
            //JzRectEAG _rect = null;
            DS.ClearMover();
            myMover.Clear();
            //myMover2.Clear();
            //myMover3.Clear();

            switch (icamindex)
            {
                case 0:
                    JzRectEAG _rect = //new JzRectEAG(Color.FromArgb(0, Color.Blue), new RectangleF(0, 0, 100, 100));
                    new JzRectEAG(Color.FromArgb(0, Color.Blue), myRecipe.rect_start);
                    _rect.RelateLevel = 2;
                    _rect.RelateNo = 1;
                    _rect.RelatePosition = 0;
                    myMover.Add(_rect);


                    JzRectEAG _rect1_1 = //new JzRectEAG(Color.FromArgb(0, Color.Blue), new RectangleF(300, 300, 100, 100));
                    new JzRectEAG(Color.FromArgb(0, Color.Blue), myRecipe.rect_end);
                    _rect1_1.RelateLevel = 2;
                    _rect1_1.RelateNo = 2;
                    _rect1_1.RelatePosition = 0;
                    myMover.Add(_rect1_1);


                    if (eChangeBaseBmp)
                        DS.ReplaceDisplayImage(myRecipe.bmpORG);
                    DS.SetMover(myMover);
                    break;
                case 1:

                    //JzRectEAG _rect1 = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali2_0);
                    //_rect1.RelateLevel = 1;
                    //_rect1.RelateNo = 1;
                    //_rect1.RelatePosition = 0;
                    //myMover.Add(_rect1);

                    //JzRectEAG _rect2 = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali2_1);
                    //_rect2.RelateLevel = 1;
                    //_rect2.RelateNo = 2;
                    //_rect2.RelatePosition = 1;
                    //myMover.Add(_rect2);

                    //if (eChangeBaseBmp)
                    //    DS.ReplaceDisplayImage(RecipeCHClass.Instance.bmpCaliOrg2);
                    //DS.SetMover(myMover);

                    break;
                case 2:

                    //JzRectEAG _rect3 = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali3_0);
                    //_rect3.RelateLevel = 1;
                    //_rect3.RelateNo = 1;
                    //_rect3.RelatePosition = 0;
                    //myMover.Add(_rect3);

                    //JzRectEAG _rect4 = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali3_1);
                    //_rect4.RelateLevel = 1;
                    //_rect4.RelateNo = 2;
                    //_rect4.RelatePosition = 1;
                    //myMover.Add(_rect4);

                    //if (eChangeBaseBmp)
                    //    DS.ReplaceDisplayImage(RecipeCHClass.Instance.bmpCaliOrg3);
                    //DS.SetMover(myMover);

                    break;
            }

            DS.RefreshDisplayShape();
            DS.MappingSelect();

            update_Display(false);
        }
        void _staticMover(int icamindex)
        {
            icamindex = 0;
            JzRectEAG _rect = null;
            DS.ClearStaticMover();
            myMovers.Clear();
            //myMover2s.Clear();
            //myMover3s.Clear();



            switch (icamindex)
            {
                case 0:
                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCalis);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 1;
                    //myMovers.Add(_rect);

                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali1_1s);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 2;
                    //myMovers.Add(_rect);

                    DS.SetStaticMover(myMovers);
                    break;
                case 1:

                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali2_0s);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 1;
                    //myMovers.Add(_rect);

                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali2_1s);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 2;
                    //myMovers.Add(_rect);

                    //DS.SetStaticMover(myMovers);

                    break;
                case 2:

                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali3_0s);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 1;
                    //myMovers.Add(_rect);

                    //_rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), RecipeCHClass.Instance.rectCali3_1s);
                    //_rect.RelateLevel = 6;
                    //_rect.RelateNo = 2;
                    //myMovers.Add(_rect);

                    DS.SetStaticMover(myMovers);

                    break;
            }

            DS.RefreshDisplayShape();
            DS.MappingSelect();

            update_Display(false);
        }
        private void _autoRowCol()
        {
            GraphicalObject grobj = myMover[0].Source;
            //RecipeTrayClass.Instance.rect_start = (grobj as JzRectEAG).GetRect;
            GraphicalObject grobj1 = myMover[1].Source;
            //RecipeTrayClass.Instance.rect_end = (grobj1 as JzRectEAG).GetRect;

            AnalyzeClass visionTestClass = new AnalyzeClass();
            visionTestClass.RectFStart = (grobj as JzRectEAG).GetRect;
            visionTestClass.RectFEnd = (grobj1 as JzRectEAG).GetRect;
            visionTestClass.CreateRowCol(myRecipe.TrayRowCount, myRecipe.TrayColCount);

            DS.ClearStaticMover();
            myMovers.Clear();

            int i = 0;
            while (i < visionTestClass.myListCell.Count)
            {
                //if (visionTestClass.myListCell[i].viewRectF.IntersectsWith(visionTestClass.RectFStart) ||
                //    visionTestClass.myListCell[i].viewRectF.IntersectsWith(visionTestClass.RectFEnd))
                //    continue;

                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), visionTestClass.myListCell[i].viewRectF);
                _rect.RelateLevel = 3;
                _rect.RelateNo = i;
                _rect.RelatePosition = 0;
                myMovers.Add(_rect);

                i++;
            }

            ////分步赋值 每张图片单独处理
            //string pos_Str = string.Empty;
            //i = 0;
            //while (i < 1)
            //{
            //    foreach (AutoRegionCellClass cellClass in visionTestClass.myListCell)
            //    {
            //        if (i == cellClass.CellCol)
            //        {
            //            pos_Str += cellClass.Index.ToString() + ",";
            //            pos_Str += cellClass.viewRectF.X.ToString() + ",";
            //            pos_Str += cellClass.viewRectF.Y.ToString() + ",";
            //            pos_Str += cellClass.viewRectF.Width.ToString() + ",";
            //            pos_Str += cellClass.viewRectF.Height.ToString() + Environment.NewLine;
            //        }
            //    }
            //    i++;
            //}

            //System.IO.StreamWriter streamWriter = new System.IO.StreamWriter("D:\\000.csv");
            //streamWriter.Write(pos_Str);
            //streamWriter.Close();
            //streamWriter.Dispose();
            DS.SetStaticMover(myMovers);
            //DS.SetStaticMover(myMovers);
            DS.RefreshDisplayShape();
            DS.MappingSelect();

            update_Display(true);

            InitializeDataGridView();

            SetMappingStr(myRecipe.MappingInspectStr);

        }
        private void _autoRowColInit()
        {
            DS.ClearStaticMover();
            myMovers.Clear();

            int i = 0;
            while (i < myRecipe.rect_list.Count)
            {
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), myRecipe.rect_list[i]);
                _rect.RelateLevel = 3;
                _rect.RelateNo = i;
                _rect.RelatePosition = 0;
                myMovers.Add(_rect);

                i++;
            }


            DS.SetStaticMover(myMovers);
            //DS.SetStaticMover(myMovers);
            DS.RefreshDisplayShape();
            DS.MappingSelect();

            update_Display(true);
        }
        void init_CboList()
        {
            int i = 0;
            cboCamList.Items.Clear();
            //while (i < CameraConfig.Instance.COUNT)
            //{
            //    var cam = Universal.CAMERAS[i];
            //    if (cam != null)
            //    {
            //        cboCamList.Items.Add(i.ToString() + "-" + cam.CameraCfg.SerialNumber);
            //    }
            //    i++;
            //}

            if (cboCamList.Items.Count > 0)
            {
                cboCamList.SelectedIndex = 0;
                //numExpo.Value = (decimal)RecipeNeedleClass.Instance.GetCamExpo(0);
                //numGain.Value = (decimal)RecipeNeedleClass.Instance.GetCamGain(0);
                _setCamLabelName(0);
            }
            cboCamList.SelectedIndexChanged += CboCamList_SelectedIndexChanged;
        }

        private void CboCamList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCamList.SelectedIndex < 0)
                return;

            int index = cboCamList.SelectedIndex;
            _setCamLabelName(index);
            //numExpo.Value = (decimal)RecipeNeedleClass.Instance.GetCamExpo(index);
            //numGain.Value = (decimal)RecipeNeedleClass.Instance.GetCamGain(index);
        }


        private void InitializeDataGridView()
        {
            // 创建 DataGridView

            //{
            //    Dock = DockStyle.Fill,
            //    RowCount = 10,
            //    ColumnCount = 10,
            //    AllowUserToAddRows = false,
            //    AllowUserToDeleteRows = false,
            //    AllowUserToResizeRows = false,
            //    AllowUserToResizeColumns = false,
            //    RowHeadersVisible = false,
            //    ColumnHeadersVisible = false,
            //    SelectionMode = DataGridViewSelectionMode.CellSelect,
            //    MultiSelect = true,
            //    ScrollBars = ScrollBars.None
            //};
            dgv.Dock = DockStyle.Fill;
            dgv.RowCount = myRecipe.TrayRowCount;
            dgv.ColumnCount = myRecipe.TrayColCount;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            //dgv.RowHeadersVisible = false;
            //dgv.ColumnHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.MultiSelect = true;
            //dgv.ScrollBars = ScrollBars.None;


            dgv.Columns.Clear();
            dgv.Rows.Clear();
            for (int col = 0; col < myRecipe.TrayColCount; col++)
            {
                dgv.Columns.Add($"Col{col}", $"Col{col + 1}");
                dgv.Columns[col].Width = 40;
                dgv.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv.Columns[col].ReadOnly = true;
            }
            for (int row = 0; row < myRecipe.TrayRowCount; row++)
            {
                dgv.Rows.Add($"Row{row}", $"Row{row}");
                dgv.Rows[row].Height = 40;

            }


            //// 固定大小 (假设每个单元格40x40像素)
            //for (int i = 0; i < myRecipe.TrayRowCount * myRecipe.TrayColCount; i++)
            //{
            //    dgv.Columns[i].Width = 40;
            //    //dgv.Rows[i].Height = 40;
            //}

            // 单元格样式 - 居中显示
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);

            // 单元格值改变时更新颜色
            dgv.CellValueChanged += (s, e) => UpdateCellColor(dgv, e.RowIndex, e.ColumnIndex);

            // 初始化所有单元格值为0并设置颜色
            for (int row = 0; row < myRecipe.TrayRowCount; row++)
            {
                for (int col = 0; col < myRecipe.TrayColCount; col++)
                {
                    dgv[col, row].Value = 1;
                    UpdateCellColor(dgv, row, col);
                }
            }

            // 创建右键菜单
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            for (int i = 0; i <= 1; i++)
            {
                int num = i;

                switch (i)
                {
                    case 0:
                        contextMenu.Items.Add($"{num.ToString()}-不检测", null, (s, e) =>
                        {
                            foreach (DataGridViewCell cell in dgv.SelectedCells)
                            {
                                cell.Value = num;
                                UpdateCellColor(dgv, cell.RowIndex, cell.ColumnIndex);
                            }
                        });
                        break;
                    case 1:
                        contextMenu.Items.Add($"{num.ToString()}-检测", null, (s, e) =>
                        {
                            foreach (DataGridViewCell cell in dgv.SelectedCells)
                            {
                                cell.Value = num;
                                UpdateCellColor(dgv, cell.RowIndex, cell.ColumnIndex);
                            }
                        });
                        break;
                }


            }

            // 分配右键菜单
            dgv.ContextMenuStrip = contextMenu;

            dgv.CellPainting += Dgv_CellPainting;

            //// 添加控件到窗体
            //this.Controls.Add(dgv);

            //// 设置窗体大小 (根据单元格大小计算)
            //this.ClientSize = new Size(10 * 40, 10 * 40);
            //this.Text = "10x10 DataGridView 示例";
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex < 0) // 绘制行号
            {
                e.PaintBackground(e.CellBounds, true); // 绘制背景
                TextRenderer.DrawText(e.Graphics, "Row" + (e.RowIndex + 1).ToString(), this.dataGridView1.Font, e.CellBounds, this.dataGridView1.ForeColor); // 绘制文本
                e.Handled = true; // 表示事件已处理，不需要默认处理
            }
        }

        private void UpdateCellColor(DataGridView dgv, int row, int col)
        {
            if (row < 0 || col < 0) return;

            var cell = dgv[col, row];
            if (cell.Value == null) return;

            int value;
            if (!int.TryParse(cell.Value.ToString(), out value))
                value = 0;

            // 根据数字设置不同背景色
            switch (value)
            {
                case 0:
                    cell.Style.BackColor = Color.White;
                    break;
                case 1:
                    cell.Style.BackColor = Color.Lime;
                    break;
                case 2:
                    cell.Style.BackColor = Color.LightGreen;
                    break;
                case 3:
                    cell.Style.BackColor = Color.LightYellow;
                    break;
                case 4:
                    cell.Style.BackColor = Color.LightPink;
                    break;
                case 5:
                    cell.Style.BackColor = Color.LightSalmon;
                    break;
                case 6:
                    cell.Style.BackColor = Color.LightSkyBlue;
                    break;
                case 7:
                    cell.Style.BackColor = Color.LightSteelBlue;
                    break;
                case 8:
                    cell.Style.BackColor = Color.LightGray;
                    break;
                case 9:
                    cell.Style.BackColor = Color.LightCoral;
                    break;
                default:
                    cell.Style.BackColor = Color.White;
                    break;
            }

            // 确保文本颜色与背景对比明显
            cell.Style.ForeColor = Color.Black;
        }

        string GetMappingStr()
        {
            int Row = myRecipe.TrayRowCount;
            int Column = myRecipe.TrayColCount;

            List<string> collectList = new List<string>();

            switch (myRecipe.PreLaserPos)
            {
                case LaserStartPos.LeftTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            var cell = dgv[j, i];
                            if (cell.Value == null) break;
                            collectList.Add(cell.Value.ToString());
                        }
                    }

                    break;
                case LaserStartPos.LeftBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            var cell = dgv[j, i - 1];
                            if (cell.Value == null) break;
                            collectList.Add(cell.Value.ToString());
                        }
                    }

                    break;
                case LaserStartPos.RightTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            var cell = dgv[j - 1, i];
                            if (cell.Value == null) break;
                            collectList.Add(cell.Value.ToString());
                        }
                    }

                    break;
                case LaserStartPos.RightBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            var cell = dgv[j - 1, i - 1];
                            if (cell.Value == null) break;
                            collectList.Add(cell.Value.ToString());
                        }
                    }

                    break;
            }

            string str = string.Empty;
            foreach (var cell in collectList)
            {
                str += cell;
            }

            return str;

        }
        void SetMappingStr(string eInputStr = "1")
        {
            char[] chars = eInputStr.ToCharArray();

            int Row = myRecipe.TrayRowCount;
            int Column = myRecipe.TrayColCount;

            if (chars.Length != Row * Column)
                return;

            int k = 0;

            switch (myRecipe.PreLaserPos)
            {
                case LaserStartPos.LeftTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            var cell = dgv[j, i];
                            if (cell.Value == null) break;
                            cell.Value = chars[k];

                            k++;
                        }
                    }

                    break;
                case LaserStartPos.LeftBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = 0; j < Column; j++)
                        {
                            var cell = dgv[j, i - 1];
                            if (cell.Value == null) break;
                            cell.Value = chars[k];

                            k++;
                        }
                    }

                    break;
                case LaserStartPos.RightTop:

                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            var cell = dgv[j - 1, i];
                            if (cell.Value == null) break;
                            cell.Value = chars[k];

                            k++;
                        }
                    }

                    break;
                case LaserStartPos.RightBottom:

                    for (int i = Row; i > 0; i--)
                    {
                        for (int j = Column; j > 0; j--)
                        {
                            var cell = dgv[j - 1, i - 1];
                            if (cell.Value == null) break;
                            cell.Value = chars[k];

                            k++;
                        }
                    }

                    break;
            }

        }

        void _setCamLabelName(int camid)
        {
            switch (camid)
            {
                case 0: lblCamName.Text = "全域特征定位相机"; break;
                case 1: lblCamName.Text = "特微检测高度相机"; break;
                default: lblCamName.Text = "显微相机"; break;

            }
        }
        void init_Display()
        {
            //DS = dispUI1;
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS.CaptureAction += DS_CaptureAction;
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;

            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            DS2.CaptureAction += DS_CaptureAction2;
        }

        private void DS_CaptureAction(RectangleF rectf)
        {
            GraphicalObject grobj = myMover[0].Source;
            RectangleF rectf_org = (grobj as JzRectEAG).GetRect;
            RectangleF rectf_des = (grobj as JzRectEAG).GetRect;

            if (IsFourMark1)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.fourmark1 = rectf;

                    IsFourMark1 = false;

                    myRecipe.bmpMarkOrg[0].Dispose();
                    myRecipe.bmpMarkOrg[0] = myRecipe.bmpORG.Clone(myRecipe.fourmark1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    PointF ptf1 = myRecipe.MarkCollectionStr[0].GetImageMarkCenter(myRecipe.bmpMarkOrg[0], true,
                                                                                                                           out RectangleF maxrecttemp,
                                                                                                                           out Bitmap bmpouputtemp,
                                                                                                                           out int maxareatemp);

                    rectf_des = new RectangleF(maxrecttemp.X + myRecipe.fourmark1.X,
                                                                maxrecttemp.Y + myRecipe.fourmark1.Y, maxrecttemp.Width, maxrecttemp.Height);
                    ptf1.X += myRecipe.fourmark1.X;
                    ptf1.Y += myRecipe.fourmark1.Y;
                    //calMarkBlob(myRecipe.bmpORG, rectf, myRecipe.thresholdv_mark1, out rectf_des, myRecipe.findwhite_mark1);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark0 = SimpleRectF(ptf1, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                }
            }
            else if (IsFourMark2)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.fourmark2 = rectf;

                    IsFourMark2 = false;

                    myRecipe.bmpMarkOrg[1].Dispose();
                    myRecipe.bmpMarkOrg[1] = myRecipe.bmpORG.Clone(myRecipe.fourmark2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    PointF ptf1 = myRecipe.MarkCollectionStr[1].GetImageMarkCenter(myRecipe.bmpMarkOrg[1], true,
                                                                                                                           out RectangleF maxrecttemp,
                                                                                                                           out Bitmap bmpouputtemp,
                                                                                                                           out int maxareatemp);

                    rectf_des = new RectangleF(maxrecttemp.X + myRecipe.fourmark2.X,
                                                                maxrecttemp.Y + myRecipe.fourmark2.Y, maxrecttemp.Width, maxrecttemp.Height);
                    ptf1.X += myRecipe.fourmark2.X;
                    ptf1.Y += myRecipe.fourmark2.Y;
                    //calMarkBlob(myRecipe.bmpORG, rectf, myRecipe.thresholdv_mark1, out rectf_des, myRecipe.findwhite_mark1);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark0 = SimpleRectF(ptf1, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                }
            }
            else if (IsFourMark3)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.fourmark3 = rectf;

                    IsFourMark3 = false;

                    myRecipe.bmpMarkOrg[2].Dispose();
                    myRecipe.bmpMarkOrg[2] = myRecipe.bmpORG.Clone(myRecipe.fourmark3, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    PointF ptf1 = myRecipe.MarkCollectionStr[2].GetImageMarkCenter(myRecipe.bmpMarkOrg[2], true,
                                                                                                                           out RectangleF maxrecttemp,
                                                                                                                           out Bitmap bmpouputtemp,
                                                                                                                           out int maxareatemp);

                    rectf_des = new RectangleF(maxrecttemp.X + myRecipe.fourmark3.X,
                                                                maxrecttemp.Y + myRecipe.fourmark3.Y, maxrecttemp.Width, maxrecttemp.Height);
                    ptf1.X += myRecipe.fourmark3.X;
                    ptf1.Y += myRecipe.fourmark3.Y;
                    //calMarkBlob(myRecipe.bmpORG, rectf, myRecipe.thresholdv_mark1, out rectf_des, myRecipe.findwhite_mark1);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark0 = SimpleRectF(ptf1, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                }
            }
            else if (IsFourMark4)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.fourmark4 = rectf;

                    IsFourMark4 = false;

                    myRecipe.bmpMarkOrg[3].Dispose();
                    myRecipe.bmpMarkOrg[3] = myRecipe.bmpORG.Clone(myRecipe.fourmark4, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    PointF ptf1 = myRecipe.MarkCollectionStr[3].GetImageMarkCenter(myRecipe.bmpMarkOrg[3], true,
                                                                                                                           out RectangleF maxrecttemp,
                                                                                                                           out Bitmap bmpouputtemp,
                                                                                                                           out int maxareatemp);

                    rectf_des = new RectangleF(maxrecttemp.X + myRecipe.fourmark4.X,
                                                                maxrecttemp.Y + myRecipe.fourmark4.Y, maxrecttemp.Width, maxrecttemp.Height);
                    ptf1.X += myRecipe.fourmark4.X;
                    ptf1.Y += myRecipe.fourmark4.Y;
                    //calMarkBlob(myRecipe.bmpORG, rectf, myRecipe.thresholdv_mark1, out rectf_des, myRecipe.findwhite_mark1);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark0 = SimpleRectF(ptf1, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                }
            }
            else if (IsCheckWholeDir)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.AnaDirRectF = rectf;

                    IsCheckWholeDir = false;

                    myRecipe.bmpOrgAnaDirPattern.Dispose();
                    Bitmap bmptemp = myRecipe.bmpORG.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    myRecipe.bmpOrgAnaDirPattern = new Bitmap(bmptemp);
                    myRecipe.SaveAnaDirPattern();
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);

                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf });

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    //DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmptemp.Dispose();

                }
            }
            else if (IsCheckUseBarcode)
            {
                BoundRect(ref rectf, myRecipe.bmpORG.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.AnaReadBarcodeRectF = rectf;

                    IsCheckUseBarcode = false;

                    //myRecipe.bmpOrgAnaDirPattern.Dispose();
                    Bitmap bmptemp = myRecipe.bmpORG.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    //myRecipe.bmpOrgAnaDirPattern = new Bitmap(bmptemp);
                    //myRecipe.SaveAnaDirPattern();
                    myRecipe.SaveMark();

                    string _barcodeStr = myRecipe.AnaReadBarcode(bmptemp);

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORG);
                    Graphics g = Graphics.FromImage(bmpx);

                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf });
                    g.DrawString($"{_barcodeStr}", new Font("Arial", 100),
                                                   new SolidBrush(Color.Lime), new PointF(rectf.X + rectf.Width, rectf.Y));

                    g.Dispose();
                    DS.ReplaceDisplayImage(bmpx);
                    //DS.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmptemp.Dispose();

                }
            }
        }
        private void DS_CaptureAction2(RectangleF rectf)
        {
            GraphicalObject grobj = myMover[0].Source;
            RectangleF rectf_org = (grobj as JzRectEAG).GetRect;
            RectangleF rectf_des = (grobj as JzRectEAG).GetRect;

            if (IsStartMark0RoiRegion)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.Mark0 = rectf;

                    IsStartMark0RoiRegion = false;

                    myRecipe.ptMark0 = calMarkBlob(myRecipe.bmpORGPattern, rectf, myRecipe.PreThresholdValue, out rectf_des);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark0 = SimpleRectF(myRecipe.ptMark0, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                }
            }
            else if (IsStartMark1RoiRegion)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark1 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.Mark1 = rectf;

                    IsStartMark1RoiRegion = false;

                    myRecipe.ptMark1 = calMarkBlob(myRecipe.bmpORGPattern, rectf, myRecipe.PreThresholdValueMark2, out rectf_des);
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);
                    RectangleF rectangleFmark1 = SimpleRectF(myRecipe.ptMark1, 2, 2);

                    g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark1 });
                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();
                }
            }
            else if (IsStartMarkLineRoiRegion)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark1 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.MarkLine = rectf;

                    IsStartMarkLineRoiRegion = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(myRecipe.MarkLine, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreMarkLineThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    AForge.Imaging.Filters.Invert invert = new Invert();
                    bmpinput0 = invert.Apply(bmpinput0);

                    AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    fillHoles2.MaxHoleHeight = 100;
                    fillHoles2.MaxHoleWidth = 100;
                    fillHoles2.CoupledSizeFiltering = false;
                    bmpinput0 = fillHoles2.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    LineClass lineClass = getVericalLine(bmpinput0, true);


                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += myRecipe.MarkLine.X;
                        p1.Y += myRecipe.MarkLine.Y;

                        p2.X += myRecipe.MarkLine.X;
                        p2.Y += myRecipe.MarkLine.Y;

                        LineClass lineClass2 = new LineClass(p1, p2);
                        p1 = lineClass2.GetPtFromX(0);
                        p2 = lineClass2.GetPtFromX(bmpx.Width);

                        myRecipe.MarkLineAngle = GetAngle(p1, p2);

                        //RectangleF rectangleF = SimpleRectF(p1, 2, 2);
                        //g.DrawRectangles(new Pen(Color.Red, 7), new RectangleF[] { rectangleF });

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);
                    }
                    catch (Exception ex)
                    {
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找直线失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsML1Region)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark1 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.MarkLine1 = rectf;

                    IsML1Region = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(myRecipe.MarkLine1, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreMarkLineThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    AForge.Imaging.Filters.Invert invert = new Invert();
                    bmpinput0 = invert.Apply(bmpinput0);

                    AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    fillHoles2.MaxHoleHeight = 100;
                    fillHoles2.MaxHoleWidth = 100;
                    fillHoles2.CoupledSizeFiltering = false;
                    bmpinput0 = fillHoles2.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    LineClass lineClass = getVericalLine(bmpinput0, true);


                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += myRecipe.MarkLine1.X;
                        p1.Y += myRecipe.MarkLine1.Y;

                        p2.X += myRecipe.MarkLine1.X;
                        p2.Y += myRecipe.MarkLine1.Y;

                        lineClassx1 = new LineClass(p1, p2);
                        //LineClass lineClass2 = new LineClass(p1, p2);
                        //p1 = lineClass2.GetPtFromX(0);
                        //p2 = lineClass2.GetPtFromX(bmpx.Width);

                        //myRecipe.MarkLineAngle = GetAngle(p1, p2);

                        //RectangleF rectangleF = SimpleRectF(p1, 2, 2);
                        //g.DrawRectangles(new Pen(Color.Red, 7), new RectangleF[] { rectangleF });

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        IsML1RegionOK = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassx1 = null;
                        IsML1RegionOK = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找直线1失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsML2Region)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    //myRecipe.Mark1 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
                    myRecipe.MarkLine2 = rectf;

                    IsML2Region = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(myRecipe.MarkLine2, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreMarkLineThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    AForge.Imaging.Filters.Invert invert = new Invert();
                    bmpinput0 = invert.Apply(bmpinput0);

                    AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    fillHoles2.MaxHoleHeight = 100;
                    fillHoles2.MaxHoleWidth = 100;
                    fillHoles2.CoupledSizeFiltering = false;
                    bmpinput0 = fillHoles2.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    LineClass lineClass = getVericalLine(bmpinput0, true);


                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += myRecipe.MarkLine2.X;
                        p1.Y += myRecipe.MarkLine2.Y;

                        p2.X += myRecipe.MarkLine2.X;
                        p2.Y += myRecipe.MarkLine2.Y;

                        lineClassx2 = new LineClass(p1, p2);
                        //LineClass lineClass2 = new LineClass(p1, p2);
                        //p1 = lineClass2.GetPtFromX(0);
                        //p2 = lineClass2.GetPtFromX(bmpx.Width);

                        //myRecipe.MarkLineAngle = GetAngle(p1, p2);

                        //RectangleF rectangleF = SimpleRectF(p1, 2, 2);
                        //g.DrawRectangles(new Pen(Color.Red, 7), new RectangleF[] { rectangleF });

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        IsML2RegionOK = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassx2 = null;
                        IsML2RegionOK = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找直线2失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsLeft)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.SideLeft = rectf;

                    IsLeft = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(rectf, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    xinliFindLineClass.IsLeftToRight = myRecipe.dir_left;
                    xinliFindLineClass.IsBlack = myRecipe.dir_left_black;
                    xinliFindLineClass.SampleValue = myRecipe.SampleValue;
                    xinliFindLineClass.LeastPix = myRecipe.PreLineLeastDistance;
                    LineClass lineClass = null;// xinliFindLineClass.GetLevelLine(bmpinput0);
                    switch (myRecipe.PreMode)
                    {
                        case ProcessImageMode.V11:
                            lineClass = xinliFindLineClass.GetLevelLineVM(bmpinput0);
                            break;
                        default:
                            lineClass = xinliFindLineClass.GetLevelLine(bmpinput0);
                            break;
                    }

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += rectf.X;
                        p1.Y += rectf.Y;

                        p2.X += rectf.X;
                        p2.Y += rectf.Y;

                        lineClassxFindLine = new LineClass(p1, p2);

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        //IsLeft = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassxFindLine = null;
                        IsLeft = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找左边直线失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsRight)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.SideRight = rectf;

                    IsRight = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(rectf, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    xinliFindLineClass.IsLeftToRight = myRecipe.dir_right;
                    xinliFindLineClass.IsBlack = myRecipe.dir_right_black;
                    xinliFindLineClass.SampleValue = myRecipe.SampleValue;
                    xinliFindLineClass.LeastPix = myRecipe.PreLineLeastDistance;
                    LineClass lineClass = null;// xinliFindLineClass.GetLevelLine(bmpinput0);
                    switch (myRecipe.PreMode)
                    {
                        case ProcessImageMode.V11:
                            lineClass = xinliFindLineClass.GetLevelLineVM(bmpinput0);
                            break;
                        default:
                            lineClass = xinliFindLineClass.GetLevelLine(bmpinput0);
                            break;
                    }

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += rectf.X;
                        p1.Y += rectf.Y;

                        p2.X += rectf.X;
                        p2.Y += rectf.Y;

                        lineClassxFindLine = new LineClass(p1, p2);

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        //IsLeft = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassxFindLine = null;
                        IsRight = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找右边直线失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsTop)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.SideTop = rectf;

                    IsTop = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(rectf, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    xinliFindLineClass.IsLeftToRight = myRecipe.dir_top;
                    xinliFindLineClass.IsBlack = myRecipe.dir_top_black;
                    xinliFindLineClass.SampleValue = myRecipe.SampleValue;
                    xinliFindLineClass.LeastPix = myRecipe.PreLineLeastDistance;
                    LineClass lineClass = null;// xinliFindLineClass.GetVericalLine(bmpinput0);
                    switch (myRecipe.PreMode)
                    {
                        case ProcessImageMode.V11:
                            lineClass = xinliFindLineClass.GetVericalLineVM(bmpinput0);
                            break;
                        default:
                            lineClass = xinliFindLineClass.GetVericalLine(bmpinput0);
                            break;
                    }
                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += rectf.X;
                        p1.Y += rectf.Y;

                        p2.X += rectf.X;
                        p2.Y += rectf.Y;

                        lineClassxFindLine = new LineClass(p1, p2);

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        //IsLeft = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassxFindLine = null;
                        IsTop = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找上边直线失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsBottom)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.SideBottom = rectf;

                    IsBottom = false;

                    Bitmap bmpinput0 = myRecipe.bmpORGPattern.Clone(rectf, PixelFormat.Format24bppRgb);

                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    bmpinput0 = grayscale.Apply(bmpinput0);

                    AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreThresholdValue);
                    bmpinput0 = threshold.Apply(bmpinput0);

                    //bmpinput0.Save("D:\\bmp00.bmp", ImageFormat.Bmp);

                    xinliFindLineClass.IsLeftToRight = myRecipe.dir_bottom;
                    xinliFindLineClass.IsBlack = myRecipe.dir_bottom_black;
                    xinliFindLineClass.SampleValue = myRecipe.SampleValue;
                    xinliFindLineClass.LeastPix = myRecipe.PreLineLeastDistance;
                    LineClass lineClass = null;// xinliFindLineClass.GetVericalLine(bmpinput0);
                    switch (myRecipe.PreMode)
                    {
                        case ProcessImageMode.V11:
                            lineClass = xinliFindLineClass.GetVericalLineVM(bmpinput0);
                            break;
                        default:
                            lineClass = xinliFindLineClass.GetVericalLine(bmpinput0);
                            break;
                    }

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    try
                    {

                        PointF p1 = lineClass.FirstPt;// lineClass.GetPtFromY(0);
                        PointF p2 = lineClass.SecondPt;// lineClass.GetPtFromY(bmpx.Width);

                        p1.X += rectf.X;
                        p1.Y += rectf.Y;

                        p2.X += rectf.X;
                        p2.Y += rectf.Y;

                        lineClassxFindLine = new LineClass(p1, p2);

                        g.DrawLine(new Pen(Color.Lime, 7), p1, p2);

                        //IsLeft = true;
                    }
                    catch (Exception ex)
                    {
                        lineClassxFindLine = null;
                        IsBottom = false;
                        JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("找下边直线失败")}{ex.Message}");
                    }

                    myRecipe.SaveMark();

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmpinput0.Dispose();
                }
            }
            else if (IsCheckDir)
            {
                BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
                if (rectf.Width > 1 && rectf.Height > 1)
                {
                    myRecipe.DirRectF = rectf;

                    IsCheckDir = false;

                    myRecipe.bmpOrgDirPattern.Dispose();
                    Bitmap bmptemp = myRecipe.bmpORGPattern.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    myRecipe.bmpOrgDirPattern = new Bitmap(bmptemp);
                    myRecipe.SaveDirPattern();
                    myRecipe.SaveMark();

                    Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
                    Graphics g = Graphics.FromImage(bmpx);

                    g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf });

                    g.Dispose();
                    DS2.ReplaceDisplayImage(bmpx);
                    bmpx.Dispose();

                    bmptemp.Dispose();

                }
            }
        }

        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
        }

        void update_Display(bool eChangeToDefault = true)
        {
            DS.Refresh();
            if (eChangeToDefault)
                DS.DefaultView();

            DS2.Refresh();
            if (eChangeToDefault)
                DS2.DefaultView();
        }
        protected ICam GetCamera(int camID)
        {
            return Universal.CAMERAS[camID];
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

        private Bitmap preProcessImage(Bitmap bmpinput)
        {
            Bitmap bmpSizedPre = new Bitmap(bmpinput);
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmpSizedPre = grayscale.Apply(bmpSizedPre);

            switch (myRecipe.PreMode)
            {
                case ProcessImageMode.V1:
                    AForge.Imaging.Filters.BradleyLocalThresholding bradleyLocalThresholding = new AForge.Imaging.Filters.BradleyLocalThresholding();
                    bmpSizedPre = bradleyLocalThresholding.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.FillHoles fillHoles = new AForge.Imaging.Filters.FillHoles();
                    fillHoles.MaxHoleHeight = 100;
                    fillHoles.MaxHoleWidth = 100;
                    //fillHoles.CoupledSizeFiltering = false;
                    bmpSizedPre = fillHoles.Apply(bmpSizedPre);
                    break;
                case ProcessImageMode.V2:

                    //AForge.Imaging.Filters.Threshold threshold = new Threshold(myRecipe.PreThresholdValue);
                    //bmpSizedPre = threshold.Apply(bmpSizedPre);
                    //AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    //fillHoles2.MaxHoleHeight = 100;
                    //fillHoles2.MaxHoleWidth = 100;
                    //fillHoles2.CoupledSizeFiltering = false;
                    //bmpSizedPre = fillHoles2.Apply(bmpSizedPre);

                    break;
                case ProcessImageMode.V3:
                    AForge.Imaging.Filters.HistogramEqualization histogramEqualization = new HistogramEqualization();
                    bmpSizedPre = histogramEqualization.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.SISThreshold sISThreshold = new SISThreshold();
                    bmpSizedPre = sISThreshold.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.Closing closing = new Closing();
                    bmpSizedPre = closing.Apply(bmpSizedPre);
                    AForge.Imaging.Filters.FillHoles fillHoles2 = new AForge.Imaging.Filters.FillHoles();
                    fillHoles2.MaxHoleHeight = 100;
                    fillHoles2.MaxHoleWidth = 100;
                    fillHoles2.CoupledSizeFiltering = false;
                    bmpSizedPre = fillHoles2.Apply(bmpSizedPre);

                    break;
            }

            return bmpSizedPre;
        }
        JzFindObjectClass m_Find = new JzFindObjectClass();
        PointF calMarkBlob(Bitmap bmpinput, RectangleF cropRect, int threshold, out RectangleF maxrect, bool iswhite = true)
        {
            PointF ret = new PointF(cropRect.X + cropRect.Width / 2, cropRect.Y + cropRect.Height / 2);
            maxrect = new RectangleF(cropRect.X + 1, cropRect.Y + 1, cropRect.Width - 2, cropRect.Height - 2);
            Bitmap bmptemp = (Bitmap)bmpinput.Clone(cropRect, PixelFormat.Format24bppRgb);
            m_Find.AH_SetThreshold(ref bmptemp, threshold);
            m_Find.AH_FindBlob(bmptemp, iswhite);

            if (m_Find.FoundList.Count > 0)
            {
                int maxindex = m_Find.GetMaxRectIndex();
                ret = new PointF((float)m_Find.FoundList[maxindex].rotatedRectangleF.fCX + cropRect.X,
                                 (float)m_Find.FoundList[maxindex].rotatedRectangleF.fCY + cropRect.Y);

                maxrect = new RectangleF(m_Find.rectMaxRect.X + cropRect.X, m_Find.rectMaxRect.Y + cropRect.Y, m_Find.rectMaxRect.Width, m_Find.rectMaxRect.Height);
            }
            bmptemp.Dispose();

            return ret;
        }

        LineClass getVericalLine(Bitmap bmpInput, bool istoptobottom = true)
        {
            int _sample = myRecipe.SampleValue;
            int _cropValue = bmpInput.Width - 1;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(ix * _sample, 0, _sample, bmpInput.Height - 1);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1 = 0;
                if (myRecipe.PreDir)
                {
                    if (istoptobottom)
                        y1 = GetLineInYDir(bmp2, false, 0, 0, Color.Black);
                    else
                        y1 = GetLineInYDir(bmp2, true, bmp2.Height, 0, Color.Black);
                }
                else
                {
                    if (!istoptobottom)
                        y1 = GetLineInYDir(bmp2, false, 0, 0, Color.White);
                    else
                        y1 = GetLineInYDir(bmp2, true, bmp2.Height, 0, Color.White);
                }
                points1[ix] = new PointF(rectangle.X, y1);
                bmp2.Dispose();

                ix++;
            }
            LineClass line = getLineForPointF(points1, false, pix: myRecipe.PreLineLeastDistance);
            return line;
        }
        object obj = new object();
        private int GetLineInYDir(Bitmap bmp, bool IsReverse, int FromY, int XLocation, Color StopColor)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)(void*)Scan0;
                        byte* pucPtr;
                        byte* pucStart;

                        int xmin = rectbmp.X;
                        int ymin = rectbmp.Y;
                        int xmax = xmin + rectbmp.Width;
                        int ymax = ymin + rectbmp.Height;

                        int x = XLocation;
                        int y = ymin;
                        int iStride = bmpData.Stride;

                        y = FromY;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        pucStart[0] = (byte)(255 - StopColor.R);
                        pucStart[1] = (byte)(255 - StopColor.G);
                        pucStart[2] = (byte)(255 - StopColor.B);

                        if (!IsReverse)
                        {
                            while (y < ymax - 1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    //y--;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart += iStride;

                                y++;
                            }
                        }
                        else
                        {
                            while (y > 0)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    y++;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart -= iStride;

                                y--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return y;
                    }
                }
                //catch (Exception e)
                //{
                //    bmp.UnlockBits(bmpData);

                //    //if (IsDebug)
                //    //    MessageBox.Show("Error :" + e.ToString());

                //    return -1;
                //}
            }
        }
        Rectangle SimpleRect(Size Sz)
        {
            return new Rectangle(0, 0, Sz.Width, Sz.Height);
        }
        private LineClass getLineForPointF(PointF[] points1, bool swap = true, float pix = 0.5f)
        {
            QvLineFit jzLineFit = new QvLineFit();
            jzLineFit.Swap = swap;
            jzLineFit.LeastSquareFit(points1);


            PointF p1 = new PointF(points1[0].X, points1[0].Y);
            PointF p2 = new PointF(points1[points1.Length - 1].X, points1[points1.Length - 1].Y);

            if (swap)
            {
                if (jzLineFit.A != 0)
                    p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
            }
            else
            {
                if (jzLineFit.A != 0)
                    p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
            }

            //if (jzLineFit.A != 0)
            //    points1[0].X = (float)((double)points1[0].Y * jzLineFit.A + jzLineFit.B);

            //if (jzLineFit.A != 0)
            //    points1[points1.Length - 1].X = (float)((double)points1[points1.Length - 1].Y * jzLineFit.A + jzLineFit.B);

            LineClass lineClass = new LineClass(p1, p2);
            lineClass.IsSwap = swap;
            //if (swap)
            {
                //lineClass.IsSwap = true;
                List<PointF> lines = new List<PointF>();
                int i = 0;
                while (i < points1.Length)
                {
                    double xlen = jzLineFit.GetPointLength(points1[i]);
                    if (xlen < pix)
                    {
                        lines.Add(points1[i]);
                    }

                    i++;
                }

                if (lines.Count >= 2)
                {
                    jzLineFit.Swap = swap;
                    jzLineFit.LeastSquareFit(lines.ToArray());

                    p1 = lines[0];
                    p2 = lines[lines.Count - 1];

                    if (swap)
                    {
                        if (jzLineFit.A != 0)
                            p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
                    }
                    else
                    {
                        if (jzLineFit.A != 0)
                            p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
                    }

                    lineClass.IsSwap = swap;
                    lineClass = new LineClass(p1, p2);
                }
            }


            return lineClass;
        }

        double GetAngle(PointF xP2World, PointF xP1World)
        {
            double angleOfLine = 0;
            if (xP2World.X > xP1World.X)
                angleOfLine = Math.Atan2((xP2World.Y - xP1World.Y), (xP2World.X - xP1World.X)) * 180 / Math.PI;
            else
                angleOfLine = Math.Atan2((xP1World.Y - xP2World.Y), (xP1World.X - xP2World.X)) * 180 / Math.PI;
            return angleOfLine;
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
