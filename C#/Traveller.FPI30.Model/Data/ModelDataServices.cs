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

namespace LaserAlignDX.Model
{
    /// <summary>Live settings for image storage and PLC result formatting.</summary>
    public interface IDataOutputSettings
    {
        string LogImagePath { get; }
        string ResultImagePath { get; }
        bool IsSaveDebugBmp { get; }
        bool IsSaveDebugOrgBmp { get; }
        bool IsSaveTestImage { get; }
        bool UseOkNgDiffImageFolders { get; }
        int ImageQuality { get; }
        bool UsingSingleNgCode { get; }
        int SingleNgCode { get; }
        bool IsSimulation { get; }
        string PlcTracePath { get; }
    }

    /// <summary>The host owns upload configuration, credentials and transport.</summary>
    public interface IImageUploadService
    {
        bool Enabled { get; }
        bool UploadFile(string fileName, string category, DateTime time, string lotId);
    }

    public sealed class ModelDataServices
    {
        #region PRIVATE_DATA
        readonly Func<CarrierEnum> _activeCarrier;
        #endregion

        public ModelDataServices(IDataOutputSettings settings, IImageUploadService imageUpload, Func<CarrierEnum> activeCarrier)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ImageUpload = imageUpload ?? throw new ArgumentNullException(nameof(imageUpload));
            _activeCarrier = activeCarrier ?? throw new ArgumentNullException(nameof(activeCarrier));
        }

        public IDataOutputSettings Settings { get; }
        public IImageUploadService ImageUpload { get; }
        public CarrierEnum ActiveCarrier => _activeCarrier();
    }

    /// <summary>
    /// Binds data output independently of AOI. The host must finish background saves/uploads
    /// before disposing this binding. Settings and upload service are borrowed.
    /// </summary>
    public static class ModelDataRuntime
    {
        #region PRIVATE_DATA
        static readonly object _gate = new object();
        static ModelDataServices _services;
        #endregion

        public static ModelDataServices Services
        {
            get
            {
                lock (_gate)
                    return _services ?? throw new InvalidOperationException("Bind model data services before formatting results or saving images.");
            }
        }

        public static IDisposable Bind(ModelDataServices services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            lock (_gate)
            {
                if (_services != null) throw new InvalidOperationException("Model data services are already bound.");
                _services = services;
                return new Binding();
            }
        }

        #region PRIVATE_TYPES
        sealed class Binding : IDisposable
        {
            #region PRIVATE_DATA
            bool _disposed;
            #endregion

            public void Dispose()
            {
                lock (_gate)
                {
                    if (_disposed) return;
                    _disposed = true;
                    _services = null;
                }
            }
        }
        #endregion
    }
}
