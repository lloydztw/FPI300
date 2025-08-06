using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using System;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector
{
    public partial class FormTestDemo : Form
    {
        const string PATH_RECIPES = "D:\\AUTOMATION\\Eazy FPI30\\Aoi\\Recipes";
        const string TEST_IMAGE_FILE = @"D:\Lloydz\JetEazy\Projects\FPI300\Images\boundary_test_all_pass.jpg";

        public FormTestDemo()
        {
            InitializeComponent();
            updateRecipeNames();

            FormClosed += (s, e) => AoiFactory.DisposeAll();
            button1.Click += (s, e) => test_OpenTool();
            button2.Click += (s, e) => test_DirectRunAoiModel(0);
            button3.Click += (s, e) => test_DirectRunAoiModel(1);
        }

        void updateRecipeNames()
        {
            var files = System.IO.Directory.GetFiles(PATH_RECIPES);
            cboRecipeNames.Items.Clear();
            foreach (var file in files)
                cboRecipeNames.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            if (cboRecipeNames.Items.Count > 0)
                cboRecipeNames.SelectedIndex = 0;
        }

        string getRecipeName()
        {
            return cboRecipeNames.Text;
        }

        void updateInfo(string msg)
        {
            label2.Text = msg;
            label2.Refresh();
        }

        void test_OpenTool()
        {
            string recipeName = getRecipeName();
            var frm = AoiFactory.OpenEmptyTrayInspectorTool(this, recipeName);
            frm.Show();
        }

        void test_DirectRunAoiModel(int option)
        {
            string recipeName = getRecipeName();
            var aoiModel = AoiFactory.InstanceModel(recipeName);
            if (aoiModel == null)
                return;

            if (option == 0)
            {
                updateInfo("使用 OpenCvSharp 載入圖檔");
                using (IEzImage img = new EzQuickImage())
                {
                    img.Load(TEST_IMAGE_FILE, 8);
                    aoiModel.RunAll(img, wait: true);
                }
            }
            else
            {
                updateInfo("使用 FreeImage 載入圖檔");
                using (IEzImage img = new EzFreeBitmap())
                {
                    img.Load(TEST_IMAGE_FILE, 8);
                    aoiModel.RunAll(img.Bitmap, wait: true);
                }
            }

            var result = aoiModel.GetResult();
            show_result(result);
        }

        void show_result(EzEmptyTrayResult result)
        { 
            string msg;
            bool isPass;

            if (result == null)
            {
                msg = "無結果!";
                isPass = false;
            }
            else
            {
                isPass = result.IsPass();
                msg = result.ToString();
                msg = msg.Replace(",", "\n");
            }

            updateInfo(msg);
        }
    }
}
