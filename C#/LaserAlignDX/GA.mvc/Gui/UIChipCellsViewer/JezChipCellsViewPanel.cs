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
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using OpenCvSharp;
using System.Collections.Generic;
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
        #endregion

        public JezChipCellsViewPanel()
        {
            InitializeComponent();
            cvMatViewer.Attach(lblCoordInfo, lblBlinker);
            cvMatViewer.AddInteractor(_resultBox);
            _resultBox.Visible = false;
            _resultBox.lblSummaryTitle = this.lblTitle;
            HandleDestroyed += (s, e) => cleanUp();
        }
        public IvImageViewer ImgViewer
        {
            get => cvMatViewer;
        }

        Control IvChipCellsViewer.Window => this;
        
        void IvChipCellsViewer.Reset()
        {
            _resultBox.Reset();
            Mat old = cvMatViewer.Image;
            if (old != null)
                old.SetTo(Scalar.Black);
            cvMatViewer.Invalidate();
        }
        void IvChipCellsViewer.UpdateImageSrc(object fullfovImage, string srcName)
        {
            if (!IsHandleCreated)
                return;

            _resultBox.Reset();

            if (fullfovImage is GaBigImageHolder imgHolder)
            {
                Bitmap bmp = imgHolder.PeekBitmap();
                update_LineScanImage(bmp);
            }
            else if (fullfovImage is CMvdImage mvdImage)
            {
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

            cvMatViewer.Invalidate();
        }

        #region PRIVATE_IMAGE_FUCNTIONS
        /// <summary>
        /// Caller 必須負責 mvdImage 生命週期
        /// </summary>
        void update_LineScanImage(CMvdImage mvdImage)
        {
            Mat old = cvMatViewer.Image;
            Mat img = GaImageUtil.PeekMat(mvdImage);
            cvMatViewer.CopyFrom(img);
            old?.Dispose();
        }

        /// <summary>
        /// Caller 必須負責 bmp 生命週期
        /// </summary>
        void update_LineScanImage(Bitmap bmp)
        {
            Mat old = cvMatViewer.Image;
            using (var bridge = new QxImageBridge(bmp))
            {
                cvMatViewer.CopyFrom(bridge.Image);
            }
            old?.Dispose();
        }

        //Mat peekMat(CMvdImage mvdImage)
        //{
        //    MVD_IMAGE_DATA_INFO info = mvdImage.GetImageData();
        //    MVD_DATA_CHANNEL_INFO ch0 = info.stDataChannel[0];
        //    int w = (int)ch0.nRowStep;
        //    int h = (int)(ch0.nLen / ch0.nRowStep);
        //    var bytes = ch0.arrDataBytes;
        //    Mat mat = new Mat(h, w, MatType.CV_8UC1, bytes, w);
        //    return mat;
        //}

        void cleanUp()
        {
            try
            {
                cvMatViewer.Image?.Dispose();
                //cvMatViewer.Image = null;
            }
            catch
            {

            }
        }
        #endregion
    }
}
