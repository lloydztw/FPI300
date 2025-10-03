using LaserAlignDX.OPSpace.RecipeSpace;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;

namespace LaserAlignDX.FormSpace.FPI30Form
{
    public partial class CalibrationUI : UserControl
    {
        string m_Name = $"";
        int m_Index = 0;

        LineScanCalibrateClass m_Calibration
        {
            get { return Universal.LineScanCalibrateClasses[m_Index]; }
        }

        public CalibrationUI()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="eName">名称</param>
        /// <param name="eIndex">第几组</param>
        public void Init(string eName, int eIndex)
        {
            m_Name = eName;
            m_Index = eIndex;
            InitializeDataGridView();
            UpdateDataViews();
            UpdateDataWorlds();
        }
        /// <summary>
        /// 写入当前点位
        /// </summary>
        /// <param name="eCurrentIndex">当前点位</param>
        /// <param name="ePtf">虚拟点</param>
        public void SetViewPoints(int eCurrentIndex, PointF ePtf)
        {
            m_Calibration.ptsview[eCurrentIndex] = new PointF(ePtf.X, ePtf.Y);
            UpdateDataViews();
        }
        public void GetViewWorldPoints()
        {
            int i = 0;
            while (i < 4)
            {
                m_Calibration.ptsview[i].X = float.Parse(dgv.Rows[i].Cells[2].Value.ToString());
                m_Calibration.ptsview[i].Y = float.Parse(dgv.Rows[i].Cells[3].Value.ToString());
                m_Calibration.ptsworld[i].X = float.Parse(dgv.Rows[i].Cells[4].Value.ToString());
                m_Calibration.ptsworld[i].Y =  float.Parse(dgv.Rows[i].Cells[5].Value.ToString());
                i++;
            }
        }
        void UpdateDataViews()
        {
            int i = 0;
            while (i < 4)
            {
                dgv.Rows[i].Cells[2].Value = m_Calibration.ptsview[i].X;
                dgv.Rows[i].Cells[3].Value = m_Calibration.ptsview[i].Y;
                //dgv.Rows[i].Cells[3].Value = m_Calibration.ptsworld[i].X;
                //dgv.Rows[i].Cells[4].Value = m_Calibration.ptsworld[i].Y;
                i++;
            }
        }
        void UpdateDataWorlds()
        {
            int i = 0;
            while (i < 4)
            {
                //dgv.Rows[i].Cells[1].Value = m_Calibration.ptsview[i].X;
                //dgv.Rows[i].Cells[2].Value = m_Calibration.ptsview[i].Y;
                dgv.Rows[i].Cells[4].Value = m_Calibration.ptsworld[i].X;
                dgv.Rows[i].Cells[5].Value = m_Calibration.ptsworld[i].Y;
                i++;
            }
        }
        private void InitializeDataGridView()
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            //dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;

            dgv.Columns.Add("col0", "载台");
            dgv.Columns.Add("col01", "吸嘴");
            dgv.Columns.Add("col1", "图像虚拟点X");
            dgv.Columns.Add("col2", "图像虚拟点Y");
            dgv.Columns.Add("col3", "实际点X");
            dgv.Columns.Add("col4", "实际点Y");

            dgv.Columns["col0"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col01"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col3"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col4"].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 初始化数据
            dgv.Rows.Add($"{m_Name}点1", $"{m_Name}点1", 0, 0, 0, 0);
            dgv.Rows.Add($"{m_Name}点2", $"{m_Name}点2", 0, 0, 0, 0);
            dgv.Rows.Add($"{m_Name}点3", $"{m_Name}点3", 0, 0, 0, 0);
            dgv.Rows.Add($"{m_Name}点4", $"{m_Name}点4", 0, 0, 0, 0);

            int i = 0;
            while (i < 4)
            {
                switch (m_Index)
                {
                    case 0:
                        dgv.Rows[i].Cells[0].Value = $"载台一";
                        break;
                    case 1:
                        dgv.Rows[i].Cells[0].Value = $"载台一";
                        break;
                    case 2:
                        dgv.Rows[i].Cells[0].Value = $"载台二";
                        break;
                    case 3:
                        dgv.Rows[i].Cells[0].Value = $"载台二";
                        break;
                }
                dgv.Rows[i].Cells[1].Value = $"吸嘴{i + 1}点{i + 1}";
                //dgv.Rows[i].Cells[0].Value = $"{m_Name}点{i+1}";
                i++;
            }

            m_Calibration.Load();

            //dgv.Rows.Add($"{m_Name}点1", 0, 0, 0, 0);
            //dgv.Rows.Add($"{m_Name}点2", 0, 0, 0, 0);
            //dgv.Rows.Add($"{m_Name}点3", 0, 0, 0, 0);
            //dgv.Rows.Add($"{m_Name}点4", 0, 0, 0, 0);


        }
    }
}
