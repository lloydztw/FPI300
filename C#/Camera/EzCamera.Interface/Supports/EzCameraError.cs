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


using System;

namespace EzCamera.Interface
{
    /// <summary>
    /// 泛用的 Class 來包含所有廠家異常
    /// </summary>
    public class EzCameraError : ICloneable
    {
        public int ErrorCode;
        public string Message;
        public string Description;

        public EzCameraError(int errorCode, string message = null, string description = null)
        {
            ErrorCode = errorCode;
            Message = message;
            Description = description;
        }

        #region NO_ERROR_之判定
        public static bool IsNoError(EzCameraError err)
        {
            return err == null || err == NoError || err.IsNoError();
        }
        public bool IsNoError()
        {
            return ErrorCode == 0 && Message == null;
        }
        #endregion

        #region 預設可視化字串
        public override string ToString()
        {
            string text = $"[{ErrorCode}]";
            if (!string.IsNullOrEmpty(Message))
                text += $" {Message}";
            if (!string.IsNullOrEmpty(Description))
                text += $" ({Description})";
            return text;
        }
        #endregion

        #region CLONEABLE
        public EzCameraError Clone()
        {
            return MemberwiseClone() as EzCameraError;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        // 共通的異常 ---------------------------------------------------------------------------------

        public static readonly EzCameraError NoError = new EzCameraError(0);
        public static readonly EzCameraError Timeout = new EzCameraError(-1, "Timeout");
    }
}
