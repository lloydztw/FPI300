#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-05-06 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Gui
{
    public partial class FormSegOffsetSettings : Form
    {
        #region PRIVATE_DATA
        IList<JxTraySegItem> _segsList = null;
        #endregion

        public FormSegOffsetSettings()
        {
            InitializeComponent();
            DefaultOffsetY = 10.5;
            Load += FormSegOffsetSettings_Load;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        #region EVENT_HANDLERS
        private void FormSegOffsetSettings_Load(object sender, System.EventArgs e)
        {
            UpdateSegsList(false);
            numSegsNumber.ValueChanged += NumSegsNumber_ValueChanged;
        }
        private void FormSegOffsetSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateSegsList(true);
        }
        private void NumSegsNumber_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateDgvRows((int)numSegsNumber.Value);
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            UpdateSegsList(true);
            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion

        public double DefaultOffsetY
        {
            get;
            set;
        }
        public IList<JxTraySegItem> SegsList
        {
            get => _segsList;
            set
            {
                if (value != null)
                    _segsList = new List<JxTraySegItem>(value);
            }
        }

        #region PRIVATE_FUNCTIONS
        void UpdateDgvRows(int targetSegsNum)
        {
            if (targetSegsNum < 1)
                targetSegsNum = 1;

            var dgv = gwSegOffsetDataGridView1.DataGridView;
            for (int r = dgv.Rows.Count; r < targetSegsNum; r++)
            {
                dgv.Rows.Add($"{r}", DefaultOffsetY);
            }
            while(dgv.Rows.Count > targetSegsNum)
            {
                dgv.Rows.RemoveAt(dgv.Rows.Count - 1);
            }
        }
        void UpdateSegsList(bool toModel)
        {
            var dgv = gwSegOffsetDataGridView1.DataGridView;

            if (toModel)
            {
                if (SegsList == null)
                    SegsList = new List<JxTraySegItem>();

                int rowsCount = dgv.Rows.Count;

                for (int i = 0; i < rowsCount; i++)
                {
                    var dgvRow = dgv.Rows[i];
                    double.TryParse(dgvRow.Cells[1].Value.ToString(), out double offsetY);

                    JxTraySegItem segItem;

                    // 更新 SegItem
                    if (i < SegsList.Count)
                    {
                        segItem = SegsList[i];
                        if (segItem == null)
                            segItem = SegsList[i] = new JxTraySegItem(i);
                    }
                    // 新增 SegItem
                    else
                    {
                        segItem = new JxTraySegItem(i);
                        SegsList.Add(segItem);
                    }

                    segItem.OffsetY.Value = (decimal)offsetY;
                }

                // 移除多餘 SegItems
                while (SegsList.Count > rowsCount)
                {
                    SegsList.RemoveAt(SegsList.Count - 1);
                }
            }
            else
            {
                if (SegsList == null || SegsList.Count == 0)
                {
                    numSegsNumber.Value = 1;
                    UpdateDgvRows(1);
                    return;
                }

                SafeSet(numSegsNumber, (decimal)SegsList.Count);
                UpdateDgvRows((int)numSegsNumber.Value);

                int rowsCount = Math.Min(SegsList.Count, dgv.Rows.Count);
                for (int i = 1; i < rowsCount; i++)
                {
                    var dgvRow = dgv.Rows[i];
                    dgvRow.Cells[0].Value = $"{i}";
                    dgvRow.Cells[1].Value = (double)SegsList[i].OffsetY.Value;
                }
            }
        }
        void SafeSet(NumericUpDown num, decimal value)
        {
            if (num == null)
                return;
            if (value < num.Minimum)
                value = num.Minimum;
            if (value > num.Maximum)
                value = num.Maximum;
            num.Value = value;
        }
        #endregion
    }
}
