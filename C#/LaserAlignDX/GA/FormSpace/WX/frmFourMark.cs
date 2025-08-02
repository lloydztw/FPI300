using Common.RecipeSpace;
using JetEazy.BasicSpace;
using JetEazy.PropertyGridSpace;
using JzDisplay;
using LaserAlignDX.OPSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaserAlignDX.FormSpace.WX
{
    public partial class frmFourMark : Form
    {

        Bitmap bmpOperate = new Bitmap(1, 1);

        protected RecipeMiniX6Class myRecipe
        {
            get { return RecipeMiniX6Class.Instance; }
        }
        MarkItemClass markItemClass
        {
            get { return myRecipe.MarkCollectionStr[cboMarkIndex.SelectedIndex]; }
        }

        Button btnGetImage;
        Button btnCheck;
        Button btnSavePara;

        public frmFourMark()
        {
            InitializeComponent();

            this.Load += FrmFourMark_Load;
            this.SizeChanged += FrmFourMark_SizeChanged;
            
        }

        private void FrmFourMark_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }

        private void FrmFourMark_Load(object sender, EventArgs e)
        {
            this.Text = "调整Mark点参数界面";

            init_Display();
            update_Display();

            btnGetImage = button1;
            btnCheck = button2;
            btnSavePara = button3;
          
            cboMarkIndex.SelectedIndexChanged += CboMarkIndex_SelectedIndexChanged;
            cboMarkIndex.SelectedIndex = 0;

            btnGetImage.Click += BtnGetImage_Click;
            btnCheck.Click += BtnCheck_Click;
            btnSavePara.Click += BtnSavePara_Click;

            pgPara.PropertyValueChanged += PgPara_PropertyValueChanged;

            LanguageExClass.Instance.EnumControls(this);
        }

        private void PgPara_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //switch (e.ChangedItem.Label)
            //{
            //    case "Brightness":
            //        markItemClass.Brightness = (int)e.ChangedItem.Value;
            //        break;
            //    case "Contrast":
            //        markItemClass.Contrast = (int)e.ChangedItem.Value;
            //        break;
            //}
            markItemClass.SaveIniSetup();
            _preImage();
        }

        private void CboMarkIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            pgPara.SelectedObject = markItemClass.XPropsMark;
        }

        private void BtnSavePara_Click(object sender, EventArgs e)
        {
            myRecipe.SaveFourMark();
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            _getImage();
            markItemClass.GetImageMarkCenter(bmpOperate, true, out RectangleF maxtemp, out Bitmap bmpoutputtemp, out int maxareatemp);
            DS.ReplaceDisplayImage(bmpoutputtemp);
        }

        private void BtnGetImage_Click(object sender, EventArgs e)
        {

            _getImage();
            DS.ReplaceDisplayImage(bmpOperate);

        }

        void _getImage()
        {
            bmpOperate.Dispose();
            switch (cboMarkIndex.SelectedIndex)
            {
                case 0:
                    bmpOperate = myRecipe.bmpORG.Clone(myRecipe.fourmark1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    break;
                case 1:
                    bmpOperate = myRecipe.bmpORG.Clone(myRecipe.fourmark2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    break;
                case 2:
                    bmpOperate = myRecipe.bmpORG.Clone(myRecipe.fourmark3, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    break;
                case 3:
                    bmpOperate = myRecipe.bmpORG.Clone(myRecipe.fourmark4, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    break;
                default:
                    bmpOperate = myRecipe.bmpORG.Clone(myRecipe.fourmark1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    break;
            }
        }
        void _preImage()
        {
            _getImage();

            //预处理
            Bitmap bmptemp = new Bitmap(bmpOperate);
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmptemp = grayscale.Apply(bmptemp);
            AForge.Imaging.Filters.BrightnessCorrection brightnessCorrection = new AForge.Imaging.Filters.BrightnessCorrection(markItemClass.Brightness);
            bmptemp = brightnessCorrection.Apply(bmptemp);
            AForge.Imaging.Filters.ContrastCorrection contrastCorrection = new AForge.Imaging.Filters.ContrastCorrection(markItemClass.Contrast);
            bmptemp = contrastCorrection.Apply(bmptemp);

            DS.ReplaceDisplayImage(bmptemp);
            bmptemp.Dispose();

        }
        void init_Display()
        {
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
        }
        void update_Display(bool eChangeToDefault = true)
        {
            DS.Refresh();
            if (eChangeToDefault)
                DS.DefaultView();
        }
    }
}
