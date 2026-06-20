#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2025
 * 2025-03-10 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using Camera_NET;
using EzCamera.Driver;
using EzCamera.Driver.DShow;
using EzCamera.Interface;
using EzCamera.Manager;
using JetEazy.DShow;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace EzCamera.GUI
{
    public partial class FormCameraConfig : Form
    {
        #region PRIVATE_DATA
        GlobalCameraFactory _globalCameraFactory => GlobalCameraFactory.Instance;
        AppCameraFactory _appCameraFactory = AppCameraFactory.Instance;
        List<IEzDeviceInfo> _appDeviceInfosCache;
        List<IEzDeviceInfo> _globalDeviceInfosCache;
        object _arg;
        #endregion

        public FormCameraConfig(object arg = null)
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.None;

            _arg = arg;
            // APP 所有 cameras config
            // 以當下的 AppCameraFactory 為準則
            //_appCameraFactory.FromJX(_arg);

            _globalDeviceInfosCache = new List<IEzDeviceInfo>(_globalCameraFactory.GetAvailableCameraInfos());
            _appDeviceInfosCache = new List<IEzDeviceInfo>(_appCameraFactory.GetAvailableCameraInfos());            

            updateAppDeviceInfos(check: true);
            updateGlobalDeviceInfos();
            initEventHandlers();
            updateButtonsStatus();
        }
        
        public void SetSelection(string infoStr)
        {
            int gid = 0;
            foreach (var info in _appDeviceInfosCache)
            {
                if (info.ToString() == infoStr)
                {
                    lstAppDeviceInfos.SelectedIndex = Math.Min(gid, lstAppDeviceInfos.Items.Count - 1);
                    SelectedGlobalCamID = gid;
                    SelectedDeviceInfo = info;
                    return;
                }
                gid++;
            }
        }

        public IEzDeviceInfo SelectedDeviceInfo
        {
            get;
            private set;
        }

        public int SelectedGlobalCamID
        {
            get; private set;
        }

        /// <summary>
        /// Runtime Result Flags
        /// </summary>
        public bool NeedsToForceRebuildCamera
        {
            get;
            private set;
        } = false;

        #region EVENT_HANDLERS
        private void initEventHandlers()
        {
            btnAdd.Click += (s, e) => addAppDeviceInfo();
            btnDel.Click += (s, e) => removeAppDeviceInfo();
            btnMovUp.Click += (s, e) => changeAppDeviceOrder(-1);
            btnMovDn.Click += (s, e) => changeAppDeviceOrder(+1);
            btnImport.Click += (s, e) => importGlobalDeviceInfos();
            btnOK.Click += (s, e) => confirmAppSelection();
            btnCancel.Click += (s, e) => cancel();
            btnDsVideoSettings.Click += (s, e) => openDshowVideoSettings();
            btnDsCamSettings.Click += (s, e) => openDshowCamSettings();
            lstAppDeviceInfos.SelectedIndexChanged += (s, e) => updateButtonsStatus();
            lstGlobalDeviceInfos.SelectedIndexChanged += (s, e) => updateButtonsStatus();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        bool isUsedByApp(IEzDeviceInfo deviceInfo)
        {
            //if(_jxConfigWork!=null)
            //{
            //    var vendorInfo = deviceInfo.ToString();
            //    foreach(var jxDevInfo in _jxConfigWork.IterateItem())
            //    {
            //        if (vendorInfo.Contains(jxDevInfo.VendorInfo))
            //            return true;
            //    }
            //}

            if (_appDeviceInfosCache != null)
            {
                string infoStr = deviceInfo.ToString();
                foreach(var info in _appDeviceInfosCache)
                    if(infoStr == info.ToString())
                        return true;
            }

            return false;
        }
        bool isGlobalAvailable(string vendorInfo)
        {
            if (vendorInfo != null && vendorInfo.ToUpper().Contains("SIM"))
                return true;
            if (_globalCameraFactory.GetDeviceInfo(vendorInfo) != null)
                return true;
            return false;
        }

        void updateAppDeviceInfos(bool check = false)
        {
            lstAppDeviceInfos.Items.Clear();

            var usedSimIDs = new List<int>();

            int gid = 0;
            foreach (var devInfo in _appDeviceInfosCache)
            {
                if (check)
                {
                    if (!isGlobalAvailable(devInfo.ToString()))
                        continue;
                }

                lstAppDeviceInfos.Items.Add(formatDisplayText(devInfo, gid));
                var vendorInfo = devInfo.ToString();
                if (vendorInfo.ToUpper().Contains("SIM"))
                {
                    //var strs = vendorInfo.Split(EzDispText.C_LOCAL);
                    //if (strs.Length > 1 && int.TryParse(strs[strs.Length - 1], out int simID))
                    //    usedSimIDs.Add(simID);
                    if (EzDispText.Parse(vendorInfo, out int simID))
                        usedSimIDs.Add(simID);
                }

                gid++;
            }

            autoAdjustGlobalSimDeviceInfos(usedSimIDs);
        }
        void changeAppDeviceOrder(int delta)
        {
            if (delta == 0)
                return;

            int index = lstAppDeviceInfos.SelectedIndex;
            int index2 = delta > 0 ? index + 1 : index - 1;

            var devInfos = _appDeviceInfosCache;

            if (index2 >= devInfos.Count || index2 < 0 ||
                index >= devInfos.Count || index < 0)
                return;

            var swap = devInfos[index2];
            devInfos[index2] = devInfos[index];
            devInfos[index] = swap;

            // update GUI
            updateAppDeviceInfos();
            setSelectedIndex(lstAppDeviceInfos, index2);
        }
        void removeAppDeviceInfo()
        {
            var indexA = lstAppDeviceInfos.SelectedIndex;
            if (indexA < 0)
                return;

            var devInfo = _appDeviceInfosCache;
            if (indexA >= devInfo.Count)
                return;

            devInfo.RemoveAt(indexA);

            // update GUI
            updateAllDataToGui(indexA);
        }
        void addAppDeviceInfo()
        {
            int indexA = lstAppDeviceInfos.SelectedIndex;
            int indexG = lstGlobalDeviceInfos.SelectedIndex;
            if (indexG < 0)
                return;

            var dispText = lstGlobalDeviceInfos.Items[indexG].ToString();
            parseDisplayText(dispText, out string vendorInfo);
            var newInfo = _globalCameraFactory.GetDeviceInfo(vendorInfo);

            if (indexA >= 0)
            {
                _appDeviceInfosCache.Insert(++indexA, newInfo);
            }
            else
            {
                _appDeviceInfosCache.Add(newInfo);
                indexA = _appDeviceInfosCache.Count - 1;
            }

            // update GUI
            updateAllDataToGui(indexA, indexG);
        }
        void confirmAppSelection()
        {
            try
            {
                _appCameraFactory.RebuildCache(_appDeviceInfosCache);
                _appCameraFactory.SaveIni();
                _appCameraFactory.ToJX(_arg);

                int index = lstAppDeviceInfos.SelectedIndex;
                if (index >= 0 && index < _appDeviceInfosCache.Count)
                {
                    //>>> SelectedDeviceInfo = _appCameraFactory.GetCameraInfo(index);
                    SelectedDeviceInfo = _appDeviceInfosCache[index];
                    SelectedGlobalCamID = index;
                }
                else
                {
                    SelectedDeviceInfo = null;
                    SelectedGlobalCamID = -1;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                SelectedDeviceInfo = null;
                DialogResult = DialogResult.Cancel;
            }
        }
        void cancel()
        {
            if (NeedsToForceRebuildCamera)
            {
                confirmAppSelection();
                return;
            }

            SelectedDeviceInfo = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void autoAdjustGlobalSimDeviceInfos(List<int> usedSimIDs)
        {
            bool changed = _globalCameraFactory.AutoAdjustGlobalSimDeviceInfos(usedSimIDs);
            if (changed)
            {
                _globalDeviceInfosCache = new List<IEzDeviceInfo>(_globalCameraFactory.GetAvailableCameraInfos());
            }
        }
        void updateGlobalDeviceInfos()
        {
            lstGlobalDeviceInfos.Items.Clear();
            if (_globalDeviceInfosCache == null)
                return;

            int gid = lstAppDeviceInfos.Items.Count;
            foreach (var info in _globalDeviceInfosCache)
            {
                if(!isUsedByApp(info))
                {
                    lstGlobalDeviceInfos.Items.Add(formatDisplayText(info, gid));
                    gid++;
                }
            }
        }
        void importGlobalDeviceInfos()
        {
            string vendorInfo = null;
            using (var dlg = new FormCameraVendors())
            {
                if (DialogResult.OK != dlg.ShowDialog())
                    return;
                vendorInfo = dlg.VendorName;
            }

            _globalCameraFactory.ImportVendor(vendorInfo, simNumber: 1);
            _globalDeviceInfosCache = new List<IEzDeviceInfo>(_globalCameraFactory.GetAvailableCameraInfos());

            // Update GUI
            updateGlobalDeviceInfos();
            var index = Math.Max(0, lstGlobalDeviceInfos.SelectedIndex);
            setSelectedIndex(lstGlobalDeviceInfos, index);
            _globalCameraFactory.SaveIni();
        }

        void updateAllDataToGui(int indexA = -1, int indexG = -1)
        {
            indexA = indexA >= 0 ? indexA : lstAppDeviceInfos.SelectedIndex;
            indexG = indexG >= 0 ? indexG : lstGlobalDeviceInfos.SelectedIndex;
            updateAppDeviceInfos();
            updateGlobalDeviceInfos();
            setSelectedIndex(lstAppDeviceInfos, indexA);
            setSelectedIndex(lstGlobalDeviceInfos, indexG);
            updateButtonsStatus();
        }
        void updateButtonsStatus()
        {
            var appIndex = lstAppDeviceInfos.SelectedIndex;
            var globalIndex = lstGlobalDeviceInfos.SelectedIndex;

            btnImport.Enabled = true;
            btnMovDn.Enabled = appIndex < lstAppDeviceInfos.Items.Count - 1;
            btnMovUp.Enabled = appIndex > 0;
            btnAdd.Enabled = globalIndex >= 0;
            btnDel.Enabled = appIndex >= 0;
            btnOK.Enabled = appIndex >= 0;

            string deviceInfoStr = appIndex >= 0 ? lstAppDeviceInfos.Items[appIndex].ToString() : "";
            //string deviceInfoStr = globalIndex >= 0 ? lstGlobalDeviceInfos.Items[globalIndex].ToString() : "";
            bool isDshow = deviceInfoStr.ToLower().Contains("dshow");
            btnDsVideoSettings.Visible = isDshow;
            btnDsVideoSettings.Tag = deviceInfoStr;
            btnDsCamSettings.Visible = isDshow;
            btnDsCamSettings.Tag = deviceInfoStr;
        }
        void setSelectedIndex(ListBox lstBox, int index)
        {
            lstBox.SelectedIndex = Math.Min(index, lstBox.Items.Count - 1);
        }
        string formatDisplayText(IEzDeviceInfo devInfo, int gid)
        {
            //>>> return $"{EzDispText.C_GLOBAL}{gid} {devInfo}";
            return EzDispText.Format(gid, devInfo);
        }
        void parseDisplayText(string displayText, out string venderInfo)
        {
            //var strs = displayText.Split(' ');
            //if (strs.Length > 1)
            //{
            //    venderInfo = displayText.Replace(strs[0], "").Trim();
            //}
            //else
            //{
            //    venderInfo = displayText.Trim();
            //}
            EzDispText.Parse(displayText, out int gid, out venderInfo);
        }
        #endregion

        IEzCamera getSelectedCamera(out bool needsToDispose)
        {
            needsToDispose = false;
            string devInfoDispStr = btnDsVideoSettings.Tag as string;
            if (string.IsNullOrEmpty(devInfoDispStr))
                return null;

            EzDispText.Parse(devInfoDispStr, out int gid, out string devInfoStr);
            //EzDispText.Parse(devInfo, out int dsCamIndex);

            //var factory = _globalCameraFactory;
            var factory = _appCameraFactory;
            var devInfo = factory.GetDeviceInfo(gid);
            var cam = AllCreatedCameras.FindCamera(devInfo);
            if (cam != null)
            {
                needsToDispose = false;
                return cam;
            }

            cam = factory.LoadCamera(gid);
            needsToDispose = true;
            return cam;
        }
        void openDshowVideoSettings()
        {
            var cam = getSelectedCamera(out bool needsToDispose);

            if (cam is EzDshowCamera ezCamera)
            {
                bool isChanged = ezCamera.ShowPinConfigDlg(this.Handle);
                DshowCameraFactory.Instance.SaveAllSettings(LibPaths.DEFAULT_INI_FILE, ezCamera.GetNativeDshowCamera());

                if (isChanged)
                {
                    NeedsToForceRebuildCamera = true;
                    cam?.Dispose();
                    cam = null;
                    //>>> MessageBox.Show("視頻組態已改變, 必須新啟動!");
                    //>>> postAction(100, this.confirmAppSelection);
                }
            }

            if (needsToDispose)
                cam?.Dispose();
        }
        void openDshowCamSettings()
        {
            var cam = getSelectedCamera(out bool needsToDispose);

            if (cam is EzDshowCamera ezCamera)
            {
                ezCamera.ShowCameraCtrlDlg(this.Handle);
                DshowCameraFactory.Instance.SaveAllSettings(LibPaths.DEFAULT_INI_FILE, ezCamera.GetNativeDshowCamera());
            }

            if (needsToDispose)
                cam?.Dispose();
        }
        void postAction(int delay, Action action)
        {
            new Action<int, Action>((d, a) =>
            {
                System.Threading.Thread.Sleep(d);
                this.BeginInvoke(a);
            }).BeginInvoke(delay, action, null, null);
        }
    }
}
