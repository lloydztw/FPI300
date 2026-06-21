#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.GUI;
using EzCamera.Interface;
using LeTian.JxProps;
using System;

namespace EzAoiChipLocQC.Ctrl
{
    /// <summary>
    /// 建立 JxCamSettings 與 IEzCamera 之資料交換
    /// </summary>
    internal class CamJxCtrl
    {
        #region PROTECTED_MEMBERS
        protected JxCamSettings _settings;
        protected IEzCamera _camera;
        protected bool _bypassWindowEvents;
        #endregion

        public void Bind(IEzCamera camera, JxCamSettings camSettings)
        {
            bool isChanged = (camera != _camera);

            //強制設定新的 camera
            _camera = camera;

            if (_settings != camSettings && camSettings != null)
            {
                isChanged = true;
                var old = _settings;
                _settings = camSettings;
                connectEventHandlers(_settings);
                disconnectEventHandlers(old);
            }

            if (isChanged)
            {
                // 寫入相機設備
                UpdateData(true);
                // 再讀出來更新
                UpdateData(false);
            }
        }
        public void Bind(IEzCamera camera)
        {
            //if (_camera != camera)
            //{
            //    _camera = camera;
            //    // 寫入相機設備
            //    UpdateData(true);
            //    // 再讀出來更新
            //    UpdateData(false);
            //}
            Bind(camera, null);
        }
        public void Detach()
        {
            Bind(null, null);
            _settings = null;
            _camera = null;
        }
        public IEzCamera Camera
        {
            get { return _camera; }
        }

        #region CONNECT_FUNCTIONS
        void initSettingsRange(JxCamSettings settings)
        {
            var drvCamera = this.Camera;
            if (drvCamera == null)
                return;

            if (settings != null)
            {
                drvCamera.GetExposureRange(out double min, out double max);
                _settings.ExposureTime.Range = new LeTian.JxProps.Range((decimal)min, (decimal)max, 10, 1);

                drvCamera.GetHardwareGainRange(out min, out max);
                _settings.HardwareGain.Range = new LeTian.JxProps.Range((decimal)min, (decimal)max, 0.1m, 2);
            }
        }
        void disconnectEventHandlers(JxCamSettings settings)
        {
            if (settings != null)
            {
                settings.Inverse.OnModified -= cam_Sw_Settings_OnModified;
                settings.Brightness.OnModified -= cam_Sw_Settings_OnModified;
                settings.Contrast.OnModified -= cam_Sw_Settings_OnModified;
                settings.ExposureTime.OnModified -= cam_Hw_Settings_OnModified;
                settings.HardwareGain.OnModified -= cam_Hw_Settings_OnModified;
                settings.Rotation.OnModified -= cam_Sw_Rotation_Changed;
            }
        }
        void connectEventHandlers(JxCamSettings settings)
        {
            if (settings != null)
            {
                initSettingsRange(settings);
                settings.Inverse.OnModified += cam_Sw_Settings_OnModified;
                settings.Brightness.OnModified += cam_Sw_Settings_OnModified;
                settings.Contrast.OnModified += cam_Sw_Settings_OnModified;
                settings.ExposureTime.OnModified += cam_Hw_Settings_OnModified;
                settings.HardwareGain.OnModified += cam_Hw_Settings_OnModified;
                settings.Rotation.OnModified += cam_Sw_Rotation_Changed;
            }
        }
        #endregion

