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

using EzAoiChipLocQC.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Gui
{
    public partial class FormSegOffsetSettings : Form
    {
        #region PRIVATE_DATA
        IList<JxTraySegItem> _segsList = null;
        #endregion

        public FormSegOffsetSettings()
        {
            InitializeComponent();
            PitchY = 10.5;
            Load += FormSegOffsetSettings_Load;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        #region EVENT_HANDLERS
        private void FormSegOffsetSettings_Load(object sender, System.EventArgs e)
        {
            UpdateSegsList(false);
            var dgv = gwSegOffsetDataGridView1.DataGridView;
            numSegsNumber.ValueChanged += NumSegsNumber_ValueChanged;
            dgv.CellValidating += Dgv_CellValidating;
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
        private void Dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                //(1) e.FormattedValue 代表使用者剛輸入進去、尚未儲存的內容
                string dgvStr = e.FormattedValue.ToString();

                //(2) 驗證
                string errMsg = ParseOffsetY(e.RowIndex, dgvStr, out double offsetY);

                //(3) 警示
                if (errMsg != null)
                {
                    // 3.1 驗證失敗：設定 Cancel 為 true，這會阻止使用者離開目前 Cell
                    e.Cancel = true;

                    // 3.2 Message Box
                    MessageBox.Show(errMsg, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        #endregion

        public double PitchY
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
        string ParseOffsetY(int rowIndex, string str, out double value)
        {
            string errMsg;

            if (!double.TryParse(str, out value))
            {
                value = -1;
            }

            if (rowIndex == 0 && value != 0)
            {
                errMsg = "must be 0.000 !";
                value = 0;
            }
            else if (rowIndex > 0 && value < PitchY)
            {
                errMsg = $"must > {PitchY:0.000} !";
                value = PitchY;
            }
            else
            {
                errMsg = null;
            }

            return errMsg;
        }
        void UpdateDgvRows(int targetSegsNum)
        {
            if (targetSegsNum < 1)
                targetSegsNum = 1;

            var dgv = gwSegOffsetDataGridView1.DataGridView;
            for (int r = dgv.Rows.Count; r < targetSegsNum; r++)
            {
                dgv.Rows.Add($"{r}", PitchY);
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
                    var dgvStr = dgvRow.Cells[1].Value.ToString();
                    ParseOffsetY(i, dgvStr, out double offsetY);

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
