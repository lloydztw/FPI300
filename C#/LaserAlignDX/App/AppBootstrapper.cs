#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
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
    /// INI / hardware / UI initialization order. 
    /// GaMvcConfig is only a compatibility facade.
    /// </summary>
    internal sealed class AppBootstrapper : IDisposable
    {
        #region PRIVATE_DATA

        static readonly Lazy<AppBootstrapper> _instance =
            new Lazy<AppBootstrapper>(() => new AppBootstrapper());

        readonly AppResourceScope _resources = new AppResourceScope();
        readonly Lazy<ITravellerSysModel> _system;
        readonly Lazy<MainControllerImpl> _controller;
        readonly AppDialogService _dialogs;
        bool _initializationAttempted;
        bool _disposed;

        #endregion

        #region PRIVATE_CONSTRUCTOR

        AppBootstrapper()
        {
            _system = new Lazy<ITravellerSysModel>(CreateSystemModel);
            _controller = new Lazy<MainControllerImpl>(() => new MainControllerImpl(SystemModel));
            _dialogs = new AppDialogService(() => SystemModel);

            var dataBinding = new AppDataServices().Bind(() => SystemModel.ActiveCarrierID);
            _resources.Add(dataBinding.Dispose);

            var modelBindings = new AppModelBindings(
                () => SystemModel.AoiModel?.Train(),
                carrier => SystemModel.TransformsModel.GetCalibCamGrid(carrier));
            _resources.Add(modelBindings.Dispose);

            // Registration is reversed on shutdown: controller, hardware, system (owns AOI), recipe.
            _resources.Add(() =>
            {
                if (_initializationAttempted || _system.IsValueCreated) RecipeFPIX3Class.DisposeAll();
            });
            _resources.Add(() => { if (_system.IsValueCreated) _system.Value.Dispose(); });
            _resources.Add(() => { if (_initializationAttempted) Traveller106.Universal.DisposeHardware(); });
            _resources.Add(() => { if (_controller.IsValueCreated) _controller.Value.Dispose(); });
        }

        #endregion

        public static AppBootstrapper Instance => _instance.Value;

        public ITravellerSysModel SystemModel { get { ThrowIfDisposed(); return _system.Value; } }
        public IProcessRunFPI Aoi { get { ThrowIfDisposed(); return SystemModel.AoiModel; } }
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

        ITravellerSysModel CreateSystemModel()
        {
            return TravellerModelFactory.Create(
                new AppAoiSettings(), 
                new AppSysModelEnvironment(),
                new AppCoordinateWriter(), 
                CreateCalibAoiModel);
        }
        void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AppBootstrapper));
        }

        #endregion
    }
}
