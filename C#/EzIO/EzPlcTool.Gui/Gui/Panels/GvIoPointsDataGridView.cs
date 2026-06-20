#region AUTHOR
/*
 * EzIO GUI
 * Copyright (C) 2026
 * 2026-04-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Ctrl;
using EzIO.Device;
using EzIO.Mem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace EzIO.Gui
{
    public partial class GvIoPointsDataGridView : UserControl, IoPointsView
    {
        public event EventHandler SelectedIndexChanged;

        #region PRIVATE_DATA
        int _activeSelectedIndex = -2;
        bool _bypassWindowsEvent = false;
        #endregion

        public GvIoPointsDataGridView()
        {
            InitializeComponent();
            
            setupDataGridView();
            dataGridView1.CellValidating += DataGridView1_CellValidating;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            dataGridView1.DataError += DataGridView1_DataError;
            
            //HandleCreated += (s, e) => ActiveRowID = -1;

            if (DesignMode)
                initDefaultData();
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
                    MessageBox.Show("馬達座標欄位只能輸入數字!", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                "位址",
                "說明",
                "數值",
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
            //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
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
                column.ReadOnly = true;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;

                if (i == 0)
                {
                    column.Width = 80;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    //column.DefaultCellStyle.SelectionBackColor = Color.Gold;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }
                else if (i == 1)
                {
                    //column.ValueType = typeof(string);
                    column.Width = 300;
                    //column.DefaultCellStyle.Format = "0.0";
                    column.DefaultCellStyle.BackColor = Color.LightGray;
                    column.DefaultCellStyle.SelectionBackColor = Color.LightGray;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    column.DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
                }
                else
                {
                    //column.ValueType = typeof(int);
                    column.Width = 80;
                    //column.DefaultCellStyle.Format = "0.000";
                    column.DefaultCellStyle.BackColor = Color.Black;
                    column.DefaultCellStyle.ForeColor = Color.White;
                    column.DefaultCellStyle.SelectionBackColor = Color.Black;
                    column.DefaultCellStyle.SelectionForeColor = Color.White;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    column.DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
                    column.DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                }

                dataGridView1.Columns.Add(column);
                i++;
            }
        }
        private void initDefaultData()
        {
            // 新增一些範例資料
            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add("M0", "急停", 1);
            dataGridView1.Rows.Add("R3840", "傳感器數據", 2814);
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
        public DataGridView DataGridView
        {
            get => dataGridView1;
        }
        public static implicit operator DataGridView(GvIoPointsDataGridView wnd)
        {
            return wnd?.dataGridView1;
        }

        #region DataGridView_FUNCTIONS
        public void Clear()
        {
            DataGridView.Rows.Clear();
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
        #endregion

        AutoScanUiCtrl _ctrl;
        Control IoPointsView.Window => this;
        Control IoPointsView.lblSamplingRate => null;

        public void Attach(IEnumerable<IoPoint> ioPoints, IAutoScan autoScan)
        {
            if (_ctrl == null)
            {
                _ctrl = new AutoScanUiCtrl();
                _ctrl.Attach(this, ioPoints, autoScan);
            }
        }
        public void Update(IEnumerable<IoPoint> ioPoints)
        {
            if (IsHandleCreated)
            {
                updateIoPoints(ioPoints, autoCreate: true);
            }
        }

        #region PRIVATE_GUI_FUNCTIONS
        void updateIoPoints(IEnumerable<IoPoint> ioPoints, bool autoCreate = false)
        {
            var dgv = this.DataGridView;

            int rowId = 0;
            foreach (var ioPoint in ioPoints)
            {
                if (ioPoint == null) 
                    continue;
                
                if (rowId >= dgv.Rows.Count)
                {
                    if (autoCreate)
                        dgv.Rows.Add("", "", "");
                    else
                        break;
                }

                var cells = dgv.Rows[rowId].Cells;
                cells[0].Value = ioPoint.Address.ToString();
                cells[1].Value = ioPoint.Description;

                (var value, var color) = getValue(ioPoint);
                cells[2].Value = value;
                cells[2].Style.ForeColor = color;

                rowId++;
            }
        }
        (string value, Color color) getValue(IoPoint ioPoint)
        {
            if (ioPoint == null)
                return ("", Color.White);

            if (ioPoint.Bits == 1)
            {
                var value = ioPoint.IsOn ? $"{ioPoint.Data} (On)" : $"{ioPoint.Data} (Off)";
                Color color = ioPoint.IsOn ? (ioPoint.Inverted ? Color.Red : Color.Lime) : Color.White;
                return (value.ToString(), color);
            }
            else
            {
                var value = ioPoint.Data;
                Color color = value != 0 ? Color.Lime : Color.White;
                //string fmt = $"D{ioPoint.Bits / 4}";
                return (value.ToString(), color);
            }
        }
        #endregion
    }
}
