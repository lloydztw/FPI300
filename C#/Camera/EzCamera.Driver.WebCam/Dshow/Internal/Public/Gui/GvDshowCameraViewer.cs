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


using Camera_NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DsCamera = Camera_NET.Camera;


namespace JetEazy.DShow.GUI
{
    public partial class GvDshowCameraViewer : UserControl, IDshowCameraViewer
    {
        public event EventHandler<string> OnFFmpegError;

        public GvDshowCameraViewer()
        {
            InitializeComponent();
            //_dsCamFactory.SetDisplayHostWnd(_wndDisplayPanel);
        }

        #region PRIVATE_DATA
        private DshowCameraFactory _dsCamFactory => DshowCameraFactory.Instance;
        private DsCamera _dsCamera = null;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Control _wndDisplayPanel => panel1;
        Control _lblRecBlinker => lblRecBlinker;
        Control _lblCoordInfo;
        #endregion

        public List<string> GetAvailableAudioDevices()
        {
            //return GetAvailableDevices("audio");
            return DshowCameraFactory.Instance.GetAvailableAudioDevices();
        }
        public List<string> GetAvailableCameras()
        {
            //return GetAvailableDevices("video");
            return DshowCameraFactory.Instance.GetAvailableCameras();
        }
        public List<Size> GetCurrentAvailableFrameSizes()
        {
            return _dsCamFactory.GetAvailableFrameSizes(_dsCamera);
        }

