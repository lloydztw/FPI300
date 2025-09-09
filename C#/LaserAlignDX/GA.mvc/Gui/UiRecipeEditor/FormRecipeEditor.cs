using LaserAlignDX.Mvc.Ctrl;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormRecipeEditor : Form, IvRecipeEditorUI
    {
        public FormRecipeEditor()
        {
            InitializeComponent();

            var ctrl = new GaRecipeEditCtrl();
            ctrl.Attach(this);
        }

        Control IvRecipeEditorUI.Window => this;

        JezTransImageViewPanel IvRecipeEditorUI.ImgViewer => jezTransImageViewPanel1;
        Control IvRecipeEditorUI.wndVisionSettingsPanel => propertyGrid1;

        Button IvRecipeEditorUI.btnLoadImage => btnLoadImage;
        Button IvRecipeEditorUI.btnGrabImage => btnGrabImage;
        Button IvRecipeEditorUI.btnSaveImage => btnSaveImage;

        Button IvRecipeEditorUI.btnPickGoldenEmptyRegion => null;
        Button IvRecipeEditorUI.btnRunEmptyTrayInspect => null;

        Button IvRecipeEditorUI.btnPickGoldenChipRegion => btnPickGoldenRegion;
        Button IvRecipeEditorUI.btnCreateCellRegions => btnCreateCellRegions;

        Button IvRecipeEditorUI.btnOpenTemplateMatchWindow => btnOpenTemplateMatchWindow;
        Button IvRecipeEditorUI.btnOpenEmptyTrayWindow => btnOpenEmptyTrayWindow;
        Button IvRecipeEditorUI.btnOpenFlyCamRcpWindow => btnOpenFlyCamRcpWindow;
        Button IvRecipeEditorUI.btnOpenLightCtrlWindow => btnOpenLightCtrlWindow;

        Button IvRecipeEditorUI.btnCancel => btnCancel;
        Button IvRecipeEditorUI.btnOK => btnOK;
    }
}