#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model.Recipe;
using NeedleX.ProcessSpace;
using System;

namespace LaserAlignDX.Mvc.Model
{
    public interface ITravelerModel : IDisposable
    {
        event EventHandler<ProcessEventArgs> OnError;

        IProcessRunFPI AoiModel { get; }

        ICalibAoiModel CalibAoiModel { get; }
        
        IxEmptyTrayInspector EmptyTrayAoiModel { get; }
        
        TravellerTransforms TransformsModel { get; }
        
        GaBigImageHolder LineScanImageHolder { get; }

        void ApplyRecipe(string gaaraRecipeName = null, bool optWritebackToRecipe = false);
        
        JxRecipeCombo GetCurrentRecipe();
    }
}
