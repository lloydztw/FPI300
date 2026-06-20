#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CameraViewerT = JetEazy.DShow.GUI.GvDshowCameraViewer;


namespace JetEazy.DShow.Tool
{
    public partial class FormDshowCamTool : Form
    {
        static bool OPT_USE_DSHOW_PROPERTY_DLG = true;
        static string INI_FILE => ToolPaths.INI_FILE;
        static string AUTO_DUMP_VIDEO_FILE_NAME => ToolPaths.AUTO_DUMP_VIDEO_FILE_NAME;
        static string AUTO_DUMP_IMAGE_FILE_NAME => ToolPaths.AUTO_DUMP_IMAGE_FILE_NAME;

        #region PRIVATE_DATA
        CameraViewerT _dsCamViewer => gPaneUsbCameraViewer1;
        bool _bypassWindowEvents = false;
        bool _dirty_ini = false;
        #endregion

        #region GUI_LINKS
        IEnumerable<Button> iterateButtons()
        {
            foreach (var c in tableLayoutPanelAtLeft.Controls)
            {
                if (c is Button btn)
                    yield return btn;
            }
        }
        #endregion

        public FormDshowCamTool()
        {
            InitializeComponent();
            ToolPaths.InitFolders();
            tableLayoutPanel0.Dock = DockStyle.Fill;
            this.Load += new EventHandler(FormTestCam_Load);
            this.FormClosing += new FormClosingEventHandler(FormTestCam_FormClosing);
            this.gPaneUsbCameraViewer1.AttachGui(lblCurrentFps);
            cboAvailableCameras.SelectedIndexChanged += cboAvailableCameras_SelectedIndexChanged;
            cboAvailableAudios.SelectedIndexChanged += cboAudioDevices_SelectedIndexChanged;
        }
        public int DsCameraIndex = -1;

