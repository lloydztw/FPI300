using JetEazy.BasicSpace;
using JetEazy.Interface;
using JetEazy.Lang;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VsCommon.ControlSpace.MachineSpace;

namespace NeedleX.UISpace.UIMVC.Controler
{
    public class CtlPosClass
    {
        PosUI posUI;

        Button btnADD;
        Button btnDEL;
        Button btnUpdate;
        Button btnGO;

        DataGridView DGVIEW;

        NeedleMachineClass MACHINE;
        private string m_pos = string.Empty;

        public CtlPosClass(PosUI ePos)
        {
            this.posUI = ePos;
            InitialInternal();
        }
        public void Init(NeedleMachineClass eMACHINE,string ePOS)
        {
            MACHINE = eMACHINE;
            m_pos = ePOS;

            DGVIEW.SelectionChanged += DGVIEW_SelectionChanged;
            DGVIEW.RowPostPaint += DGVIEW_RowPostPaint;

            FillDisplay();
        }
        public string GetPositionList()
        {
            string strresult = string.Empty;
            int i = 0;
            while (i < DGVIEW.Rows.Count)
            {
                strresult += DGVIEW.Rows[i].Cells[0].Value.ToString() + ",";
                strresult += DGVIEW.Rows[i].Cells[1].Value.ToString() + ",";
                strresult += DGVIEW.Rows[i].Cells[2].Value.ToString() + ";";
                i++;
            }

            strresult = RemoveLastChar(strresult, 1);
            return strresult;
        }

        void InitialInternal()
        {
            btnADD = this.posUI.button8;
            btnDEL = this.posUI.button6;
            btnUpdate = this.posUI.button7;
            btnGO = this.posUI.button5;
            DGVIEW = this.posUI.dataGridView1;


            btnADD.Click += BtnADD_Click;
            btnDEL.Click += BtnDEL_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnGO.Click += BtnGO_Click;

        }
        private void DGVIEW_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                                                                               e.RowBounds.Location.Y,
                                                                               DGVIEW.RowHeadersWidth - 4,
                                                                               e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                                                       DGVIEW.RowHeadersDefaultCellStyle.Font,
                                                       rectangle,
                                                       DGVIEW.RowHeadersDefaultCellStyle.ForeColor,
                                                       TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void DGVIEW_SelectionChanged(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;


        }

        private void BtnGO_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;

            string onStrMsg = "更新 表第 " + rowindex + " 行？";
            string offStrMsg = "更新 表第 " + rowindex + " 行？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (QMessageBox.Question(msg) != DialogResult.Yes)
            {
                return;
            }

            string[] strpos = _getModulePosition().Split(',').ToArray();

            if (strpos.Length >= 3)
            {
                this.DGVIEW.Rows[rowindex].Cells[0].Value = strpos[0];
                this.DGVIEW.Rows[rowindex].Cells[1].Value = strpos[1];
                this.DGVIEW.Rows[rowindex].Cells[2].Value = strpos[2];
            }
        }

        private void BtnDEL_Click(object sender, EventArgs e)
        {
            if (DGVIEW.Rows.Count <= 0)
                return;

            int rowindex = DGVIEW.CurrentCell.RowIndex;
            if (rowindex == -1)
                return;

            string onStrMsg = "删除 表第 " + rowindex + " 行？";
            string offStrMsg = "删除 表第 " + rowindex + " 行？";
            string msg = (true ? offStrMsg : onStrMsg);

            if (QMessageBox.Question(msg) != DialogResult.Yes)
            {
                return;
            }

            DGVIEW.Rows.RemoveAt(rowindex);
        }

        private void BtnADD_Click(object sender, EventArgs e)
        {
            //if (this.DGVIEW.Rows.Count < m_pos_count)
            {
                string[] strpos = _getModulePosition().Split(',').ToArray();
                if (strpos.Length >= 3)
                {
                    int index = this.DGVIEW.Rows.Add();
                    this.DGVIEW.Rows[index].Cells[0].Value = strpos[0];
                    this.DGVIEW.Rows[index].Cells[1].Value = strpos[1];
                    this.DGVIEW.Rows[index].Cells[2].Value = strpos[2];
                }
            }
        }


        string _getModulePosition()
        {
            string str = string.Empty;
            str += GetAxis(0).GetPos().ToString("0.000000") + ",";
            str += GetAxis(1).GetPos().ToString("0.000000") + ",";
            str += GetAxis(2).GetPos().ToString("0.000000");
            return str;
        }
        private IAxis GetAxis(int axisID)
        {
            return MACHINE.PLCMOTIONCollection[axisID];
        }
        string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }
        void FillDisplay()
        {
            DGVIEW.Rows.Clear();
            List<string> listpos = m_pos.Split(';').ToList();
            if (listpos.Count > 0)
            {
                foreach (string str in listpos)
                {
                    List<string> listpostemp = str.Split(',').ToList();
                    if (listpostemp.Count == 3)
                    {
                        if (string.IsNullOrEmpty(listpostemp[0]) || string.IsNullOrEmpty(listpostemp[1]) || string.IsNullOrEmpty(listpostemp[2]))
                        {

                        }
                        else
                        {
                            //if (this.DGVIEW.Rows.Count < m_pos_count)
                            {
                                int index = this.DGVIEW.Rows.Add();
                                this.DGVIEW.Rows[index].Cells[0].Value = listpostemp[0];
                                this.DGVIEW.Rows[index].Cells[1].Value = listpostemp[1];
                                this.DGVIEW.Rows[index].Cells[2].Value = listpostemp[2];
                            }
                        }
                    }
                }
            }
        }


    }
}
