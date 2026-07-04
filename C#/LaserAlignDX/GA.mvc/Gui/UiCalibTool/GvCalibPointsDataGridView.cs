#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-06 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Lang;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class GvCalibPointsDataGridView : UserControl
    {
        readonly Color MOTOR_COORD_FORE_COLOR = Color.White;

        public event EventHandler SelectedIndexChanged;

        #region PRIVATE_DATA
        int _activeSelectedIndex = -2;
        bool _bypassWindowsEvent = false;
        #endregion

        public GvCalibPointsDataGridView()
        {
            InitializeComponent();
            setupDataGridView();
            dataGridView1.CellValidating += DataGridView1_CellValidating;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            dataGridView1.DataError += DataGridView1_DataError;
            //HandleCreated += (s, e) => ActiveRowID = -1;
            initDemoData();
        }

        #region EVENT_HANDLERS
        HashSet<int> _warnedList = new HashSet<int>();
        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }
        private void DataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
            {
                int code = e.ColumnIndex + e.RowIndex * 10000;

                // 嘗試解析輸入的值
                if (e.FormattedValue == null || !double.TryParse(e.FormattedValue.ToString(), out double newDoubleValue))
                {
                    // 使用 _warnList 防止無限迴圈報警
                    if (_warnedList.Contains(code))
                        return;

                    // 如果轉換失敗，表示輸入的不是數字
                    e.Cancel = true; // 取消變更，將編輯模式留在原儲存格

                    // 加入 _warnedList
                    _warnedList.Add(code);

                    // 顯示警告訊息給使用者
                    //MessageBox.Show("馬達座標欄位只能輸入數字!", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    QMessageBox.Warning(EzAoiEmptyTrayInspector.Model.Prompts.Waring_Motor_Coords_Input_Must_Be_Number);
                }
                else
                {
                    _warnedList.Remove(code);
                }
            }
        }
        private void DataGridView1_SelectionChanged(object sender, System.EventArgs e)
        {
            if (_bypassWindowsEvent)
                return;

            var index = getDgvActiveSelectedIndex();
            if (index != _activeSelectedIndex)
            {
                setDgvActiveSelectedIndex(index);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void setupDataGridView()
        {
            // 清除任何現有的欄位以避免重複添加
            dataGridView1.Columns.Clear();

            var headers = new string[]
            {
                "點位",
                "相機 X",
                "相機 Y",
                "吸嘴馬達 X",
                "載台馬達 Y",
            };

            // 設定標頭樣式 文字水平對齊方式為置中
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            // 星號 column 隱藏
            dataGridView1.RowHeadersVisible = false;
            // Row Height 高度固定
            dataGridView1.AllowUserToResizeRows = false;
            // 讓 DataGridView 自動調整欄位寬度以填滿可用空間
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //// 設定 DataGridView 的預設樣式
            //// 注意：這會影響到所有的儲存格
            var defaultCellStyle = dataGridView1.DefaultCellStyle;
            //// 將選取時的背景顏色設定為與非選取時的背景顏色相同
            defaultCellStyle.SelectionBackColor = defaultCellStyle.BackColor;
            //// 將選取時的前景（文字）顏色設定為與非選取時的前景顏色相同
            defaultCellStyle.SelectionForeColor = defaultCellStyle.ForeColor;
            // CellSelect
            dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView1.MultiSelect = true;
            // 將 DataGridView 的 AllowUserToAddRows 屬性設為 false
            dataGridView1.AllowUserToAddRows = false;

            int i = 0;
            foreach (string headTxt in headers)
            {
                var column = new DataGridViewTextBoxColumn();
                column.HeaderText = headTxt;
                column.Name = $"column_{i}";
                column.ReadOnly = i < 3;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

                if (i == 0)
                {
                    column.Width = 80;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    column.DefaultCellStyle.SelectionBackColor = Color.Gold;
                }
                else if (i == 1 || i == 2)
                {
                    column.ValueType = typeof(double);
                    column.DefaultCellStyle.Format = "0.0";
                    column.DefaultCellStyle.BackColor = Color.LightGray;
                    column.DefaultCellStyle.SelectionBackColor = Color.LightGray;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
                }
                else if (i == 3 || i == 4)
                {
                    column.ValueType = typeof(double);
                    column.DefaultCellStyle.Format = "0.000";
                    column.DefaultCellStyle.BackColor = Color.Black;
                    column.DefaultCellStyle.ForeColor = MOTOR_COORD_FORE_COLOR;
                    column.DefaultCellStyle.SelectionBackColor = Color.Black;
                    column.DefaultCellStyle.SelectionForeColor = MOTOR_COORD_FORE_COLOR;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
                    column.DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                }

                dataGridView1.Columns.Add(column);
                i++;
            }
        }
        private void initDemoData()
        {
            // 新增一些範例資料
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add("左上", 10000, 10000, -1000.0, -1200.0);
            dataGridView1.Rows.Add("右上", 38000, 10000, -500.0, -100.0);
            dataGridView1.Rows.Add("右下", 38000, 18000, -500.0, -500.0);
            dataGridView1.Rows.Add("左下", 10000, 38000, -1000.0, -1500.0);
        }
        private int getDgvActiveSelectedIndex()
        {
            var cells = dataGridView1.SelectedCells;
            if (cells != null && cells.Count > 0)
            {
                if(cells[0].Selected)
                {
                    return cells[0].RowIndex;
                }
            }
            return -1;
            //for (int i = 0, N = dataGridView1.Rows.Count; i < N; i++)
            //{
            //    bool isSelected = dataGridView1.Rows[i].Cells[0].Selected;
            //    if (isSelected)
            //        return i;
            //}
            //return -1;
        }
        private void setDgvActiveSelectedIndex(int index)
        {
            _bypassWindowsEvent = true;
            _activeSelectedIndex = index;
            if (index < 0)
            {
                dataGridView1.ClearSelection();
            }
            else
            {
                for (int i = 0, N = dataGridView1.Rows.Count; i < N; i++)
                {
                    bool isSelected = index == i;
                    dataGridView1.Rows[i].Cells[0].Selected = isSelected;
                }
            }
            _bypassWindowsEvent = false;

            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);

        }
        #endregion

        public int SelectedIndex
        {
            get => _activeSelectedIndex;
            set
            {
                if (value != _activeSelectedIndex)
                    setDgvActiveSelectedIndex(value);
            }
        }
        public DataGridView DataGridView => dataGridView1;
        public static implicit operator DataGridView(GvCalibPointsDataGridView wnd)
        {
            return wnd?.dataGridView1;
        }

        public void SetReadOnly(int columnIndex, bool readOnly, Color? backColor = null)
        {
            var dgv = dataGridView1;
            if (columnIndex >= 0 && columnIndex < dgv.Columns.Count)
            {
                DataGridViewColumn targetColumn = dgv.Columns[columnIndex];

                // 设置只读
                targetColumn.ReadOnly = readOnly;

                // 设置背景颜色
                if (backColor != null)
                    targetColumn.DefaultCellStyle.BackColor = backColor.Value;

                // 刷新 DataGridView 以确保立即应用视觉更改
                dgv.Invalidate();
            }
        }
    }
}
