using JetEazy.QMath;
using LaserAlignDX.Mvc.Ctrl;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormRecipeEditor : Form, IvRecipeEditorUI
    {
        public FormRecipeEditor()
        {
            InitializeComponent();
            initDataGridView();
            rdoCarriers = new[] { rdoCarrier1, rdoCarrier2 };

            var ctrl = new GaRecipeEditCtrl();
            ctrl.Attach(this);
        }

        void initDataGridView()
        {
            DataGridView dgv = gvCalibPointsDataGridView1.DataGridView;

            while (dgv.Rows.Count > 2)
                dgv.Rows.RemoveAt(dgv.Rows.Count - 1);

            dgv.Rows[0].Cells[0].Value = "吸嘴1";
            dgv.Rows[1].Cells[0].Value = "吸嘴2";

            int col = dgv.Columns.Count - 2;
            foreach (string coordName in new[] { "World X", "World Y" })
            {
                dgv.Columns[col].HeaderText = coordName;
                dgv.Columns[col].ReadOnly = true;
                col++;
            }

            dgv.Columns[0].DefaultCellStyle.SelectionBackColor = dgv.DefaultCellStyle.BackColor;
            dgv.BackgroundColor = dgv.Parent.BackColor;

            //// 暫時隱藏
            //dgv.Visible = false;
            //btnWriteCoordsToPlc.Visible = false;
        }

        Control IvRecipeEditorUI.Window => this;

        JezTransImageViewPanel IvRecipeEditorUI.ImgViewer => jezTransImageViewPanel1;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => propertyGrid1;

        Button IvRecipeEditorUI.btnLoadImage => btnLoadImage;
        Button IvRecipeEditorUI.btnGrabImage => btnGrabImage;
        Button IvRecipeEditorUI.btnSaveImage => btnSaveImage;

        public RadioButton[] rdoCarriers
        {
            get; 
            private set;
        }
        Button IvRecipeEditorUI.btnPickGoldenChipRegion => btnPickGoldenRegion;
        Button IvRecipeEditorUI.btnAutoCreateCellRegions => btnCreateCellRegions;

        Button IvRecipeEditorUI.btnOpenTemplateMatchWindow => btnOpenTemplateMatchWindow;
        Button IvRecipeEditorUI.btnOpenEmptyTrayWindow => btnOpenEmptyTrayWindow;
        Button IvRecipeEditorUI.btnOpenFlyCamRcpWindow => btnOpenFlyCamRcpWindow;
        Button IvRecipeEditorUI.btnOpenLightCtrlWindow => btnOpenLightCtrlWindow;
        Button IvRecipeEditorUI.btnWriteCoordsToPlc => btnWriteCoordsToPlc;

        Button IvRecipeEditorUI.btnCancel => btnCancel;
        Button IvRecipeEditorUI.btnOK => btnOK;

        void IvRecipeEditorUI.UpdateCoordsRef(QVector camPt, QVector worldPtSucker1, QVector worldPtSucker2)
        {
            //throw new System.NotImplementedException();
            DataGridView dgv = gvCalibPointsDataGridView1.DataGridView;
            int ri = 0;
            foreach (var worldPt in new[] { worldPtSucker1, worldPtSucker2 })
            {
                int ci = 1;
                dgv.Rows[ri].Cells[ci++].Value = camPt.X;
                dgv.Rows[ri].Cells[ci++].Value = camPt.Y;
                dgv.Rows[ri].Cells[ci++].Value = worldPt.X;
                dgv.Rows[ri].Cells[ci++].Value = worldPt.Y;
                ri++;
            }
        }
    }
}