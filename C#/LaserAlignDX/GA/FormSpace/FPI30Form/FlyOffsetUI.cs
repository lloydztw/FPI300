using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LaserAlignDX.GA.FormSpace.FPI30Form
{
    public partial class FlyOffsetUI : UserControl
    {
        const int POINT_COUNT = 8;
        public FlyParaClass flyPara
        {
            get { return FlyParaClass.Instance; }
        }

        public FlyOffsetUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InitializeDataGridView();
            UpdateData();
        }
        public void GetPoints()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                flyPara.ptsOffset[i].X = float.Parse(dgv.Rows[i].Cells[1].Value.ToString());
                flyPara.ptsOffset[i].Y = float.Parse(dgv.Rows[i].Cells[2].Value.ToString());
                i++;
            }
        }
        void UpdateData()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                dgv.Rows[i].Cells[1].Value = flyPara.ptsOffset[i].X;
                dgv.Rows[i].Cells[2].Value = flyPara.ptsOffset[i].Y;
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

            dgv.Columns.Add("col0", "吸嘴");
            dgv.Columns.Add("col1", "补偿X");
            dgv.Columns.Add("col2", "补偿Y");

            dgv.Columns["col0"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 初始化数据

            int i = 0;
            while (i < POINT_COUNT)
            {
                dgv.Rows.Add($"吸嘴{(i+1).ToString()}", $"吸嘴{i.ToString()}", 0, 0);

                i++;
            }

            i = 0;
            while (i < POINT_COUNT)
            {
                dgv.Rows[i].Cells[0].Value = $"吸嘴{(i + 1).ToString()}";

                i++;
            }


        }
    }
}
