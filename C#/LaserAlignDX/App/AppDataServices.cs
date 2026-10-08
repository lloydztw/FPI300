#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using System;
using JetEazy.Utils;
using LaserAlignDX.Model;
using Traveller106;

namespace LaserAlignDX
{
    /// <summary>
    /// Supplies live application paths/settings and the existing FTP transport.
    /// </summary>
    internal sealed class AppDataServices : IDataOutputSettings, IImageUploadService
    {
        #region PRIVATE_DATA
        readonly Lazy<FtpUploader> _uploader = new Lazy<FtpUploader>(() =>
            new FtpUploader(INI.Instance.FtpSettings)
            {
                OnLog = text => GaUtil.LOG(text)
            });
        #endregion

        public IDisposable Bind(Func<CarrierEnum> activeCarrier) =>
            ModelDataRuntime.Bind(new ModelDataServices(this, this, activeCarrier));

        public string LogImagePath => Universal.LOG_IMG_PATH;
        public string ResultImagePath => INI.Instance.ResultImagePath;
        public bool IsSaveDebugBmp => INI.Instance.IsSaveDebugBmp;
        public bool IsSaveDebugOrgBmp => INI.Instance.IsSaveDebugOrgBmp;
        public bool IsSaveTestImage => INI.Instance.IsSaveTestImage;
        public bool UseOkNgDiffImageFolders => INI.Instance.UseOkNgDiffImageFolders;
        public int ImageQuality => INI.Instance.ImageQuality;
        public bool UsingSingleNgCode => INI.Instance.UsingSingleNgCode;
        public int SingleNgCode => INI.Instance.SingleNgCode;
        public bool IsSimulation => Universal.IsNoUseCCD;
        public string PlcTracePath => @"d:\paso.log\PlcResultData";
        public bool Enabled => _uploader.Value.Enabled;
        public bool UploadFile(string fileName, string category, DateTime time, string lotId) =>
            _uploader.Value.UploadFile(fileName, category, time, lotId);
    }
}
