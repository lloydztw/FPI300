using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Common.RecipeSpace;
using Eazy_Project_III.FormSpace;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.PropertyGridSpace;
using JzDisplay;
using MoveGraphLibrary;
//using OpenCvSharp;
using Traveller106;
using WorldOfMoveableObjects;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace LaserAlignDX.FormSpace
{
    public partial class frmMSR : Form
    {
        string m_format = "0.000000";
        PropGrid_CaliClass propGrid_CaliClass = new PropGrid_CaliClass();
        Mover myMover = new Mover();
        Bitmap m_bmpOpeate = new Bitmap(1, 1);
        Bitmap m_bmpOpeateTest = new Bitmap(1, 1);
        JzFindObjectClass m_Find = new JzFindObjectClass();
        List<MSRItemClass> MSRItemList = new List<MSRItemClass>();
        public CAoiCalibration MSRCalibration = new CAoiCalibration();
        JetEazy.FormSpace.VsMessageBox vsMessageBox = null;

        bool IsUseMsrFile = false;
        public CAoiCalibration MSRCalibrationUse = new CAoiCalibration();

        List<MSRItemClass> BaseItemList = new List<MSRItemClass>();
        //string m_format = "0.000000";
        int RowIndex = 0;
        int ColIndex = 0;

        bool m_IsAutoRegionOpen = false;

        public frmMSR()
        {
            InitializeComponent();
            this.Load += FrmMSR_Load;
            this.SizeChanged += FrmMSR_SizeChanged;
        }

        private void FrmMSR_SizeChanged(object sender, EventArgs e)
        {
            update_Display();
        }
        private void FrmMSR_Load(object sender, EventArgs e)
        {
            this.Text = "校正窗口";



            JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), new RectangleF(100, 100, 500, 500));
            //new JzRectEAG(Color.FromArgb(0, Color.Blue), myRecipe.rect_start);
            _rect.RelateLevel = 2;
            _rect.RelateNo = 1;
            _rect.RelatePosition = 0;
            myMover.Add(_rect);

            propGrid_CaliClass.FromingStr(INI.Instance.cali_paras);
            pgParas.SelectedObject = propGrid_CaliClass.XProps;
            pgParas.PropertyValueChanged += PgParas_PropertyValueChanged;

            btnLoadImage.Click += BtnLoadImage_Click;
            btnAutoFind.Click += BtnAutoFind_Click;
            btnCalibrate.Click += BtnCalibrate_Click;
            btnCalibrateTest.Click += BtnCalibrateTest_Click;
            btnSaveCalibrateMsr.Click += BtnSaveCalibrateMsr_Click;
            btnCreatePointF.Click += BtnCreatePointF_Click;
            btnLoadPointF.Click += BtnLoadPointF_Click;
            btnCreateXml.Click += BtnCreateXml_Click;
            btnLoadFileCali.Click += BtnLoadFileCali_Click;
            btnLaserBoard.Click += BtnLaserBoard_Click;
            btnTestLaser.Click += BtnTestLaser_Click;
            btnVarFile.Click += BtnVarFile_Click;

            switch(Universal.FACTORYNAME)
            {
                case Eazy_Project_III.FactoryName.DONGGUAN:
                case Eazy_Project_III.FactoryName.DAGUI:
                    btnLaserBoard.Visible = false;
                    btnTestLaser.Visible = false;
                    btnVarFile.Visible = false;
                    btnLoadFileCali.Visible = false;
                    btnRectYz.Visible = false;
                    btnCreatePointF.Visible = false;
                    btnLoadPointF.Visible = false;
                    btnCreateXml.Visible = false;
                    break;
            }

            init_Display();
            update_Display();

            DS.SetMover(myMover);

            LanguageExClass.Instance.EnumControls(this);
        }
        /// <summary>
        /// 验证校正档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVarFile_Click(object sender, EventArgs e)
        {
            CAoiCalibration _MSRCalibrationUse = new CAoiCalibration();
            //这里判断是否使用标定档转换坐标
            string _pathMsr = OpenFilePicker("MSR Files (*.msr)|*.MSR|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(_pathMsr))
                return;

            if (File.Exists(_pathMsr))
            {
                _MSRCalibrationUse.LoadBin(_pathMsr);
                _MSRCalibrationUse.CalculateTransformMatrix();
            }

            List<MSRItemClass> mSRItemClasses = new List<MSRItemClass>();
            _MSRCalibrationUse.GetCalibrationPoints(out PointF[,] views, out PointF[,] worlds);
            for (int i = 0; i < views.GetLength(0); i++)
            {
                for (int j = 0; j < views.GetLength(1); j++)
                {
                    MSRItemClass mSRItem = new MSRItemClass();
                    mSRItem.CenterPointF = views[i, j];
                    mSRItem.RelatePointF = worlds[i, j];

                    _MSRCalibrationUse.TransformViewToWorld(mSRItem.CenterPointF, out mSRItem.RelatePointFViewToWorld);
                    _MSRCalibrationUse.TransformWorldToView(mSRItem.RelatePointF, out mSRItem.RelatePointFWorldToView);

                    mSRItemClasses.Add(mSRItem);
                }
            }

            string reportstr = ",,,CXV,CYV,RXV,RYV,RW,RH,,CXW,CYW" + Environment.NewLine;
            int indexreport = 1;
            foreach (var item in mSRItemClasses)
            {
                reportstr += $"{item.ReportIndex},{item.ToReportString()},{Environment.NewLine}";
                indexreport++;
            }
            if (!System.IO.Directory.Exists("D:\\report"))
                System.IO.Directory.CreateDirectory("D:\\report");
            SaveDataEXD(reportstr, "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv");

            JetEazy.BasicSpace.VsMSG.Instance.Warning(ToChangeLanguage("数据存储于 ") + "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv", false);
        }

        //校正测试直接输出到laser打点
        private void BtnTestLaser_Click(object sender, EventArgs e)
        {
            string _path = OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(_path))
                return;
            IsUseMsrFile = false;
            //这里判断是否使用标定档转换坐标
            string _pathMsr = OpenFilePicker("MSR Files (*.msr)|*.MSR|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_pathMsr))
            {
                if (File.Exists(_pathMsr))
                {
                    MSRCalibrationUse.LoadBin(_pathMsr);
                    MSRCalibrationUse.CalculateTransformMatrix();
                    IsUseMsrFile = true;
                }
            }

            vsMessageBox = new VsMessageBox($"测试校正点位中...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            float updownoffset = propGrid_CaliClass.sort_offset;//方框上下波動範圍

            Bitmap bmptemp = new Bitmap(_path);
            m_bmpOpeateTest.Dispose();
            m_bmpOpeateTest = new Bitmap(bmptemp);
            bmptemp.Dispose();

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            #region MARK点位置
            if (propGrid_CaliClass.use_mark_pointf)
            {
                Bitmap bmpinputx = (Bitmap)m_bmpOpeateTest.Clone(INI.Instance.mark_rect, PixelFormat.Format24bppRgb);
                m_Find.AH_SetThreshold(ref bmpinputx, INI.Instance.mark_thresholdvalue);
                m_Find.AH_FindBlob(bmpinputx, true);

                if (m_Find.FoundList.Count > 0)
                {
                    //Rectangle rectmax = m_Find.rectMaxRect;
                    Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + INI.Instance.mark_rect.X,
                       m_Find.rectMaxRect.Y + INI.Instance.mark_rect.Y,
                       m_Find.rectMaxRect.Width,
                       m_Find.rectMaxRect.Height);

                    Point MarkRunPtCenter = GetRectCenter(rectmax);

                    _markOffset = new PointF(MarkRunPtCenter.X - INI.Instance.mark_org.X, MarkRunPtCenter.Y - INI.Instance.mark_org.Y);
                }
                bmpinputx.Dispose();

                //_markOffset = new PointF(0, 0);
            }
            #endregion

            GraphicalObject grobj = myMover[0].Source;
            Rectangle rectx = (grobj as JzRectEAG).GetRect;
            Bitmap bmpinput = (Bitmap)m_bmpOpeateTest.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int iSized = propGrid_CaliClass.samplingvalue;
            Bitmap bmpinputtemp = new Bitmap(bmpinput, new Size(bmpinput.Width / iSized, bmpinput.Height / iSized));
            m_Find.AH_SetThreshold(ref bmpinputtemp, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinputtemp, true);
            //m_Find.AH_SetThreshold(ref bmpinput, propGrid_CaliClass.threshold_value);
            //m_Find.AH_FindBlob(bmpinput, true);

            List<Rectangle> boxes = new List<Rectangle>();
            List<MSRItemClass> items = new List<MSRItemClass>();
            foreach (FoundClass found in m_Find.FoundList)
            {
                found.Area *= iSized;
                found.rect = ResizeWithLocation3(found.rect, iSized);

                if (found.Area > propGrid_CaliClass.area_min && found.Area < propGrid_CaliClass.area_max
                    && found.rect.Width < propGrid_CaliClass.width_max && found.rect.Width > propGrid_CaliClass.width_min
                    && found.rect.Height < propGrid_CaliClass.height_max && found.rect.Height > propGrid_CaliClass.height_min
                    )
                {
                    Rectangle rx = new Rectangle(found.rect.X + rectx.X, found.rect.Y + rectx.Y, found.rect.Width, found.rect.Height);

                    //if(rx.X < 10000 || rx.X > 18000)
                    {
                        MSRItemClass mSRItemClass = new MSRItemClass();

                        boxes.Add(rx);
                        mSRItemClass.CenterPointF = GetRectCenter(rx);
                        mSRItemClass.Bounds = rx;
                        PointF ptfworld = new PointF(mSRItemClass.CenterPointF.X, mSRItemClass.CenterPointF.Y);

                        PointF ptfViewAddOffset = new PointF(mSRItemClass.CenterPointF.X - _markOffset.X,
                                                                                      mSRItemClass.CenterPointF.Y - _markOffset.Y);

                        //ptfViewAddOffset.X /= 10000f;
                        //ptfViewAddOffset.Y /= 10000f;

                        ptfworld = ToWorld(ptfViewAddOffset);

                        ptfworld.Y = -ptfworld.Y;

                        mSRItemClass.RelatePointF = new PointF(ptfworld.X, ptfworld.Y);
                        mSRItemClass.RelatePointFWorldToView = ToView(ptfworld);


                        INI.Instance.MSRCaliLaserWorldToLaserCmd.TransformViewToWorld(ptfworld, out PointF r1);
                        mSRItemClass.RelatePointF = new PointF(r1.X, r1.Y);
                        //mSRItemClass.RelatePointFWorldToView.X *= 10000f;
                        //mSRItemClass.RelatePointFWorldToView.Y *= 10000f;

                        items.Add(mSRItemClass);
                    }
                }

            }

            #region 排序数据

            List<MSRItemClass> BranchList = items;

            int Highest = 100000;
            int HighestIndex = -1;
            int ReportIndex = 0;
            List<string> CheckList = new List<string>();

            int i = 0;
            int j = 0;
            //Clear All Index To 0 and Check the Highest

            foreach (MSRItemClass keyassign in BranchList)
            {

                keyassign.ReportRowCol = "";
                keyassign.ReportIndex = 0;
                ReportIndex = 1;

            }

            i = 0;
            while (true)
            {
                i = 0;
                Highest = 100000;
                HighestIndex = -1;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (keyassign.CenterPointF.Y < Highest && keyassign.RowTag == 0)
                        {
                            Highest = (int)keyassign.CenterPointF.Y;
                            HighestIndex = i;
                        }
                    }

                    i++;
                }

                if (HighestIndex == -1)
                    break;

                CheckList.Clear();

                //把相同位置的人找出來
                i = 0;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (IsInRange((int)keyassign.CenterPointF.Y, Highest, updownoffset))
                        {
                            CheckList.Add(keyassign.CenterPointF.X.ToString("00000000") + "," + i.ToString());
                        }
                    }
                    i++;
                }


                //if (j > 0 && j % 2 == 0)
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });
                ////CheckList.Sort();
                //else
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });
                ////CheckList.Sort();

                ////从大到小排序
                //CheckList.Sort((item1, item2) =>
                //{ return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });

                //从小到大排序
                CheckList.Sort((item1, item2) =>
                { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });

                i = 1;
                foreach (string Str in CheckList)
                {
                    string[] Strs = Str.Split(',');

                    BranchList[int.Parse(Strs[1])].ReportIndex = ReportIndex;
                    BranchList[int.Parse(Strs[1])].ReportRowCol = (j + 1).ToString() + "-" + i.ToString();
                    //BranchList[int.Parse(Strs[1])].ReportRowCol = CheckList.Count.ToString() + "-" + i.ToString();

                    ReportIndex++;
                    i++;
                }

                j++;
            }

            //从小到大排序
            BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? 1 : -1; });

            //从大到小排序
            //BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? -1 : 1; });


            #endregion

            Bitmap bmpinputdraw = new Bitmap(m_bmpOpeateTest);
            Graphics g = Graphics.FromImage(bmpinputdraw);
            if (boxes.Count > 0)
                g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());

            string reportstr = ",,,CXV,CYV,RXV,RYV,RW,RH,,CXW,CYW" + Environment.NewLine;
            int indexreport = 1;
            foreach (var item in items)
            {
                g.DrawString(item.ToDrawString(), new Font("宋体", 20), Brushes.Lime, item.CenterPointF);
                reportstr += $"{item.ReportIndex},{item.ToReportString()},{Environment.NewLine}";
                indexreport++;
            }
            g.Dispose();

            //bmpinputdraw.Save("D:\\report\\Img_" + JzTimes.DateTimeSerialStringFFF + ".png", System.Drawing.Imaging.ImageFormat.Png);
            DS.ReplaceDisplayImage(bmpinputdraw);
            bmpinput.Dispose();


            #region 保存xml 供镭雕机调用

            if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
            {
                if (!Directory.Exists(INI.Instance.LaserSharePath))
                    Directory.CreateDirectory(INI.Instance.LaserSharePath);
                if (Directory.Exists(INI.Instance.LaserSharePath))
                {
                    XmlDocument xmldoc;
                    XmlNode xmlnode;
                    XmlElement xmlelem;

                    xmldoc = new XmlDocument();
                    XmlDeclaration xmldecl;
                    xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                    xmldoc.AppendChild(xmldecl);

                    //加入一个根元素
                    xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                    xmldoc.AppendChild(xmlelem);

                    XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                    xmlelem.AppendChild(xeMatrix);

                    XmlElement x2 = xmldoc.CreateElement("Center");
                    x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                    x2.SetAttribute("y", (0).ToString());
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Angle");
                    x2.InnerText = "0";
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Row");
                    x2.InnerText = propGrid_CaliClass.dir_y_count.ToString();
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Col");
                    x2.InnerText = propGrid_CaliClass.dir_x_count.ToString();
                    xeMatrix.AppendChild(x2);

                    XmlNode xeCells = xmldoc.CreateElement("Cells");
                    xmlelem.AppendChild(xeCells);

                    i = 0;
                    foreach (var itemClass in items)
                    {
                        float fx = itemClass.RelatePointF.X;// itemClass.xPCenterOffsetWorld.X;// + (int)numericUpDown1.Value;
                        float fy = itemClass.RelatePointF.Y;// itemClass.xPCenterOffsetWorld.Y;// + (int)numericUpDown2.Value;

                        //PointF PTTEMP = new PointF(itemClass.X, itemClass.Y);
                        //INI.Instance.MSRCalibration1.TransformViewToWorld(itemClass, out PTTEMP);

                        //fx = PTTEMP.X;
                        //fy = PTTEMP.Y;

                        x2 = xmldoc.CreateElement("cell" + i.ToString());
                        xeCells.AppendChild(x2);

                        XmlElement x3 = xmldoc.CreateElement("Center");
                        x3.SetAttribute("x", fx.ToString(m_format));
                        x3.SetAttribute("y", fy.ToString(m_format));
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("dbAngle");
                        x3.InnerText = 0.ToString(m_format);
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("nType");
                        x3.InnerText = 1.ToString();
                        x2.AppendChild(x3);

                        i++;
                    }

                    xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                }
            }



            #endregion

            vsMessageBox.Close();
            vsMessageBox.Dispose();
            if (!System.IO.Directory.Exists("D:\\report"))
                System.IO.Directory.CreateDirectory("D:\\report");

            //SaveDataEXD(reportstr, "D:\\report\\MsrData_" + "All" + ".csv");
            SaveDataEXD(reportstr, "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv");
            JetEazy.BasicSpace.VsMSG.Instance.Warning(ToChangeLanguage("数据存储于 ") + "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv", false);

        }

        //校正laser打点的板
        private void BtnLaserBoard_Click(object sender, EventArgs e)
        {
            if (!_assginposition(false))
                JetEazy.BasicSpace.VsMSG.Instance.Warning("请确认相关资料是否正确！");
            else
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(IsUseMsrFile ? ToChangeLanguage("使用标定档") : ToChangeLanguage("未使用标定档"))} {ToChangeLanguage("校正完成！")}", false);

        }

        private void BtnLoadFileCali_Click(object sender, EventArgs e)
        {
            string _path = OpenFilePicker("CSV Files (*.csv)|*.CSV|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(_path))
                return;

            BaseItemList.Clear();
            StreamReader sr = new StreamReader(_path);
            try
            {
                int i = 0;
                int j = 0;

                string line = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    string[] strings = line.Split(',');

                    MSRItemClass mSRItemClass = new MSRItemClass();

                    mSRItemClass.CenterPointF = new PointF(float.Parse(strings[3]), float.Parse(strings[4]));
                    mSRItemClass.RelatePointF = new PointF(float.Parse(strings[10]), float.Parse(strings[11]));
                    mSRItemClass.ReportIndex = i;
                    mSRItemClass.ReportRowCol = strings[1];
                    BaseItemList.Add(mSRItemClass);
                    i++;

                }

                int XDirCount = propGrid_CaliClass.dir_x_count;
                int YDirCount = propGrid_CaliClass.dir_y_count;

                if (XDirCount * YDirCount != BaseItemList.Count)
                {
                    JetEazy.BasicSpace.VsMSG.Instance.Warning("行列数目不正确！" + Environment.NewLine);
                    return;
                }

                PointF[,] ScreenArray = new PointF[YDirCount, XDirCount];
                PointF[,] RealArray = new PointF[YDirCount, XDirCount];

                i = 0;
                j = 0;

                foreach (var msritem in BaseItemList)
                {
                    string[] strs = msritem.ReportRowCol.Replace("No_", "").Split('-');
                    i = int.Parse(strs[0]) - 1;
                    j = int.Parse(strs[1]) - 1;
                    ScreenArray[i, j] = msritem.CenterPointF;
                    RealArray[i, j] = msritem.RelatePointF;
                }

                CAoiCalibration LtCalibration = MSRCalibration;

                LtCalibration.Dispose();
                LtCalibration.SetCalibrationPoints(ScreenArray, RealArray);
                LtCalibration.CalculateTransformMatrix();

                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("读档校正完成。")}{Environment.NewLine}{ToChangeLanguage("数据个数")}{BaseItemList.Count}", false);

            }
            catch (Exception ex)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning(ToChangeLanguage("请确认相关资料是否正确！") + Environment.NewLine + ex.Message);
            }
            finally
            {
                sr.Close();
                sr.Dispose();
                sr = null;
            }
        }

        private void BtnCreateXml_Click(object sender, EventArgs e)
        {
            string path = INI.Instance.LaserSharePath;
            if (string.IsNullOrEmpty(path))
            {
                if (!Directory.Exists(path))
                {
                    //richTextBox1.BackColor = Color.Red;
                    //richTextBox1.Text = $"路径 {label3.Text} 不存在";
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("路径")} {path} {ToChangeLanguage("不存在")}", true);
                    return;
                }
            }
            if (BaseItemList.Count <= 0)
            {
                //richTextBox1.BackColor = Color.Red;
                //richTextBox1.Text = $"未导入原始数据";
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"未导入原始数据", true);
                return;
            }

            XmlDocument xmldoc;
            XmlNode xmlnode;
            XmlElement xmlelem;

            xmldoc = new XmlDocument();
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
            xmldoc.AppendChild(xmlelem);

            XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
            xmlelem.AppendChild(xeMatrix);

            XmlElement x2 = xmldoc.CreateElement("Center");
            x2.SetAttribute("x", ((int)propGrid_CaliClass.centerx).ToString());
            x2.SetAttribute("y", ((int)propGrid_CaliClass.centery).ToString());
            xeMatrix.AppendChild(x2);
            x2 = xmldoc.CreateElement("Angle");
            x2.InnerText = "0";
            xeMatrix.AppendChild(x2);
            x2 = xmldoc.CreateElement("Row");
            x2.InnerText = RowIndex.ToString();
            xeMatrix.AppendChild(x2);
            x2 = xmldoc.CreateElement("Col");
            x2.InnerText = ColIndex.ToString();
            xeMatrix.AppendChild(x2);

            XmlNode xeCells = xmldoc.CreateElement("Cells");
            xmlelem.AppendChild(xeCells);

            int i = 0;
            foreach (MSRItemClass itemClass in BaseItemList)
            {
                float fx = itemClass.CenterPointF.X + propGrid_CaliClass.offsetx;
                float fy = itemClass.CenterPointF.Y + propGrid_CaliClass.offsety;

                x2 = xmldoc.CreateElement("cell" + i.ToString());
                xeCells.AppendChild(x2);

                XmlElement x3 = xmldoc.CreateElement("Center");
                x3.SetAttribute("x", fx.ToString(m_format));
                x3.SetAttribute("y", fy.ToString(m_format));
                x2.AppendChild(x3);
                x3 = xmldoc.CreateElement("dbAngle");
                x3.InnerText = itemClass.Angle.ToString(m_format);
                x2.AppendChild(x3);
                x3 = xmldoc.CreateElement("nType");
                x3.InnerText = itemClass.nType.ToString();
                x2.AppendChild(x3);
                i++;
            }

            xmldoc.Save(path + "\\linescanData.xml");
            //richTextBox1.Text = $"保存 {label3.Text}\\linescanData.xml 完成";
            //richTextBox1.BackColor = Color.Lime;
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"保存Save {path}\\linescanData.xml 完成Completed", false);
        }
        private void BtnLoadPointF_Click(object sender, EventArgs e)
        {
            string _path = OpenFilePicker("CSV Files (*.csv)|*.CSV|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(_path))
                return;

            BaseItemList.Clear();
            StreamReader sr = new StreamReader(_path);
            int i = 0;
            RowIndex = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                string[] strings = line.Split(',');
                ColIndex = 0;
                foreach (string str in strings)
                {
                    if (string.IsNullOrEmpty(str))
                        continue;
                    MSRItemClass mSRItemClass = new MSRItemClass();
                    string[] strings1 = str.Split(';');
                    mSRItemClass.CenterPointF = new PointF(float.Parse(strings1[0]), float.Parse(strings1[1]));
                    mSRItemClass.ReportIndex = i;
                    mSRItemClass.Angle = 0;
                    mSRItemClass.nType = 1;

                    BaseItemList.Add(mSRItemClass);
                    i++;

                    ColIndex++;
                }

                RowIndex++;
            }
            sr.Close();
            sr.Dispose();
            //MessageBox.Show($"读档完成。数据个数{BaseItemList.Count}");
            //richTextBox1.Text = $"读档完成。数据个数{BaseItemList.Count} Row={RowIndex},Col={ColIndex}";
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"读档完成。Read File Completed {Environment.NewLine} {BaseItemList.Count} Row={RowIndex},Col={ColIndex}", false);
        }
        private void BtnCreatePointF_Click(object sender, EventArgs e)
        {
            //中心为原点0,0 

            PointF ptfCenter = new PointF(0, 0);
            int irow = propGrid_CaliClass.dir_y_count;// (int)numericUpDown1.Value;
            int icol = propGrid_CaliClass.dir_x_count; //(int)numericUpDown2.Value;

            float irowoffset = Math.Abs(propGrid_CaliClass.gap_y);// (float)numericUpDown4.Value;
            float icoloffset = Math.Abs(propGrid_CaliClass.gap_x);// (float)numericUpDown3.Value;

            PointF ptlefttop = new PointF(ptfCenter.X - icoloffset * (icol / 2), ptfCenter.Y + irowoffset * (irow / 2));

            string str = string.Empty;
            for (int i = 0; i < irow; i++)
            {
                for (int j = 0; j < icol; j++)
                {
                    PointF pttemp = new PointF(ptlefttop.X + j * icoloffset, ptlefttop.Y - i * irowoffset);
                    str += $"{pttemp.X.ToString()};{pttemp.Y.ToString()},";
                }
                str += Environment.NewLine;

            }
            string path = $"D:\\{DateTime.Now.ToString("yyyyMMddHHmmss")}_{irow}X{icol}.csv";
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(path);
            streamWriter.Write(str);
            streamWriter.Close();
            streamWriter.Dispose();

            //this.Text = $"生成{path} 成功";
            JetEazy.BasicSpace.VsMSG.Instance.Warning($"{path} OK", false);
        }
        private void PgParas_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            INI.Instance.cali_paras = propGrid_CaliClass.ToParaString();
            INI.Instance.SaveCaliParas();
        }
        //保存校正档
        private void BtnSaveCalibrateMsr_Click(object sender, EventArgs e)
        {
            string _path = SaveFilePicker("校正档案 (*.msr)|*.MSR|" + "All files (*.*)|*.*", JzTimes.DateTimeSerialStringFFF + ".msr");
            if (!string.IsNullOrEmpty(_path))
            {
                MSRCalibration.SaveBin(_path);
                //MSRCalibration.SaveIni(_path.Replace(".msr", ".ini"));
                JetEazy.BasicSpace.VsMSG.Instance.Warning("保存成功！", false);
            }
        }
        //校正测试
        private void BtnCalibrateTest_Click(object sender, EventArgs e)
        {
            string _path = OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(_path))
                return;
            IsUseMsrFile = false;
            //这里判断是否使用标定档转换坐标
            string _pathMsr = OpenFilePicker("MSR Files (*.msr)|*.MSR|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_pathMsr))
            {
                if (File.Exists(_pathMsr))
                {
                    MSRCalibrationUse.LoadBin(_pathMsr);
                    MSRCalibrationUse.CalculateTransformMatrix();
                    IsUseMsrFile = true;
                }
            }

            vsMessageBox = new VsMessageBox($"测试校正点位中...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            float updownoffset = propGrid_CaliClass.sort_offset;//方框上下波動範圍

            Bitmap bmptemp = new Bitmap(_path);
            m_bmpOpeateTest.Dispose();
            m_bmpOpeateTest = new Bitmap(bmptemp);
            bmptemp.Dispose();

            int irow = 0;
            int icol = 0;

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            #region MARK点位置
            if (propGrid_CaliClass.use_mark_pointf)
            {
                Bitmap bmpinputx = (Bitmap)m_bmpOpeateTest.Clone(INI.Instance.mark_rect, PixelFormat.Format24bppRgb);
                m_Find.AH_SetThreshold(ref bmpinputx, INI.Instance.mark_thresholdvalue);
                m_Find.AH_FindBlob(bmpinputx, true);

                if (m_Find.FoundList.Count > 0)
                {
                    //Rectangle rectmax = m_Find.rectMaxRect;
                    Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + INI.Instance.mark_rect.X,
                       m_Find.rectMaxRect.Y + INI.Instance.mark_rect.Y,
                       m_Find.rectMaxRect.Width,
                       m_Find.rectMaxRect.Height);

                    Point MarkRunPtCenter = GetRectCenter(rectmax);

                    _markOffset = new PointF(MarkRunPtCenter.X - INI.Instance.mark_org.X, MarkRunPtCenter.Y - INI.Instance.mark_org.Y);
                }
                bmpinputx.Dispose();

                //_markOffset = new PointF(0, 0);
            }
            #endregion

            GraphicalObject grobj = myMover[0].Source;
            Rectangle rectx = (grobj as JzRectEAG).GetRect;
            Bitmap bmpinput = (Bitmap)m_bmpOpeateTest.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            propGrid_CaliClass.autoFindRegion = new RectangleF(rectx.X, rectx.Y, rectx.Width, rectx.Height);

            INI.Instance.cali_paras = propGrid_CaliClass.ToParaString();
            INI.Instance.SaveCaliParas();

            int iSized = propGrid_CaliClass.samplingvalue;
            Bitmap bmpinputtemp = new Bitmap(bmpinput, new Size(bmpinput.Width / iSized, bmpinput.Height / iSized));
            m_Find.AH_SetThreshold(ref bmpinputtemp, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinputtemp, true);
            //m_Find.AH_SetThreshold(ref bmpinput, propGrid_CaliClass.threshold_value);
            //m_Find.AH_FindBlob(bmpinput, true);

            List<Rectangle> boxes = new List<Rectangle>();
            List<MSRItemClass> items = new List<MSRItemClass>();
            foreach (FoundClass found in m_Find.FoundList)
            {
                found.Area *= iSized;
                found.rect = ResizeWithLocation3(found.rect, iSized);

                if (found.Area > propGrid_CaliClass.area_min && found.Area < propGrid_CaliClass.area_max
                    && found.rect.Width < propGrid_CaliClass.width_max && found.rect.Width > propGrid_CaliClass.width_min
                    && found.rect.Height < propGrid_CaliClass.height_max && found.rect.Height > propGrid_CaliClass.height_min
                    )
                {
                    Rectangle rx = new Rectangle(found.rect.X + rectx.X, found.rect.Y + rectx.Y, found.rect.Width, found.rect.Height);

                    //if(rx.X < 10000 || rx.X > 18000)
                    {
                        MSRItemClass mSRItemClass = new MSRItemClass();

                        boxes.Add(rx);
                        mSRItemClass.CenterPointF = GetRectCenter(rx);
                        mSRItemClass.Bounds = rx;
                        PointF ptfworld = new PointF(mSRItemClass.CenterPointF.X, mSRItemClass.CenterPointF.Y);

                        PointF ptfViewAddOffset = new PointF(mSRItemClass.CenterPointF.X - _markOffset.X,
                                                                                      mSRItemClass.CenterPointF.Y - _markOffset.Y);

                        //ptfViewAddOffset.X /= 10000f;
                        //ptfViewAddOffset.Y /= 10000f;

                        ptfworld = ToWorld(ptfViewAddOffset);

                        ptfworld.Y = -ptfworld.Y;

                        mSRItemClass.RelatePointF = new PointF(ptfworld.X, ptfworld.Y);
                        mSRItemClass.RelatePointFWorldToView = ToView(ptfworld);


                        //INI.Instance.MSRCaliLaserWorldToLaserCmd.TransformViewToWorld(ptfworld, out PointF r1);
                        //mSRItemClass.RelatePointF = new PointF(r1.X, r1.Y);
                        //mSRItemClass.RelatePointFWorldToView.X *= 10000f;
                        //mSRItemClass.RelatePointFWorldToView.Y *= 10000f;

                        items.Add(mSRItemClass);
                    }
                }

            }

            #region 排序数据

            List<MSRItemClass> BranchList = items;

            int Highest = 100000;
            int HighestIndex = -1;
            int ReportIndex = 0;
            List<string> CheckList = new List<string>();

            int i = 0;
            int j = 0;
            //Clear All Index To 0 and Check the Highest

            foreach (MSRItemClass keyassign in BranchList)
            {

                keyassign.ReportRowCol = "";
                keyassign.ReportIndex = 0;
                ReportIndex = 1;

            }

            i = 0;
            while (true)
            {
                i = 0;
                Highest = 100000;
                HighestIndex = -1;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (keyassign.CenterPointF.Y < Highest && keyassign.RowTag == 0)
                        {
                            Highest = (int)keyassign.CenterPointF.Y;
                            HighestIndex = i;
                        }
                    }

                    i++;
                }

                if (HighestIndex == -1)
                    break;

                CheckList.Clear();

                //把相同位置的人找出來
                i = 0;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (IsInRange((int)keyassign.CenterPointF.Y, Highest, updownoffset))
                        {
                            CheckList.Add(keyassign.CenterPointF.X.ToString("00000000") + "," + i.ToString());
                        }
                    }
                    i++;
                }


                //if (j > 0 && j % 2 == 0)
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });
                ////CheckList.Sort();
                //else
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });
                ////CheckList.Sort();

                ////从大到小排序
                //CheckList.Sort((item1, item2) =>
                //{ return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });

                //从小到大排序
                CheckList.Sort((item1, item2) =>
                { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });

                i = 1;
                foreach (string Str in CheckList)
                {
                    string[] Strs = Str.Split(',');

                    BranchList[int.Parse(Strs[1])].ReportIndex = ReportIndex;
                    BranchList[int.Parse(Strs[1])].ReportRowCol = (j + 1).ToString() + "-" + i.ToString();
                    //BranchList[int.Parse(Strs[1])].ReportRowCol = CheckList.Count.ToString() + "-" + i.ToString();

                    ReportIndex++;
                    i++;
                }

                j++;

                irow = j;
                icol = i - 1;
            }

            //从小到大排序
            BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? 1 : -1; });

            //从大到小排序
            //BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? -1 : 1; });


            #endregion

            Bitmap bmpinputdraw = new Bitmap(m_bmpOpeateTest);
            Graphics g = Graphics.FromImage(bmpinputdraw);
            if (boxes.Count > 0)
                g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());

            string reportstr = ",,,CXV,CYV,RXV,RYV,RW,RH,,CXW,CYW" + Environment.NewLine;
            int indexreport = 1;
            foreach (var item in items)
            {
                g.DrawString(item.ToDrawString(), new Font("宋体", 20), Brushes.Lime, item.CenterPointF);
                reportstr += $"{item.ReportIndex},{item.ToReportString()},{Environment.NewLine}";
                indexreport++;
            }
            g.Dispose();

            //bmpinputdraw.Save("D:\\report\\Img_" + JzTimes.DateTimeSerialStringFFF + ".png", System.Drawing.Imaging.ImageFormat.Png);
            DS.ReplaceDisplayImage(bmpinputdraw);
            bmpinput.Dispose();


            #region 保存xml 供镭雕机调用

            if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
            {
                if (!Directory.Exists(INI.Instance.LaserSharePath))
                    Directory.CreateDirectory(INI.Instance.LaserSharePath);
                if (Directory.Exists(INI.Instance.LaserSharePath))
                {
                    XmlDocument xmldoc;
                    XmlNode xmlnode;
                    XmlElement xmlelem;

                    xmldoc = new XmlDocument();
                    XmlDeclaration xmldecl;
                    xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                    xmldoc.AppendChild(xmldecl);

                    //加入一个根元素
                    xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                    xmldoc.AppendChild(xmlelem);

                    XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                    xmlelem.AppendChild(xeMatrix);

                    XmlElement x2 = xmldoc.CreateElement("Center");
                    x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                    x2.SetAttribute("y", (0).ToString());
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Angle");
                    x2.InnerText = "0";
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Row");
                    x2.InnerText = irow.ToString();
                    //x2.InnerText = propGrid_CaliClass.dir_y_count.ToString();
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Col");
                    x2.InnerText = icol.ToString();
                    //x2.InnerText = propGrid_CaliClass.dir_x_count.ToString();
                    xeMatrix.AppendChild(x2);

                    XmlNode xeCells = xmldoc.CreateElement("Cells");
                    xmlelem.AppendChild(xeCells);

                    i = 0;
                    foreach (var itemClass in items)
                    {
                        float fx = itemClass.RelatePointF.X;// itemClass.xPCenterOffsetWorld.X;// + (int)numericUpDown1.Value;
                        float fy = itemClass.RelatePointF.Y;// itemClass.xPCenterOffsetWorld.Y;// + (int)numericUpDown2.Value;

                        //PointF PTTEMP = new PointF(itemClass.X, itemClass.Y);
                        //INI.Instance.MSRCalibration1.TransformViewToWorld(itemClass, out PTTEMP);

                        //fx = PTTEMP.X;
                        //fy = PTTEMP.Y;

                        x2 = xmldoc.CreateElement("cell" + i.ToString());
                        xeCells.AppendChild(x2);

                        XmlElement x3 = xmldoc.CreateElement("Center");
                        x3.SetAttribute("x", fx.ToString(m_format));
                        x3.SetAttribute("y", fy.ToString(m_format));
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("dbAngle");
                        x3.InnerText = 0.ToString(m_format);
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("nType");
                        x3.InnerText = 1.ToString();
                        x2.AppendChild(x3);

                        i++;
                    }

                    xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                }
            }



            #endregion

            vsMessageBox.Close();
            vsMessageBox.Dispose();
            if (!System.IO.Directory.Exists("D:\\report"))
                System.IO.Directory.CreateDirectory("D:\\report");

            //SaveDataEXD(reportstr, "D:\\report\\MsrData_" + "All" + ".csv");
            SaveDataEXD(reportstr, "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv");
            JetEazy.BasicSpace.VsMSG.Instance.Warning(ToChangeLanguage("数据存储于 ") + "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv", false);
        }
        //校正标准board板
        private void BtnCalibrate_Click(object sender, EventArgs e)
        {
            //IsUseMsrFile = false;
            ////这里判断是否使用标定档转换坐标
            //string _pathMsr = OpenFilePicker("MSR Files (*.msr)|*.MSR|" + "All files (*.*)|*.*", "");
            //if (!string.IsNullOrEmpty(_pathMsr))
            //{
            //    if (File.Exists(_pathMsr))
            //    {
            //        MSRCalibrationUse.LoadBin(_pathMsr);
            //        MSRCalibrationUse.CalculateTransformMatrix();
            //        IsUseMsrFile = true;
            //    }
            //}

            if (!_assginposition())
                JetEazy.BasicSpace.VsMSG.Instance.Warning("请确认相关资料是否正确！");
            else
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{(IsUseMsrFile ? ToChangeLanguage("使用标定档") : ToChangeLanguage("未使用标定档"))} {ToChangeLanguage("校正完成！")}", false);
        }
        private void btnMarkCali_Click(object sender, EventArgs e)
        {
            vsMessageBox = new VsMessageBox($"计算固定点中...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            GraphicalObject grobj = myMover[0].Source;
            Rectangle rectx = (grobj as JzRectEAG).GetRect;
            Bitmap bmpinput = (Bitmap)m_bmpOpeate.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            m_Find.AH_SetThreshold(ref bmpinput, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinput, true);

            if (m_Find.FoundList.Count > 0)
            {
                Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + rectx.X,
                    m_Find.rectMaxRect.Y + rectx.Y,
                    m_Find.rectMaxRect.Width,
                    m_Find.rectMaxRect.Height);

                //List<Rectangle> boxes = new List<Rectangle>();
                //foreach (FoundClass found in m_Find.FoundList)
                //{
                //    if (found.Area > propGrid_CaliClass.area_min && found.Area < propGrid_CaliClass.area_max
                //        && found.rect.Width < propGrid_CaliClass.width_max && found.rect.Width > propGrid_CaliClass.width_min
                //        && found.rect.Height < propGrid_CaliClass.height_max && found.rect.Height > propGrid_CaliClass.height_min
                //        )
                //    {
                //        MSRItemClass mSRItemClass = new MSRItemClass();
                //        Rectangle rx = new Rectangle(found.rect.X + rectx.X, found.rect.Y + rectx.Y, found.rect.Width, found.rect.Height);
                //        boxes.Add(rx);
                //        mSRItemClass.CenterPointF = GetRectCenter(rx);
                //    }

                //}

                Bitmap bmpinputdraw = new Bitmap(m_bmpOpeate);
                Graphics g = Graphics.FromImage(bmpinputdraw);
                //if (boxes.Count > 0)
                {
                    //g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());
                    g.DrawRectangle(new Pen(Color.Red, 11), rectmax);

                    INI.Instance.mark_thresholdvalue = propGrid_CaliClass.threshold_value;
                    INI.Instance.mark_rect = new Rectangle(rectx.X, rectx.Y, rectx.Width, rectx.Height);
                    INI.Instance.mark_org = GetRectCenter(rectmax);
                    INI.Instance.Save();
                }


                g.Dispose();
                DS.ReplaceDisplayImage(bmpinputdraw);

                bmpinput.Dispose();
                bmpinputdraw.Dispose();
            }

            vsMessageBox.Close();
            vsMessageBox.Dispose();
        }
        private void BtnAutoFind_Click(object sender, EventArgs e)
        {
            vsMessageBox = new VsMessageBox($"自动寻找点位中...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            float updownoffset = propGrid_CaliClass.sort_offset;//方框上下波動範圍

            GraphicalObject grobj = myMover[0].Source;
            Rectangle rectx = (grobj as JzRectEAG).GetRect;
            Bitmap bmpinput = (Bitmap)m_bmpOpeate.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int iSized = propGrid_CaliClass.samplingvalue;
            Bitmap bmpinputtemp = new Bitmap(bmpinput, new Size(bmpinput.Width / iSized, bmpinput.Height / iSized));
            m_Find.AH_SetThreshold(ref bmpinputtemp, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinputtemp, true);

            List<Rectangle> boxes = new List<Rectangle>();
            List<RectangleF> boxesCenter = new List<RectangleF>();
            MSRItemList.Clear();
            foreach (FoundClass found in m_Find.FoundList)
            {
                found.Area *= iSized;
                found.rect = ResizeWithLocation3(found.rect, iSized);

                if (found.Area > propGrid_CaliClass.area_min && found.Area < propGrid_CaliClass.area_max
                    && found.rect.Width < propGrid_CaliClass.width_max && found.rect.Width > propGrid_CaliClass.width_min
                    && found.rect.Height < propGrid_CaliClass.height_max && found.rect.Height > propGrid_CaliClass.height_min
                    )
                {
                    MSRItemClass mSRItemClass = new MSRItemClass();
                    Rectangle rx = new Rectangle(found.rect.X + rectx.X, found.rect.Y + rectx.Y, found.rect.Width, found.rect.Height);
                    boxes.Add(rx);
                    mSRItemClass.CenterPointF = GetRectCenter(rx);
                    boxesCenter.Add(SimpleRectF(mSRItemClass.CenterPointF, 2, 2));
                    MSRItemList.Add(mSRItemClass);
                }

            }

            #region 排序数据

            List<MSRItemClass> BranchList = MSRItemList;

            int Highest = 100000;
            int HighestIndex = -1;
            int ReportIndex = 0;
            List<string> CheckList = new List<string>();

            int i = 0;
            int j = 0;
            //Clear All Index To 0 and Check the Highest

            foreach (MSRItemClass keyassign in BranchList)
            {

                keyassign.ReportRowCol = "";
                keyassign.ReportIndex = 0;
                ReportIndex = 1;

            }

            i = 0;
            while (true)
            {
                i = 0;
                Highest = 100000;
                HighestIndex = -1;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (keyassign.CenterPointF.Y < Highest && keyassign.RowTag == 0)
                        {
                            Highest = (int)keyassign.CenterPointF.Y;
                            HighestIndex = i;
                        }
                    }

                    i++;
                }

                if (HighestIndex == -1)
                    break;

                CheckList.Clear();

                //把相同位置的人找出來
                i = 0;
                foreach (MSRItemClass keyassign in BranchList)
                {
                    if (keyassign.ReportIndex == 0)
                    {
                        if (IsInRange((int)keyassign.CenterPointF.Y, Highest, updownoffset))
                        {
                            CheckList.Add(keyassign.CenterPointF.X.ToString("00000000") + "," + i.ToString());
                        }
                    }
                    i++;
                }


                //if (j > 0 && j % 2 == 0)
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });
                ////CheckList.Sort();
                //else
                //    CheckList.Sort((item1, item2) =>
                //    { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });
                ////CheckList.Sort();

                ////从大到小排序
                //CheckList.Sort((item1, item2) =>
                //{ return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? -1 : 1; });

                //从小到大排序
                CheckList.Sort((item1, item2) =>
                { return int.Parse(item1.Split(',')[0]) >= int.Parse(item2.Split(',')[0]) ? 1 : -1; });

                i = 1;
                foreach (string Str in CheckList)
                {
                    string[] Strs = Str.Split(',');

                    BranchList[int.Parse(Strs[1])].ReportIndex = ReportIndex;
                    BranchList[int.Parse(Strs[1])].ReportRowCol = (j + 1).ToString() + "-" + i.ToString();
                    //BranchList[int.Parse(Strs[1])].ReportRowCol = CheckList.Count.ToString() + "-" + i.ToString();

                    ReportIndex++;
                    i++;
                }

                j++;
            }

            //从小到大排序
            BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? 1 : -1; });

            //从大到小排序
            //BranchList.Sort((item1, item2) => { return item1.ReportIndex >= item2.ReportIndex ? -1 : 1; });


            #endregion

            Bitmap bmpinputdraw = new Bitmap(m_bmpOpeate);
            Graphics g = Graphics.FromImage(bmpinputdraw);
            if (boxes.Count > 0)
                g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());
            if (boxesCenter.Count > 0)
                g.DrawRectangles(new Pen(Color.Lime, 2), boxesCenter.ToArray());
            foreach (var item in BranchList)
            {
                g.DrawString(item.ToDrawString(), new Font("宋体", 20), Brushes.Lime, item.CenterPointF);
            }
            g.Dispose();
            DS.ReplaceDisplayImage(bmpinputdraw);

            bmpinput.Dispose();
            bmpinputdraw.Dispose();
            bmpinputtemp.Dispose();

            vsMessageBox.Close();
            vsMessageBox.Dispose();
        }
        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            string _path = OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(_path))
            {
                Bitmap bmptemp = new Bitmap(_path);
                m_bmpOpeate.Dispose();
                m_bmpOpeate = new Bitmap(bmptemp);
                bmptemp.Dispose();

                DS.ReplaceDisplayImage(m_bmpOpeate);
            }
        }
        private void btnRectYz_Click(object sender, EventArgs e)
        {
            Rectangle myTestYzRect = new Rectangle();
            #region

            GraphicalObject grobj = myMover[0].Source;
            Rectangle rectx = (grobj as JzRectEAG).GetRect;
            Bitmap bmpinput = (Bitmap)m_bmpOpeate.Clone(rectx, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            vsMessageBox = new VsMessageBox($"验证四边形点位中...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            myTestYzRect = rectx;

            m_Find.AH_SetThreshold(ref bmpinput, propGrid_CaliClass.threshold_value);
            m_Find.AH_FindBlob(bmpinput, true);

            if (m_Find.FoundList.Count > 0)
            {
                Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + rectx.X,
                    m_Find.rectMaxRect.Y + rectx.Y,
                    m_Find.rectMaxRect.Width,
                    m_Find.rectMaxRect.Height);

                Bitmap bmpinputdraw = new Bitmap(m_bmpOpeate);
                Graphics g = Graphics.FromImage(bmpinputdraw);
                //if (boxes.Count > 0)
                {
                    //g.DrawRectangles(new Pen(Color.Red, 11), boxes.ToArray());
                    g.DrawRectangle(new Pen(Color.Red, 11), rectmax);

                    //INI.Instance.mark_thresholdvalue = propGrid_CaliClass.threshold_value;
                    //INI.Instance.mark_rect = new Rectangle(rectx.X, rectx.Y, rectx.Width, rectx.Height);
                    //INI.Instance.mark_org = GetRectCenter(rectmax);
                    //INI.Instance.Save();

                    myTestYzRect = new Rectangle(rectmax.X, rectmax.Y, rectmax.Width, rectmax.Height);
                }


                g.Dispose();
                DS.ReplaceDisplayImage(bmpinputdraw);

                bmpinput.Dispose();
                bmpinputdraw.Dispose();
            }

            #endregion

            //计算mark点位置
            PointF _markOffset = new PointF(0, 0);
            #region MARK点位置
            if (propGrid_CaliClass.use_mark_pointf)
            {
                Bitmap bmpinputx = (Bitmap)m_bmpOpeate.Clone(INI.Instance.mark_rect, PixelFormat.Format24bppRgb);
                m_Find.AH_SetThreshold(ref bmpinputx, INI.Instance.mark_thresholdvalue);
                m_Find.AH_FindBlob(bmpinputx, true);

                if (m_Find.FoundList.Count > 0)
                {
                    //Rectangle rectmax = m_Find.rectMaxRect;
                    Rectangle rectmax = new Rectangle(m_Find.rectMaxRect.X + INI.Instance.mark_rect.X,
                       m_Find.rectMaxRect.Y + INI.Instance.mark_rect.Y,
                       m_Find.rectMaxRect.Width,
                       m_Find.rectMaxRect.Height);

                    Point MarkRunPtCenter = GetRectCenter(rectmax);

                    _markOffset = new PointF(MarkRunPtCenter.X - INI.Instance.mark_org.X, MarkRunPtCenter.Y - INI.Instance.mark_org.Y);
                }
                bmpinputx.Dispose();

                //_markOffset = new PointF(0, 0);
            }
            #endregion



            PointF ptlt = new PointF(myTestYzRect.Location.X - _markOffset.X, myTestYzRect.Location.Y - _markOffset.Y);
            PointF ptlb = new PointF(myTestYzRect.X - _markOffset.X, myTestYzRect.Y + myTestYzRect.Height - _markOffset.Y);
            PointF ptrt = new PointF(myTestYzRect.X + myTestYzRect.Width - _markOffset.X, myTestYzRect.Y - _markOffset.Y);
            PointF ptrb = new PointF(myTestYzRect.X + myTestYzRect.Width - _markOffset.X, myTestYzRect.Y + myTestYzRect.Height - _markOffset.Y);

            //PointF p1 = new PointF();
            //PointF p2 = new PointF();
            //PointF p3 = new PointF();
            //PointF p4 = new PointF();
            //MSRCalibration.TransformViewToWorld(ptlt, out p1);
            //MSRCalibration.TransformViewToWorld(ptlb, out p2);
            //MSRCalibration.TransformViewToWorld(ptrt, out p3);
            //MSRCalibration.TransformViewToWorld(ptrb, out p4);
            List<PointF> _list = new List<PointF>();
            //_list.Add(p1);
            //_list.Add(p2);
            //_list.Add(p3);
            //_list.Add(p4);

            _list.Add(ptlt);
            _list.Add(ptlb);
            _list.Add(ptrt);
            _list.Add(ptrb);


            #region 保存xml 供镭雕机调用

            if (!string.IsNullOrEmpty(INI.Instance.LaserSharePath))
            {
                if (!Directory.Exists(INI.Instance.LaserSharePath))
                    Directory.CreateDirectory(INI.Instance.LaserSharePath);
                if (Directory.Exists(INI.Instance.LaserSharePath))
                {
                    XmlDocument xmldoc;
                    XmlNode xmlnode;
                    XmlElement xmlelem;

                    xmldoc = new XmlDocument();
                    XmlDeclaration xmldecl;
                    xmldecl = xmldoc.CreateXmlDeclaration("1.0", "GB2312", null);
                    xmldoc.AppendChild(xmldecl);

                    //加入一个根元素
                    xmlelem = xmldoc.CreateElement("", "xmlRoot", "");
                    xmldoc.AppendChild(xmlelem);

                    XmlNode xeMatrix = xmldoc.CreateElement("Matrix");
                    xmlelem.AppendChild(xeMatrix);

                    XmlElement x2 = xmldoc.CreateElement("Center");
                    x2.SetAttribute("x", (INI.Instance.BoundaryValue).ToString());
                    x2.SetAttribute("y", (0).ToString());
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Angle");
                    x2.InnerText = "0";
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Row");
                    x2.InnerText = 2.ToString();
                    xeMatrix.AppendChild(x2);
                    x2 = xmldoc.CreateElement("Col");
                    x2.InnerText = 2.ToString();
                    xeMatrix.AppendChild(x2);

                    XmlNode xeCells = xmldoc.CreateElement("Cells");
                    xmlelem.AppendChild(xeCells);

                    int i = 0;
                    foreach (PointF itemClass in _list)
                    {
                        float fx = itemClass.X;// itemClass.xPCenterOffsetWorld.X;// + (int)numericUpDown1.Value;
                        float fy = itemClass.Y;// itemClass.xPCenterOffsetWorld.Y;// + (int)numericUpDown2.Value;

                        PointF PTTEMP = new PointF(itemClass.X, itemClass.Y);
                        INI.Instance.MSRCalibration1.TransformViewToWorld(itemClass, out PTTEMP);

                        fx = PTTEMP.X;
                        fy = PTTEMP.Y;

                        x2 = xmldoc.CreateElement("cell" + i.ToString());
                        xeCells.AppendChild(x2);

                        XmlElement x3 = xmldoc.CreateElement("Center");
                        x3.SetAttribute("x", fx.ToString(m_format));
                        x3.SetAttribute("y", fy.ToString(m_format));
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("dbAngle");
                        x3.InnerText = 0.ToString(m_format);
                        x2.AppendChild(x3);
                        x3 = xmldoc.CreateElement("nType");
                        x3.InnerText = 1.ToString();
                        x2.AppendChild(x3);

                        i++;
                    }

                    xmldoc.Save(INI.Instance.LaserSharePath + "\\linescanData.xml");
                }
            }



            #endregion

            vsMessageBox.Close();
            vsMessageBox.Dispose();
        }
        
        void init_Display()
        {
            DS.Initial(100, 0.01f);
            DS.SetDisplayType(DisplayTypeEnum.NORMAL);
            //DS2.Initial(100, 0.01f);
            //DS2.SetDisplayType(DisplayTypeEnum.SHOW);
            //DS3.Initial(100, 0.01f);
            //DS3.SetDisplayType(DisplayTypeEnum.SHOW);
            //DS.CaptureAction += DS_CaptureAction;
        }

        //private void DS_CaptureAction(RectangleF rectf)
        //{
        //    GraphicalObject grobj = myMover[0].Source;
        //    RectangleF rectf_org = (grobj as JzRectEAG).GetRect;
        //    RectangleF rectf_des = (grobj as JzRectEAG).GetRect;

        //    if (m_IsAutoRegionOpen)
        //    {
        //        BoundRect(ref rectf, myRecipe.bmpORGPattern.Size);
        //        if (rectf.Width > 1 && rectf.Height > 1)
        //        {
        //            //myRecipe.Mark0 = new RectangleF(rectf.X + rectf_org.X, rectf.Y + rectf_org.Y, rectf.Width, rectf.Height);
        //            myRecipe.Mark0 = rectf;

        //            m_IsAutoRegionOpen = false;

        //            myRecipe.ptMark0 = calMarkBlob(myRecipe.bmpORGPattern, rectf, myRecipe.PreThresholdValue, out rectf_des);
        //            myRecipe.SaveMark();

        //            Bitmap bmpx = new Bitmap(myRecipe.bmpORGPattern);
        //            Graphics g = Graphics.FromImage(bmpx);
        //            RectangleF rectangleFmark0 = SimpleRectF(myRecipe.ptMark0, 2, 2);

        //            g.DrawRectangles(new Pen(Color.Lime, 3), new RectangleF[] { rectangleFmark0 });
        //            g.DrawRectangles(new Pen(Color.Red, 3), new RectangleF[] { rectf_des });

        //            g.Dispose();
        //            DS2.ReplaceDisplayImage(bmpx);
        //            bmpx.Dispose();

        //        }
        //    }
        //}

        void update_Display()
        {
            DS.Refresh();
            DS.DefaultView();
            //DS2.Refresh();
            //DS2.DefaultView();
            //DS3.Refresh();
            //DS3.DefaultView();
        }
        private bool _assginposition(bool IsCaliBorad = true)
        {

            int XDirCount = propGrid_CaliClass.dir_x_count;
            int YDirCount = propGrid_CaliClass.dir_y_count;
            float LTX = propGrid_CaliClass.loc_x;
            float LTY = propGrid_CaliClass.loc_y;
            float XGap = propGrid_CaliClass.gap_x;
            float YGap = propGrid_CaliClass.gap_y;

            int i = 0;
            int j = 0;
            bool ret = true;
            float updownoffset = propGrid_CaliClass.sort_offset;//方框上下波動範圍

            int RowIndex = 0;
            int ColumnIndex = 0;

            List<string> RegionCheckList = new List<string>();
            List<string> RegionArrayList = new List<string>();

            int Highest = 1000000;

            foreach (MSRItemClass msritem in MSRItemList)
            {
                //if (region.Cell.CellProperty == CellPropertyEnum.STATIC)
                //{
                msritem.RowTag = 0;
                //}
            }

            RowIndex = 0;

            while (true)
            {
                Highest = 1000000;
                RegionCheckList.Clear();

                foreach (MSRItemClass msritem in MSRItemList)
                {
                    //if (region.Cell.CellProperty == CellPropertyEnum.STATIC)
                    //{
                    if (msritem.CenterPointF.Y < Highest && msritem.RowTag == 0)
                    {
                        Highest = (int)msritem.CenterPointF.Y;
                    }
                    //}
                }

                if (Highest == 1000000)
                {
                    if (RowIndex != YDirCount)
                    {
                        //MessageBox.Show("請確認相關資料是否正確。");
                        ret = false;
                    }
                    break;
                }

                i = 0;
                foreach (MSRItemClass msritem in MSRItemList)
                {
                    //if (region.Cell.CellProperty == CellPropertyEnum.STATIC)
                    //{
                    if (IsInRange(Highest, msritem.CenterPointF.Y, updownoffset) && msritem.RowTag == 0)
                    {
                        RegionCheckList.Add((((int)msritem.CenterPointF.X).ToString("000000")) + "#" + i.ToString());
                    }
                    //}
                    i++;
                }

                RegionCheckList.Sort();

                if (RegionCheckList.Count != XDirCount)
                {
                    //MessageBox.Show("請確認相關資料是否正確。");

                    ret = false;
                    break;
                }

                ColumnIndex = 0;

                string RegionArrayString = "";

                foreach (string str in RegionCheckList)
                {
                    string[] strs = str.Split('#');

                    int regionindex = int.Parse(strs[1]);

                    MSRItemList[regionindex].RelatePointF = new PointF(LTX + XGap * (float)ColumnIndex, LTY + YGap * (float)RowIndex);
                    MSRItemList[regionindex].RowTag = 10;

                    RegionArrayString += regionindex.ToString() + ",";

                    ColumnIndex++;
                }

                RegionArrayString = RemoveLastChar(RegionArrayString, 1);

                RegionArrayList.Add(RegionArrayString);

                RowIndex++;
            }

            if (ret)
            {
                PointF[,] ScreenArray = new PointF[YDirCount, XDirCount];
                PointF[,] RealArray = new PointF[YDirCount, XDirCount];

                i = 0;

                foreach (string Str in RegionArrayList)
                {
                    string[] strs = Str.Split(',');

                    j = 0;
                    foreach (string str in strs)
                    {
                        MSRItemClass msritem = MSRItemList[int.Parse(str)];
                        ScreenArray[i, j] = msritem.CenterPointF;
                        //ScreenArray[i, j] = ToWorld(msritem.CenterPointF);
                        if (IsCaliBorad)
                            RealArray[i, j] = new PointF(msritem.RelatePointF.X, -msritem.RelatePointF.Y);
                        else
                            RealArray[i, j] = new PointF(msritem.RelatePointF.X, msritem.RelatePointF.Y);

                        j++;
                    }
                    i++;
                }

                CAoiCalibration LtCalibration = MSRCalibration;

                LtCalibration.Dispose();
                LtCalibration.SetCalibrationPoints(ScreenArray, RealArray);
                LtCalibration.CalculateTransformMatrix();
            }

            return ret;
        }
        string OpenFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";

            OpenFileDialog dlg = new OpenFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        string SaveFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";

            SaveFileDialog dlg = new SaveFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;
            dlg.InitialDirectory = Traveller106.Universal.PATH_CALI;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        bool IsInRange(float FromValue, float CompValue, float DiffValue)
        {
            return Math.Abs(FromValue - CompValue) < DiffValue;
        }
        string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }
        Point GetRectCenter(Rectangle Rect)
        {
            return new Point(Rect.X + (Rect.Width >> 1), Rect.Y + (Rect.Height >> 1));
        }
        void SaveDataEXD(string DataStr, string FileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.Default);
                stm.WriteLine(DataStr);
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
        public Rectangle ResizeWithLocation3(Rectangle rect, int ratio)
        {
            Size retSize;
            Point retPtF;

            if (ratio > 0)
            {
                retPtF = new Point(rect.X * ratio, rect.Y * ratio);
                retSize = new Size(rect.Width * ratio, rect.Height * ratio);
            }
            else
            {
                retPtF = new Point(rect.X / -ratio, rect.Y / -ratio);
                retSize = new Size(rect.Width / -ratio, rect.Height / -ratio);
            }

            retSize.Width = Math.Max(retSize.Width, 1);
            retSize.Height = Math.Max(retSize.Height, 1);

            return new Rectangle(retPtF.X, retPtF.Y, retSize.Width, retSize.Height);
        }
        public PointF ToWorld(PointF eView)
        {
            PointF ret = eView;
            if (IsUseMsrFile)
                MSRCalibrationUse.TransformViewToWorld(eView, out ret);
            return ret;
        }
        public PointF ToView(PointF eWorld)
        {
            PointF ret = eWorld;
            if (IsUseMsrFile)
                MSRCalibrationUse.TransformWorldToView(eWorld, out ret);
            return ret;
        }

        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
        }


        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

        
    }
}
