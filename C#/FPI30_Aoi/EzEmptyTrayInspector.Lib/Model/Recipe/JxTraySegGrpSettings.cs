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

#if (OPT_ORG)
    public class JxTraySegGrpSettings : JxContainer
    {
        public JxInt SegsNumber = new JxInt("SegsNumber", "子群數量", 1, new Range(1, 3));
        public JxTraySegItemsList _SegsItems = new JxTraySegItemsList();

        public JxTraySegGrpSettings() : this(false)
        {
        }
        public JxTraySegGrpSettings(bool initOne) : base("Group Settings", "子群設定")
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
                _SegsItems
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
                syncSegmentsNumber();
                Modified = flag;
            }
        }
        private void syncSegmentsNumber()
        {
            int lstCount = _SegsItems.ListCount;
            if (SegsNumber.Value != lstCount)
            {
                SegsNumber.Value = lstCount;
            }
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
        }
        public void UpdateSegment(int id, Rectangle boundRect, decimal yOffset)
        {
            if (id >= _SegsItems.ListCount)
                _SegsItems.Add(new JxTraySegItem(id));

            var item = _SegsItems[id];
            if (item == null)
            {
                item = _SegsItems[id] = new JxTraySegItem(id);
                item.Modified = true;
                this.Modified = true;
            }

            item.BoundBox.Value = boundRect;

            if (id > 0 && item.OffsetY.Value <= 0m && yOffset > 0m && item.OffsetY.Value != yOffset)
            {
                item.OffsetY.Value = yOffset;
                item.Modified = true;
                this.Modified = true;
            }
        }
        public void RemoveSegments(int targetCount)
        {
            bool isChanged = false;
            while (_SegsItems.ListCount > targetCount)
            {
                _SegsItems.RemoveAt(_SegsItems.ListCount - 1);
                isChanged = true;
            }
            if (isChanged)
            {
                Modified = true;
                syncSegmentsNumber();
                AutoHidden();
            }
        }
        void AutoHidden()
        {
            _SegsItems?.AutoHidden();
        }
    }

    public class JxTraySegItemsList : JxListContainer<JxTraySegItem>
    {
        public JxTraySegItemsList() : this("SegsList", "子群列表")
        {
        }
        public JxTraySegItemsList(string name, string desc) : base(name, desc)
        {
        }
        public override void OnBindingSubItems()
        {
            base.OnBindingSubItems();

            if (ListCount == 0)
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
        public void AutoHidden()
        {
            var jx = this;
            int itemsCount = jx.ListCount;
            if (itemsCount == 0)
            {
                if (!jx.IsHidden())
                    jx.Description += "(Hidden)";
            }
            else
            {
                if (jx.IsHidden())
                    jx.Description = Description.Replace("(Hidden)", "");
            }
        }
    }
#endif

    public class JxTraySegGrpSettings : JxListContainer<JxTraySegItem>
    {
        public JxInt SegsNumber = new JxInt("SegsNumber", "子群數量", 1, new Range(1, 3));
        public JxTraySegGrpSettings() : this(false)
        {
        }
        public JxTraySegGrpSettings(bool initOne) : base("Segment Settings", "子群設定")
        {
            if (initOne)
            {
                initOneItem();
            }
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                SegsNumber,
            });
            base.OnBindingSubItems();
            syncSegmentsNumber();
        }

        #region PRIVATE_HELPER_FUNCTIONS
        private void initOneItem()
        {
            var jxList = this;
            int lstCount = jxList.ListCount;
            if (lstCount == 0)
            {
                bool flag = Modified;
                jxList.Add(new JxTraySegItem(0));
                syncSegmentsNumber();
                Modified = flag;
            }
        }
        private void syncSegmentsNumber()
        {
            int lstCount = this.ListCount;
            if (SegsNumber.Value != lstCount)
                SegsNumber.Value = lstCount;
        }
        private void syncSegItems()
        {
            var keyNames = this.GetKeyNames().ToList();
            keyNames.Remove(SegsNumber.Name);

            var jxList = this.SegsList;

            // 重置ID並保持Modified狀態, 以確保ID與索引一致.
            for (int id = 0; id < jxList.Count; id++)
            {
                var item = jxList[id];
                bool flag = item.Modified;
                item.SetID(id);
                item.Modified = flag;
            }

            for (int id = 0; id < jxList.Count; id++)
            {
                var segItem = jxList[id];
                segItem.SetHidden(id <= 0);

                string name = segItem.Name;
                if (keyNames.Contains(name))
                {
                    var old = this[name];
                    if (old != segItem)
                    {
                        if (old is JxTraySegItem segItem2)
                        {
                            // 將 segItem 複製到 segItem2, 保持Modified狀態, 以確保ID與索引一致.
                            bool flag = segItem.Modified;
                            old.CopyFrom(segItem);
                            segItem2.SetHidden(id <= 0);
                            segItem2.Modified = flag;
                            // 保留 segItem2
                            jxList[id] = segItem2;
                        }
                        else
                        {
                            this[name] = segItem;
                        }
                    }
                }
            }

            var segNames = Array.ConvertAll(_DynamicItems_, seg => seg.Name);
            foreach (var kName in keyNames)
            {
                if (!segNames.Contains(kName))
                    this[kName] = null;
            }
        }
        #endregion

        [JsonIgnore]
        public IList<JxTraySegItem> SegsList
        {
            get
            {
                initOneItem();
                return this;
            }
        }
        public void UpdateSegment(int id, Rectangle boundRect, decimal yOffset)
        {
            bool isChanged = false;

            var jxList = this;

            if (id >= jxList.ListCount)
                jxList.Add(new JxTraySegItem(id));

            var item = jxList[id];
            if (item == null)
            {
                item = jxList[id] = new JxTraySegItem(id);
                item.Modified = true;
                isChanged = true;
            }

            item.BoundBox.Value = boundRect;

            if (id > 0 && item.OffsetY.Value <= 0m && yOffset > 0m && item.OffsetY.Value != yOffset)
            {
                item.OffsetY.Value = yOffset;
                item.Modified = true;
                isChanged = true;
            }

            syncSegItems();

            if(isChanged)
                Modified = true;
        }
        public void RemoveSegments(int targetCount)
        {
            bool isChanged = false;

            var jxList = this;
            while (jxList.ListCount > targetCount)
            {
                int id = jxList.ListCount - 1;
                jxList[id].SetHidden(true);
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
        public void AutoHidden()
        {
            syncSegItems();
        }
    }

    public class JxTraySegItem : JxContainer
    {
        #region PRIVATE_DATA
        int _id = 0;
        #endregion

        public JxNumber OffsetX = new JxNumber("OffsetX", "X方向 跨距 mm (Hidden)", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber OffsetY = new JxNumber("OffsetY", "Y方向 跨距 mm", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxRect BoundBox = new JxRect("BoundBox", "邊界框(唯讀)(Hidden)");

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
