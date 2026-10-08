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
using System.Diagnostics;

namespace Traveller.FPI30.Model
{
    /// <summary>
    /// Optional host logging. 
    /// The model does not own or create any UI.
    /// </summary>
    public static class ModelDiagnostics
    {
        public static event Action<string, object[]> Message;
        public static event Action<Exception, string> Error;

        internal static void Log(string message, params object[] args)
        {
            var handler = Message;
            if (handler != null) handler(message, args);
            else Trace.WriteLine(message);
        }

        internal static void LogError(Exception exception, string message)
        {
            var handler = Error;
            if (handler != null) handler(exception, message);
            else Trace.TraceError("{0}: {1}", message, exception);
        }
    }


    internal static class ModelUtilities
    {
        internal static bool Clip(ref OpenCvSharp.Rect rect, int width, int height)
        {
            if (!(rect.X < 0 || rect.Y < 0 || rect.Right > width || rect.Bottom > height)) return false;
            int x = Math.Max(rect.X, 0), y = Math.Max(rect.Y, 0);
            int right = Math.Min(rect.Right, width), bottom = Math.Min(rect.Bottom, height);
            rect = new OpenCvSharp.Rect(x, y, right - x, bottom - y);
            return true;
        }

        internal static bool Clip(ref System.Drawing.Rectangle rect, System.Drawing.Size size)
        {
            if (!(rect.X < 0 || rect.Y < 0 || rect.Right > size.Width || rect.Bottom > size.Height)) return false;
            int x = Math.Max(rect.X, 0), y = Math.Max(rect.Y, 0);
            int right = Math.Min(rect.Right, size.Width), bottom = Math.Min(rect.Bottom, size.Height);
            rect = new System.Drawing.Rectangle(x, y, right - x, bottom - y);
            return true;
        }

        internal static bool Clip(ref System.Drawing.RectangleF rect, int width, int height)
        {
            if (!(rect.X < 0 || rect.Y < 0 || rect.Right > width || rect.Bottom > height)) return false;
            float x = Math.Max(rect.X, 0), y = Math.Max(rect.Y, 0);
            float right = Math.Min(rect.Right, width), bottom = Math.Min(rect.Bottom, height);
            rect = new System.Drawing.RectangleF(x, y, right - x, bottom - y);
            return true;
        }

        internal static void SaveData(string data, string fileName)
        {
            // Diagnostic output remains best effort, as in the original application helper.
            try
            {
                using (var writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                    writer.WriteLine(data);
            }
            catch (Exception) { }
        }

        internal static string GetEnumDescription(Enum value) => JetEazy.Lang.QMSG.Text(value);
        internal static void LOG(string message, params object[] args) => ModelDiagnostics.Log(message, args);
        internal static void LOG_ERROR(Exception exception, string message) => ModelDiagnostics.LogError(exception, message);
    }
}
