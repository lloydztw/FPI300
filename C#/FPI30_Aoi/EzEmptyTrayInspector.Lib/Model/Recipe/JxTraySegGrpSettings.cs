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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EzAoiEmptyTrayInspector.Model
{
    using JxRect = JxBase<Rectangle>;

    public class JxTraySegGrpSettings : JxContainer
    {
        public JxText SegsNumber = new JxText("SegsNumber", "1", "群組數量", hasDetailButton: true);

        #region INTERNAL_DATA
        public JxTraySegItemsList _SegsItems = new JxTraySegItemsList();
        #endregion

        public JxTraySegGrpSettings() : this(false)
        {
        }
        public JxTraySegGrpSettings(bool initOne) : base("Segment Settings", "群組設定")
        {
            if (initOne)
            {
                initOneItem();
            }
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                SegsNumber,
                _SegsItems,
            });
            base.OnBindingSubItems();
            syncSegmentsNumber();
        }

        #region PRIVATE_HELPER_FUNCTIONS
        private void initOneItem()
        {
            int lstCount = _SegsItems.ListCount;
            if (lstCount == 0)
            {
                bool flag = Modified;
                _SegsItems.Add(new JxTraySegItem(0));
                syncSegItems();
                syncSegmentsNumber();
                Modified = flag;
            }
        }
        private void syncSegmentsNumber()
        {
            var lstCount = _SegsItems.ListCount.ToString();
            if (SegsNumber.Value != lstCount)
                SegsNumber.Value = lstCount;
        }
        private void syncSegItems()
        {
            _SegsItems?.SyncAndSort();
            _DUMP(SegsList);
        }
        #endregion

        [JsonIgnore]
        public IList<JxTraySegItem> SegsList
        {
            get
            {
                initOneItem();
                return _SegsItems;
            }
            set
            {
                var src = value;
                if (src != _SegsItems && src != null && src.Count > 0)
                {
                    _SegsItems._DynamicItems_ = src.ToArray();
                    syncSegItems();
                    syncSegmentsNumber();
                }
            }
        }
        public void UpdateSegment(int id, Rectangle boundRect, decimal yOffset)
        {
            bool isChanged = false;

            var jxList = _SegsItems;

            JxTraySegItem item;

            if (id >= jxList.ListCount)
            {
                jxList.Add(item = new JxTraySegItem(id));
                item.Modified = true;
                isChanged = true;
            }
            else
            {
                item = jxList[id];
                if (item == null)
                {
                    item = jxList[id] = new JxTraySegItem(id);
                    item.Modified = true;
                    isChanged = true;
                }
            }

            if (item.BoundBox.Value != boundRect)
            {
                item.BoundBox.Value = boundRect;
                item.Modified = true;
                isChanged = true;
            }

            if (id > 0 && item.OffsetY.Value <= 0m && yOffset > 0m && item.OffsetY.Value != yOffset)
            {
                item.OffsetY.Value = yOffset;
                item.Modified = true;
                isChanged = true;
            }


            if (isChanged)
            {
                syncSegItems();
                syncSegmentsNumber();

                Modified = true;
            }
        }
        public void RemoveSegments(int targetCount)
        {
            bool isChanged = false;

            syncSegItems();

            var jxList = _SegsItems;
            while (jxList.ListCount > targetCount)
            {
                int id = jxList.ListCount - 1;
                //jxList[id].SetHidden(true);
                jxList.RemoveAt(id);
                isChanged = true;
            }

            if (isChanged)
            {
                syncSegItems();
                syncSegmentsNumber();
                Modified = true;
            }
        }

        #region DEBUG_FUNCTIONS
        void _DUMP(IList<JxTraySegItem> segs)
        {
            if (segs == null)
                return;

            System.Diagnostics.Debug.WriteLine($"\n{this.GetType().Name}:");
            foreach (var seg in segs)
            {
                System.Diagnostics.Debug.WriteLine($"{seg} @ {seg.BoundBox.Value} @ {seg.OffsetY.Value:0.000}");
            }
        }
        #endregion
    }

    public class JxTraySegItemsList : JxListContainer<JxTraySegItem>
    {
        public JxTraySegItemsList() : this("SegsList", "群組成員(Hidden)")
        {
        }
        public JxTraySegItemsList(string name, string desc) : base(name, desc)
        {
            SyncAndSort();
        }
        public override void OnBindingSubItems()
        {
            base.OnBindingSubItems();
            SyncAndSort();
        }
        internal void SyncAndSort()
        {
            if (!IsHidden())
            {
                Description += "(Hidden)";
            }

            var items = _DynamicItems_;
            Array.Sort(items, (a, b) => a.GetID() - b.GetID());
            for (int i = 0; i < items.Length; i++)
            {
                items[i].SetID(i);
            }
            foreach (var kName in GetKeyNames())
            {
                this[kName] = null;
            }
            BindItems(items);
        }
    }

    public class JxTraySegItem : JxContainer
    {
        #region PRIVATE_DATA
        int _id = 0;
        #endregion

        public JxNumber OffsetX = new JxNumber("OffsetX", "X方向 跨距 mm (Hidden)", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber OffsetY = new JxNumber("OffsetY", "Y方向 跨距 mm", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxRect BoundBox = new JxRect("BoundBox", "邊界框 (唯讀)(Hidden)");

        public JxTraySegItem() : this(0)
        {
        }
        public JxTraySegItem(int id) : base($"Seg_{id}", $"子群_{id}(Hidden)")
        {
            _id = id;
        }
        public override void OnBindingSubItems()
        {
            if (Name.EndsWith("_0"))
                Description = "(Hidden)";

            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                OffsetX,
                OffsetY,
                BoundBox,
            });

            base.OnBindingSubItems();
        }

        #region HELPER_FUNCTIONS    
        public void SetHidden(bool isHidden)
        {
            if (isHidden)
            {
                if (!IsHidden())
                    Description += "(Hidden)";
            }
            else
            {
                if (IsHidden())
                    Description = Description.Replace("(Hidden)", "");
            }
        }
        public void SetID(int newID)
        {
            if (_id != newID)
            {
                Name = Name.Replace($"_{_id}", $"_{newID}");
                Description = Description.Replace($"_{_id}", $"_{newID}");
                _id = newID;
            }
        }
        public int GetID()
        {
            return _id;
        }
        #endregion
    }
}
