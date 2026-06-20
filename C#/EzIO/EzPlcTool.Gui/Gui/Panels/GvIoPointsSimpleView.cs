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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EzIO.Gui
{
    public partial class GvIoPointsSimpleView : UserControl, IoPointsView
    {
        #region GUI_LINKS
        Label _lblTemplate => label0;
        Label _lblStatus => label1;
        AutoScanUiCtrl _ctrl;
        #endregion

        #region PRIVATE_DATA
        List<Control[]> _rows = new List<Control[]>(); // 儲存目前的行列快照
        List<Label> _pool = new List<Label>();         // 物件池：存放所有已產生的 Label
        private int _cellRows = 1;
        private int _cellCols = 1;
        private int _cellWidth = 0;
        #endregion

        public GvIoPointsSimpleView()
        {
            InitializeComponent();
            initGui();
        }

        #region INIT_FUNCTIONS
        void initGui()
        {
            CellsBackColor = _lblTemplate.BackColor = Color.FromArgb(64, 64, 64);
            CellsForeColor = _lblTemplate.ForeColor = Color.White;
            CellsActiveBackColor = Color.Lime;
            CellsActiveForeColor = Color.Black;
            CellsInvertedBackColor = Color.Red;
            CellsInvertedForeColor = Color.White;
            CellsRegBackColor = Color.Black;
            CellsRegForeColor = Color.White;
            CellsRegActiveBackColor = CellsRegBackColor;
            CellsRegActiveForeColor = Color.Lime;

            CellCols = 3;

            PaddingChanged += (s, e) => RefreshLayout();
            SizeChanged += (s, e) => RefreshLayout();
        }
        #endregion

        #region PUBLIC_GUI_PROPERTIES
        public int CellRows
        {
            get => _cellRows;
            set
            {
                if (_cellRows != value && value > 0)
                {
                    _cellRows = value;
                    RefreshLayout();
                }
            }
        }
        public int CellCols
        {
            get => _cellCols;
            set
            {
                if (_cellCols != value && value > 0)
                {
                    _cellCols = value;
                    RefreshLayout();
                }
            }
        }
        public int CellWidth
        {
            get => _cellWidth;
            set
            {
                // 若為 0 則自動平均分配
                if (_cellWidth != value && value > 0)
                {
                    _cellWidth = value;
                    RefreshLayout();
                }
            }
        }
        public Padding CellsMargin
        {
            get => _lblTemplate.Margin;
            set
            {
                if (_lblTemplate.Margin != value)
                {
                    _lblTemplate.Margin = value;
                    RefreshLayout();
                }
            }
        }

        public Color CellsBackColor
        {
            get;
            set;
        }
        public Color CellsForeColor
        {
            get;
            set;
        }
        public Color CellsActiveBackColor
        {
            get;
            set;
        }
        public Color CellsActiveForeColor
        {
            get;
            set;
        }
        public Color CellsInvertedBackColor
        {
            get;
            set;
        }
        public Color CellsInvertedForeColor
        {
            get;
            set;
        }
        public Color CellsRegBackColor
        {
            get;
            set;
        }
        public Color CellsRegForeColor
        {
            get;
            set;
        }
        public Color CellsRegActiveBackColor
        {
            get;
            set;
        }
        public Color CellsRegActiveForeColor
        {
            get;
            set;
        }

        public Control lblStatus
        {
            get => _lblStatus;
        }
        public bool IsStatusPanelVisible
        {
            get => _lblStatus.Visible;
            set => _lblStatus.Visible = panel2.Visible = value;
        }
        public bool ReadOnly
        {
            get;
            set;
        }
        #endregion

        #region LAYOUT_FUNCTIONS
        public void RefreshLayout()
        {
            if (_cellRows <= 0 || _cellCols <= 0) return;

            int totalNeeded = _cellRows * _cellCols;

            // 1. 物件池擴張 (保留 label0 為 [0,0])
            while (_pool.Count < totalNeeded - 1)
            {
                Label newLabel = CreateNewLabelFromTemplate();
                _pool.Add(newLabel);
                this.Controls.Add(newLabel);
                newLabel.DoubleClick += ItemLabel_DoubleClick;
            }

            // 2. 取得間距參數
            Padding m = _lblTemplate.Margin;            // 每個 Cell 的外間距
            Padding p = this.Padding;                   // 容器本身的內襯距離

            int itemHeight = _lblTemplate.Height;

            // 3. 計算可用寬度 (扣除容器 Padding 與所有 Cell 的 Margin)
            // 可用總寬 = ClientWidth - Padding(左+右)
            int availableWidth = this.ClientSize.Width - p.Left - p.Right;

            // 每個 Item 分配到的寬度 (扣除自己的 Margin)
            int itemWidth = CellWidth > 0 ? CellWidth :
                (availableWidth - (_cellCols * (m.Left + m.Right))) / _cellCols;

            this.SuspendLayout();

            _rows.Clear();
            int poolIndex = 0;

            for (int r = 0; r < _cellRows; r++)
            {
                Control[] currentRow = new Control[_cellCols];
                for (int c = 0; c < _cellCols; c++)
                {
                    Label lbl = (r == 0 && c == 0) ? _lblTemplate : _pool[poolIndex++];

                    // 4. 計算座標 (Location)
                    // X = 容器Padding.Left + 已過欄數*(Item寬+左右Margin) + 當前Margin.Left
                    int x = p.Left + c * (itemWidth + m.Left + m.Right) + m.Left;

                    // Y = 容器Padding.Top + 已過列數*(Item高+上下Margin) + 當前Margin.Top
                    int y = p.Top + r * (itemHeight + m.Top + m.Bottom) + m.Top;

                    lbl.Bounds = new Rectangle(x, y, itemWidth, itemHeight);
                    lbl.Visible = true;

                    currentRow[c] = lbl;
                }
                _rows.Add(currentRow);
            }

            // 5. 回收餘下控制項
            for (int i = poolIndex; i < _pool.Count; i++)
            {
                _pool[i].Visible = false;
            }

            this.ResumeLayout();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private Label CreateNewLabelFromTemplate()
        {
            return new Label
            {
                Font = _lblTemplate.Font,
                ForeColor = CellsForeColor,
                BackColor = CellsBackColor,
                TextAlign = _lblTemplate.TextAlign,
                BorderStyle = _lblTemplate.BorderStyle,
                AutoSize = false, // 必須設為 false，否則 Size 設定會無效
                Visible = false
            };
        }
        #endregion

        #region EVENT_HANDLER
        private void ItemLabel_DoubleClick(object sender, System.EventArgs e)
        {
            if (ReadOnly)
                return;
            if (sender is Control c && c.Tag is IoPoint p && p.Bits == 1)
            {
                p.Set(!p.IsOn);
            }
        }
        #endregion

        Control IoPointsView.Window => this;
        Control IoPointsView.lblSamplingRate => lblStatus;

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
            if (!IsHandleCreated)
                return;
            
            var src = new List<IoPoint>(ioPoints);
            src.RemoveAll(p => p == null);
            int rows = (src.Count + _cellCols - 1) / _cellCols;
            this.CellRows = rows;

            for (int idx = 0, N = _cellRows * _cellCols; idx < N; idx++)
            {
                int r = idx / _cellCols;
                int c = idx % _cellCols;
                var p = idx < src.Count ? src[idx] : null;
                updateItem(_rows[r][c], p);
            }
        }

        #region PRIVATE_UPDATA_FUNCTIONS
        void updateItem(Control wnd, IoPoint p)
        {
            if (wnd == null)
                return;

            wnd.Tag = p;

            if (p == null)
            {
                wnd.BackColor = this.BackColor;
                wnd.Text = "";
                return;
            }

            if (p.Bits == 1)
            {
                string text = p.Description;
                if (string.IsNullOrEmpty(text))
                    text = p.Address.ToString();

                wnd.Text = text;
                if (!p.IsOn)
                {
                    wnd.BackColor = CellsBackColor;
                    wnd.ForeColor = CellsForeColor;
                }
                else if (p.Inverted)
                {
                    wnd.BackColor = CellsInvertedBackColor;
                    wnd.ForeColor = CellsInvertedForeColor;
                }
                else
                {
                    wnd.BackColor = CellsActiveBackColor;
                    wnd.ForeColor = CellsActiveForeColor;
                }
            }
            else
            {
                wnd.Text = $"{p.Address}={p.Data}";
                wnd.BackColor = p.Data != 0 ? CellsRegActiveBackColor : CellsRegBackColor;
                wnd.ForeColor = p.Data != 0 ? CellsRegActiveForeColor : CellsRegForeColor;
            }

            wnd.Visible = true;
        }
        #endregion
    }
}
