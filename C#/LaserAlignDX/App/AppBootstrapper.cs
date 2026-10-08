#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Windows.Forms;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using AoiEngine = LaserAlignDX.AoiModel.V3.ProcessRunFPIClass;
using MainControllerImpl = LaserAlignDX.Mvc.Ctrl.V3.GaMainCtrl;

#if OPT_CALIB_V5 || OPT_CALIB_V4
using CalibModel = LaserAlignDX.AoiModel.Calib.V5.CalibAoiModel;
#elif OPT_CALIB_V3
using CalibModel = LaserAlignDX.AoiModel.Calib.V3.CalibAoiModel;
#else
using CalibModel = LaserAlignDX.AoiModel.Calib.V25.CalibAoiModel;
#endif

namespace LaserAlignDX
{
    /// <summary>
    /// Application composition root. Shared services remain lazy to preserve the existing
    /// INI / hardware / UI initialization order. GaMvcConfig is only a compatibility facade.
    /// </summary>
    internal sealed class AppBootstrapper : IDisposable
    {
        #region PRIVATE_DATA

        static readonly Lazy<AppBootstrapper> _instance =
            new Lazy<AppBootstrapper>(() => new AppBootstrapper());

        readonly AppResourceScope _resources = new AppResourceScope();
        readonly Lazy<IProcessRunFPI> _aoi;
        readonly Lazy<TravellerSysModel> _system;
        readonly Lazy<MainControllerImpl> _controller;
        readonly AppDialogService _dialogs;
        bool _initializationAttempted;
        bool _disposed;

        #endregion

        #region PRIVATE_CONSTRUCTOR

        AppBootstrapper()
        {
            _aoi = new Lazy<IProcessRunFPI>(() => AoiEngine.Instance);
            _system = new Lazy<TravellerSysModel>(CreateSystemModel);
            _controller = new Lazy<MainControllerImpl>(() => new MainControllerImpl(SystemModel));
            _dialogs = new AppDialogService(() => SystemModel);

            // Registration is reversed on shutdown: controller, hardware, AOI, system, recipe.
            _resources.Add(() =>
            {
                if (_initializationAttempted || _aoi.IsValueCreated) RecipeFPIX3Class.DisposeAll();
            });
            _resources.Add(() => { if (_system.IsValueCreated) _system.Value.Dispose(); });
            _resources.Add(() => { if (_aoi.IsValueCreated) AoiEngine.DisposeAll(); });
            _resources.Add(() => { if (_initializationAttempted) Traveller106.Universal.DisposeHardware(); });
            _resources.Add(() => { if (_controller.IsValueCreated) _controller.Value.Dispose(); });
        }

        #endregion

        public static AppBootstrapper Instance => _instance.Value;

        public ITravelerModel SystemModel { get { ThrowIfDisposed(); return _system.Value; } }
        public IProcessRunFPI Aoi { get { ThrowIfDisposed(); return _aoi.Value; } }
        public Mvc.Ctrl.Abs.GaMainCtrl MainController { get { ThrowIfDisposed(); return _controller.Value; } }
        public AppDialogService Dialogs { get { ThrowIfDisposed(); return _dialogs; } }

        public bool InitializeMachine(int languageIndex)
        {
            ThrowIfDisposed();
            if (_initializationAttempted)
                throw new InvalidOperationException("Machine initialization has already been attempted.");
            _initializationAttempted = true;
            return Traveller106.Universal.InitializeCore(languageIndex);
        }

        public Form CreateMainForm()
        {
            ThrowIfDisposed();
            return new Traveller106.FormMainDX();
        }

        // Transient models are owned by their caller (system model or calibration editor).
        public ICalibAoiModel CreateCalibAoiModel()
        {
            ThrowIfDisposed();
            return new CalibModel();
        }

        public IxReportBuilder CreateReportBuilder()
        {
            ThrowIfDisposed();
            return new XReportBuilderProxy();
        }

        public static void Shutdown()
        {
            if (_instance.IsValueCreated) _instance.Value.Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _resources.Dispose();
        }

        #region PRIVATE_METHODS

        TravellerSysModel CreateSystemModel()
        {
            var aoi = Aoi;
            try { return new TravellerSysModel(aoi, CreateCalibAoiModel); }
            catch (Exception creationError)
            {
                // Transform factories can allocate shared objects in field initializers,
                // before the system model constructor has returned to its owner.
                try { TravellerSysModel.DisposeSharedResources(); }
                catch (Exception cleanupError) { throw new AggregateException(creationError, cleanupError); }
                throw;
            }
        }

        void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AppBootstrapper));
        }

        #endregion
    }
}
