#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace JetEazy.DShow.Utils
{
    public class NativeWindowHelper
    {
        #region WIN32_API
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        // Win32 API declarations
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // RECT structure for GetClientRect
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }
        }
        #endregion

        /// <summary>
        /// 將一個 Control 作為指定 HWND 的子視窗，並使其填滿父 HWND 的客戶區。
        /// </summary>
        /// <param name="childControl">要作為子視窗的 Control。</param>
        /// <param name="parentHwnd">父視窗的 HWND。</param>
        public static void SetControlAsChildAndFill(Control childControl, IntPtr parentHwnd)
        {
            if (childControl == null)
            {
                //throw new ArgumentNullException(nameof(childControl));
                throw new Exception($"NativeWindowHelper.SetControlAsChildAndFill: childControl == null!");
            }

            if (parentHwnd==IntPtr.Zero || !IsWindow(parentHwnd))
            {
                //throw new ArgumentException("Parent HWND cannot be zero.", nameof(parentHwnd));
                throw new Exception($"NativeWindowHelper.SetControlAsChildAndFill: parentHwnd 不存在!");
            }

            // 1. 將 Control 的父視窗設置為指定的 HWND
            // 注意：這裡設置了 Control 的 Handle 的父級，而不是 Control.Parent 屬性。
            // Control.Parent 屬性只適用於 .NET 內部管理。
            IntPtr oldParent = SetParent(childControl.Handle, parentHwnd);
            if (oldParent == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
            {
                // 處理錯誤，例如記錄日誌或拋出異常
                throw new InvalidOperationException($"Failed to set parent window. Error: {Marshal.GetLastWin32Error()}");
            }

            // 2. 移除 Control 自身的邊框樣式 (可選，但通常建議對於內嵌控制項)
            // 這一步通常對於 Dock.Fill 是必要的，因為我們希望 Control 完全填充客戶區。
            // 可以根據需要調整。
            // 為了簡單起見，這裡不包含 GetWindowLong/SetWindowLong 的 P/Invoke。
            // 如果你的 Control 是 Form，可能需要更複雜的處理。

            // 3. 獲取父 HWND 的客戶區大小
            RECT clientRect;
            if (!GetClientRect(parentHwnd, out clientRect))
            {
                throw new InvalidOperationException($"Failed to get client rect of parent window. Error: {Marshal.GetLastWin32Error()}");
            }

            // 4. 調整子 Control 的位置和大小以填滿父 HWND 的客戶區
            // 子視窗的位置和尺寸是相對於父視窗客戶區的左上角。
            if (!MoveWindow(childControl.Handle, 0, 0, clientRect.Width, clientRect.Height, true))
            {
                throw new InvalidOperationException($"Failed to move child window. Error: {Marshal.GetLastWin32Error()}");
            }

            // 5. 確保子 Control 可見並刷新
            childControl.Visible = true;
            childControl.Invalidate();
            childControl.Refresh();
        }

        /// <summary>
        /// 當父視窗大小改變時，重新調整子 Control 的大小。
        /// </summary>
        /// <param name="childControl">子 Control。</param>
        /// <param name="parentHwnd">父視窗的 HWND。</param>
        public static void ResizeChildControlToFill(Control childControl, IntPtr parentHwnd)
        {
            if (childControl == null || parentHwnd == IntPtr.Zero)
            {
                return;
            }

            RECT clientRect;
            if (GetClientRect(parentHwnd, out clientRect))
            {
                MoveWindow(childControl.Handle, 0, 0, clientRect.Width, clientRect.Height, true);
            }
        }
    }
}
