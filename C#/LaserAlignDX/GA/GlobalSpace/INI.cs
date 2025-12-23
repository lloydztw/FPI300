//using JetEazy;
using Eazy_Project_III;
using JetEazy;
using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
//using Eazy_Project_III;

namespace Traveller106
{

    public class GetPositionPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    //將顯示得視窗放在這邊，並透過ShowDialog方式來呼叫
                    //取得到的值再回傳回去
                    //MessageBox.Show("sfsf");
                    //frmDataGridViewPosition msrdataform = new frmDataGridViewPosition(pContext.PropertyDescriptor.DisplayName,
                    //    pContext.PropertyDescriptor.Name,(string)pValue);
                    ////msrdataform.Show();
                    //if (msrdataform.ShowDialog() == DialogResult.OK)
                    //{
                    //    pValue = JzToolsClass.PassingString;
                    //}

                    //pValue = "FUCK YOU!";
                }
            }
            return pValue;
        }
    }

    public class GetFilePathPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {

                    OpenFileDialog fileDlg = new OpenFileDialog();

                    switch (pContext.PropertyDescriptor.Name)
                    {
                        case "CfgPath":
                            fileDlg.Filter = "控制器配置文件|*.dcfg";
                            fileDlg.Title = "请选择匹配的控制器配置文件";
                            break;
                        case "HWCPath":
                            fileDlg.Filter = "控制器位移校准文件|*.hwc";
                            fileDlg.Title = "请选择匹配的控制器位移校准文件";
                            break;
                        case "L1Path":
                        case "L2Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择镭射头的标定校准文件";
                            break;
                        case "L3Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择相机标定板校准文件";
                            break;
                        case "L4Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择转换到镭射头校准文件";
                            break;
                        default:
                            break;
                    }

                    if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        pValue = fileDlg.FileName;
                    }
                }
            }
            return pValue;
        }
    }
    public class SetFilePathPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {

                    FolderBrowserDialog fd = new FolderBrowserDialog();
                    //fd.Description = Description;
                    fd.ShowNewFolderButton = false;
                    //fd.SelectedPath = DefaultPath;

                    switch (pContext.PropertyDescriptor.Name)
                    {
                        case "LaserSharePath":
                            fd.SelectedPath = (string)pValue;
                            fd.Description = "请选择与镭雕机共享文件路径";
                            break;
                    }

                    if (fd.ShowDialog().Equals(DialogResult.OK))
                    {
                        if (fd.SelectedPath != "")
                            pValue = fd.SelectedPath;
                    }
                }
            }
            return pValue;
        }
    }

    public class INI
    {
        private static INI _instance = null;
        public static INI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new INI();
                return _instance;
            }
        }

        #region INI Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(1024);
            int Length = GetPrivateProfileString(section, key, "", temp, 1024, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            //else
            //    retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;

        }
        #endregion

        string MAINPATH = "";
        string INIFILE = "";

        public string HistoryDataPath = string.Empty;
        public string HistoryDataBarcode = string.Empty;

        public bool IsOpenMotorWindows = false;
        public bool IsOpenIOWindows = false;
        public bool IsOpenFocusWindows = false;

        //static JzToolsClass JzTools = new JzToolsClass();

        public int LANGUAGE = 0;

        public CAoiCalibration MSRCalibration1 = new CAoiCalibration();
        public CAoiCalibration MSRCalibration2 = new CAoiCalibration();
        /// <summary>
        /// 相机拍摄标准版与标准板之间的标定
        /// </summary>
        public CAoiCalibration MSRCaliCameraToWorld = new CAoiCalibration();
        /// <summary>
        /// laser打的点转换到world后与laser生成点之间的标定
        /// </summary>
        public CAoiCalibration MSRCaliLaserWorldToLaserCmd = new CAoiCalibration();

        #region 调针机

        const string LSCat1 = "A01.基础参数";

        //[CategoryAttribute(X1Cat1), DescriptionAttribute("即特微相机与定位相机的XY偏移")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.特微相机与定位相机偏移距离")]
        //[Browsable(true)]
        //public string offsetCAM0CAM1 { get; set; } = string.Empty;
        //[CategoryAttribute(X1Cat1), DescriptionAttribute("即显微相机与定位相机的XY偏移")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.显微相机与定位相机偏移距离")]
        //[Browsable(true)]
        //public string offsetCAM2CAM1 { get; set; } = string.Empty;

        

        //[CategoryAttribute(LSCat1), DescriptionAttribute("true开 false关")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.保存结果图")]
        //[Browsable(false)]
        //public bool IsSaveResultImage { get; set; } = false;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        [Editor(typeof(SetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.共享路径")]
        [Browsable(false)]
        public string LaserSharePath { get; set; } = "D:\\share\\linescan";

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04.是否开启多线程")]
        [Browsable(false)]
        public bool IsOpenThread { get; set; } = true;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.是否画出编号")]
        [Browsable(false)]
        public bool IsOpenDrawNumber { get; set; } = true;

        [CategoryAttribute(LSCat1), DescriptionAttribute("单位(秒)")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06.线扫超时时间")]
        [Browsable(false)]
        public int GetImageDelayTime { get; set; } = 20;
        [CategoryAttribute(LSCat1), DescriptionAttribute("单位(毫秒)")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("07.灯光延时时间")]
        [Browsable(false)]
        public int LightDelayTime { get; set; } = 500;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("08.是否缩放显示图")]
        [Browsable(false)]
        public bool IsOpenShowSize { get; set; } = true;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("09.缩放倍数")]
        [Browsable(false)]
        public int ShowSizeValue { get; set; } = 3;

        //[CategoryAttribute(LSCat1), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("10.调整值x")]
        //[Browsable(false)]
        //public float Cal_Bcx { get; set; } = 0;

        //[CategoryAttribute(LSCat1), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("11.调整值y")]
        //[Browsable(false)]
        //public float Cal_Bcy { get; set; } = 0;

        //[CategoryAttribute(LSCat1), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("12.调整值a")]
        //[Browsable(false)]
        //public float Cal_Bca { get; set; } = 0;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("13.只打第一颗")]
        [Browsable(false)]
        public bool IsOpenPrintFirst { get; set; } = false;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("14.X方向解析度")]
        [Browsable(false)]
        public float DIRRES_X { get; set; } = 0.0102f;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("15.Y方向解析度")]
        [Browsable(false)]
        public float DIRRES_Y { get; set; } = 0.0102f;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("16.是否使用固定点")]
        [Browsable(false)]
        public bool IsUseFixedMark { get; set; } = true;

        [CategoryAttribute(LSCat1), DescriptionAttribute("判断直角的角度误差 抓四边数据使用")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("17.直角的公差")]
        [Browsable(false)]
        public float GC_Angle { get; set; } = 5f;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("18.光源控制器个数")]
        [Browsable(false)]
        public int LedControlCount { get; set; } = 1;

        #endregion

        #region 线扫相关INI设置
        //const string LSCat2 = "A02.线扫相关设置";

        //[CategoryAttribute(LSCat2), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("00.线扫测试速度")]
        ////[Browsable(X1Visable)]
        ///// <summary>
        ///// 线扫测试速度
        ///// </summary>
        //public int LineScanTestSpeed { get; set; } = 2000;

        //[CategoryAttribute(LSCat2), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.线扫Y起点位置")]
        //public float linescan_Xpos_start { get; set; } = 0;

        //[CategoryAttribute(LSCat2), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.线扫Y结束位置")]
        //public float linescan_Xpos_end { get; set; } = 340;

        //[CategoryAttribute(LSCat2), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("03.线扫X起点位置")]
        //public float linescan_Ypos_start { get; set; } = 0;

        //[CategoryAttribute(LSCat2), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("04.线扫Z拍照位置")]
        //public float linescan_Zpos_start { get; set; } = 0;
        #endregion

        #region 其他设置

        const string LSCat6 = "A02.通讯设置";

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("00.校正参数")]
        [Browsable(false)]
        public string cali_paras { get; set; } = string.Empty;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.固定点ROI")]
        [Browsable(false)]
        [ReadOnly(true)]
        public Rectangle mark_rect { get; set; } = new Rectangle(0, 0, 500, 500);
        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.固定点原始坐标")]
        [Browsable(false)]
        [ReadOnly(false)]
        public PointF mark_org { get; set; } = new PointF(0, 0);
        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.固定点阈值")]
        [Browsable(false)]
        [ReadOnly(false)]
        public int mark_thresholdvalue { get; set; } = 50;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.镭雕服务器IP")]
        [Browsable(false)]
        public string tcp_ip { get; set; } = "127.0.0.1";

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.镭雕服务器端口")]
        [Browsable(false)]
        //[ReadOnly(false)]
        public int tcp_port { get; set; } = 33000;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.是否连接Handle")]
        [Browsable(false)]
        public bool tcp_handle_open { get; set; } = false;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04.Handle服务器IP")]
        [Browsable(false)]
        public string tcp_handle_ip { get; set; } = "127.0.0.1";

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.Handle服务器端口")]
        [Browsable(false)]
        //[ReadOnly(false)]
        public int tcp_handle_port { get; set; } = 33001;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06.通讯时间")]
        [Browsable(false)]
        //[ReadOnly(false)]
        public int handle_delaytime { get; set; } = 1500;


        //[CategoryAttribute(LSCat6), DescriptionAttribute("即 出盘进盘抖动的次数")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.抖动次数")]
        ////[Browsable(X1Visable)]
        //public int plc_quiverCount { get; set; } = 2;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("即 报警时蜂鸣器静音")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.报警静音")]
        ////[Browsable(X1Visable)]
        //public bool plc_mute_buzzer { get; set; } = false;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("即 强制停止plc true为强制 false为plc控制")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("03.强制停止plc")]
        ////[Browsable(X1Visable)]
        //public bool pcForceStopPlc { get; set; } = false;

        //[CategoryAttribute(LSCat6), DescriptionAttribute("即 开启试运行plc true为开启 false为关闭")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("04.开启试运行plc")]
        ////[Browsable(X1Visable)]
        //public bool plc_testrun { get; set; } = false;

        #endregion

        #region 文件路径设置

        const string Cat5 = "A05.文件路径设置";

        [CategoryAttribute(Cat5), DescriptionAttribute("")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("00.只启用左校正档")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public bool IsOnlyUseLeft { get; set; } = true;
        [CategoryAttribute(Cat5), DescriptionAttribute("")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("00A.启用标准板")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public bool IsUseStandBoard { get; set; } = true;

        [CategoryAttribute(Cat5), DescriptionAttribute("")]
        [Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.左镭射头校正档")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public string L1Path { get; set; } = string.Empty;

        [CategoryAttribute(Cat5), DescriptionAttribute("")]
        [Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.右镭射头校正档")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public string L2Path { get; set; } = string.Empty;

        [CategoryAttribute(Cat5), DescriptionAttribute("")]
        //[Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.左右分割点")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public int BoundaryValue { get; set; } = 15000;

        [CategoryAttribute(Cat5), DescriptionAttribute("CameraToWorld")]
        [Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04.相机标准板档")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public string L3Path { get; set; } = string.Empty;

        [CategoryAttribute(Cat5), DescriptionAttribute("LaserWorldToLaserCmd")]
        [Editor(typeof(GetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.标准板镭雕档")]
        //[ReadOnly(true)]
        [Browsable(false)]
        public string L4Path { get; set; } = string.Empty;

        #endregion

        #region 供盤设置

        //const string LSCat3 = "A03.供盤设置";

        //[CategoryAttribute(LSCat3), DescriptionAttribute("即 供盤Y位置")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("00.供盤Y位置")]
        ////[Browsable(X1Visable)]
        //public float feed_ypos { get; set; } = 1;

        //[CategoryAttribute(LSCat3), DescriptionAttribute("即 供盤下Z低位")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.供盤下Z低位")]
        ////[Browsable(X1Visable)]
        //public float feed_zposlow { get; set; } = 36.5f;

        //[CategoryAttribute(LSCat3), DescriptionAttribute("即 供盤下Z高位")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.供盤下Z高位")]
        ////[Browsable(X1Visable)]
        //public float feed_zposhigh { get; set; } = 43.5f;

        #endregion

        #region 供盤设置

        //const string LSCat4 = "A04.收盤设置";

        //[CategoryAttribute(LSCat4), DescriptionAttribute("即 收盤Y位置")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("00.收盤Y位置")]
        ////[Browsable(X1Visable)]
        //public float take_ypos { get; set; } = 974;

        //[CategoryAttribute(LSCat4), DescriptionAttribute("即 收盤下Z低位")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.收盤下Z低位")]
        ////[Browsable(X1Visable)]
        //public float take_zposlow { get; set; } = 36.8f;

        //[CategoryAttribute(LSCat4), DescriptionAttribute("即 收盤下Z高位")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.收盤下Z高位")]
        ////[Browsable(X1Visable)]
        //public float take_zposhigh { get; set; } = 43.5f;

        #endregion

        #region 其他專案設定
        [Browsable(false)]
        public LangIndex mLangIndex { get; set; } = LangIndex.LangCN;
        [Browsable(false)]
        public int AutoLogoutTime { get; set; } = 30;
        //[Browsable(false)]
        //public bool IsForceInspect { get; set; } = false;
        [Browsable(false)]
        public bool IsOpenUpload { get; set; } = false;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("21.左下调整值x")]
        [Browsable(false)]
        public float Cal_BcxLB { get; set; } = 0;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("22.左下调整值y")]
        [Browsable(false)]
        public float Cal_BcyLB { get; set; } = 0;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("23.左下调整值a")]
        [Browsable(false)]
        public float Cal_BcaLB { get; set; } = 0;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("24.画图字体大小")]
        [Browsable(false)]
        public int DrawFontSize { get; set; } = 100;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("25.画图线宽")]
        [Browsable(false)]
        public int DrawLineWidth { get; set; } = 10;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("工厂名称序号")]
        [Browsable(false)]
        public int FactoryNameIndex { get; set; } = 0;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("灯光值下限")]
        [Browsable(false)]
        public int LightMinValue { get; set; } = 150;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("记录当前LOT号")]
        [Browsable(false)]
        public string CurrentLotName { get; set; } = "NONE";
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("记录当前条码")]
        [Browsable(false)]
        public string CurrentBarcodeStr { get; set; } = "";
        #endregion

        #region X3_AOI_SETUP

        //----------------------------------------------------------------------------------------------------
        const string X3_Cat1 = "A01.相機設定";
        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("001.图像解析度")]
        [Browsable(false)]
        public float ImageResolution { get; set; } = 0.0134f;

        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("001a.图像X方向精度")]
        [Browsable(false)]
        public float ImageResolutionX { get; set; } = 0.0073f;

        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("001b.图像Y方向精度")]
        [Browsable(false)]
        public float ImageResolutionY { get; set; } = 0.00715f;

        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("01.飞拍图像解析度")]
        [Browsable(true)]
        public float FlyImageResolution { get; set; } = 0.034f;

        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位(毫秒)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [DisplayName("02.取像延时")]
        [Browsable(true)]
        public int DelayImageTime { get; set; } = 1000;

        //----------------------------------------------------------------------------------------------------
        const string X3_Cat2 = "A02.圖檔保存設定";
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [Editor(typeof(SetFilePathPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.结果图路径")]
        [Browsable(true)]
        public string ResultImagePath { get; set; } = "D:\\01FPI30ImagePath";

        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.存储压缩图片")]
        [Browsable(true)]
        public bool IsSaveDebugBMP { get; set; } = false;

        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.存储原始图片")]
        [Browsable(true)]
        public bool IsSaveDebugOrgBmp { get; set; } = false;

        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04.结果图质量")]
        [Browsable(true)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        public long ImageQuality { get; set; } = 10;

        [CategoryAttribute(X3_Cat2), DescriptionAttribute("true开 false关")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.保存单颗测试图")]
        [Browsable(true)]
        public bool IsSaveTestImage { get; set; } = false;

        [CategoryAttribute(X3_Cat2), DescriptionAttribute("true开 false关")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06.保存Strip资料")]
        [Browsable(false)]
        public bool IsSaveStripImage { get; set; } = false;

        //----------------------------------------------------------------------------------------------------
        const string X3_Cat3 = "A03.全域補償設定";
        [CategoryAttribute(X3_Cat3), DescriptionAttribute("true开 false关")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("001.强制全检")]
        [Browsable(false)]
        public bool IsForceInspect
        {
            get { return true; }
            set { }
        }

        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.线扫补偿X")]
        [Browsable(true)]
        public float Cal_Bcx { get; set; } = 0;

        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.线扫补偿Y")]
        [Browsable(true)]
        public float Cal_Bcy { get; set; } = 0;

        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.线扫补偿角度")]
        [Browsable(true)]
        public float Cal_Bca { get; set; } = 0;

        //----------------------------------------------------------------------------------------------------
        const string X3_Cat4 = "A04.其他設定";
        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.结果显示数据")]
        [Browsable(true)]
        public bool IsResultShowChar { get; set; } = false;

        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.飛拍自動停止縮放")]
        [Browsable(true)]
        public bool IsAutoDisableZoom { get; set; } = false;

        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.增强抓边")]
        [Browsable(false)]
        public bool IsCheat { get; set; } = false;
        #endregion

        #region SQL_SETUP
        const string LSCat7 = "A03.SQL设置";
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        [DisplayName("00.服务器ip")]
        [Browsable(false)]
        public string mysql_server_ip { get; set; } = "localhost";
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        [DisplayName("01.服务器port")]
        [Browsable(false)]
        public int mysql_server_port { get; set; } = 3306;
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        [DisplayName("02.用户名")]
        [Browsable(false)]
        public string mysql_server_user { get; set; } = "root";
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        [DisplayName("03.密码")]
        [Browsable(false)]
        public string mysql_server_pwd { get; set; } = "";
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        [DisplayName("04.数据库名称")]
        [Browsable(false)]
        public string mysql_server_db { get; set; } = "db_2ds_sys";
        #endregion


        public void Initial()
        {
            MAINPATH = Universal.MAINPATH;
            INIFILE = MAINPATH + "\\CONFIG.ini";

            LoadLanguage();

            //初始化语言
            JetEazy.BasicSpace.LanguageExClass.Instance.Load(Universal.WORKPATH);
            JetEazy.BasicSpace.LanguageExClass.Instance.LanguageIndex = (int)mLangIndex;
            //JetEazy.BasicSpace.LanguageExClass.Instance.FirstCsv = true;

            Load();
        }

        public void LoadLanguage()
        {
            mLangIndex = (LangIndex)int.Parse(ReadINIValue("Basic", "mLangIndex", ((int)mLangIndex).ToString(), INIFILE));

        }
        public void SaveLanguage()
        {
            WriteINIValue("Basic", "mLangIndex", ((int)mLangIndex).ToString(), INIFILE);
        }

        public void Load()
        {
            ImageResolution = float.Parse(ReadINIValue("Basic", "ImageResolution", ImageResolution.ToString(), INIFILE));
            FlyImageResolution = float.Parse(ReadINIValue("Basic", "FlyImageResolution", FlyImageResolution.ToString(), INIFILE));
            DelayImageTime = int.Parse(ReadINIValue("Basic", "DelayImageTime", DelayImageTime.ToString(), INIFILE));
            ImageResolutionX = float.Parse(ReadINIValue("Basic", "ImageResolutionX", ImageResolutionX.ToString(), INIFILE));
            ImageResolutionY = float.Parse(ReadINIValue("Basic", "ImageResolutionY", ImageResolutionY.ToString(), INIFILE));
            mysql_server_ip = ReadINIValue("sql", "mysql_server_ip", mysql_server_ip.ToString(), INIFILE);
            mysql_server_port = int.Parse(ReadINIValue("sql", "mysql_server_port", mysql_server_port.ToString(), INIFILE));
            mysql_server_user = ReadINIValue("sql", "mysql_server_user", mysql_server_user.ToString(), INIFILE);
            mysql_server_pwd = ReadINIValue("sql", "mysql_server_pwd", mysql_server_pwd.ToString(), INIFILE);
            mysql_server_db = ReadINIValue("sql", "mysql_server_db", mysql_server_db.ToString(), INIFILE);

            //L1Path = ReadINIValue("Basic", "L1Path", L1Path.ToString(), INIFILE);
            //L2Path = ReadINIValue("Basic", "L2Path", L2Path.ToString(), INIFILE);

            //L3Path = ReadINIValue("Basic", "L3Path", L3Path.ToString(), INIFILE);
            //L4Path = ReadINIValue("Basic", "L4Path", L4Path.ToString(), INIFILE);
            //cali_paras = ReadINIValue("Basic", "cali_paras", cali_paras.ToString(), INIFILE);

            //CurrentLotName = ReadINIValue("Basic", "CurrentLotName", CurrentLotName.ToString(), INIFILE);

            //IsOpenUpload = ReadINIValue("Basic", "IsOpenUpload", (IsOpenUpload ? "1" : "0"), INIFILE) == "1";
            IsSaveStripImage = ReadINIValue("Basic", "IsSaveStripImage", (IsSaveStripImage ? "1" : "0"), INIFILE) == "1";
            IsSaveTestImage = ReadINIValue("Basic", "IsSaveTestImage", (IsSaveTestImage ? "1" : "0"), INIFILE) == "1";
            IsSaveDebugBMP = ReadINIValue("Basic", "IsSaveDebugBMP", (IsSaveDebugBMP ? "1" : "0"), INIFILE) == "1";
            IsSaveDebugOrgBmp = ReadINIValue("Basic", "IsSaveDebugOrgBmp", (IsSaveDebugOrgBmp ? "1" : "0"), INIFILE) == "1";
            //IsOnlyUseLeft = ReadINIValue("Basic", "IsOnlyUseLeft", (IsOnlyUseLeft ? "1" : "0"), INIFILE) == "1";
            //BoundaryValue = int.Parse(ReadINIValue("Basic", "BoundaryValue", BoundaryValue.ToString(), INIFILE));
            //LaserSharePath = ReadINIValue("Basic", "LaserSharePath", LaserSharePath.ToString(), INIFILE);
            //IsUseStandBoard = ReadINIValue("Basic", "IsUseStandBoard", (IsUseStandBoard ? "1" : "0"), INIFILE) == "1";
            IsResultShowChar = ReadINIValue("Basic", "IsResultShowChar", (IsResultShowChar ? "1" : "0"), INIFILE) == "1";
            IsCheat = ReadINIValue("Basic", "IsCheat", (IsCheat ? "1" : "0"), INIFILE) == "1";
            IsAutoDisableZoom = ReadINIValue("Basic", "IsAutoDisableZoom", (IsAutoDisableZoom ? "1" : "0"), INIFILE) == "1";
            //mark_rect = StringtoRect(ReadINIValue("Basic", "mark_rect", RecttoString(mark_rect), INIFILE));
            //mark_org = StringToPointF(ReadINIValue("Basic", "mark_org", PointFtoString(mark_org), INIFILE));
            //mark_thresholdvalue = int.Parse(ReadINIValue("Basic", "mark_thresholdvalue", mark_thresholdvalue.ToString(), INIFILE));

            //IsOpenThread = ReadINIValue("Basic", "IsOpenThread", (IsOpenThread ? "1" : "0"), INIFILE) == "1";
            //IsOpenDrawNumber = ReadINIValue("Basic", "IsOpenDrawNumber", (IsOpenDrawNumber ? "1" : "0"), INIFILE) == "1";

            //GetImageDelayTime = int.Parse(ReadINIValue("Basic", "GetImageDelayTime", GetImageDelayTime.ToString(), INIFILE));
            //LightDelayTime = int.Parse(ReadINIValue("Basic", "LightDelayTime", LightDelayTime.ToString(), INIFILE));

            //IsOpenShowSize = ReadINIValue("Basic", "IsOpenShowSize", (IsOpenShowSize ? "1" : "0"), INIFILE) == "1";
            //ShowSizeValue = int.Parse(ReadINIValue("Basic", "ShowSizeValue", ShowSizeValue.ToString(), INIFILE));

            tcp_ip = ReadINIValue("Basic", "tcp_ip", tcp_ip.ToString(), INIFILE);
            tcp_port = int.Parse(ReadINIValue("Basic", "tcp_port", tcp_port.ToString(), INIFILE));

            tcp_handle_open = ReadINIValue("Basic", "tcp_handle_open", (tcp_handle_open ? "1" : "0"), INIFILE) == "1";
            tcp_handle_ip = ReadINIValue("Basic", "tcp_handle_ip", tcp_handle_ip.ToString(), INIFILE);
            tcp_handle_port = int.Parse(ReadINIValue("Basic", "tcp_handle_port", tcp_handle_port.ToString(), INIFILE));
            handle_delaytime = int.Parse(ReadINIValue("Basic", "handle_delaytime", handle_delaytime.ToString(), INIFILE));

            Cal_Bcx = float.Parse(ReadINIValue("Basic", "Cal_Bcx", Cal_Bcx.ToString(), INIFILE));
            Cal_Bcy = float.Parse(ReadINIValue("Basic", "Cal_Bcy", Cal_Bcy.ToString(), INIFILE));
            Cal_Bca = float.Parse(ReadINIValue("Basic", "Cal_Bca", Cal_Bca.ToString(), INIFILE));
            //IsOpenPrintFirst = ReadINIValue("Basic", "IsOpenPrintFirst", (IsOpenPrintFirst ? "1" : "0"), INIFILE) == "1";
            //IsUseFixedMark = ReadINIValue("Basic", "IsUseFixedMark", (IsUseFixedMark ? "1" : "0"), INIFILE) == "1";

            //DIRRES_X = float.Parse(ReadINIValue("Basic", "DIRRES_X", DIRRES_X.ToString(), INIFILE));
            //DIRRES_Y = float.Parse(ReadINIValue("Basic", "DIRRES_Y", DIRRES_Y.ToString(), INIFILE));

            ImageQuality = long.Parse(ReadINIValue("Basic", "ImageQuality", ImageQuality.ToString(), INIFILE));
            ResultImagePath = ReadINIValue("Basic", "ResultImagePath", ResultImagePath.ToString(), INIFILE);

            //GC_Angle = float.Parse(ReadINIValue("Basic", "GC_Angle", GC_Angle.ToString(), INIFILE));
            //LedControlCount = int.Parse(ReadINIValue("Basic", "LedControlCount", LedControlCount.ToString(), INIFILE));
            //AutoLogoutTime = int.Parse(ReadINIValue("Basic", "AutoLogoutTime", AutoLogoutTime.ToString(), INIFILE));
            IsForceInspect = ReadINIValue("Basic", "IsForceInspect", (IsForceInspect ? "1" : "0"), INIFILE) == "1";

            //Cal_BcxLB = float.Parse(ReadINIValue("Basic", "Cal_BcxLB", Cal_BcxLB.ToString(), INIFILE));
            //Cal_BcyLB = float.Parse(ReadINIValue("Basic", "Cal_BcyLB", Cal_BcyLB.ToString(), INIFILE));
            //Cal_BcaLB = float.Parse(ReadINIValue("Basic", "Cal_BcaLB", Cal_BcaLB.ToString(), INIFILE));

            //DrawFontSize = int.Parse(ReadINIValue("Basic", "DrawFontSize", DrawFontSize.ToString(), INIFILE));
            //DrawLineWidth = int.Parse(ReadINIValue("Basic", "DrawLineWidth", DrawLineWidth.ToString(), INIFILE));

            //FactoryNameIndex = int.Parse(ReadINIValue("Basic", "FactoryNameIndex", FactoryNameIndex.ToString(), INIFILE));
            //LightMinValue = int.Parse(ReadINIValue("Basic", "LightMinValue", LightMinValue.ToString(), INIFILE));

            //LineScanTestSpeed = int.Parse(ReadINIValue("Basic", "LineScanTestSpeed", LineScanTestSpeed.ToString(), INIFILE));

            //linescan_Xpos_start = float.Parse(ReadINIValue("Basic", "linescan_Xpos_start", linescan_Xpos_start.ToString(), INIFILE));
            //linescan_Xpos_end = float.Parse(ReadINIValue("Basic", "linescan_Xpos_end", linescan_Xpos_end.ToString(), INIFILE));
            //linescan_Ypos_start = float.Parse(ReadINIValue("Basic", "linescan_Ypos_start", linescan_Ypos_start.ToString(), INIFILE));
            //linescan_Zpos_start = float.Parse(ReadINIValue("Basic", "linescan_Zpos_start", linescan_Zpos_start.ToString(), INIFILE));

            //plc_loopCount = int.Parse(ReadINIValue("Basic", "plc_loopCount", plc_loopCount.ToString(), INIFILE));
            //plc_quiverCount = int.Parse(ReadINIValue("Basic", "plc_quiverCount", plc_quiverCount.ToString(), INIFILE));
            //plc_mute_buzzer = ReadINIValue("Basic", "plc_mute_buzzer", (plc_mute_buzzer ? "1" : "0"), INIFILE) == "1";
            //pcForceStopPlc = ReadINIValue("Basic", "pcForceStopPlc", (pcForceStopPlc ? "1" : "0"), INIFILE) == "1";
            ////plc_testrun = ReadINIValue("Basic", "plc_testrun", (plc_testrun ? "1" : "0"), INIFILE) == "1";

            //feed_ypos = float.Parse(ReadINIValue("Basic", "feed_ypos", feed_ypos.ToString(), INIFILE));
            //feed_zposlow = float.Parse(ReadINIValue("Basic", "feed_zposlow", feed_zposlow.ToString(), INIFILE));
            //feed_zposhigh = float.Parse(ReadINIValue("Basic", "feed_zposhigh", feed_zposhigh.ToString(), INIFILE));

            //take_ypos = float.Parse(ReadINIValue("Basic", "take_ypos", take_ypos.ToString(), INIFILE));
            //take_zposlow = float.Parse(ReadINIValue("Basic", "take_zposlow", take_zposlow.ToString(), INIFILE));
            //take_zposhigh = float.Parse(ReadINIValue("Basic", "take_zposhigh", take_zposhigh.ToString(), INIFILE));
            //cali_load();

            //LoadIniSetup();
        }
        public void Save()
        {
            WriteINIValue("Basic", "ImageResolution", ImageResolution.ToString(), INIFILE);
            WriteINIValue("Basic", "FlyImageResolution", FlyImageResolution.ToString(), INIFILE);
            WriteINIValue("Basic", "DelayImageTime", DelayImageTime.ToString(), INIFILE);
            WriteINIValue("Basic", "ImageResolutionX", ImageResolutionX.ToString(), INIFILE);
            WriteINIValue("Basic", "ImageResolutionY", ImageResolutionY.ToString(), INIFILE);

            WriteINIValue("sql", "mysql_server_ip", mysql_server_ip.ToString(), INIFILE);
            WriteINIValue("sql", "mysql_server_port", mysql_server_port.ToString(), INIFILE);
            WriteINIValue("sql", "mysql_server_user", mysql_server_user.ToString(), INIFILE);
            WriteINIValue("sql", "mysql_server_pwd", mysql_server_pwd.ToString(), INIFILE);
            WriteINIValue("sql", "mysql_server_db", mysql_server_db.ToString(), INIFILE);

            //SaveIniSetup();
            //WriteINIValue("Basic", "offsetCAM0CAM1", offsetCAM0CAM1.ToString(), INIFILE);
            //WriteINIValue("Basic", "offsetCAM2CAM1", offsetCAM2CAM1.ToString(), INIFILE);
            //SaveLanguage();

            //WriteINIValue("Basic", "L1Path", L1Path.ToString(), INIFILE);
            //WriteINIValue("Basic", "L2Path", L2Path.ToString(), INIFILE);
            //WriteINIValue("Basic", "L3Path", L3Path.ToString(), INIFILE);
            //WriteINIValue("Basic", "L4Path", L4Path.ToString(), INIFILE);

            //WriteINIValue("Basic", "IsOpenUpload", (IsOpenUpload ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsSaveStripImage", (IsSaveStripImage ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsSaveTestImage", (IsSaveTestImage ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsSaveDebugBMP", (IsSaveDebugBMP ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsSaveDebugOrgBmp", (IsSaveDebugOrgBmp ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "IsOnlyUseLeft", (IsOnlyUseLeft ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "BoundaryValue", BoundaryValue.ToString(), INIFILE);
            //WriteINIValue("Basic", "LaserSharePath", LaserSharePath.ToString(), INIFILE);
            //WriteINIValue("Basic", "IsUseStandBoard", (IsUseStandBoard ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "IsResultShowChar", (IsResultShowChar ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsCheat", (IsCheat ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "IsAutoDisableZoom", (IsAutoDisableZoom ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "mark_rect", RecttoString(mark_rect), INIFILE);
            //WriteINIValue("Basic", "mark_org", PointFtoString(mark_org), INIFILE);
            //WriteINIValue("Basic", "mark_thresholdvalue", mark_thresholdvalue.ToString(), INIFILE);

            //WriteINIValue("Basic", "IsOpenThread", (IsOpenThread ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsOpenDrawNumber", (IsOpenDrawNumber ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "GetImageDelayTime", GetImageDelayTime.ToString(), INIFILE);
            //WriteINIValue("Basic", "LightDelayTime", LightDelayTime.ToString(), INIFILE);

            //WriteINIValue("Basic", "IsOpenShowSize", (IsOpenShowSize ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "ShowSizeValue", ShowSizeValue.ToString(), INIFILE);

            WriteINIValue("Basic", "tcp_ip", tcp_ip.ToString(), INIFILE);
            WriteINIValue("Basic", "tcp_port", tcp_port.ToString(), INIFILE);

            WriteINIValue("Basic", "tcp_handle_open", (tcp_handle_open ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "tcp_handle_ip", tcp_handle_ip.ToString(), INIFILE);
            WriteINIValue("Basic", "tcp_handle_port", tcp_handle_port.ToString(), INIFILE);
            WriteINIValue("Basic", "handle_delaytime", handle_delaytime.ToString(), INIFILE);

            WriteINIValue("Basic", "Cal_Bcx", Cal_Bcx.ToString(), INIFILE);
            WriteINIValue("Basic", "Cal_Bcy", Cal_Bcy.ToString(), INIFILE);
            WriteINIValue("Basic", "Cal_Bca", Cal_Bca.ToString(), INIFILE);
            //WriteINIValue("Basic", "IsOpenPrintFirst", (IsOpenPrintFirst ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsUseFixedMark", (IsUseFixedMark ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "DIRRES_X", DIRRES_X.ToString(), INIFILE);
            //WriteINIValue("Basic", "DIRRES_Y", DIRRES_Y.ToString(), INIFILE);

            WriteINIValue("Basic", "ImageQuality", ImageQuality.ToString(), INIFILE);
            WriteINIValue("Basic", "ResultImagePath", ResultImagePath.ToString(), INIFILE);

            //WriteINIValue("Basic", "GC_Angle", GC_Angle.ToString(), INIFILE);
            //WriteINIValue("Basic", "LedControlCount", LedControlCount.ToString(), INIFILE);
            //WriteINIValue("Basic", "AutoLogoutTime", AutoLogoutTime.ToString(), INIFILE);
            WriteINIValue("Basic", "IsForceInspect", (IsForceInspect ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "Cal_BcxLB", Cal_BcxLB.ToString(), INIFILE);
            //WriteINIValue("Basic", "Cal_BcyLB", Cal_BcyLB.ToString(), INIFILE);
            //WriteINIValue("Basic", "Cal_BcaLB", Cal_BcaLB.ToString(), INIFILE);

            //WriteINIValue("Basic", "DrawFontSize", DrawFontSize.ToString(), INIFILE);
            //WriteINIValue("Basic", "DrawLineWidth", DrawLineWidth.ToString(), INIFILE);
            //WriteINIValue("Basic", "FactoryNameIndex", FactoryNameIndex.ToString(), INIFILE);
            //WriteINIValue("Basic", "LightMinValue", LightMinValue.ToString(), INIFILE);

            //WriteINIValue("Basic", "LineScanTestSpeed", LineScanTestSpeed.ToString(), INIFILE);

            //WriteINIValue("Basic", "linescan_Xpos_start", linescan_Xpos_start.ToString(), INIFILE);
            //WriteINIValue("Basic", "linescan_Xpos_end", linescan_Xpos_end.ToString(), INIFILE);
            //WriteINIValue("Basic", "linescan_Ypos_start", linescan_Ypos_start.ToString(), INIFILE);
            //WriteINIValue("Basic", "linescan_Zpos_start", linescan_Zpos_start.ToString(), INIFILE);

            //WriteINIValue("Basic", "plc_loopCount", plc_loopCount.ToString(), INIFILE);
            //WriteINIValue("Basic", "plc_quiverCount", plc_quiverCount.ToString(), INIFILE);
            //WriteINIValue("Basic", "plc_mute_buzzer", (plc_mute_buzzer ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "pcForceStopPlc", (pcForceStopPlc ? "1" : "0"), INIFILE);
            ////WriteINIValue("Basic", "plc_testrun", (plc_testrun ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "feed_ypos", feed_ypos.ToString(), INIFILE);
            //WriteINIValue("Basic", "feed_zposlow", feed_zposlow.ToString(), INIFILE);
            //WriteINIValue("Basic", "feed_zposhigh", feed_zposhigh.ToString(), INIFILE);

            //WriteINIValue("Basic", "take_ypos", take_ypos.ToString(), INIFILE);
            //WriteINIValue("Basic", "take_zposlow", take_zposlow.ToString(), INIFILE);
            //WriteINIValue("Basic", "take_zposhigh", take_zposhigh.ToString(), INIFILE);
            //cali_load();


        }
        public void SaveCaliParas()
        {
            SaveIniSetup();

            WriteINIValue("Basic", "cali_paras", cali_paras.ToString(), INIFILE);
            //WriteINIValue("Basic", "offsetCAM2CAM1", offsetCAM2CAM1.ToString(), INIFILE);
        }
        public void SaveLotName()
        {
            WriteINIValue("Basic", "CurrentLotName", CurrentLotName.ToString(), INIFILE);
        }

        private void cali_load()
        {
            if (File.Exists(L1Path))
            {
                MSRCalibration1.LoadBin(L1Path);
                MSRCalibration1.CalculateTransformMatrix();
            }
            if (File.Exists(L2Path))
            {
                MSRCalibration2.LoadBin(L2Path);
                MSRCalibration2.CalculateTransformMatrix();
            }
            if (File.Exists(L3Path))
            {
                MSRCaliCameraToWorld.LoadBin(L3Path);
                MSRCaliCameraToWorld.CalculateTransformMatrix();
            }
            if (File.Exists(L4Path))
            {
                MSRCaliLaserWorldToLaserCmd.LoadBin(L4Path);
                MSRCaliLaserWorldToLaserCmd.CalculateTransformMatrix();
            }
        }

        public string RecttoString(Rectangle Rect)
        {
            return Rect.X.ToString().PadLeft(4) + "," + Rect.Y.ToString().PadLeft(4) + "," + Rect.Width.ToString().PadLeft(4) + "," + Rect.Height.ToString().PadLeft(4);
        }
        public Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
        public string PointFtoString(PointF Pt)
        {
            return Pt.X.ToString().PadLeft(8) + "," + Pt.Y.ToString().PadLeft(8);
        }
        public PointF StringToPointF(string Str)
        {
            string[] strs = Str.Split(',');
            return new PointF(float.Parse(strs[0]), float.Parse(strs[1]));
        }

        private XProps m_xprops = new XProps();
        [Browsable(false)]
        public XProps XPropsConfig
        {
            get { return m_xprops; }
        }

        public void LoadIniSetup()
        {
            m_xprops.Clear();
            string cat0 = "inicat0";
            AddProperty(cat0, "IsAutoTestSavePicture", "IsAutoTestSavePicture", IsSaveTestImage, "");
            AddProperty(cat0, "IsSaveResultImage", "IsSaveResultImage", IsSaveStripImage, "");
            AddProperty(cat0, "ResultImagePath", "ResultImagePath", ResultImagePath, "");
            AddProperty(cat0, "ImageQuality", "ImageQuality", ImageQuality, "");
            AddProperty(cat0, "IsSaveDebugBMP", "IsSaveDebugBMP", IsSaveDebugBMP, "");
            AddProperty(cat0, "LaserSharePath", "LaserSharePath", LaserSharePath, "");
            AddProperty(cat0, "IsOpenThread", "IsOpenThread", IsOpenThread, "");
            AddProperty(cat0, "IsOpenDrawNumber", "IsOpenDrawNumber", IsOpenDrawNumber, "");
            AddProperty(cat0, "GetImageDelayTime", "GetImageDelayTime", GetImageDelayTime, "");
            AddProperty(cat0, "LightDelayTime", "LightDelayTime", LightDelayTime, "");
            AddProperty(cat0, "IsOpenShowSize", "IsOpenShowSize", IsOpenShowSize, "");
            AddProperty(cat0, "ShowSizeValue", "ShowSizeValue", ShowSizeValue, "");
            AddProperty(cat0, "Cal_Bcx", "Cal_Bcx", Cal_Bcx, "");
            AddProperty(cat0, "Cal_Bcy", "Cal_Bcy", Cal_Bcy, "");
            AddProperty(cat0, "Cal_Bca", "Cal_Bca", Cal_Bca, "");
            AddProperty(cat0, "IsOpenPrintFirst", "IsOpenPrintFirst", IsOpenPrintFirst, "");
            AddProperty(cat0, "DIRRES_X", "DIRRES_X", DIRRES_X, "");
            AddProperty(cat0, "DIRRES_Y", "DIRRES_Y", DIRRES_Y, "");
            AddProperty(cat0, "IsUseFixedMark", "IsUseFixedMark", IsUseFixedMark, "");
            AddProperty(cat0, "GC_Angle", "GC_Angle", GC_Angle, "");
            AddProperty(cat0, "LedControlCount", "LedControlCount", LedControlCount, "");
            AddProperty(cat0, "mLangIndex", "mLangIndex", mLangIndex, "ChangeLangIndex");
            AddProperty(cat0, "AutoLogoutTime", "AutoLogoutTime", AutoLogoutTime, "");
            AddProperty(cat0, "IsForceInspect", "IsForceInspect", IsForceInspect, "");
            AddProperty(cat0, "Cal_BcxLB", "Cal_BcxLB", Cal_BcxLB, "");
            AddProperty(cat0, "Cal_BcyLB", "Cal_BcyLB", Cal_BcyLB, "");
            AddProperty(cat0, "Cal_BcaLB", "Cal_BcaLB", Cal_BcaLB, "");
            AddProperty(cat0, "DrawFontSize", "DrawFontSize", DrawFontSize, "");
            AddProperty(cat0, "DrawLineWidth", "DrawLineWidth", DrawLineWidth, "");
            AddProperty(cat0, "IsOpenUpload", "IsOpenUpload", IsOpenUpload, "");

            string cat1 = "inicat1";
            AddProperty(cat1, "tcp_ip", "tcp_ip", tcp_ip, "");
            AddProperty(cat1, "tcp_port", "tcp_port", tcp_port, "");

            string cat2 = "inicat2";
            AddProperty(cat2, "IsOnlyUseLeft", "IsOnlyUseLeft", IsOnlyUseLeft, "");
            AddProperty(cat2, "IsUseStandBoard", "IsUseStandBoard", IsUseStandBoard, "");
            AddProperty(cat2, "L1Path", "L1Path", L1Path, "");
            AddProperty(cat2, "L2Path", "L2Path", L2Path, "");
            AddProperty(cat2, "BoundaryValue", "BoundaryValue", BoundaryValue, "");
            AddProperty(cat2, "L3Path", "L3Path", L3Path, "");
            AddProperty(cat2, "L4Path", "L4Path", L4Path, "");

        }
        public void SaveIniSetup()
        {
            foreach (XProp xpropItem in m_xprops)
            {
                switch (xpropItem.ReleateName)
                {
                    #region Cat0
                    case "IsAutoTestSavePicture":
                        IsSaveTestImage = (bool)xpropItem.Value;
                        break;
                    case "IsSaveResultImage":
                        IsSaveStripImage = (bool)xpropItem.Value;
                        break;
                    case "ResultImagePath":
                        ResultImagePath = (string)xpropItem.Value;
                        break;
                    case "ImageQuality":
                        ImageQuality = (long)xpropItem.Value;
                        break;
                    case "IsSaveDebugBMP":
                        IsSaveDebugBMP = (bool)xpropItem.Value;
                        break;
                    case "LaserSharePath":
                        LaserSharePath = (string)xpropItem.Value;
                        break;
                    case "IsOpenThread":
                        IsOpenThread = (bool)xpropItem.Value;
                        break;
                    case "IsOpenDrawNumber":
                        IsOpenDrawNumber = (bool)xpropItem.Value;
                        break;
                    case "GetImageDelayTime":
                        GetImageDelayTime = (int)xpropItem.Value;
                        break;
                    case "LightDelayTime":
                        LightDelayTime = (int)xpropItem.Value;
                        break;
                    case "IsOpenShowSize":
                        IsOpenShowSize = (bool)xpropItem.Value;
                        break;
                    case "ShowSizeValue":
                        ShowSizeValue = (int)xpropItem.Value;
                        break;
                    case "Cal_Bcx":
                        Cal_Bcx = (float)xpropItem.Value;
                        break;
                    case "Cal_Bcy":
                        Cal_Bcy = (float)xpropItem.Value;
                        break;
                    case "Cal_Bca":
                        Cal_Bca = (float)xpropItem.Value;
                        break;
                    case "IsOpenPrintFirst":
                        IsOpenPrintFirst = (bool)xpropItem.Value;
                        break;
                    case "DIRRES_X":
                        DIRRES_X = (float)xpropItem.Value;
                        break;
                    case "DIRRES_Y":
                        DIRRES_Y = (float)xpropItem.Value;
                        break;
                    case "IsUseFixedMark":
                        IsUseFixedMark = (bool)xpropItem.Value;
                        break;
                    case "GC_Angle":
                        GC_Angle = (float)xpropItem.Value;
                        break;
                    case "LedControlCount":
                        LedControlCount = (int)xpropItem.Value;
                        break;
                    case "mLangIndex":
                        mLangIndex = (LangIndex)xpropItem.Value;
                        break;
                    case "AutoLogoutTime":
                        AutoLogoutTime = (int)xpropItem.Value;
                        break;
                    case "IsForceInspect":
                        IsForceInspect = (bool)xpropItem.Value;
                        break;
                    case "Cal_BcxLB":
                        Cal_BcxLB = (float)xpropItem.Value;
                        break;
                    case "Cal_BcyLB":
                        Cal_BcyLB = (float)xpropItem.Value;
                        break;
                    case "Cal_BcaLB":
                        Cal_BcaLB = (float)xpropItem.Value;
                        break;
                    case "DrawFontSize":
                        DrawFontSize = (int)xpropItem.Value;
                        break;
                    case "DrawLineWidth":
                        DrawLineWidth = (int)xpropItem.Value;
                        break;
                    case "IsOpenUpload":
                        IsOpenUpload = (bool)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat1
                    case "tcp_ip":
                        tcp_ip = (string)xpropItem.Value;
                        break;
                    case "tcp_port":
                        tcp_port = int.Parse(xpropItem.Value.ToString());
                        break;
                    #endregion
                    #region Cat2
                    case "IsOnlyUseLeft":
                        IsOnlyUseLeft = (bool)xpropItem.Value;
                        break;
                    case "IsUseStandBoard":
                        IsUseStandBoard = (bool)xpropItem.Value;
                        break;
                    case "L1Path":
                        L1Path = (string)xpropItem.Value;
                        break;
                    case "L2Path":
                        L2Path = (string)xpropItem.Value;
                        break;
                    case "BoundaryValue":
                        BoundaryValue = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "L3Path":
                        L3Path = (string)xpropItem.Value;
                        break;
                    case "L4Path":
                        L4Path = (string)xpropItem.Value;
                        break;
                        #endregion
                }
                //if (xpropItem.ReleateName == "SHOW_CHANGE_TIME")
                //    SHOW_CHANGE_TIME = int.Parse(xpropItem.Value.ToString());

                //if (xpropItem.ReleateName == "CHANGE_LANGUAGE")
                //    CHANGE_LANGUAGE = int.Parse(xpropItem.Value.ToString());

                //if (xpropItem.ReleateName == "IsSaveImage")
                //    IsSaveImage = (bool)xpropItem.Value;
            }
        }

        void AddProperty(string eProperty, string eName, string eReleateName, object eValue, string eDescription)
        {
            eProperty = LanguageExClass.Instance.GetLanguageText(eProperty);
            eName = LanguageExClass.Instance.GetLanguageText(eName);
            eDescription = LanguageExClass.Instance.GetLanguageText(eDescription);

            XProp xProp1 = new XProp();
            xProp1.Category = eProperty;
            xProp1.ReleateName = eReleateName;
            xProp1.Name = eReleateName;
            xProp1.Value = eValue;
            xProp1.Description = eDescription;
            xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
            switch (eReleateName)
            {
                case "ResultImagePath":
                case "LaserSharePath":
                    xProp1.Editor = new SetFilePathPropertyEditor();
                    break;
                case "L1Path":
                case "L2Path":
                case "L3Path":
                case "L4Path":
                    xProp1.Editor = new GetFilePathPropertyEditor();
                    break;
                case "mLangIndex":
                    xProp1.Converter = new JzEnumConverter(typeof(LangIndex));
                    break;
                default:
                    break;
            }

            m_xprops.Add(xProp1);
        }

    }
}
