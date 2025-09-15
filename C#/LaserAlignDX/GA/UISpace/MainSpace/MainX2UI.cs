using Eazy_Project_III;
using JetEazy.BasicSpace;
using JzDisplay;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.ControlSpace.MachineSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LaserAlignDX.RunSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

//using System.Reflection.Emit;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.ProcessSpace;
using VisionDesigner;
using VsCommon.ControlSpace;

namespace LaserAlignDX.UISpace.MainSpace
{
    public partial class MainX2UI : UserControl, IMainUI
    {
        List<CollectResultClass> collectResultClasses = new List<CollectResultClass>();

        protected RecipeMainX2Class xRecipe
        {
            get { return RecipeMainX2Class.Instance; }
        }
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Traveller106.Universal.MACHINECollection;
            }
        }
        protected MainX2MachineClass MACHINE
        {
            get { return (MainX2MachineClass)Traveller106.Universal.MACHINECollection.MACHINE; }
        }

        bool m_plcStartOld = false;
        bool m_plcGetImageOld = false;

        Button btnReady;
        Label[] m_MappingItem;
        Button btnChangeReplaceImage;

        Control IMainUI.Window => this;

        public MainX2UI()
        {
            InitializeComponent();
        }


        public void Init()
        {
            //init_Display();
            //update_Display();
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);
            InitAllProcesses();

            init_Display();
            update_Display();

            InitializeDataGridView();

            MappingInit();

            //lblState = label11;

            //btnSoftwareReady = button6;
            //btnSoftwareReady.Click += BtnSoftwareReady_Click;

            btnReady = button6;
            btnReady.Click += BtnReady_Click;
            btnChangeReplaceImage = button1;
            btnChangeReplaceImage.Click += BtnChangeReplaceImage_Click;

            SizeChanged += MainX2UI_SizeChanged;

            //删除矩形菜单项，右键菜单中对应项会被删除
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdAddShape),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdFile),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdZoom),
              System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdRotate),
               System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdEraser),
               System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdShapeMenuPaste),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);

        }

        private void BtnChangeReplaceImage_Click(object sender, EventArgs e)
        {
            int iret = ProcessRunClass.Instance.ChangeModelBackgroudImage();
            string msg = string.Empty;

            switch (iret)
            {
                //case -1:
                //    break;
                case -2:
                    msg = $"基准点定位失败";
                    break;
                case -3:
                    msg = $"模板定位失败";
                    break;
                case -4:
                    msg = $"训练失败";
                    break;
                default:
                    msg = $"切换模板底图成功";
                    break;
            }
            if (iret != 0)
            {
                VsMSG.Instance.Warning(msg, true);
            }
            else
            {
                MappingInit();
            }
            CommonLogClass.Instance.LogMessage($"{msg}", Color.Black);
        }

        private void BtnReady_Click(object sender, EventArgs e)
        {
            MACHINE.PLCIO.Ready = !MACHINE.PLCIO.Ready;
            //if (m_LineScanProcess.IsOn)
            //    m_LineScanProcess.Stop();
        }

        public void MappingInit()
        {
            if (xRecipe.xRow < 1 || xRecipe.xColumn < 1)
                return;
            m_MappingItem = new Label[xRecipe.xRow * xRecipe.xColumn];

            int iWH = 45;

            int iw = panel2.Width - iWH;
            int ih = panel2.Height - iWH;

            int itemw = iWH;// iw / xRecipe.xColumn;
            int itemh = iWH;// ih / xRecipe.xRow;

            int xoffset = (iw - xRecipe.xColumn * itemw) / (xRecipe.xColumn - 1);
            int yoffset = (ih - xRecipe.xRow * itemh) / (xRecipe.xRow - 1);

            this.panel2.Controls.Clear();
            int ix = 0;
            int iy = 0;

            //string colname = "A";
            int colindex = 1;

            int colnameindex = 1;

            int i = 0;
            while (i < m_MappingItem.Length)
            {
                m_MappingItem[i] = new Label();
                m_MappingItem[i].Name = $"lbl_{i}";
                m_MappingItem[i].Text = "";
                //m_MappingItem[i].AccessibleName = colindex.ToString() + "-" + analyzetmp.ReportRowCol.Split('-')[1];

                m_MappingItem[i].BorderStyle = BorderStyle.FixedSingle;
                m_MappingItem[i].BackColor = Color.White;
                m_MappingItem[i].Font = new System.Drawing.Font("黑体", 9F);
                m_MappingItem[i].TextAlign = ContentAlignment.MiddleCenter;
                m_MappingItem[i].Width = itemw;
                m_MappingItem[i].Height = itemh;
                m_MappingItem[i].Location = new Point(itemw / 2 + ix, itemh / 2 + iy);
                //m_MappingItem[i].DoubleClick += RunUI_DoubleClick;
                //m_MappingItem[i].MouseEnter += RunUI_MouseEnter;
                ix += m_MappingItem[i].Width;
                colnameindex++;
                if ((i + 1) % xRecipe.xColumn == 0)
                {
                    //iy += m_MappingItem[i].Height;
                    //ix = 0;

                    iy += m_MappingItem[i].Height + yoffset;
                    ix = 0;

                    colindex++;

                    colnameindex = 1;
                }
                else
                {
                    ix += xoffset;
                }
                this.panel2.Controls.Add(m_MappingItem[i]);

                i++;
            }

            DS.ReplaceDisplayImage(xRecipe.bmpprinttemplate);
            CGOperate();
        }
        private void MappingReset()
        {
            int i = 0;
            while (i < m_MappingItem.Length)
            {
                m_MappingItem[i].BackColor = Color.White;
                i++;
            }
        }
        private void MappingUpdate()
        {
            int i = 0;
            while (i < m_MappingItem.Length)
            {
                RegionCellX2Class cell = xRecipe.xRegionCells[i];
                if (cell.inspectReasons.Count > 0)
                {
                    InspectReason reason = cell.inspectReasons[cell.inspectReasons.Count - 1];
                    m_MappingItem[i].BackColor = categoryColors[(int)reason];
                }
                else
                    m_MappingItem[i].BackColor = categoryColors[0];

                i++;
            }
        }

        //private DataTable statsTable;
        private void InitializeDataGridView()
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;

            dgv.Columns["col1"].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv.Columns["col2"].SortMode = DataGridViewColumnSortMode.NotSortable;

            // 初始化数据
            dgv.Rows.Add("PASS", 0);
            dgv.Rows.Add("印字错误", 0);
            dgv.Rows.Add("印字偏移", 0);
            dgv.Rows.Add("印字缺失", 0);
            dgv.Rows.Add("2D读取错误", 0);
            dgv.Rows.Add("2D比对错误", 0);
            dgv.Rows.Add("2D重复", 0);
            dgv.Rows.Add("不检测", 0);
            dgv.Rows.Add("芯片数", 0);
            dgv.Rows.Add("良率", 0);

            int i = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.DefaultCellStyle.BackColor = categoryColors[i];
                i++;
            }
        }
        // 定义不同错误类别的颜色
        Color[] categoryColors =
        {
            Color.Lime,     // PASS
            Color.Cyan,      // 印字错误
            Color.Violet,     // 印字偏移
            Color.Red, // 印字缺失
            Color.Fuchsia,    // 2D读取错误
            Color.Orange,   // 2D比对错误
            Color.LightPink,       // 2D重复
            Color.Purple,       // 不检测
            Color.Gray,       // 芯片数
            Color.LightBlue,       // 良率
        };
        void _updateDgvData()
        {
            int i = 0;
            while (i < xRecipe.AnalyzeDatas.Length - 1)
            {
                dgv.Rows[i].Cells[1].Value = xRecipe.AnalyzeDatas[i];
                i++;
            }
            dgv.Rows[i].Cells[1].Value = $"{xRecipe.AnalyzeDatas[i].ToString("0.00")} %";
        }

        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_resetprocess
        {
            get { return ResetProcess.Instance; }
        }
        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }
        BaseProcess m_MainProcess
        {
            get { return MainProcess.Instance; }
        }
        BaseProcess m_SingleProcess
        {
            get { return LineScanSingleProcess.Instance; }
        }

        void InitAllProcesses()
        {
            //----------------------------------------------------------------
            // (1) 大部的 Processes 應該可以當成 MainProcess 的 Child Process,
            //      可以集中由 MainProcess 管理, 形成一體 Model.
            // (2) 以下對 Process Event Handler 的掛載.
            //      在 Model-View-Control 的架構規範下, 屬於 Control.
            //      ~ 以後再從 GUI(MainGdx3UI) 抽離出來.
            //----------------------------------------------------------------
            //m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的結束 用來檢視是否有 NG 發生.
            m_MainProcess.OnCompleted += process_OnCompleted;
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnCompleted += process_OnCompleted;
            m_LineScanProcess.OnLiveImage += process_OnLiveImage;
            m_MainProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnMessage += process_OnMessage;
            m_LineScanProcess.OnMessage += process_OnMessage;
            m_SingleProcess.OnLiveImage += process_OnLiveImage;

        }

        private void process_OnMessage(object sender, ProcessEventArgs e)
        {
            if (sender == m_MainProcess)
            {
                if (e.Message.Contains("Reset.Data"))
                {

                }
                else if (e.Message.Contains("Record.Start"))
                {
                    FireChangeState(MainS1State.LS_START);
                }
                else if (e.Message.Contains("Record.Stop"))
                {
                    FireChangeState(MainS1State.LS_STOP);
                }
            }
            else if (sender == m_LineScanProcess || sender == m_SingleProcess)
            {
                if (e.Message.Contains("Result.1"))
                {
                    FireChangeState(MainS1State.M_PASS);
                }
                else if (e.Message.Contains("Result.2"))
                {
                    FireChangeState(MainS1State.M_NG);
                }
                else if (e.Message.Contains("Record.Start"))
                {
                    MappingReset();
                    //FireChangeState(MainS1State.LS_START);
                }
                else if (e.Message.Contains("Record.Stop"))
                {
                    //FireChangeState(MainS1State.LS_STOP);
                }
                else if (e.Message.Contains("Show.X"))
                {
                    //mvdRenderActivex1.LoadImageFromObject(ProcessRunClass.Instance.cMvdInput.Clone());

                    //if (mvdRenderActivex1.BackgroundImage == null)
                    //{
                    //    CommonLogClass.Instance.LogMessage($"Err.图像未加载", Color.Red);
                    //    return;
                    //}

                    //List<RectangleF> listDraw = new List<RectangleF>();
                    //List<CollectResultClass> collectResultClasses = new List<CollectResultClass>();

                    MVD_POINT_F _base0Org = new MVD_POINT_F(xRecipe.template0Center.X, xRecipe.template0Center.Y);
                    MVD_POINT_F _base1Org = new MVD_POINT_F(xRecipe.template1Center.X, xRecipe.template1Center.Y);
                    MVD_POINT_F _base0Run = ProcessRunClass.Instance.mVD_POINT_F0;
                    MVD_POINT_F _base1Run = ProcessRunClass.Instance.mVD_POINT_F1;

                    collectResultClasses.Clear();

                    CMvdLineSegmentF cMvdLineSegmentF = new CMvdLineSegmentF(_base0Org, _base1Org);
                    cMvdLineSegmentF.BorderWidth = 1;
                    if (_base0Run.fX == -9999 || _base1Run.fX == -9999)
                    {
                        cMvdLineSegmentF.BorderColor = new MVD_COLOR(255, 0, 0);
                        AddCross(_base0Org, new MVD_COLOR(255, 0, 0));
                        AddCross(_base1Org, new MVD_COLOR(255, 0, 0));
                    }
                    else
                    {
                        cMvdLineSegmentF = new CMvdLineSegmentF(_base0Run, _base1Run);
                        cMvdLineSegmentF.BorderColor = new MVD_COLOR(0, 255, 0);

                        AddCross(_base0Run, new MVD_COLOR(0, 255, 0));
                        AddCross(_base1Run, new MVD_COLOR(0, 255, 0));
                    }
                    mvdRenderActivex1.AddShape(cMvdLineSegmentF);

                    CMvdTextF cMvdTextF = new CMvdTextF(1800, 500, $"耗时:{ProcessRunClass.Instance.ElapsedTime.ToString("0.00")}ms");
                    cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                    cMvdTextF.FontWidth = 18;
                    mvdRenderActivex1.AddShape(cMvdTextF);


                    //收集所有信息
                    string _collectStrMsg = string.Empty;

                    //所有框的显示
                    foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
                    {
                        _collectStrMsg += $"({cell.ToResultStr()})";

                        //定位框
                        mvdRenderActivex1.AddShape(cell.DrawResultRectF());

                        CollectResultClass collectResult = new CollectResultClass();
                        collectResult.loc = new RectangleF(cell.DrawResultRectF().LeftTopX,
                                cell.DrawResultRectF().LeftTopY,
                                cell.DrawResultRectF().Width,
                                cell.DrawResultRectF().Height);
                        collectResult.ispass = cell.DrawResultRectF().BorderColor.nG == 255;
                        collectResult.desc = string.Empty;
                        collectResultClasses.Add(collectResult);

                        //二维码
                        if (cell.DrawBarcodePosition != null)
                        {
                            mvdRenderActivex1.AddShape(cell.DrawBarcodePosition);
                            CMvdTextF _CodeText
                                = new CMvdTextF(cell.DrawBarcodePosition.GetVertex(2).fX,
                                                              cell.DrawBarcodePosition.GetVertex(2).fY + 120,
                                                              cell.RunCodeInfo.Content);
                            _CodeText.BorderColor = new MVD_COLOR(0, 255, 0);
                            //_CodeText.FontWidth = 11;
                            _CodeText.FillColor = new MVD_COLOR(0, 0, 0);
                            mvdRenderActivex1.AddShape(_CodeText);


                            CollectResultClass collectResult2D = new CollectResultClass();
                            MVD_RECT_F mVD_RECT = cell.DrawBarcodePosition.GetBoundingRect();
                            collectResult2D.loc = new RectangleF(mVD_RECT.fX,
                                    mVD_RECT.fY,
                                    mVD_RECT.fWidth,
                                    mVD_RECT.fHeight);
                            collectResult2D.ispass = true;
                            collectResult2D.desc = cell.RunCodeInfo.Content;
                            collectResultClasses.Add(collectResult2D);

                        }

                        //缺失
                        if (cell.inspectReasons.Count > 0)
                        {
                            //listDraw.Add(new RectangleF(cell.DrawResultRectF().LeftTopX,
                            //    cell.DrawResultRectF().LeftTopY,
                            //    cell.DrawResultRectF().Width,
                            //    cell.DrawResultRectF().Height));
                            foreach (var reason in cell.inspectReasons)
                            {
                                switch (reason)
                                {
                                    case InspectReason.INS_DEFECTERR:
                                        foreach (var myShape in cell.DrawBlobNGList())
                                        {
                                            mvdRenderActivex1.AddShape(myShape);

                                            //listDraw.Add(new RectangleF(myShape.LeftTopX,
                                            //                            myShape.LeftTopY,
                                            //                            myShape.Width,
                                            //                            myShape.Height));


                                            CollectResultClass collectResultDEFECT = new CollectResultClass();
                                            //MVD_RECT_F mVD_RECT = cell.DrawBarcodePosition.GetBoundingRect();
                                            collectResultDEFECT.loc = new RectangleF(myShape.LeftTopX,
                                                                        myShape.LeftTopY,
                                                                        myShape.Width,
                                                                        myShape.Height);
                                            collectResultDEFECT.ispass = false;
                                            collectResultDEFECT.desc = string.Empty;
                                            collectResultClasses.Add(collectResultDEFECT);
                                        }
                                        break;
                                }
                            }
                        }

                    }

                    CommonLogClass.Instance.LogMessage($"批号:{xRecipe.xLotNoStr}#数据信息:{_collectStrMsg}", Color.Black);

                    _updateDgvData();
                    mvdRenderActivex1.Display();
                    MappingUpdate();
                    FireChangeState(MainS1State.M_SHOWRESULT, e.Tag as string);
                    if (ProcessRunClass.Instance.IsPass)
                        FireChangeState(MainS1State.M_PASS);
                    else
                        FireChangeState(MainS1State.M_NG);

                    #region 保存Strip图片资料

                    if (INI.Instance.IsSaveDebugBMP)
                    {
                        MainX6Save();
                    }

                    if (INI.Instance.IsSaveStripImage)
                    {
                        ReportReset();
                        if (!ProcessRunClass.Instance.IsPass)
                        {
                            int reportIndex2 = 0;
                            while (reportIndex2 < m_MappingItem.Length)
                            {
                                //messageStr2 = string.Empty;
                                Label lbl = m_MappingItem[reportIndex2];
                                string STR = lbl.Name + ",";
                                STR += lbl.Location.X + ",";
                                STR += lbl.Location.Y + ",";
                                STR += lbl.Size.Width + ",";
                                STR += lbl.Size.Height + ",";
                                STR += _getColorIndex(lbl.BackColor).ToString() + ",";
                                STR += lbl.Text + ",";
                                CMvdRectangleF cMvd = xRecipe.xRegionCells[reportIndex2].DrawResultRectF();
                                STR += "0" + ",";
                                STR += cMvd.LeftTopX.ToString() + ",";
                                STR += cMvd.LeftTopY.ToString() + ",";
                                STR += cMvd.Width.ToString() + ",";
                                STR += cMvd.Height.ToString() + ",";
                                //STR += lbl.AccessibleName + ",";
                                //messageStr2 = (string)lbl.Tag;
                                //JzMainSDPositionParas.ReportGradeAdd(_getLabelText(lbl.Text) + ";" + messageStr2 + ";");
                                //STR += _getLabelText(lbl.Text) + ";" + messageStr2 + ";" + ",";
                                if (xRecipe.xRegionCells[reportIndex2].RunCodeInfo == null)
                                    STR += lbl.Text + ";" + "" + ";" + ",";
                                else
                                    STR += lbl.Text + ";" + xRecipe.xRegionCells[reportIndex2].RunCodeInfo.Content + ";" + ",";
                                ReportAdd(STR);

                                reportIndex2++;
                            }

                            ReportAUTOSave(xRecipe.NGCount, false, true);
                            MainX6StripImageDataSave();
                        }
                    }

                    #endregion

                }
                else if (e.Message.Contains("ResultX.Code"))
                {
                    INI.Instance.CurrentBarcodeStr = e.Tag as string;
                    FireChangeState(MainS1State.M_SHOWCODE, e.Tag as string);
                }
            }

            try
            {
                // Do whatever message you want to show to the operators.
                string msg = $"Process {((BaseProcess)sender).Name}, {e.Message}\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
            CGOperate();

        }

        void TickAllProcesses()
        {
            m_resetprocess.Tick();
            m_BuzzerProcess.Tick();
            m_LineScanProcess.Tick();
            m_MainProcess.Tick();
            m_SingleProcess.Tick();
        }

        private void process_OnLiveImage(object sender, ProcessEventArgs e)
        {
            if (e.Tag != null && e.Tag is Bitmap)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        EventHandler<ProcessEventArgs> h = process_OnLiveImage;
                        this.Invoke(h, sender, e);
                    }
                    else
                    {
                        //@LETIAN: 2022/07/01 改用 GdxDispUI 增加一些 fps
                        // bmp 由 Sender maintains life cycle.
                        // 在此不用 Dispose
                        //Bitmap bmp = (Bitmap)e.Tag;
                        //dispUI1.UpdateLiveImage(bmp);
                        //DS1.ReplaceDisplayImage(bmp);
                        mvdRenderActivex1.LoadImageFromObject(ProcessRunClass.Instance.cMvdInput.Clone());
                        mvdRenderActivex1.ClearShapes();
                    }
                }
                catch (Exception ex)
                {
                    //>>> 此一層的 try - catch 以後可以省略.
                    //>>> 會由 Event Sender 處理 exception
                    //throw ex;
                }
            }
        }
        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if (m_resetprocess.RelateString == "CloseWindows")
                {
                    //執行的關閉流程 這裏則跳出
                    return;
                }
            }

            try
            {
                string msg = $"Process {((BaseProcess)sender).Name}, Completed!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
                BeginInvoke(h, sender, e);
            }
            else
            {

            }
        }
        private void handle_main_process_completed(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = handle_main_process_completed;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (e.Message == "PartialCompleted")
                {

                }
                else
                {
                }
            }
        }

        public void Tick()
        {
            _getPlcRunTick();
            TickAllProcesses();
        }
        public void ChangeRecipe()
        {
            this.MappingInit();
        }
        public void SetEnable(bool isendable)
        {
        }
        public void SetEnableState(bool isendable)
        {
        }
        private void _getPlcRunTick()
        {

            btnReady.BackColor = (MACHINE.PLCIO.Ready ? Color.Red : Color.FromArgb(192, 255, 192));
            //if (m_LineScanProcess.IsOn)
            //    lblState.Text = ToChangeLanguage("执行-线扫测试中") + m_LineScanProcess.ID.ToString();
            //else
            //    lblState.Text = ToChangeLanguage("等待");

            if (MACHINE.PLCIO.Ready)
            {
                if (MACHINE.PLCIO.IsStart)
                {
                    if (!m_plcStartOld)
                    {
                        m_plcStartOld = true;

                        CommonLogClass.Instance.LogMessage("接收到plc启动信号", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            m_LineScanProcess.Start();
                        }
                        else
                        {
                            CommonLogClass.Instance.LogMessage("测试中#PLC重复启动", Color.Black);
                        }
                    }
                }
                else
                {
                    m_plcStartOld = false;
                }


                if (MACHINE.PLCIO.IsGetImage)
                {
                    if (!m_plcGetImageOld)
                    {
                        m_plcGetImageOld = true;

                        CommonLogClass.Instance.LogMessage("接收到plc抓图信号", Color.Black);
                        if (!m_LineScanProcess.IsOn)
                        {
                            m_LineScanProcess.Start("Snap");
                        }
                        else
                        {
                            CommonLogClass.Instance.LogMessage("测试中#PLC重复抓图", Color.Black);
                        }
                    }
                }
                else
                {
                    m_plcGetImageOld = false;
                }

            }

        }

        string mainx6_path = "D:\\CollectPictures";
        private string m_reportpath = @"D:\report\work";
        private string m_reportmappingpath = @"D:\report\mapping";
        /// <summary>
        /// 储存信息资料 保存label的位置信息(为了查看时画出位置) 及 错误结果 1:OK 2:NG
        /// </summary>
        private List<string> ReportList = new List<string>();

        /// <summary>
        /// 存储等级的信息
        /// </summary>
        private List<string> ReportGradeList = new List<string>();
        private void MainX6Save()
        {
            Task task = new Task(() =>
            {
                try
                {
                    mainx6_path = "D:\\CollectPictures\\" + JzTimes.DateSerialString + "\\" + (ProcessRunClass.Instance.IsPass ? "P-" : "F-") + ProcessRunClass.Instance.FileBarcodeStr;

                    if (!Directory.Exists(mainx6_path + "\\000"))
                        Directory.CreateDirectory(mainx6_path + "\\000");

                    int qi = 0;
                    ProcessRunClass.Instance.cMvdInput.Clone().SaveImage(mainx6_path + "\\000\\P00-" + qi.ToString("000") + ".jpg", MVD_FILE_FORMAT.MVD_FILE_JPEG);


                }
                catch (Exception ex)
                {
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                }
            });
            task.Start();
        }
        private void MainX6StripImageDataSave()
        {
            Task task = new Task(() =>
            {
                try
                {
                    //string _imagePath = "D:\\REPORT\\work\\Image\\auto_" + xRecipe.xLotNoStr + "\\" + xRecipe.NGCount.ToString("00000");
                    string _imagePath = "D:\\REPORT\\work\\Image\\" + JzTimes.DateSerialString + "\\auto_" + xRecipe.xLotNoStr + "\\" + xRecipe.NGCount.ToString("00000");

                    if (!Directory.Exists(_imagePath + "\\000"))
                        Directory.CreateDirectory(_imagePath + "\\000");
                    int qi = 0;
                    ProcessRunClass.Instance.cMvdInput.Clone().SaveJpeg(10, _imagePath + "\\000\\P00-" + qi.ToString("000") + ".jpg");

                    //if (INI.Instance.IsSaveScreen)
                    //{
                    //    int width = Screen.PrimaryScreen.Bounds.Width;
                    //    int height = Screen.PrimaryScreen.Bounds.Height;

                    //    Bitmap m = new Bitmap(width, height);
                    //    using (Graphics g = Graphics.FromImage(m))
                    //    {
                    //        g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
                    //        g.Dispose();
                    //    }

                    //    m.Save(_imagePath + "\\000\\Result" + ".jpg", ImageFormat.Jpeg);
                    //    m.Dispose();
                    //}
                    //复制一份
                    List<CollectResultClass> mylist = new List<CollectResultClass>();
                    mylist.Clear();
                    foreach (CollectResultClass c in collectResultClasses)
                    {
                        mylist.Add(c.Clone());
                    }

                    ////收集所有PASS框
                    //List<CollectResultClass> passlist = new List<CollectResultClass>();
                    //foreach (CollectResultClass c in collectResultClasses)
                    //{
                    //    if (c.ispass && string.IsNullOrEmpty(c.desc))
                    //        passlist.Add(c.Clone());
                    //}

                    ////收集所有FAIL框
                    //List<CollectResultClass> faillist = new List<CollectResultClass>();
                    //foreach (CollectResultClass c in collectResultClasses)
                    //{
                    //    if (!c.ispass && string.IsNullOrEmpty(c.desc))
                    //        faillist.Add(c.Clone());
                    //}

                    ////收集所有barcode
                    //List<CollectResultClass> bar2dlist = new List<CollectResultClass>();
                    //foreach (CollectResultClass c in collectResultClasses)
                    //{
                    //    if (c.ispass && !string.IsNullOrEmpty(c.desc))
                    //        bar2dlist.Add(c.Clone());
                    //}

                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Restart();
                    Bitmap bmpDrawImage = new Bitmap(CMvdImageToBitmap(ProcessRunClass.Instance.cMvdInput.Clone()));
                    using (Graphics g = Graphics.FromImage(bmpDrawImage))
                    {
                        foreach (CollectResultClass c in mylist)
                        {
                            g.DrawRectangle(new Pen((c.ispass ? Color.Lime : Color.Red), 11), Rectangle.Ceiling(c.loc));
                            if (!string.IsNullOrEmpty(c.desc))
                            {
                                g.DrawString(c.desc,
                                    new Font("Arial", 55),
                                    new SolidBrush(Color.Lime),
                                    new PointF(c.loc.Location.X + c.loc.Width + 5, c.loc.Location.Y));
                            }
                        }

                        //stopwatch.Stop();
                        //g.DrawString($"{stopwatch.ElapsedMilliseconds} ms",
                        //            new Font("Arial", 23),
                        //            new SolidBrush(Color.Lime),
                        //            new PointF(30, 30));
                        g.Dispose();
                    }
                    bmpDrawImage.Save(_imagePath + "\\000\\Result" + ".jpg", ImageFormat.Jpeg);
                    bmpDrawImage.Dispose();

                    //this.Invoke(new Action(() =>
                    //{
                    //    mvdRenderActivex1.SaveImage(_imagePath + "\\000\\Result" + ".jpg",
                    //        MVD_FILE_FORMAT.MVD_FILE_BMP,
                    //        10,
                    //        MVD_SAVE_TYPE.MVD_SAVE_RESULT_IMAGE);

                    //    //Bitmap bmpDraw = new Bitmap((int)ProcessRunClass.Instance.cMvdInput.Width,
                    //    //                        (int)ProcessRunClass.Instance.cMvdInput.Height);
                    //    //mvdRenderActivex1.DrawToBitmap(bmpDraw, mvdRenderActivex1.ClientRectangle);
                    //    //bmpDraw.Save(_imagePath + "\\000\\Result" + ".jpg", ImageFormat.Jpeg);
                    //    //bmpDraw.Dispose();
                    //}));

                    //this.Invoke(new Action(() =>
                    //{
                    //    mvdRenderActivex1.SaveImage(_imagePath + "\\000\\Result" + ".jpg",
                    //        MVD_FILE_FORMAT.MVD_FILE_JPEG,
                    //        10,
                    //        MVD_SAVE_TYPE.MVD_SAVE_RESULT_IMAGE);
                    //}));
                }
                catch (Exception ex)
                {
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                }
            });
            task.Start();
        }
        int _getColorIndex(Color eColor)
        {
            int iret = 0;
            if (eColor == Color.Cyan)
            {
                iret = 1;
            }
            else if (eColor == Color.Violet)
            {
                iret = 2;
            }
            else if (eColor == Color.Yellow)
            {
                iret = 3;
            }
            else if (eColor == Color.Red)
            {
                iret = 4;
            }
            else if (eColor == Color.Purple)
            {
                iret = 5;
            }
            else if (eColor == Color.Blue)
            {
                iret = 6;
            }
            else if (eColor == Color.Orange)
            {
                iret = 7;
            }
            else if (eColor == Color.Fuchsia)
            {
                iret = 8;
            }
            else if (eColor == Color.LightPink)
            {
                iret = 9;
            }
            return iret;
        }
        public void ReportReset()
        {
            ReportList.Clear();
            //ReportGradeList.Clear();
        }
        public void ReportAdd(string eStr)
        {
            ReportList.Add(eStr);
        }
        public void ReportGradeAdd(string eStr)
        {
            //ReportGradeList.Add(eStr);
        }
        public void ReportAUTOSave(int eIndex, bool eIspass, bool eUseDataSave = false)
        {
            if (string.IsNullOrEmpty(xRecipe.xLotNoStr))
                xRecipe.xLotNoStr = "none";

            xRecipe.xLotNoStr = xRecipe.xLotNoStr.Replace('-', '_');

            //路径 + auto + 批号 
            string str = m_reportpath + "\\auto\\auto_" + xRecipe.xLotNoStr;
            string strfilename = (eIspass ? "P-" : "F-") + "auto-" + eIndex.ToString("00000") + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (eUseDataSave)
            {
                str = m_reportpath + "\\auto\\" + JzTimes.DateSerialString + "\\auto_" + xRecipe.xLotNoStr;
            }

            //mySqlTableCreate("AUTO_" + Report_LOT);
            //mySqlTableInsert("AUTO_" + Report_LOT, strfilename);
            //mySqlTableInsert("auto_" + Report_LOT);
            ReportSave(str, strfilename);

            str = m_reportmappingpath + "\\auto\\auto_" + xRecipe.xLotNoStr;
            ReportMappingSave(str, strfilename);
        }
        private void ReportSave(string ePath, string eFilename)
        {
            if (ReportList.Count == 0)
                return;

            string Str = "";
            foreach (string str in ReportList)
            {
                Str += str + Environment.NewLine;
            }

            if (!System.IO.Directory.Exists(ePath))
                System.IO.Directory.CreateDirectory(ePath);

            _save(Str, ePath + "\\" + eFilename + ".csv");
        }
        private void ReportMappingSave(string ePath, string eFilename)
        {
            if (ReportList.Count == 0)
                return;

            string Str = "";
            string str_text_tmp = "1";
            foreach (string str in ReportList)
            {
                //Str += str + Environment.NewLine;

                string[] strs = str.Split(',');
                if (strs.Length >= 7)
                {
                    string[] strs_text = strs[6].Split('-');
                    if (str_text_tmp != strs_text[0])
                    {
                        str_text_tmp = strs_text[0];
                        Str += Environment.NewLine;
                    }
                    Str += strs[5] + ",";
                    //Str += (strs[5] == "0" ? "P" : strs[5]) + ",";
                }
            }

            Str += Environment.NewLine;

            Str += $"0-正确-Pass{Environment.NewLine}";
            Str += $"1-印字错误-Printing error{Environment.NewLine}";
            Str += $"2-印字偏移-Printing offset{Environment.NewLine}";
            Str += $"3-油墨错误-Ink error{Environment.NewLine}";
            Str += $"4-印字缺失-Missing printed characters{Environment.NewLine}";
            Str += $"5-不检测-Not to detect{Environment.NewLine}";
            Str += $"6-其他-Other{Environment.NewLine}";
            Str += $"7-2D比对错误-2D comparison error{Environment.NewLine}";
            Str += $"8-2D读取错误-2D reading error{Environment.NewLine}";
            Str += $"9-2D重复-2D repetition{Environment.NewLine}";

            if (!System.IO.Directory.Exists(ePath))
                System.IO.Directory.CreateDirectory(ePath);

            _save(Str, ePath + "\\" + eFilename + ".csv");
        }
        private void _save(string DataStr, string FileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(FileName, false, System.Text.Encoding.Default);
                stm.Write(DataStr);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
            }

            if (stm != null)
                stm.Dispose();
        }
        void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        //public delegate void ChangeStateHandler(MainS1State status, object tag = null);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(MainS1State status, object tag = null)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status, tag);
            }
        }

        #region Add Shpae to display

        void init_Display()
        {
            //DS = dispUI1;
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS.CaptureAction += DS_CaptureAction;
            //m_DispUI.MoverAction += M_DispUI_MoverAction;
            //m_DispUI.AdjustAction += M_DispUI_AdjustAction;

            //DS2.Initial(100, 0.01f);
            //DS2.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS2.CaptureAction += DS_CaptureAction2;
        }
        void update_Display(bool eChangeToDefault = true)
        {
            DS.Refresh();
            if (eChangeToDefault)
                DS.DefaultView();

            //DS2.Refresh();
            //if (eChangeToDefault)
            //    DS2.DefaultView();
        }

        void AddCross(MVD_POINT_F eCrossCenter, MVD_COLOR eColor, float eLen = 50f)
        {
            MVD_POINT_F _w1 = new MVD_POINT_F(eCrossCenter.fX - eLen, eCrossCenter.fY);
            MVD_POINT_F _w2 = new MVD_POINT_F(eCrossCenter.fX + eLen, eCrossCenter.fY);
            CMvdLineSegmentF cMvdLineSegmentFW = new CMvdLineSegmentF(_w1, _w2);
            cMvdLineSegmentFW.BorderColor = eColor;
            MVD_POINT_F _h1 = new MVD_POINT_F(eCrossCenter.fX, eCrossCenter.fY - eLen);
            MVD_POINT_F _h2 = new MVD_POINT_F(eCrossCenter.fX, eCrossCenter.fY + eLen);
            CMvdLineSegmentF cMvdLineSegmentFH = new CMvdLineSegmentF(_h1, _h2);
            cMvdLineSegmentFH.BorderColor = eColor;
            mvdRenderActivex1.AddShape(cMvdLineSegmentFW);
            mvdRenderActivex1.AddShape(cMvdLineSegmentFH);
        }

        private Bitmap CMvdImageToBitmap(CMvdImage eCMvdImage)
        {
            MVD_IMAGE_DATA_INFO _MvdImage = eCMvdImage.GetImageData();
            MVD_DATA_CHANNEL_INFO ch0 = _MvdImage.stDataChannel[0];
            //Bitmap _bmpFromMVD = ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
            return ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
        }
        private Bitmap ByteArrayToBitmap(byte[] byteArray, int width, int height)
        {
            // 创建 Bitmap 对象
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // 设置调色板为灰度
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
            // 锁定 Bitmap 数据
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            // 将字节数组复制到 Bitmap 数据中
            System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bitmapData.Scan0, byteArray.Length);
            // 解锁 Bitmap 数据
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        #endregion

        #region AUTO_LAYOUT
        private void MainX2UI_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                _auto_layout();
            }
            catch
            {
            }
        }
        void _auto_layout()
        {
#if OPT_LETIAN_AUTO_LAYOUT

            var rcc = ClientRectangle;
            int pad = 3;

            int w = tabControl1.Width;
            int h = tabControl1.Height;

            //tabControl2.Width = rcc.Width - pad * 2;
            //tabControl2.Height = rcc.Height - pad * 3 - h;
            //tabControl2.Location = new Point(pad, pad);

            //groupBox1.Location = new Point(pad, tabControl2.Height + pad);
            //groupBox1.Width = rcc.Width - pad * 3 - w;
            //groupBox1.Height = h;

            //tabControl1.Location = new Point(groupBox1.Width + pad, tabControl2.Height + pad);

#endif
        }
        #endregion

    }
}
