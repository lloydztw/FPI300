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
    public class JxTempMatchMasks : JxContainer
    {
        public JxInt MasksNumber = new JxInt("MasksNumber", "遮罩數量", 0, new Range(0, 16));

        #region INTERNAL_DATA
        public JxMaskItemsList _masksList = new JxMaskItemsList();
        #endregion

        public JxTempMatchMasks() : base("MaskRects", "遮罩區塊")
        {
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                MasksNumber,
                _masksList,
            });
            base.OnBindingSubItems();
            updateListCountToTotalNumber();
        }

        #region PRIVATE_HELPER_FUNCTIONS
        private void updateListCountToTotalNumber()
        {
            var lstCount = _masksList.ListCount;
            if (MasksNumber.Value != lstCount)
                MasksNumber.Value = lstCount;
        }
        private void syncItemsList()
        {
            _masksList?.SyncAndSort();
            _DUMP(MasksList);
        }
        #endregion

        [JsonIgnore]
        public IList<JxMaskItem> MasksList
        {
            get
            {
                return _masksList;
            }
            set
            {
                var src = value;
                if (src != _masksList && src != null && src.Count > 0)
                {
                    _masksList._DynamicItems_ = src.ToArray();
                    syncItemsList();
                    updateListCountToTotalNumber();
                }
            }
        }
        
        public void UpdateMask(int id, Rectangle boundRect, bool silent = false)
        {
            bool isChanged = false;

            var jxList = _masksList;

            JxMaskItem item;
            var defaultRect = new Rectangle(100, 100, 400, 300);

            if (id >= jxList.ListCount)
            {
                jxList.Add(item = new JxMaskItem());
                item.Value = defaultRect;
                item.Modified = true;
                isChanged = true;
            }
            else 
            {
                if (jxList.Count > MasksNumber.Value)
                    RemoveMasks(MasksNumber.Value);

                item = jxList[id];
                if (item == null)
                {
                    item = jxList[id] = new JxMaskItem(id);
                    item.Value = defaultRect;
                    item.Modified = true;
                    isChanged = true;
                }
            }

            if (item.Value != boundRect)
            {
                item.Value = boundRect;
                item.Modified = true;
                isChanged = true;
            }

            //if (id > 0 && item.OffsetY.Value <= 0m && yOffset > 0m && item.OffsetY.Value != yOffset)
            //{
            //    item.OffsetY.Value = yOffset;
            //    item.Modified = true;
            //    isChanged = true;
            //}

            if (isChanged && !silent)
            {
                syncItemsList();
                updateListCountToTotalNumber();
                Modified = true;
            }
        }
        public void SyncMasks(int targetCount)
        {
            SetMasks(targetCount);
        }

        void RemoveMasks(int targetCount)
        {
            bool isChanged = false;

            syncItemsList();

            var jxList = _masksList;
            while (jxList.ListCount > targetCount)
            {
                int id = jxList.ListCount - 1;
                //jxList[id].SetHidden(true);
                jxList.RemoveAt(id);
                isChanged = true;
            }

            if (isChanged)
            {
                syncItemsList();
                updateListCountToTotalNumber();
                Modified = true;
            }
        }
        void AddMasks(int targetCount)
        {
            bool isChanged = false;

            syncItemsList();
            var jxList = _masksList;
            var defaultRect = jxList.ListCount > 0 ?
                              jxList[jxList.ListCount - 1].Value :
                              new Rectangle(100, 100, 300, 300);

            while (jxList.ListCount < targetCount)
            {
                int id = jxList.ListCount;
                defaultRect.X += 50;
                defaultRect.Y += 50;
                UpdateMask(id, defaultRect, silent: true);
                isChanged = true;
            }

            if (isChanged)
            {
                syncItemsList();
                updateListCountToTotalNumber();
                Modified = true;
            }
        }
        void SetMasks(int targetCount)
        {
            syncItemsList();
            var jxList = _masksList;
            if (jxList.ListCount > targetCount)
            {
                RemoveMasks(targetCount);
            }
            else if (jxList.ListCount < targetCount)
            {
                AddMasks(targetCount);
            }
        }

        #region DEBUG_FUNCTIONS
        void _DUMP(IList<JxMaskItem> items)
        {
            if (items == null)
                return;

            System.Diagnostics.Debug.WriteLine($"\n{this.GetType().Name}:");
            foreach (var item in items)
            {
                System.Diagnostics.Debug.WriteLine($"{item} @ {item.Value}");
            }
        }
        #endregion
    }

    public class JxMaskItemsList : JxListContainer<JxMaskItem>
    {
        public JxMaskItemsList() : this("MaskItemsList", "群組成員(Hidden)")
        {
        }
        public JxMaskItemsList(string name, string desc) : base(name, desc)
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

    public class JxMaskItem : JxBase<Rectangle>
    {
        #region PRIVATE_DATA
        int _id = 0;
        #endregion

        //public JxNumber OffsetX = new JxNumber("OffsetX", "X方向 跨距 mm (Hidden)", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        //public JxNumber OffsetY = new JxNumber("OffsetY", "Y方向 跨距 mm", 0m, new Range(-5000m, 5000m, 0.01m, 3));
        //public JxRect BoundBox = new JxRect("BoundBox", "邊界框 (唯讀)(Hidden)");

        public JxMaskItem() : this(0)
        {
        }
        public JxMaskItem(int id) : base($"Mask_{id}", $"子群_{id}(Hidden)")
        {
            _id = id;
        }

        //public override void OnBindingSubItems()
        //{
        //    if (Name.EndsWith("_0"))
        //        Description = "(Hidden)";

        //    //綁定以下成員, 會自動顯示在GUI編輯視窗.
        //    BindItems(new IProp[] {
        //        OffsetX,
        //        OffsetY,
        //        BoundBox,
        //    });

        //    base.OnBindingSubItems();
        //}

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