        private void FormTestCam_Load(object sender, EventArgs e)
        {
            _postInitGui();
            _updateAvailableCameras();
            _updateAvailableAudios();
            _loadAllSettings(silent: true);
            _updateActiveCamera();
            _updateActiveAudio(false);
            _updateGuiStatus();

            if (DsCameraIndex >= 0)
                cboAvailableCameras.SelectedIndex = DsCameraIndex;
        }
        private void FormTestCam_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dirty_ini)
            {
                _saveAllSettings(silent: true);
            }
            _handleFormClosing(this, e);
        }

        private void cboAudioDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents)
                return;

            _updateActiveAudio(true);
            _dirty_ini = true;
        }
        private void cboAvailableCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents)
                return;

            _openCamera(cboAvailableCameras.SelectedIndex);
            _updateAvailableSizes(autoSelectToFirst: true);
            _dirty_ini = true;
        }
        private void cboAvailableSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents)
                return;

            _updateResolution(true);
        }
        private void numFps_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents)
                return;

            _updateFrameRate(true);
            _updateInfo();
        }

        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            _loadAllSettings();
        }
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            _saveAllSettings();
        }
        private void btnCamSettings_Click(object sender, EventArgs e)
        {
            var ret = _dsCamViewer.ShowCameraCtrlDlg();
            if (ret == DialogResult.OK)
            {
                _dirty_ini = true;
            }
        }
        private void btnPinConfig_Click(object sender, EventArgs e)
        {
            var ret = _dsCamViewer.ShowPinConfigDlg();
            if (ret == DialogResult.OK)
            {
                _updateInfo();
                _dirty_ini = true;
            }
        }
        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            _snapshot();
        }
        private void btnRecord_Click(object sender, EventArgs e)
        {
            _startRecord();
        }
        private void btnRecStop_Click(object sender, EventArgs e)
        {
            _stopRecord();
        }
        private void _dsCamViewer_OnFFmpegError(object sender, string e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<string>)_dsCamViewer_OnFFmpegError, sender, e);
            }
            else
            {
                string errMsg = "FFmpeg 異常:\n\r\n\r" + e;
                MessageBox.Show(errMsg, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _dsCamViewer.StopRecord();
                _updateGuiStatus();
                return;
            }
        }

        #region PRIVATE_GUI_FUNCTIONS
        void _postInitGui()
        {
            //this.gwPaneClock1.timer1.Stop();
            //this.gwPaneClock1.timer1.Interval = 1000;
            //this.gwPaneClock1.timer1.Start();
            _dsCamViewer.OnFFmpegError += _dsCamViewer_OnFFmpegError;
        }
        void _updateAvailableCameras(bool autoSelectToFirst = false)
        {
            _bypassWindowEvents = true;

            var cbo = this.cboAvailableCameras;
            cbo.Items.Clear();

            var names = _dsCamViewer.GetAvailableCameras();
            foreach (var name in names)
                cbo.Items.Add(name);

            _bypassWindowEvents = false;

            if (autoSelectToFirst)
            {
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
        }
        void _updateAvailableAudios(bool autoSelectToFirst = false)
        {
            _bypassWindowEvents = true;

            var cbo = this.cboAvailableAudios;
            cbo.Items.Clear();

            var names = _dsCamViewer.GetAvailableAudioDevices();
            cbo.Items.Add("無");
            foreach (var name in names)
                cbo.Items.Add(name);

            _bypassWindowEvents = false;

            if (autoSelectToFirst)
            {
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
        }
        void _updateAvailableSizes(bool autoSelectToFirst = false)
        {
            if (OPT_USE_DSHOW_PROPERTY_DLG)
                return;

            _bypassWindowEvents = true;

            var cbo = this.cboAvailableSizes;
            cbo.Items.Clear();

            var sizes = _dsCamViewer.GetCurrentAvailableFrameSizes();
            foreach (var sz in sizes)
            {
                cbo.Items.Add(string.Format("{0}x{1}", sz.Width, sz.Height));
            }

            _bypassWindowEvents = false;

            if (autoSelectToFirst)
            {
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
        }
        void _updateActiveCamera(int index = -1)
        {
            _bypassWindowEvents = true;

            if (index < 0)
                index = _dsCamViewer != null ? _dsCamViewer.CamIndex : -1;
            if (index >= 0 && index < cboAvailableCameras.Items.Count)
                cboAvailableCameras.SelectedIndex = index;
            else
                cboAvailableCameras.SelectedIndex = -1;

            _bypassWindowEvents = false;
        }
        void _updateActiveAudio(bool toKernel)
        {
            var cbo = cboAvailableAudios;

            if (toKernel)
            {
                if (_dsCamViewer != null)
                {
                    _dsCamViewer.AudioDeviceIndex = cbo.SelectedIndex - 1;
                }
            }
            else
            {
                _bypassWindowEvents = true;
                cbo.SelectedIndex = _dsCamViewer != null ? _dsCamViewer.AudioDeviceIndex + 1 : -1;
                _bypassWindowEvents = false;
            }
        }
        void _updateResolution(bool toModel)
        {
            if (OPT_USE_DSHOW_PROPERTY_DLG)
                return;

            var cbo = this.cboAvailableSizes;
            if (toModel)
            {
                if (cbo.SelectedIndex < 0)
                    return;
                var str = cbo.Items[cbo.SelectedIndex].ToString();
                var strs = str.Split('x');
                if (strs.Length < 2)
                    return;
                int w, h;
                if (!int.TryParse(strs[0], out w) || !int.TryParse(strs[1], out h))
                    return;

                var size = new Size(w, h);
                if (_dsCamViewer.TargetFrameSize != size)
                {
                    _dsCamViewer.TargetFrameSize = size;
                    _updateResolution(false);
                }
            }
            else
            {
                _bypassWindowEvents = true;
                int idx = -1;
                var size = _dsCamViewer.TargetFrameSize;
                var sizeStr = string.Format("{0}x{1}", size.Width, size.Height);
                for (int i = 0, count = cbo.Items.Count; i < count; i++)
                {
                    if (string.Compare(sizeStr, cbo.Items[i].ToString(), true) == 0)
                    {
                        idx = i;
                        break;
                    }
                }
                cbo.SelectedIndex = idx;
                _bypassWindowEvents = false;
            }
        }
        void _updateFrameRate(bool toModel)
        {
            if (OPT_USE_DSHOW_PROPERTY_DLG)
                return;

            if (toModel)
            {
                _dsCamViewer.TargetFrameRate = (double)numFps.Value;
                _updateFrameRate(false);
            }
            else
            {
                _bypassWindowEvents = true;
                var fps = (decimal)_dsCamViewer.TargetFrameRate;
                fps = Math.Min(fps, numFps.Maximum);
                fps = Math.Max(fps, numFps.Minimum);
                numFps.Value = fps;
                _bypassWindowEvents = false;
            }
        }
        void _updateInfo()
        {
            //var settings = cameraViewer.AmCameraSettings;
            var sb = new System.Text.StringBuilder();
            sb.Append("Format = ");
            sb.AppendLine(_dsCamViewer.FourCC);
            sb.Append("Size = ");
            var sz = _dsCamViewer.TargetFrameSize;
            sb.AppendLine(string.Format("{0} x {1}", sz.Width, sz.Height));
            sb.Append("Fps = ");
            sb.Append(_dsCamViewer.TargetFrameRate.ToString("0.0"));
            lblInfo.Text = sb.ToString();

            _updateResolution(false);
            _updateFrameRate(false);
        }
        void _updateGuiStatus()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _updateGuiStatus()));
                return;
            }

            bool isRecording = _dsCamViewer != null && _dsCamViewer.IsRecording();
            foreach (Button btn in iterateButtons())
            {
                if (btn == btnRecStop)
                {
                    btnRecStop.Visible = isRecording;
                    btnRecStop.Enabled = isRecording;
                }
                else
                {
                    btn.Enabled = !isRecording;
                }
            }
            cboAvailableCameras.Enabled = !isRecording;
            cboAvailableAudios.Enabled = !isRecording;
        }
        #endregion

        string _getIniFileName()
        {
            //string fname = System.IO.Path.ChangeExtension(AppDomain.CurrentDomain.FriendlyName, ".ini");
            //QxPathUtility.InitDirectory(INI_PATH);
            //return System.IO.Path.Combine(INI_PATH, fname);
            return INI_FILE;
        }
        void _loadAllSettings(bool silent = false)
        {
            if (_dsCamViewer == null)
                return;

            //var camID = _dsCamViewer.CamID;
            var iniFileName = _getIniFileName();
            if (!silent)
            {
                var ret = MessageBox.Show("是否載入 " + iniFileName, "ini", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ret != DialogResult.Yes)
                    return;
            }

            _dsCamViewer.LoadAllSettings(iniFileName);
            _updateInfo();
            _updateGuiStatus();
        }
        void _saveAllSettings(bool silent = false)
        {
            var iniFileName = _getIniFileName();
            _dsCamViewer?.SaveAllSettings(iniFileName);
            if (!silent)
                MessageBox.Show("已經保存 " + iniFileName, "ini", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _dirty_ini = false;
        }

        void _openCamera(int dshowCamIndex)
        {
            this.DsCameraIndex = dshowCamIndex;
            if (dshowCamIndex < 0)
                return;

            var iniFileName = _getIniFileName();
            _dsCamViewer.OpenCamera(dshowCamIndex, iniFileName);

            var dsCamera = _dsCamViewer.DsCamera;
            this.DsCameraIndex = dsCamera != null ? dsCamera.CamIndex : -1;

            _updateInfo();
        }
        void _closeCamera()
        {
            _stopLiveMode();
            _dsCamViewer.CloseCamera();
        }

        void _snapshot()
        {
            var bmp = _dsCamViewer.Snapshot();

            if (bmp != null)
            {
                var fileName = AUTO_DUMP_IMAGE_FILE_NAME;
                bmp.Save(fileName);
                bmp.Dispose();
                MessageBox.Show("擷取圖檔已存入:\n\r\n\r" + fileName, "Snapshot");
            }
            else
            {
                MessageBox.Show("無法 擷取圖檔!", "Snapshot");
            }
        }
        void _startRecord()
        {
            if (!_dsCamViewer.IsRecording())
            {
                _dsCamViewer.StartRecord(AUTO_DUMP_VIDEO_FILE_NAME);
                BeginInvoke((Action)_updateGuiStatus);
            }
        }
        void _stopRecord()
        {
            if (_dsCamViewer != null && _dsCamViewer.IsRecording())
            {
                _dsCamViewer.StopRecord();
                _updateGuiStatus();
                string videoFileName = _dsCamViewer.DsCamera.OutputVideoFileName;
                MessageBox.Show("影像已存入:\n\r\n\r" + videoFileName, "DShow Recorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        void _stopLiveMode()
        {
            _stopRecord();
            if (_dsCamViewer != null && _dsCamViewer.IsLiveMode())
                _dsCamViewer.StopLiveMode();
        }

        void _handleFormClosing(Form frm, FormClosingEventArgs e)
        {
            if (frm != null && _dsCamViewer != null && _dsCamViewer.IsLiveMode())
            {
                e.Cancel = true;
                frm.Cursor = Cursors.WaitCursor;
                frm.Refresh();
                _closeCamera();
                _delayAction((Action)frm.Close, 250);
                return;
            }

            if (!e.Cancel)
            {
                _closeCamera();
            }
        }
        void _delayAction(Action action, int delay)
        {
            if (action != null)
            {
                new Action(() =>
                {
                    System.Threading.Thread.Sleep(delay);
                    this.BeginInvoke(action);
                }).BeginInvoke(null, null);
            }
        }
    }
}
