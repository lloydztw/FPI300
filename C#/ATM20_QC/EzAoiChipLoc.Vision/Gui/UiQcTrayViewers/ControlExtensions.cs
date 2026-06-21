#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Gui.Util
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 將 target 原有的 Click 事件清空，並複製 source 的 Click 事件進去。
        /// </summary>
        public static void CopyClickEvent(Control source, Control target)
        {
            CopyHiddenEvent(source, target, "EventClick");
        }

        /// <summary>
        /// 將 target 原有的 MouseClick 事件清空，並複製 source 的 MouseClick 事件進去。
        /// </summary>
        public static void CopyMouseClickEvent(Control source, Control target)
        {
            CopyHiddenEvent(source, target, "EventMouseClick");
        }

        /// <summary>
        /// 核心重構方法：利用反射動態複製指定的 WinForms 底層事件
        /// </summary>
        /// <param name="source">事件的來源控制項 (例如 wnd2)</param>
        /// <param name="target">事件的目標控制項 (例如 wnd1)</param>
        /// <param name="eventKeyName">WinForms 內部定義的事件 Key 名稱 (例如 "EventClick" 或 "EventMouseClick")</param>
        private static void CopyHiddenEvent(Control source, Control target, string eventKeyName)
        {
            if (source == null || target == null) return;

            // 1. 利用反射取得 Control 類別內部隱藏的 Events 屬性 (型別為 EventHandlerList)
            PropertyInfo propertyInfo = typeof(Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null) return;

            EventHandlerList sourceEvents = (EventHandlerList)propertyInfo.GetValue(source);
            EventHandlerList targetEvents = (EventHandlerList)propertyInfo.GetValue(target);

            // 2. 取得 WinForms 內部用來識別該事件的靜態 Key 欄位
            FieldInfo fieldInfo = typeof(Control).GetField(eventKeyName, BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldInfo == null) return;

            object eventKey = fieldInfo.GetValue(null);

            // 3. 清空 target 原有的該事件處理常式
            Delegate targetHandler = targetEvents[eventKey];
            if (targetHandler != null)
            {
                foreach (Delegate d in targetHandler.GetInvocationList())
                {
                    targetEvents.RemoveHandler(eventKey, d);
                }
            }

            // 4. 複製 source 的該事件處理常式到 target
            Delegate sourceHandler = sourceEvents[eventKey];
            if (sourceHandler != null)
            {
                foreach (Delegate d in sourceHandler.GetInvocationList())
                {
                    targetEvents.AddHandler(eventKey, d);
                }
            }
        }

        /// <summary>
        /// 跨不同父容器交換
        /// </summary>
        public static void Swap(Control wnd1, Control wnd2)
        {
            if (wnd1 == null || wnd2 == null) return;

            var parent1 = wnd1.Parent;
            var parent2 = wnd2.Parent;

            if (parent1 == null || parent2 == null) return;

            // 暫停兩個父容器的佈局
            parent1.SuspendLayout();
            if (parent1 != parent2) parent2.SuspendLayout();

            // 1. 先記錄下原本的幾何資訊與控制項順序
            var bounds1 = wnd1.Bounds;
            var bounds2 = wnd2.Bounds;
            int idx1 = parent1.Controls.GetChildIndex(wnd1);
            int idx2 = parent2.Controls.GetChildIndex(wnd2);

            // 2. 先將兩者都移出原本的容器（避免同容器時干擾）
            parent1.Controls.Remove(wnd1);
            parent2.Controls.Remove(wnd2);

            // 3. 交換屬性
            wnd1.Bounds = bounds2;
            wnd2.Bounds = bounds1;

            // 如果控制項有設定 Dock 或 Anchor，交換時也必須一併處理
            var dock1 = wnd1.Dock;
            wnd1.Dock = wnd2.Dock;
            wnd2.Dock = dock1;

            // 4. 放回對方的容器，並還原到對應的索引位置
            parent1.Controls.Add(wnd2);
            parent1.Controls.SetChildIndex(wnd2, idx1);

            parent2.Controls.Add(wnd1);
            parent2.Controls.SetChildIndex(wnd1, idx2);

            // 恢復重繪
            parent1.ResumeLayout(true);
            if (parent1 != parent2) parent2.ResumeLayout(true);
        }
    }
}