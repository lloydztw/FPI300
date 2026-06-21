using EzCamera.GUI;
using JetEazy.GUI;
using AwFramework;
using System.Windows.Forms;

namespace EzDualMatch.GUI
{
    public partial class FormDualMatchMainWindow : Form //, IvAppMainWindow, IvSimpleMatchView
    {
        #region PRIVATE_DATA
        #endregion

        public FormDualMatchMainWindow()
        {
            InitializeComponent();
            build_gui();
        }

        #region GUI_LINKS
#if(false)
        Form IView.frmOwner => this;
        Control IView.Window => this;
        IvCameraViewer IvAppMainWindow.camLiveViewer => qzCameraViewer1;
        IvCameraPanel IvAppMainWindow.camPropertyPanel => jxCameraPropertyPanel1;
        Control IvAppMainWindow.lblCamCoordInfo => this.lblCoordInfo;
        Control IvAppMainWindow.lblCamBlinker => this.lblBlinker;
        Control IvAppMainWindow.lblCamFrameCount => this.lblTitle;
        Button IvAppMainWindow.btnOpenCamera => this.jxCameraPropertyPanel1.btnOpenCamera;

        IvSimpleMatchView IvAppMainWindow.matchView => this;
        Control IvSimpleMatchView.ImageViewer => qzCameraViewer1;
        Button IvSimpleMatchView.btnOpen => null;
        Button IvSimpleMatchView.btnRunMatch => this.jxCameraPropertyPanel1.btnMatch;
        Button IvSimpleMatchView.btnClearResult => this.jxCameraPropertyPanel1.btnClear;
        Control IvSimpleMatchView.lblInfo => this.jxCameraPropertyPanel1.lblDeviceInfo;
#endif
        #endregion

        IvZoneView OpView => gPaneOpZone1;
        IvZoneView MajorView => gPaneClientZone1;

        void build_gui()
        {
            tableLayoutPanel1.Dock = DockStyle.Fill;
            MajorView.TitleBar.Visible = false;
            MajorView.StatusBar.lblStatus.Parent.Visible = false;
            OpView.Docker.Add(gPaneProduction1);
            OpView.Docker.Add(gPaneSysSettings1);
            MajorView.Docker.Add(gvDualImageViewPanel1);
        }
    }
}
