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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VM.PlatformSDKCS;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmBaseX2 : Form
    {
        //Bitmap xbmpOperate = new Bitmap(1, 1);
        FreeImageAPI.FreeImageBitmap xFreeBmpOperate = new FreeImageAPI.FreeImageBitmap(1, 1);
        Mover xMover = new Mover();
        bool bSelectRegion = false;
        protected RecipeMainX2Class xRecipe
        {
            get { return RecipeMainX2Class.Instance; }
        }

        RegionName xRegionNameCurrent = RegionName.BASE0;

        Button btnSelectRegion;
        Button btnCreateTemplate;
        Button btnTestTemplate;
        Button btnSaveTemplate;

        Button btnTestBaseDistance;
        Button btnSaveBaseRecipe;

        public frmBaseX2(RegionName eRegionName)
        {
            xRegionNameCurrent = eRegionName;

            InitializeComponent();
            this.Load += FrmBaseX2_Load;
        }

        private void FrmBaseX2_Load(object sender, EventArgs e)
        {
            btnSelectRegion = button6;
            btnCreateTemplate = button1;
            btnTestTemplate = button2;
            btnSaveTemplate = button3;
            btnTestBaseDistance = button4;
            btnSaveBaseRecipe = button5;

            groupBox1.Visible = false;
            btnTestBaseDistance.Visible = false;
            btnSaveBaseRecipe.Visible = false;

            btnSelectRegion.Click += BtnSelectRegion_Click;
            btnCreateTemplate.Click += BtnCreateTemplate_Click;
            btnTestTemplate.Click += BtnTestTemplate_Click;
            btnSaveTemplate.Click += BtnSaveTemplate_Click;
            btnTestBaseDistance.Click += BtnTestBaseDistance_Click;
            btnSaveBaseRecipe.Click += BtnSaveBaseRecipe_Click;

            init_Display();
            update_Display();

            switch (xRegionNameCurrent)
            {
                case RegionName.BASE0:
                    xFreeBmpOperate.Dispose();
                    xFreeBmpOperate = new FreeImageBitmap(xRecipe.bmpbase0);

                    this.Text = "设定基准1界面";
                    break;
                case RegionName.BASE1:

                    xFreeBmpOperate.Dispose();
                    xFreeBmpOperate = new FreeImageBitmap(xRecipe.bmpbase1);

                    groupBox1.Visible = true;
                    btnTestBaseDistance.Visible = true;
                    btnSaveBaseRecipe.Visible = true;

                    this.Text = "设定基准2界面";
                    break;
            }

            DS.ReplaceDisplayImage(xFreeBmpOperate.ToBitmap());
            //DS.Refresh();

        }

        private void BtnSaveBaseRecipe_Click(object sender, EventArgs e)
        {
            xRecipe.SaveBase();
        }

        private void BtnTestBaseDistance_Click(object sender, EventArgs e)
        {
            xRecipe.distancebase0tobase1 = new System.Drawing.PointF(xRecipe.template1Center.X - xRecipe.template0Center.X,
                                                                                                                 xRecipe.template1Center.Y - xRecipe.template0Center.Y);

            textBox1.Text = xRecipe.distancebase0tobase1.X.ToString();
            textBox2.Text = xRecipe.distancebase0tobase1.Y.ToString();
        }

        private void BtnSaveTemplate_Click(object sender, EventArgs e)
        {
            xRecipe.SaveBase();
        }

        private void BtnTestTemplate_Click(object sender, EventArgs e)
        {
            RectangleF _recttemp = new RectangleF(0, 0, 1, 1);
            JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), _recttemp);
            xMover.Clear();
            DS.ClearStaticMover();

            int iOK = 0;
            switch (xRegionNameCurrent)
            {
                case RegionName.BASE0:
                    iOK = xRecipe.Base0Run(xRecipe.bmpbase0);
                    if (iOK == 0)
                    {
                        xRecipe.template0Center =
                            new System.Drawing.PointF(xRecipe.mvdbase0_Find.xResults[0].fCenterX + xRecipe.xRectRegionBase0.X,
                                                                          xRecipe.mvdbase0_Find.xResults[0].fCenterY + xRecipe.xRectRegionBase0.Y);

                        _recttemp = SimpleRect(new System.Drawing.PointF(xRecipe.mvdbase0_Find.xResults[0].fCenterX,
                            xRecipe.mvdbase0_Find.xResults[0].fCenterY), xRecipe.bmptemplate0.Size);
                        _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), _recttemp);
                        _rect.RelateLevel = 2;
                        _rect.RelateNo = 0;
                        _rect.RelatePosition = 0;
                        xMover.Add(_rect);

                    }
                    break;
                case RegionName.BASE1:
                    iOK = xRecipe.Base1Run(xRecipe.bmpbase1);
                    if (iOK == 0)
                    {
                        xRecipe.template1Center =
                            new System.Drawing.PointF(xRecipe.mvdbase1_Find.xResults[0].fCenterX + xRecipe.xRectRegionBase1.X,
                                                                          xRecipe.mvdbase1_Find.xResults[0].fCenterY + xRecipe.xRectRegionBase1.Y);

                        _recttemp = SimpleRect(new System.Drawing.PointF(xRecipe.mvdbase1_Find.xResults[0].fCenterX,
                           xRecipe.mvdbase1_Find.xResults[0].fCenterY), xRecipe.bmptemplate1.Size);
                        _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), _recttemp);
                        _rect.RelateLevel = 2;
                        _rect.RelateNo = 0;
                        _rect.RelatePosition = 0;
                        xMover.Add(_rect);

                    }
                    break;
            }

            DS.SetStaticMover(xMover);
            DS.RefreshDisplayShape();
            DS.MappingSelect();
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(iOK == 0 ? "抓取成功" : "抓取失败")}", false);
        }

        private void BtnCreateTemplate_Click(object sender, EventArgs e)
        {
            int iOK = 0;
            switch (xRegionNameCurrent)
            {
                case RegionName.BASE0:
                    iOK = xRecipe.Base0Train();
                    break;
                case RegionName.BASE1:
                    iOK = xRecipe.Base1Train();
                    break;
            }
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(iOK == 0 ? "创建成功" : "创建失败")}", false);
        }

        private void BtnSelectRegion_Click(object sender, EventArgs e)
        {
            bSelectRegion = !bSelectRegion;
            btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
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
            BoundRect(ref rectf, xFreeBmpOperate.Size);
            if (rectf.Width > 1 && rectf.Height > 1)
            {
                Bitmap bmpx = new Bitmap(xFreeBmpOperate.ToBitmap());
                Graphics g = Graphics.FromImage(bmpx);
                switch (xRegionNameCurrent)
                {
                    case RegionName.BASE0:

                        xRecipe.bmptemplate0 = xFreeBmpOperate.ToBitmap().Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        break;
                    case RegionName.BASE1:

                        xRecipe.bmptemplate1 = xFreeBmpOperate.ToBitmap().Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                        break;
                }
                g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectf });
                g.Dispose();
                DS.ReplaceDisplayImage(bmpx);
                bmpx.Dispose();

                
            }
            bSelectRegion = false;
            btnSelectRegion.BackColor = (bSelectRegion ? Color.Red : Color.FromArgb(192, 255, 192));
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
        RectangleF SimpleRect(System.Drawing.PointF PtF, Size size)
        {
            RectangleF rect = new RectangleF(PtF.X - size.Width / 2, PtF.Y - size.Height / 2, size.Width, size.Height);
            return rect;
        }

        #endregion

    }
}
