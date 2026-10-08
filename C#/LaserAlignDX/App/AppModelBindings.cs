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
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using Traveller.FPI30.Model;

namespace LaserAlignDX
{
    /// <summary>
    /// Connects the reusable model to application 
    /// training, grids and notifications.
    /// </summary>
    internal sealed class AppModelBindings : IDisposable
    {
        #region PRIVATE_DATA
        readonly Action _train;
        readonly Func<CarrierEnum, EzBlocsGrid> _grids;
        readonly Func<CarrierEnum, EzBlocsGrid> _previousGrids;
        bool _disposed;
        #endregion

        public AppModelBindings(Action train, Func<CarrierEnum, EzBlocsGrid> grids)
        {
            _train = train ?? throw new ArgumentNullException(nameof(train));
            _grids = grids ?? throw new ArgumentNullException(nameof(grids));
            _previousGrids = RecipeFPIX3Class.CameraGridProvider;
            RecipeFPIX3Class.CameraGridProvider = grids;
            RecipeFPIX3Class.TrainingRequested += _train;
            RecipeFPIX3Class.TrainingFailed += ShowTrainingFailure;
            ModelDiagnostics.Message += GaUtil.LOG;
            ModelDiagnostics.Error += GaUtil.LOG_ERROR;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            RecipeFPIX3Class.TrainingRequested -= _train;
            RecipeFPIX3Class.TrainingFailed -= ShowTrainingFailure;
            if (RecipeFPIX3Class.CameraGridProvider == _grids)
                RecipeFPIX3Class.CameraGridProvider = _previousGrids;
            ModelDiagnostics.Message -= GaUtil.LOG;
            ModelDiagnostics.Error -= GaUtil.LOG_ERROR;
        }

        #region PRIVATE_METHODS
        static void ShowTrainingFailure(Exception error) => JetEazy.FormSpace.VsMessageBox.Warning(error.Message);
        #endregion
    }
}
