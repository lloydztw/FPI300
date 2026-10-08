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

using System.Drawing;

namespace LaserAlignDX.Mvc.Model
{
    /// <summary>
    /// Live recipe selection and file locations supplied by the host.
    /// </summary>
    public interface ISysModelEnvironment
    {
        string ActiveRecipeName { get; }
        string CommonBaseTransformsFile { get; }
        string GetTransformsFile(string recipeIniFile);
        string GetEmptyTrayModelName(string recipeName, CarrierEnum carrier);
        string GetEmptyTrayRecipeFile(string recipeName, CarrierEnum carrier);
    }

    /// <summary>
    /// Host-owned coordinate output; 
    /// the model does not own the PLC connection.
    /// </summary>
    public interface ICoordinateWriter
    {
        bool IsAvailable { get; }
        void Write(CarrierEnum carrier, SuckerRowEnum suckerRow, PointF coordinate);
    }
}
