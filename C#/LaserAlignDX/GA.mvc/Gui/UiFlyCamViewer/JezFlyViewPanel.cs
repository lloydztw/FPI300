#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using LaserAlignDX.AoiModel;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class JezFlyViewPanel : UserControl, IvFlyCamViewUI
    {
        #region PRIVATE_DATA
        CviFlyAoiResultBox _cviResultBox = new CviFlyAoiResultBox();
        #endregion

        public JezFlyViewPanel()
        {
            InitializeComponent();
            initGui();
            if (!DesignMode)
            {
                _cviResultBox.Visible = false;
                ImgViewer.AddInteractor(_cviResultBox);
            }
        }
        void initGui()
        {
            jezTransImageViewPanel1.TitleBar.Height = 0;
            jezTransImageViewPanel1.StatusBar.Height = 0;
            jezTransImageViewPanel1.picIcon.Visible = false;
            jezTransImageViewPanel1.TitleBar.BackColor = MatViewer.BackColor;
            jezTransImageViewPanel1.lblTitle.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
        }

        public IvImageViewer ImgViewer
        {
            get => jezTransImageViewPanel1.ImgViewer;
        }
        public CvMatViewer MatViewer
        {
            get => jezTransImageViewPanel1.MatViewer;
        }
        Control IvFlyCamViewUI.Window => this;

        public void Update(FlyAoiResult flyAoiResult)
        {
            _cviResultBox.Update(flyAoiResult);
            _cviResultBox.Visible = flyAoiResult != null;
            string text = formatText(flyAoiResult);
            jezTransImageViewPanel1.UpdateImage(flyAoiResult?.MetaData?.bmpFly, text, false);
        }

        #region PRIVATE_FUCNTIONS
        string formatText(FlyAoiResult flyAoiResult)
        {
            var meta = flyAoiResult?.MetaData;
            if (meta != null)
                return $"FlyCam [{meta.flyID.ShowID}]";
                //return formatText(meta.flyID.ShowID, flyAoiResult.OffsetX, flyAoiResult.OffsetY, flyAoiResult.OffsetAngle);
            return "";
        }
        string formatText(int flyShowIndex, float offsetX, float offsetY, float offsetAngle)
        {
            string text = $"[{flyShowIndex}]" +
                        $" x:{offsetX:0.000}," +
                        $" y:{offsetY:0.000}," +
                        $" a:{offsetAngle:0.000}";
            return text;
        }
        #endregion
    }
}
