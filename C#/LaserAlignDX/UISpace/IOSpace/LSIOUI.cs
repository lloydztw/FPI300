using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Eazy_Project_III;
using Traveller106.ControlSpace.MachineSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Traveller106.UISpace.IOSpace
{
    public partial class LSIOUI : UserControl
    {
        ComboBox cboOnOff;
        DataTable Dtable = new DataTable();
        DataGridView DGview;
        NumericUpDown numInput;

        List<LSIOItemClass> list = new List<LSIOItemClass>();

        bool IsNeedToChange = false;
        string OperateColumnNow = "";
        string OperateColumnNowAddress = "";
        bool OperateColumnNowReadOnly = false;

        MiniX6MachineClass MACHINE;
        

        public LSIOUI()
        {
            InitializeComponent();

            DGview = dataGridView1;
            cboOnOff = comboBox1;
        }

        public void Initial(string filename, MiniX6MachineClass machine)
        {
            MACHINE = machine;
            cboOnOff.Items.Clear();
            int i = 0;
            while (i < (int)SwicthOnOff.COUNT)
            {
                cboOnOff.Items.Add(((SwicthOnOff)i));
                i++;
            }

            cboOnOff.SelectedIndexChanged += CboOnOff_SelectedIndexChanged;

            numInput = numericUpDown1;
            numInput.ValueChanged += new EventHandler(numInput_ValueChanged);

            list.Clear();

            if (!string.IsNullOrEmpty(filename))
            {
                StreamReader streamReader = new StreamReader(filename);
                string strline = string.Empty;
                while (!streamReader.EndOfStream)
                {
                    strline = streamReader.ReadLine();
                    LSIOItemClass itemClass = new LSIOItemClass(strline);
                    list.Add(itemClass);
                }

                streamReader.Close();
                streamReader.Dispose();
            }

            FillDisplay();
        }
        public void Tick()
        {
            foreach (DataGridViewRow irow in DGview.Rows)
            {
                DataGridViewCell cell = DGview[0, irow.Index];

                string currentValueStr = (string)cell.Value;
                if (string.IsNullOrEmpty(currentValueStr))
                    continue;
                switch (currentValueStr.Substring(0, 1))
                {
                    //case "M":
                    //    int inputvalue = 0;
                    //    DataGridViewCell cell2 = DGview[2, irow.Index];
                    //    //bool bOK = int.TryParse((string)cell2.Value, out inputvalue);
                    //    //if (bOK)
                    //    {
                    //        int value2c = MACHINE.PLCIO.GetBit("0:" + (string)cell.Value);
                    //        DGview[2, irow.Index].Value = value2c.ToString();
                    //    }
                    //    break;
                    case "M":
                        bool ison1 = MACHINE.PLCIO.GetBit("0:" + (string)cell.Value);
                        DGview[2, irow.Index].Value = (ison1 ? "True" : "False");
                        break;
                        //default:
                        //    bool ison = MACHINE.PLCIO.GetQXQB("0:" + (string)cell.Value);
                        //    DGview[2, irow.Index].Value = (ison ? "True" : "False");
                        //    break;
                }

                //foreach (DataGridViewColumn icol in DGview.Columns)
                //{
                //    DataGridViewCell cell = DGview[icol.Index, irow.Index];
                //    if (icol.Index != 2)
                //        break;



                //    //switch (((string)cell.Value).ToUpper())
                //    //{
                //    //    case "TRUE":
                //    //        cell.Style.BackColor = Color.Lime;
                //    //        break;
                //    //    default:
                //    //        cell.Style.BackColor = Color.White;
                //    //        break;
                //    //}
                //}
            }
        }

        private void CboOnOff_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsNeedToChange)
                return;

            //if (OperateColumnNowAddress.IndexOf("IX") > -1)
            //    return;
            if (OperateColumnNowReadOnly)
                return;

            MACHINE.PLCIO.SetBit("0:" + OperateColumnNowAddress, ((SwicthOnOff)cboOnOff.SelectedIndex) == SwicthOnOff.True);
        }
        void numInput_ValueChanged(object sender, EventArgs e)
        {
            if (!IsNeedToChange)
                return;

            //if (OperateColumnNowAddress.IndexOf("MW") > -1)
            //{
            //    int inputvalue = (int)numInput.Value;
            //    //DataGridViewCell cell2 = DGview[2, DGview.CurrentRow.Index];
            //    //bool bOK = int.TryParse((string)cell2.Value, out inputvalue);
            //    //if (bOK)
            //    {
            //        MACHINE.PLCIO.SetMW("0:" + OperateColumnNowAddress, inputvalue);
            //    }
            //}
        }

        void FillDisplay()
        {
            //DGview.DataSource = null;
            Dtable.Rows.Clear();
            Dtable.Columns.Clear();

            Dtable.Columns.Add("Address", typeof(string));
            Dtable.Columns.Add("Funtion", typeof(string));
            Dtable.Columns.Add("Current Value", typeof(string));
            Dtable.Columns.Add("Switch", typeof(SwicthOnOff));
            Dtable.Columns.Add("Ment", typeof(string));
            Dtable.Columns.Add("ReadOnly", typeof(bool));

            foreach (LSIOItemClass rcpsubitem in list)
            {
                DataRow DRow = Dtable.NewRow();

                DRow["Address"] = rcpsubitem.Address;
                DRow["Funtion"] = rcpsubitem.Funtion;
                DRow["Current Value"] = rcpsubitem.CurrentValue;
                DRow["Switch"] = rcpsubitem.Switch;
                DRow["Ment"] = rcpsubitem.Ment;
                DRow["ReadOnly"] = rcpsubitem.ReadOnly;

                Dtable.Rows.Add(DRow);
            }

            //for (int i = 0; i < this.DGview.Columns.Count; i++)
            //{
            //    this.DGview.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            //}

            foreach (DataGridViewColumn column in DGview.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.Width = 150;
            }

            DGview.DataSource = Dtable;

            DGview.CellEnter += new DataGridViewCellEventHandler(DGview_CellEnter);
            DGview.Scroll += new ScrollEventHandler(DGview_Scroll);
            DGview.Paint += DGview_Paint;
        }

        private void DGview_Paint(object sender, PaintEventArgs e)
        {
            foreach (DataGridViewRow irow in DGview.Rows)
            {
                DataGridViewCell cell = DGview[5, irow.Index];
                if((bool)cell.Value)
                {
                    DGview[0, irow.Index].Style.BackColor = Color.Yellow;
                    if (((string)DGview[2, irow.Index].Value).ToUpper() == "TRUE")
                        DGview[2, irow.Index].Style.BackColor = Color.Lime;
                }
                else
                {

                    DGview[0, irow.Index].Style.BackColor = Color.Lime;
                    if (((string)DGview[2, irow.Index].Value).ToUpper() == "TRUE")
                        DGview[2, irow.Index].Style.BackColor = Color.Lime;

                }
                

                //foreach (DataGridViewColumn icol in DGview.Columns)
                //{
                //    DataGridViewCell cell = DGview[icol.Index, irow.Index];
                //    if (icol.Index == 3)
                //        break;
                //    switch (((string)cell.Value).ToUpper())
                //    {
                //        case "TRUE":
                //            cell.Style.BackColor = Color.Lime;
                //            break;
                //        default:

                //            string valuestr = (string)cell.Value;
                //            if (valuestr.IndexOf("IX") > -1)
                //            {
                //                cell.Style.BackColor = Color.Yellow;
                //            }
                //            else if (valuestr.IndexOf("M") > -1)
                //            {
                //                cell.Style.BackColor = Color.Green;
                //            }
                //            else
                //            {
                //                cell.Style.BackColor = Color.White;
                //            }

                            
                //            break;
                //    }
                //}



            }
        }

        void DGview_Scroll(object sender, ScrollEventArgs e)
        {
            cboOnOff.Visible = false;
            numInput.Visible = false;
        }
        void DGview_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            Rectangle r = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

            r = dgv.RectangleToScreen(r);

            cboOnOff.Visible = false;

            switch (dgv.Columns[e.ColumnIndex].HeaderText)
            {
                case "Address":
                case "Funtion":
                case "Current Value":
                case "Ment":
                case "ReadOnly":
                    dgv.EditMode = DataGridViewEditMode.EditProgrammatically;
                    break;

                case "Current ValueX":

                    OperateColumnNow = dgv.Columns[e.ColumnIndex].HeaderText;
                    OperateColumnNowAddress = dgv.Rows[e.RowIndex].Cells["Address"].Value.ToString();
                    if (OperateColumnNowAddress.IndexOf("MW") > -1)
                    {

                        numInput.Location = RectangleToClient(r).Location;
                        numInput.Size = r.Size;

                        IsNeedToChange = false;
                        numInput.Maximum = 100000;
                        numInput.Minimum = 0;
                        numInput.DecimalPlaces = 0;

                        if (dgv.CurrentCell.Value.ToString() == "False")
                            numInput.Value = 0;
                        else
                            numInput.Value = decimal.Parse(dgv.CurrentCell.Value.ToString());
                        IsNeedToChange = true;

                        numInput.Visible = true;
                    }

                    break;

                case "Switch":

                    OperateColumnNow = dgv.Columns[e.ColumnIndex].HeaderText;
                    OperateColumnNowAddress = dgv.Rows[e.RowIndex].Cells["Address"].Value.ToString();
                    //if (OperateColumnNowAddress.IndexOf("IX") > -1)
                    //    return;
                    OperateColumnNowReadOnly = (bool)dgv.Rows[e.RowIndex].Cells["ReadOnly"].Value;
                    if (OperateColumnNowReadOnly)
                        return;

                    cboOnOff.Location = RectangleToClient(r).Location;
                    cboOnOff.Size = r.Size;

                    IsNeedToChange = false;
                    cboOnOff.SelectedIndex = (int)dgv.CurrentCell.Value;
                    IsNeedToChange = true;

                    cboOnOff.Visible = true;

                    break;
                default:
                    break;
            }

        }

    }
}
