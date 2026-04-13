#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GPaneMotorSimpleJogs : UserControl
    {
        #region PRIVATE_DATA
        IvSimpleJogView[] _ctrls;
        JogStickOption _option = JogStickOption.Vert;
        int _rows = 1;
        int _cols = 1;
        #endregion

        public GPaneMotorSimpleJogs()
        {
            InitializeComponent();
            gwMotorSimpleJogH1.Visible = false;
            gwMotorSimpleJogV1.Visible = true;
            //>>> Config(_rows, _cols, _option);
            _ctrls = new IvSimpleJogView[] { gwMotorSimpleJogV1 };
            SizeChanged += (s, e) => autoLayout();
        }
        public IvSimpleJogView GetView(int i)
        {
            return _ctrls?[i];
        }
        public void Config(JogStickOption option, int total)
        {
            autoRowsCols(option, total, out int rows, out int cols);
            Config(option, rows, cols, total);
        }
        public void Config(JogStickOption option, int rows, int cols, int? total = null)
        {
            tearDown();

            gwMotorSimpleJogV1.Visible = (option == JogStickOption.Vert);
            gwMotorSimpleJogH1.Visible = (option != JogStickOption.Vert);
            ICloneable protoType = getProtoType(option) as ICloneable;

            _option = option;
            _rows = rows;
            _cols = cols;

            buildGuiCtrls(protoType, rows, cols);

            if (total != null)
            {
                int N = Math.Min(_ctrls.Length, total.Value);
                for (int i = N; i < _ctrls.Length; i++)
                {
                    if (_ctrls[i] is Control wnd)
                        wnd.Enabled = false;
                }
            }
        }
        public Size EstimateClientSize()
        {
            int xgap = 8;
            int ygap = 8;
            var ctrl = getProtoType(_option) as Control;
            int W = (ctrl.Width + xgap) * _cols + xgap;
            int H = (ctrl.Height + ygap) * _rows + ygap;
            return new Size(W, H);
        }

        #region PRIVATE_FUNCTIONS
        IvSimpleJogView getProtoType(JogStickOption option)
        {
            IvSimpleJogView protoType = (option == JogStickOption.Vert)
                            ? (IvSimpleJogView)gwMotorSimpleJogV1
                            : (IvSimpleJogView)gwMotorSimpleJogH1;
            return protoType;
        }
        void autoRowsCols(JogStickOption option, int N, out int rows, out int cols)
        {
            if (N <= 4)
            {
                rows = 1;
                cols = 4;
            }
            else if (N <= 8)
            {
                rows = 2;
                cols = 4;
            }
            else
            {
                rows = 2;
                cols = (N + rows - 1) / rows;
            }

            // SWAP rows and cols
            ////if (option != JogStickOption.Vert)
            ////{
            ////    var tmp = rows;
            ////    rows = cols;
            ////    cols = tmp;
            ////}
        }
        void buildGuiCtrls(ICloneable protoType, int rows, int cols)
        {
            int N = rows * cols;
            var views = new IvSimpleJogView[N];
            for (int i = 1; i < N; i++)
            {
                views[i] = protoType.Clone() as IvSimpleJogView;
            }
            _ctrls = views;
            _ctrls[0] = protoType as IvSimpleJogView;
        }
        void tearDown()
        {
            var old = _ctrls;
            _ctrls = null;
            if (old != null)
            {
                for (int i = old.Length - 1; i > 0; i--)
                {
                    if (old[i] is IDisposable d)
                        d.Dispose();
                    old[i] = null;
                }
            }
        }
        void autoLayout()
        {
            if (_ctrls == null || _ctrls.Length == 0)
                return;
            
            if (_rows == 0 || _cols == 0)
                return;

            int xgap = 8;
            int ygap = 8;
            var rows = _rows;
            var cols = _cols;

            Control ctrl0 = _ctrls[0] as Control;
            if (ctrl0 == null)
                return;

            Size sz0 = ctrl0.Size;

            var rcc = ClientRectangle;
            int x0 = (rcc.Width - (sz0.Width + xgap) * cols + xgap) / 2;
            int y0 = (rcc.Height - (sz0.Height + ygap) * rows + ygap) / 2;
            int y = y0;
            int i = 0;           
            for (int row = 0; row < rows; row++)
            {
                int x = x0;
                for (int col = 0; col < cols; col++)
                {
                    if (_ctrls[i] is Control wnd)
                    {
                        wnd.Location = new Point(x, y);
                        wnd.Size = sz0;
                    }
                    x += (sz0.Width + xgap);
                    i++;
                }
                y += (sz0.Height + ygap);
            }
        }
        #endregion
    }
}
