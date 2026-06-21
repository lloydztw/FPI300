#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LeTian.JxProps;
using System;
using System.Drawing;


namespace EzAoiChipLocQC.Model
{
    using JxRect = JxBase<Rectangle>;

    public class JxTrayPinHoles : JxContainer
    {
        public JxBool PinHoleExcluded = new JxBool("PinHole Excluded", "啟用排除", false);
        public JxInt PinHoleDiameter = new JxInt("PinHole Diameter", "定位孔大小 (pix)", 90);
        public JxInt PinHolesNumber = new JxInt("PinHoles Number", "定位點數量 (Hidden)");
        public JxListContainer<JxRect> PinHoleRects = new JxListContainer<JxRect>("PinHoles", "(Hidden)");

        public JxTrayPinHoles() : base("Tray PinHoles", "定位孔 設定")
        {
        }

        public override void OnBindingSubItems()
        {
            //>>> 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                PinHoleExcluded,
                PinHolesNumber,
                PinHoleRects
                //PinHolesNumber,
                //PinHolesDawData,
            });
            base.OnBindingSubItems();

            //>>> update_jx_to_Cache();
            PinHolesNumber.OnModified += PinHolesNumber_OnModified;
            PinHoleRects.OnModified += PinHoleRects_OnModified;
        }

#if (false)
        public JxText PinHolesDawData = new JxText("PinHolesDawData", "", description: "(Hidden)");

        public bool GetPinHole(int index, out Rectangle rect)
        {
            if (index < _cachePinHoleRects.Count)
            {
                rect = _cachePinHoleRects[index];
                return true;
            }
            else
            {
                rect = Rectangle.Empty;
                return false;
            }
        }
        public void SetPinHole(int index, Rectangle rect)
        {
            if (index < _cachePinHoleRects.Count)
            {
                _cachePinHoleRects[index] = rect;
                Modified = true;
            }
        }
        public void AddPinHole(Rectangle? rect)
        {
            if (rect == null)
            {
                if (_cachePinHoleRects.Count == 0)
                {
                    _cachePinHoleRects.Add(new Rectangle(100, 100, 100, 100));
                    return;
                }
                var newRect = _cachePinHoleRects[_cachePinHoleRects.Count - 1];
                newRect.X += 25;
                newRect.Y += 25;
                _cachePinHoleRects.Add(newRect);
            }
            else
            {
                var newRect = rect.Value;
                newRect.Width = Math.Max(newRect.Width, 10);
                newRect.Height = Math.Max(newRect.Height, 10);
                _cachePinHoleRects.Add(newRect);
            }
            Modified = true;
            update_cache_to_jx();
        }
        public void RemovePinHole(int index)
        {
            if (index < _cachePinHoleRects.Count)
            {
                _cachePinHoleRects.RemoveAt(index);
                update_cache_to_jx();
                Modified = true;
            }
        }
        #region PRIVATE_PIN_HOLE_MEMBERS
        List<Rectangle> _cachePinHoleRects = new List<Rectangle>();
        void update_jx_to_Cache()
        {
            _cachePinHoleRects = new List<Rectangle>();
            var text = PinHolesDawData.Value;
            if (!string.IsNullOrEmpty(text))
            {
                var tokens = text.Split(':');
                foreach(var  str in tokens)
                {
                    var strs = str.Split(',');

                    if (strs.Length >= 4 &&
                        int.TryParse(strs[0], out int x) &&
                        int.TryParse(strs[1], out int y) &&
                        int.TryParse(strs[2], out int w) &&
                        int.TryParse(strs[3], out int h))
                    {
                        _cachePinHoleRects.Add(new Rectangle(x, y, w, h));
                    }
                }
            }
        }
        void update_cache_to_jx()
        {
            var sb = new StringBuilder();
            foreach (var rect in _cachePinHoleRects)
            {
                sb.Append(rect.X);
                sb.Append(",");
                sb.Append(rect.Y);
                sb.Append(",");
                sb.Append(rect.Width);
                sb.Append(",");
                sb.Append(rect.Height);
                sb.Append(':');
            }
            PinHolesDawData.Value = sb.ToString().Trim(':');
        }
        #endregion

#endif

        #region PRIVATE_MEMBERS
        bool _bypassEvents = false;
        private void PinHoleRects_OnModified(object sender, EventArgs e)
        {
            if (_bypassEvents)
                return;

            if (PinHoleRects != null)
            {
                _bypassEvents = true;
                PinHolesNumber.Value = PinHoleRects.Count;
                _bypassEvents = false;
            }
        }
        private void PinHolesNumber_OnModified(object sender, EventArgs e)
        {
            if (_bypassEvents)
                return;

            _bypassEvents = true;
            int N = PinHolesNumber.Value;
            while (N > PinHoleRects.Count)
            {
                appendNewRect();
            }
            while (N < PinHoleRects.Count)
            {
                PinHoleRects.RemoveAt(PinHoleRects.Count - 1);
            }
            _bypassEvents = false;
        }
        void appendNewRect()
        {
            if (PinHoleRects.Count == 0)
            {
                var jx = new JxRect($"rc_0");
                jx.Value = new Rectangle(100, 100, 100, 100);
                PinHoleRects.Add(jx);
            }
            else
            {
                int id = PinHoleRects.Count;
                var newRect = PinHoleRects[id - 1].Value;
                newRect.X += 25;
                newRect.Y += 25;
                var jx = new JxRect($"rc_{id}");
                jx.Value = newRect;
                PinHoleRects.Add(jx);
            }
        }
        #endregion
    }
}
