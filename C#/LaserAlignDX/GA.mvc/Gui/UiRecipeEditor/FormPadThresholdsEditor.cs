#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-08 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.Mvc.Model;
using LeTian.AoiLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormPadThresholdsEditor : Form
    {
        #region GLOBAL_MESS
        XRecipe _xRecipe => XRecipe.Instance;
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        Mat _imgOrg;
        Mat _imgBinary;
        bool _isModified = false;
        #endregion

        public FormPadThresholdsEditor()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;

            // 隱藏 TitleBar 與 StatusBar
            jezTransImageViewPanel1.TitleBar.Height = 0;
            jezTransImageViewPanel1.StatusBar.Height = 0;
            jezTransImageViewPanel1.picIcon.Visible = false;

            if (!DesignMode)
            {
                updateSettings(false);

                btnOK.Click += BtnOK_Click;
                numBinaryThreshold.ValueChanged += NumBinaryThreshold_ValueChanged;
                numDistTransThreshold.ValueChanged += NumBinaryThreshold_ValueChanged;

                FormClosed += FormPadThresholdsEditor_FormClosed;
                Load += (s, e) => tryApplyFilters();
                _isModified = false;
            }
        }



        public void SetSrcImage(Bitmap srcBmp, bool disposeSrc)
        {
            if (srcBmp == null)
                return;

            _imgOrg?.Dispose();
            _imgOrg = BitmapConverter.ToMat(srcBmp);
            if (disposeSrc)
                srcBmp.Dispose();

            //applyFilters();
        }

        #region EVENT_HANDLERS
        private void NumBinaryThreshold_ValueChanged(object sender, EventArgs e)
        {
            updateSettings(true);
            tryApplyFilters();
            _isModified = true;
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            commit();
            Close();
        }
        private void FormPadThresholdsEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            rollback();
        }
        #endregion

        void updateSettings(bool toModel)
        {
            var settings = _xRecipe.InspectParams;
            if (settings == null)
                return;

            if (toModel)
            {
                settings.xGridPadThreshold = (int)numBinaryThreshold.Value;
                settings.xDistTransThreshold = (int)numDistTransThreshold.Value;
            }
            else
            {
                GaUtil.SetNum(numBinaryThreshold, (decimal)settings.xGridPadThreshold);
                GaUtil.SetNum(numDistTransThreshold, (decimal)settings.xDistTransThreshold);
            }
        }
        void tryApplyFilters()
        {
            if (_imgOrg == null)
                return;

            if (_imgBinary == null)
                _imgBinary = new Mat();

            var filter = new EzPadsGridFinder();
            filter.PadThreshold = (int)numBinaryThreshold.Value;
            filter.DistTransThreshold = (int) numDistTransThreshold.Value;

            filter.TryApplyFilters(_imgOrg, _imgBinary);

            jezTransImageViewPanel1.MatViewer.Image = _imgBinary;
        }
        void commit()
        {
            if (_isModified)
            {
                _xRecipe.Save();
                _isModified = false;
            }
        }
        void rollback()
        {
            if (_isModified)
            {
                _xRecipe.Load();
                _isModified = false;
            }
        }
        void disposeImages()
        {
            _imgOrg?.Dispose();
            _imgOrg = null;
            _imgBinary?.Dispose();
            _imgBinary = null;
        }
}
}
