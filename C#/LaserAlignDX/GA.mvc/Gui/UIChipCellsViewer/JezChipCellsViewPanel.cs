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
using JetEazy.OpenCV;
using JetEazy.OpenCV.Viewer;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Gui.ChipCellsViewer;
using OpenCvSharp;
using OpenCvSharp.Internal.Vectors;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;

namespace LaserAlignDX.UISpace.ChipCellsViewer
{
    public partial class JezChipCellsViewPanel : UserControl, IvChipCellsViewer
    {
        #region PRIVATE_DATA
        CviCellsResultBoxes _resultBox = new CviCellsResultBoxes();
        bool _isActive = false;
        #endregion

        public JezChipCellsViewPanel()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            _resultBox.Visible = false;
            _resultBox.lblSummaryTitle = lblTitle;
            ImgViewer.AddInteractor(_resultBox);
            
            jezTransImageViewPanel1.AttachPopupMenu(contextMenuStrip1);

            HandleCreated += (s, e) => update_ActiveGuiStatus();
            HandleDestroyed += (s, e) => cleanUp();
            timBlinker.Tick += (s, e) => blinking();
        }

        public CarrierEnum CarrierID
        {
            get => _resultBox.ActiveCarrierID;
            set => _resultBox.ActiveCarrierID = value;
        }
        public IvImageViewer ImgViewer
        {
            get => jezTransImageViewPanel1.ImgViewer;
        }
        public CvMatViewer MatViewer
        {
            get => jezTransImageViewPanel1.MatViewer;
        }

        Control IvChipCellsViewer.Window => this;
        Control lblTitle => jezTransImageViewPanel1.lblTitle;
        Control lblBlinker => jezTransImageViewPanel1.lblBlinker;
        PictureBox picIcon => jezTransImageViewPanel1.picIcon;

        [Browsable(false)]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    update_ActiveGuiStatus();
                }
            }
        }
        public bool HasImage()
        {
            return MatViewer.Image != null;
        }
        void IvChipCellsViewer.Reset()
        {
            _resultBox.Reset();
            //Mat old = this.MatViewer.Image;
            //if (old != null)
            //    old.SetTo(Scalar.Black);
            this.MatViewer.Invalidate();
        }
        void IvChipCellsViewer.UpdateImageSrc(object fullfovImage, string srcName)
        {
            if (!IsHandleCreated)
                return;

            _resultBox.Reset();

            if (fullfovImage is GaBigImageHolder imgHolder)
            {
                Bitmap bmp = imgHolder.PeekBitmap();
                //lblTitle.Text = srcName;
                //update_LineScanImage(bmp);
                jezTransImageViewPanel1.UpdateImage(bmp, srcName, disposeSrc: false);
            }
            else if (fullfovImage is CMvdImage mvdImage)
            {
                lblTitle.Text = srcName;
                update_LineScanImage(mvdImage);
            }
            else
            {
            }
        }
        void IvChipCellsViewer.UpdateCells(IEnumerable<CELL> cells, int mode)
        {
            if (!IsHandleCreated)
                return;

            _resultBox.UpdateResult(cells, mode);
            _resultBox.Visible = true;

            this.MatViewer.Invalidate();
        }

        #region PRIVATE_IMAGE_FUCNTIONS
        /// <summary>
        /// Caller 必須負責 mvdImage 生命週期
        /// </summary>
        void update_LineScanImage(CMvdImage mvdImage)
        {
            Mat old = this.MatViewer.Image;
            Mat img = GaImageUtil.PeekMat(mvdImage);
            this.MatViewer.CopyFrom(img);
            old?.Dispose();
        }

        /// <summary>
        /// Caller 必須負責 bmp 生命週期
        /// </summary>
        void update_LineScanImage(Bitmap bmp)
        {
            Mat old = this.MatViewer.Image;
            using (var bridge = new QxImageBridge(bmp))
            {
                this.MatViewer.CopyFrom(bridge.Image);
            }
            old?.Dispose();
        }

        void update_ActiveGuiStatus()
        {
            lblBlinker.BackColor = _isActive ? Color.Lime : Color.DimGray;
            picIcon.BackgroundImage = _isActive ? Properties.Resources.ActiveCarrier : Properties.Resources.PassiveCarrier;
            //blink(_isActive);
        }
        void blink(bool enabled)
        {
            timBlinker.Enabled = enabled;
            if(!enabled)
            {
                lblBlinker.BackColor = Color.DimGray;
                picIcon.BackColor = Color.Black;
            }
        }
        void blinking()
        {
            bool isON = lblBlinker.Tag != null;
            isON = !isON;
            lblBlinker.Tag = isON ? lblBlinker : null;
            lblBlinker.BackColor = isON ? Color.Lime : Color.DimGray;
            picIcon.BackColor = isON ? Color.Cyan : Color.Black;
        }

        void cleanUp()
        {
            try
            {
                this.MatViewer.Image?.Dispose();
                //cvMatViewer.Image = null;
            }
            catch
            {

            }
        }
        #endregion
    }
}
