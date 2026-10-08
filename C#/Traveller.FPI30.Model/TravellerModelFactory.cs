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

using LaserAlignDX.AoiModel;
using System;
using AoiEngine = LaserAlignDX.AoiModel.V3.ProcessRunFPIClass;

namespace LaserAlignDX.Mvc.Model
{
    /// <summary>
    /// Creates one model session per process. The returned model owns its lazy AOI,
    /// calibration, shared model resources and runtime binding. 
    /// Host services are borrowed.
    /// </summary>
    public static class TravellerModelFactory
    {
        public static ITravellerSysModel Create(
            IAoiEnvironment aoiEnvir, 
            ISysModelEnvironment sysModelEnvir,
            ICoordinateWriter coordinateWriter, 
            Func<ICalibAoiModel> createCalibModel,
            Func<IProcessRunFPI> createAoiModel = null)
        {
            if (aoiEnvir == null) throw new ArgumentNullException(nameof(aoiEnvir));
            if (sysModelEnvir == null) throw new ArgumentNullException(nameof(sysModelEnvir));
            if (coordinateWriter == null) throw new ArgumentNullException(nameof(coordinateWriter));
            if (createCalibModel == null) throw new ArgumentNullException(nameof(createCalibModel));

            lock (AoiRuntime.SyncRoot)
            {
                // Reject before allocating shared transforms; a failed second Create must not
                // dispose resources belonging to the existing session.
                if (AoiRuntime.IsBound)
                    throw new InvalidOperationException("Dispose the current model before creating another model session.");
                
                TravellerSysModel model = null;
                
                try
                {
                    bool useDefaultAoi = createAoiModel == null;

                    model = new TravellerSysModel(
                        createAoiModel ?? (() => AoiEngine.Instance),
                        createCalibModel, sysModelEnvir, coordinateWriter,
                        aoi => { if (useDefaultAoi) AoiEngine.DisposeAll(); else aoi.Dispose(); }
                    );
                    
                    model.ConfigureServices(aoiEnvir);
                    model.AttachRuntime(AoiRuntime.Bind(model));
                    return model;
                }
                catch (Exception creationError)
                {
                    try
                    {
                        if (model != null) model.Dispose();
                        else TravellerSysModel.DisposeSharedResources();
                    }
                    catch (Exception cleanupError) { throw new AggregateException(creationError, cleanupError); }
                    throw;
                }
            }
        }
    }
}
