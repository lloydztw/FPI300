using Common.RecipeSpace;
using JzDisplay;
using LaserAlignDX.OPSpace;
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
using VisionDesigner.BlobFind;
using VM.PlatformSDKCS;
using WorldOfMoveableObjects;

namespace LaserAlignDX.FormSpace
{
    public partial class frmNoTrayX3 : Form
    {
        RegionCellX3Class m_ItemTest = null;
        Mover xMovers = new Mover();

        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }


        Button btnCheck;


        public frmNoTrayX3()
        {
            InitializeComponent();
            this.Load += FrmNoTrayX3_Load;
        }

        private void FrmNoTrayX3_Load(object sender, EventArgs e)
        {
            init_Display();
            update_Display();

            DS1.ReplaceDisplayImage(xRecipe.bmpprintNoTraytemplate);

            propertyGrid1.SelectedObject = NoTrayParaClass.Instance;

            this.Text = "设定BLOB模板界面";

            btnCheck = button1;
            btnCheck.Click += BtnCheck_Click;


        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            if (m_ItemTest == null)
            {
                m_ItemTest = new RegionCellX3Class();
            }
            m_ItemTest.IsSaveDebugPicture = NoTrayParaClass.Instance.xSaveDebugImage;
            CBlobInfo cBlob = m_ItemTest.CheckBlobNoTray(xRecipe.bmpprintNoTraytemplate);
            if (cBlob != null)
            {
                DS2.ReplaceDisplayImage(xRecipe.bmpprintNoTraytemplate);
                RectangleF x = new RectangleF(cBlob.RectInfo.CenterX - cBlob.RectInfo.Width / 2,
                    cBlob.RectInfo.CenterY - cBlob.RectInfo.Height / 2,
                    cBlob.RectInfo.Width,
                    cBlob.RectInfo.Height);

                DS2.ClearStaticMover();
                //DS2.ClearStaticMover();
                xMovers.Clear();
                JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), x);
                //_rect.Angle = cBlob.RectInfo.Angle;
                _rect.RelateLevel = 7;
                //_rect.RelateNo = i;
                _rect.RelatePosition = 0;
                _rect.SetAngle(-cBlob.BoxInfo.Angle + 90);
                xMovers.Add(_rect);

                DS2.SetStaticMover(xMovers);
                DS2.RefreshDisplayShape();
                DS2.MappingSelect();

                update_Display(false);

                if (m_ItemTest.inspectReason == InspectReason.PASS)
                {
                    richTextBox1.Text = $"检测无料" + Environment.NewLine;
                }
                else
                {
                    richTextBox1.Text = $"疑似有料" + Environment.NewLine;
                }
                richTextBox1.Text += $"面积:{cBlob.AreaF}" + Environment.NewLine;
            }
            else
            {
                richTextBox1.Text = $"寻找失败";
            }

        }

        void init_Display()
        {
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS1.CaptureAction += DS_CaptureAction;
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

        }
    }
}