        #region EVENT_HANDLERS
        void cam_Hw_Settings_OnModified(object sender, EventArgs e)
        {
            if (!_bypassWindowEvents)
                updateSettingsHW(_settings, true);
        }
        void cam_Sw_Settings_OnModified(object sender, EventArgs e)
        {
            if (!_bypassWindowEvents)
                updateSettingsSW(_settings, true);
        }
        void cam_Sw_Rotation_Changed(object sender, EventArgs e)
        {
            if (!_bypassWindowEvents)
                updateRotation(_settings, true);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void updateSettingsHW(JxCamSettings settings, bool toDevice)
        {
            var cam = this.Camera;
            if (cam != null && settings != null)
            {
                if (toDevice)
                {
                    cam.ExposureTime = (double)settings.ExposureTime.Value;
                    cam.HardwareGain = (double)settings.HardwareGain.Value;
                }
                else
                {
                    _bypassWindowEvents = true;
                    settings.ExposureTime.Value = (decimal)cam.ExposureTime;
                    settings.HardwareGain.Value = (decimal)cam.HardwareGain;
                    _bypassWindowEvents = false;
                }
            }
        }
        void updateSettingsSW(JxCamSettings settings, bool toDevice)
        {
            var cam = this.Camera;
            if (cam != null && settings != null)
            {
                if (toDevice)
                {
                    cam.InverseModeEnabled = settings.Inverse.Value;
                    cam.Brightness = (int)settings.Brightness.Value;
                    cam.Contrast = (int)settings.Contrast.Value;
                    cam.RotateAngle = (int)settings.Rotation.Value;
                }
                else
                {
                    _bypassWindowEvents = true;
                    settings.Inverse.Value = cam.InverseModeEnabled;
                    settings.Brightness.Value = cam.Brightness;
                    settings.Contrast.Value = cam.Contrast;
                    settings.Rotation.Value = cam.RotateAngle;
                    _bypassWindowEvents = false;
                }
            }
        }
        void updateRotation(JxCamSettings settings, bool toDevice)
        {
            var cam = this.Camera;
            if (cam != null && settings != null)
            {
                if (toDevice)
                {
                    cam.RotateAngle = (int)settings.Rotation.Value;
                }
                else
                {
                    _bypassWindowEvents = true;
                    settings.Rotation.Value = cam.RotateAngle;
                    _bypassWindowEvents = false;
                }
            }
        }
        #endregion

        public void UpdateData(bool toDevice)
        {
            updateSettingsHW(_settings, toDevice);
            updateSettingsSW(_settings, toDevice);
        }
    }

    /// <summary>
    /// 建立 JxCamViewSettings 與 IvCameraViewer 之資料交換
    /// </summary>
    internal class CamViewJxCtrl
    {
        #region PRIVATE_GUI_MEMBERS
        private JxCamViewerSettings _viewSettings;
        private IvCameraViewer _camViewer;
        private bool _bypassWindowEvents;
        #endregion

        public void Bind(IvCameraViewer viewer, JxCamViewerSettings viewSettings)
        {
            bool isChanged = _camViewer != viewer;

            //強制設定新的 viewer
            _camViewer = viewer;

            if (_viewSettings != viewSettings)
            {
                isChanged = true;
                var old = _viewSettings;
                _viewSettings = viewSettings;
                connectEventHandlers(_viewSettings);
                disconnectEventHandlers(old);
            }

            if (isChanged)
            {
                updateSettings(_viewSettings, true);
                updateSettings(_viewSettings, false);
            }
        }
        public void Bind(IvCameraViewer viewer)
        {
            if (_camViewer != viewer)
            {
                _camViewer = viewer;
                updateSettings(_viewSettings, true);
                updateSettings(_viewSettings, false);
            }
        }
        public void Detach()
        {
            disconnectEventHandlers(_viewSettings);
            _viewSettings = null;
        }
        public IvCameraViewer Viewer
        {
            get { return _camViewer; }
        }

        #region PRIVATE_FUNCTIONS
        void disconnectEventHandlers(JxCamViewerSettings settings)
        {
            if (settings != null)
            {
                settings.OnModified -= cam_View_Settings_OnModified;
            }
        }
        void connectEventHandlers(JxCamViewerSettings settings)
        {
            if (settings != null)
            {
                settings.OnModified += cam_View_Settings_OnModified;
            }
        }
        void updateSettings(JxCamViewerSettings settings, bool toModel)
        {
            if (_camViewer != null && settings != null)
            {
                if (toModel)
                {
                    _camViewer.GridLinesVisible = settings.ShowGridLines.Value;
                    _camViewer.CrosshairsVisible = settings.ShowCross.Value;
                    _camViewer.RulerVisible = settings.ShowRuler.Value;
                }
                else
                {
                    _bypassWindowEvents = true;
                    settings.ShowGridLines.Value = _camViewer.GridLinesVisible;
                    settings.ShowCross.Value = _camViewer.CrosshairsVisible;
                    settings.ShowRuler.Value = _camViewer.RulerVisible;
                    _bypassWindowEvents = false;
                }
            }
        }
        #endregion

        #region EVENT_HANDLERS
        void cam_View_Settings_OnModified(object sender, EventArgs e)
        {
            if (!_bypassWindowEvents)
                updateSettings(_viewSettings, true);
        }
        #endregion
    }
}