        #region PUBLIC_PROPERTIES
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)] // hide from property browser
        public Control Window
        {
            get
            {
                return this;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)] // hide from property browser
        public int CamIndex
        {
            get
            {
                //if (_camCtrl != null)
                //    return _camCtrl.CamID;
                //return -1;
                return _dsCamera != null ? _dsCamera.CamIndex : -1;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)] // hide from property browser
        public Camera DsCamera
        {
            get => _dsCamera;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)]
        public string FourCC
        {
            get
            {
                return _dsCamera != null ? _dsCamera.FourCC : "";
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)]
        public Size TargetFrameSize
        {
            get
            {
                return _dsCamera != null ? _dsCamera.TargetFrameSize : new Size(1, 1);
            }
            set
            {
                //if (_dsCamera != null)
                //    _dsCamera.TargetFrameSize = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)]
        public double TargetFrameRate
        {
            get
            {
                return _dsCamera != null ? _dsCamera.TargetFrameRate : 0;
            }
            set
            {
                //if (_dsCamera != null)
                //    _dsCamera.TargetFrameRate = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // do not serialize to code ever
        [Browsable(false)]
        public int AudioDeviceIndex
        {
            get
            {
                if (_dsCamera == null)
                    return -1;
                var names = GetAvailableAudioDevices();
                int index = names.IndexOf(_dsCamera.AudioDeviceName);
                return index;
            }
            set
            {
                if (_dsCamera != null)
                {
                    var names = GetAvailableAudioDevices();
                    if (value >= 0 && value < names.Count)
                        _dsCamera.AudioDeviceName = names[value];
                }
            }
        }
        #endregion

        public void OpenCamera(int dshowCamIndex, string iniFileName = null)
        {
            LoadAllSettings(iniFileName, dshowCamIndex);
        }
        public void CloseCamera()
        {
            //_dsCamFactory.SaveAllSettings(null, _dsCamera);
            string iniFileName = LibPaths.DEFAULT_INI_FILE;
            SaveAllSettings(iniFileName);

            _dsCamera?.Dispose();
            _dsCamera = null;
        }

        public bool IsLiveMode()
        {
            //return cameraControl1 != null;
            return _dsCamera != null && _dsCamera.IsLiveMode();
        }
        public void StartLiveMode()
        {
            //cameraControl1?.Camera?.RunGraph();
            _dsCamera?.StartLiveMode();
        }
        public void StopLiveMode()
        {
            //cameraControl1?.Camera?.StopGraph();
            _dsCamera?.StopLiveMode();
        }
        public Bitmap Snapshot()
        {
            //if (_dsCamCtrl != null)
            //{
            //    bool fromSource = true;
            //    var bmp = fromSource
            //             ? _dsCamCtrl.SnapshotSourceImage()
            //             : _dsCamCtrl.SnapshotOutputImage();
            //    return bmp;
            //}
            //return null;
            return _dsCamera?.Snapshot();
        }

        public bool IsRecording()
        {
            return _dsCamera != null && _dsCamera.IsRecording();
        }
        public void StartRecord(string videoFileName, double fps = 0, int segment_minutes = 0)
        {
            _dsCamera?.StartRecord(videoFileName, fps, segment_minutes);
        }
        public void StopRecord()
        {
            _dsCamera?.StopRecord();
        }

        public DialogResult ShowPinConfigDlg()
        {
            if (_dsCamera != null)
            {
                _dsCamera.ShowPinConfigDlg(this.Handle);
                return DialogResult.OK;
            }
            return DialogResult.None;
        }
        public DialogResult ShowCameraCtrlDlg()
        {
            if (_dsCamera != null)
            {
                _dsCamera.ShowCameraCtrlDlg(this.Handle);
                return DialogResult.OK;
            }
            return DialogResult.None;
        }

        public void LoadAllSettings(string fileName, int dshowCamIndex = -1)
        {
            if (_dsCamera != null && _dsCamera.CamIndex == dshowCamIndex && dshowCamIndex >= 0)
                return;

            _dsCamera?.Dispose();
            _dsCamera = _dsCamFactory.OpenCamera(_wndDisplayPanel, dshowCamIndex, fileName);

            if (_dsCamera != null)
            {
                attach_ds_camera_event_handlers();
            }

            #region 檢查_AUDIO
            if (_dsCamera != null)
            {
                var names = GetAvailableAudioDevices();
                if (!names.Contains(_dsCamera.AudioDeviceName))
                    _dsCamera.AudioDeviceName = "";
            }
            #endregion
        }
        public void SaveAllSettings(string fileName)
        {
            _dsCamFactory.SaveAllSettings(fileName, _dsCamera);
        }

        #region PRIVATE_FUNCTIONS
        private void attach_ds_camera_event_handlers()
        {
            if (_dsCamera != null)
            {
                _dsCamera.OnFrameRateUpdated += camera_OnFrameRateUpdated;
                _dsCamera.OnFFmpegError += (s, e) => OnFFmpegError?.Invoke(s, e);
            }
        }
        private void camera_OnFrameRateUpdated(object sender, System.EventArgs e)
        {
            if (InvokeRequired)
            {
                // 使用 BeginInvoke 以避免 Close Graph 時, 發生 DEAD LOCK !!!
                BeginInvoke((EventHandler)camera_OnFrameRateUpdated);
            }
            else
            {
                try
                {
                    if (!IsHandleCreated)
                        return;

                    bool isRecording = _dsCamera != null && _dsCamera.IsRecording();

                    if (_lblCoordInfo != null)
                    {
                        _lblCoordInfo.Text = $"FPS = {Math.Round(_dsCamera.CurrentFrameRate, 1):0.0}";
                        if (isRecording)
                            _lblCoordInfo.Text += " (錄影中...)";
                    }

                    if (OptUseBlinker && _lblRecBlinker != null)
                    {
                        if (isRecording)
                        {
                            _lblRecBlinker.Visible = !_lblRecBlinker.Visible;
                            _lblRecBlinker.Refresh();
                        }
                        else
                        {
                            _lblRecBlinker.Visible = false;
                        }
                    }
                }
                catch
                {
                }
            }
        }
        #endregion

        #region PUBLIC_GUI_FUNCTIONS
        public bool OptUseBlinker
        {
            get;
            set;
        }
        public void AttachGui(Control lblFrameRate)
        {
            _lblCoordInfo = lblFrameRate;
        }
        //public void HandleFormClosing(Form frm, FormClosingEventArgs e)
        //{
        //    if (IsLiveMode())
        //    {
        //        e.Cancel = true;
        //        if (IsRecording())
        //            StopRecord();
        //        StopLiveMode();
        //        delay_action((Action)frm.Close, 1000);
        //    }
        //    else
        //    {
        //        CloseCamera();
        //    }
        //}

        //private void delay_action(Action action, int delay)
        //{
        //    if (action == null)
        //        return;
        //    new Action(() =>
        //    {
        //        System.Threading.Thread.Sleep(delay);
        //        this.BeginInvoke(action);
        //    }).BeginInvoke(null, null);
        //}
        #endregion
    }
}
