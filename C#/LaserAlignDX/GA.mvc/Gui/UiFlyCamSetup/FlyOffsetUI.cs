using Eazy_Project_III;
using EzAoiEmptyTrayInspector.Lang;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Windows.Forms;

namespace LaserAlignDX.GA.FormSpace.FPI30Form
{
    public partial class FlyOffsetUI : UserControl
    {
        const int POINT_COUNT = 8;
        public FlyParaClass flyPara
        {
            get { return FlyParaClass.Instance; }
        }

        StageNumber m_StageNumber = StageNumber.N0;
        
        string _tagCompensate;
        string _tagSucker;

        public FlyOffsetUI()
        {
            InitializeComponent();
            _tagCompensate = QMSG.Text("補償", "gui");
            _tagSucker = QMSG.Text("吸嘴", "gui");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(StageNumber eStage)
        {
            m_StageNumber = eStage;

            InitializeDataGridView();
            UpdateData();
        }
        public void GetPoints()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                switch(m_StageNumber)
                {
                    case StageNumber.N1:
                        flyPara.ptsOffset2[i].X = float.Parse(dgv.Rows[i].Cells[1].Value.ToString());
                        flyPara.ptsOffset2[i].Y = float.Parse(dgv.Rows[i].Cells[2].Value.ToString());
                        break;
                    default:
                        flyPara.ptsOffset[i].X = float.Parse(dgv.Rows[i].Cells[1].Value.ToString());
                        flyPara.ptsOffset[i].Y = float.Parse(dgv.Rows[i].Cells[2].Value.ToString());
                        break;
                }
                
                i++;
            }
        }

        void UpdateData()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                switch(m_StageNumber)
                {
                    case StageNumber.N1:

                        dgv.Rows[i].Cells[1].Value = flyPara.ptsOffset2[i].X;
                        dgv.Rows[i].Cells[2].Value = flyPara.ptsOffset2[i].Y;
                        break;
                    default:

                        dgv.Rows[i].Cells[1].Value = flyPara.ptsOffset[i].X;
                        dgv.Rows[i].Cells[2].Value = flyPara.ptsOffset[i].Y;
                        break;
                }
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

            dgv.Columns.Add("col0", _tagSucker);            // "吸嘴");
            dgv.Columns.Add("col1", _tagCompensate + "X");  // "补偿X");
            dgv.Columns.Add("col2", _tagCompensate + "Y");  // "补偿Y");

            dgv.Columns["col0"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 初始化数据

            int i = 0;
            while (i < POINT_COUNT)
            {
                //dgv.Rows.Add($"吸嘴{(i + 1).ToString()}", $"吸嘴{i.ToString()}", 0, 0);
                dgv.Rows.Add($"{_tagSucker}{(i + 1)}", $"{_tagSucker}{i}", 0, 0);

                i++;
            }

            i = 0;
            while (i < POINT_COUNT)
            {
                //dgv.Rows[i].Cells[0].Value = $"吸嘴{(i + 1).ToString()}";
                dgv.Rows[i].Cells[0].Value = $"{_tagSucker}{i + 1}";

                i++;
            }
        }
    }
}
