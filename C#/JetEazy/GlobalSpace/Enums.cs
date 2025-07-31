using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace JetEazy
{
    public enum CCDTYPEEnum
    {
        EPIX,
        TIS,
        TISUSB,
        IDS,
        PTG,
        FILE,
        AISYS,
        ICAM,
        IWIN,
        MVS,
        HIK,
        USBCAM,
    }
    /// <summary>
    /// CCD属性
    /// </summary>
    public enum CCDProcAmpProperty
    {
        /// <summary>
        /// 亮度
        /// </summary>
        Brightness = 0,
        /// <summary>
        /// 对比度
        /// </summary>
        Contrast = 1,
        /// <summary>
        /// 色调
        /// </summary>
        Hue = 2,
        /// <summary>
        /// 饱和度
        /// </summary>
        Saturation = 3,
        /// <summary>
        /// 清晰度
        /// </summary>
        Sharpness = 4,
        /// <summary>
        /// 锐度
        /// </summary>
        Gamma = 5,
        /// <summary>
        /// 色彩深度
        /// </summary>
        ColorEnable = 6,
        /// <summary>
        /// 白平衡
        /// </summary>
        WhiteBalance = 7,
        /// <summary>
        /// 弱光补偿
        /// </summary>
        BacklightCompensation = 8,
        /// <summary>
        /// Gain值
        /// </summary>
        Gain = 9
    }
    public enum SizeMethodEnum
    {
        NONE,

        CORP,   //a
        EXTEND, //b
        ZOOM,   //c
    }
    public enum ReasonEnum
    {
        PASS,
        NG,

        BLINDNG,
    }
    public enum AnanlyzeProcedureEnum
    {
        ALIGNTRAIN,
        /// <summary>
        /// 定位错误 印刷错误
        /// </summary>
        ALIGNRUN,
        /// <summary>
        /// 检测错误 印刷缺失
        /// </summary>
        INSPECTION,
        /// <summary>
        /// 偏移 印刷偏移
        /// </summary>
        BIAS,

        CHECKMEAN,
        /// <summary>
        /// 脏污 
        /// </summary>
        CHECKDIRT,

        CHECKWH,
        CHECKCORNER,

        PREPARE,

        MEASURE,

        CHECKOCR,

        /// <summary>
        ///螺丝高跷
        /// </summary>
        STILTS,

        CHECKBARCODE,

        检查键盘膜,
        PADINSPECT,
    }

    public enum CornerPositionEnum
    {
        NONE = -1,
        LEFTTOP = 0,
        RIGHTTOP = 1,
        LEFTBOTTOM = 2,
        RIGTHBOTTOM = 3,
    }
    public enum CornerEnum : int
    {
        COUNT = 4,

        LT = 0,
        RT = 1,
        LB = 2,
        RB = 3,
        XDIR = 4,
        YDIR = 5,

        NONE = -1,

        MD = -2,
        CD = -3,

        XSIGNED = -4,
        YSIGNED = -5,
    }
    public enum CornerExEnum : int
    {
        COUNT = 12,

        LT = 0,
        RT = 1,
        LB = 2,
        RB = 3,

        PT1 = 4,
        PT2 = 5,
        PT3 = 6,
        PT4 = 7,
        PT5 = 8,
        PT6 = 9,

        MPT = 10, // <-= Mutual Point
        DPT = 11, // <-= Define Point

        NONE = -1,
    }
    public enum PositionEnum : int
    {
        NONE = -1,

        XDir = 0,
        YDir = 1,
        LeftTop = 2,
        RightTop = 3,
        LeftBottom = 4,
        RightBottom = 5,

        COUNT = 6,
        DIRCOUNT = 2,
    }

    public enum VersionEnum : int
    {
        BRONTES = 0,
        STEROPES = 1,
        ARGES = 2,

        KBAOI = 3,
        CNAOI = 4,
        HEIGHTMEASURE = 5,
        MEASURE2D = 6,

        ALLINONE = 7,

        AUDIX = 8,

        PROJECT = 9,
        TRAVELLER = 10,
        /// <summary>
        /// 德龙镭射
        /// </summary>
        LASER = 11,
        /// <summary>
        /// 线扫AOI  Create 20250516
        /// </summary>
        AOI = 12,
    }
    public enum OptionEnum : int
    {
        MAIN = 0,
        DISPENSING = 1,
        DISPENSINGX1 = 2,
        DISPENSINGX2 = 3,
        DISPENSINGX3 = 4,
        DISPENSINGX4 = 5,
        MAIN_X6 = 6,
        MAIN_LS = 7,
        MAIN_NEEDLE = 8,
        MAIN_MINIX6 = 9,
        /// <summary>
        /// 镭雕引导定位
        /// </summary>
        MAIN_X1 = 10,
        /// <summary>
        /// 线扫AOI
        /// </summary>
        MAIN_X2 = 11,
        /// <summary>
        /// FPI 线扫+飞拍
        /// </summary>
        MAIN_FPIX3 = 12,
    }

    public enum LinescanTypeEnum : int
    {
        [Description("华睿线扫")]
        HUARUI = 0,
        [Description("度申线扫")]
        DVP2 = 1,
        [Description("埃科线扫")]
        ITK = 2,
        [Description("迈德威视")]
        MIND = 3,
    }

    public enum ESSStatusEnum
    {
        EXIT,
        LOGIN,
        LOGOUT,
        LOGINCOMPLETE,

        ACCOUNTMANAGE,
        ACCOUNTMANAGECOMPLETE,

        CHANGERECIPE,
        RECIPESELECTED,

        RUN,
        RECIPE,
        SETUP,

        EDIT,

        SHOPFLOORON,
        SHOPFLOOROFF,

        FASTCAL,

        LOAD,
        UNLOAD,
        RESET,

        CHECKLIVE,
        SHOWSETUP,

    }
    public enum INIStatusEnum
    {
        EDIT,
        CHANGELANGUAGE,
        EXIT,
        OK,
        CANCEL,

        CAM3GETZERO,
        CAM4GETZERO,

        CALIBRATE,
        SETUP_PARA,

        SHOWASSIGN,
    }

    public enum RCPStatusEnum
    {
        PAUSE,
        CONTINUE,
        ADD,

        EDIT,
        MODIFYCOMPLETE,
        MODIFYCANCEL,
        DELETE,

        SHOWDETAIL,
        SHOWASSIGN,
        SHOWCOMPOUND,

        BASISOK,
        ASSIGNOK,

        SETPOSITION,
        SETEND,
        GOPOSITION,

        CHANGEANALYZE,
        COMBINEANALYZE,
        EDITDETAIL,

        CHANGELIGHT,
    }
    public enum ActionEnum : int
    {
        CAPTUREONCE,
        CAPTURETEST,
        ALLRESET,


        //MAIN_SD
        /// <summary>
        /// 设定马达参数界面
        /// </summary>
        ACT_MOTOR_SETUP,
        /// <summary>
        /// 测试取像
        /// </summary>
        ACT_TEST_GETIMAGE,

        /// <summary>
        /// 用户设定满盒PASS
        /// </summary>
        ACT_USER_FULL_PASS,
        /// <summary>
        /// 用户设定满盒NG
        /// </summary>
        ACT_USER_FULL_NG,

        ACT_SENSOR_FULL_PASS,
        ACT_SENSOR_FULL_NG,

        ACT_ISEMC,
        /// <summary>
        /// 连续NG个数
        /// </summary>
        ACT_CONTINUE_COUNT,
        /// <summary>
        /// 设定相机曝光
        /// </summary>
        ACT_SETCAMEXPOSURE,
    }

    //public enum MotionEnum : int
    //{
    //    COUNT = 16,

    //    M0 = 0,
    //    M1 = 1,
    //    M2 = 2,
    //    M3 = 3,
    //    M4 = 4,
    //    M5 = 5,
    //    M6 = 6,
    //    M7 = 7,
    //    M8 = 8,
    //    M9 = 9,
    //    M10 = 10,
    //    M11 = 11,
    //    M12 = 12,
    //    M13 = 13,
    //    M14 = 14,
    //    M15 = 15,


    //}
    public enum IOEnum : int
    {
        COUNT = 8,

        I1 = 0,
        I2 = 1,
        I3 = 2,
        I4 = 3,
        I5 = 4,
        I6 = 5,
        I7 = 6,
        I8 = 7,

    }
    public enum DBStatusEnum
    {
        ADD,
        MODIFY,
        NONE,
        DELETE,
    }
    public enum OPTypeEnum
    {
        ASN,
        BAS,
        EHS,

        REG,
        SIDE,
        SETUP,
        VIEW,
    }
    public enum DisplayOPTypeEnum
    {
        NONE,
        SIMPLE,
        ADJUST,
        GETKEYBOARDRANGE,

        SELECT,
        RESIZE,
        MOVE,

        FIRST,
        SECOND,
        THIRD,

        CHECKSELECT,
        CHECKRESIZE,
        CHECKMOVE,

        PTMOVE,
    }
    public enum DisplayStatusEnum
    {
        LIVE,
        FREEZE,
    }
    public enum ResultStatusEnum
    {
        CALSTART,
        CALEND,
        CALIBRATEEND,

        CALPASS,
        CALNG,

        COUNTSTART,
        COUNTEND,

        REFRESHVIEW,
        REFREREGIONSHVIEW,
        SETFOCUSBACK,

        REFRESHRESULT,
        REFRESHUB,

        ENDRESULT,

        CANCEL,
        CANCELFORNG,

        PROCESSSTART,
        PROCESSEND,
        FORECEEND,

        CALPAUSE,
        CALPAUSE1,
        CALPAUSE2,

        SETCAMLIGHT,
        CHANGEDIRECTORY,
        CHANGEENVDIRECTORY,

        SAVERAW,
        SAVEDEBUGRAW,
        SAVENGRAW,
        SAVEHIGHTRAW,

        SNSTART,

        CAPTUREONCE,

        /// <summary>
        /// 記錄鍵高機報表
        /// </summary>
        RECORDHEIGHTREPORT,

        CAPTUREONCEEND,

        STARTLOGPROCESS,
        LOGPROCESS,


        GETIMAGECOMPLETE,

    }
    public enum RunStatusEnum
    {
        /// <summary>
        /// 打开ready信号
        /// </summary>
        X6_READY,

        SHINNIGEND,

        SHOWFORM,
        HIDEFORM,

        RUNSETUPTEST,

        STARTRUN,
        STOPRUN,

        BACKTONORMAL,

        /// <summary>
        /// 切换
        /// </summary>
        CHANGERECIPE,

    }
    public enum RunLineMode
    {
        /// <summary>
        /// 跑线模式
        /// </summary>
        RUNLine,
        /// <summary>
        /// 测试模式
        /// </summary>
        Test,
        /// <summary>
        /// 加载模式
        /// </summary>
        Loading,
        /// <summary>
        /// 参数设定模式
        /// </summary>
        SetPar,
    }
    public enum TestMethodEnum
    {
        IO,
        BARCODE,
        BUTTON,
    }

    public enum MotionTypeEnum
    {
        AXIS,
        ROTATION,
    }

    public enum DataTableEnum : int
    {
        COUNT = 4,

        ACCDB = 0,
        ESSDB = 1,
        RUNDB = 2,
        RCPDB = 3,
    }

    public enum OPDataTableEnum : int
    {
        COUNT = 2,

        ALBUMDB = 0,
        ENVDB = 1,

    }
    public enum MatchMethodEnum : int
    {
        COUNT = 4,

        NONE = -1,

        OFF30 = 0,
        OFF50 = 1,
        OFF100 = 2,
        LUMINA = 3,
    }
    public enum LayoutEnum
    {
        L1440X900,
        L1280X800,
    }
    public enum MachineEventEnum : int
    {
        START = 0,
        EMC = 1,
        CURTAIN = 2,
        AUTOSTART = 3,

        /// <summary>
        /// 严重报警
        /// </summary>
        ALARM_SERIOUS = 4,
        /// <summary>
        /// 普通报警
        /// </summary>
        ALARM_COMMON = 5,

        /// <summary>
        /// 警告
        /// </summary>
        ALARM_WARNING = 6,
    }

    public enum MC100IONameEnum : int
    {
        COUNT = 14,

        PA00 = 0,
        PA01 = 1,
        PA02 = 2,
        PA03 = 3,
        PA04 = 4,
        PA05 = 5,
        PA06 = 6,
        PA07 = 7,
        PB00 = 8,
        PC00 = 9,
        PC01 = 10,
        PC02 = 11,
        PC03 = 12,
        PC04 = 13,
    }
    public enum MachineState : int
    {
        /// <summary>
        /// 綠燈-跑線
        /// </summary>
        Running = 1,
        /// <summary>
        /// 黃燈-等待測試
        /// </summary>
        Idle = 2,
        /// <summary>
        /// 藍燈-工程師模式調試參數
        /// </summary>
        Engineering_mode = 3,
        /// <summary>
        /// 白燈-停止使用
        /// </summary>
        Planned_downtime = 4,
        /// <summary>
        /// 紅燈-故障
        /// </summary>
        Error = 5,
    }
    public enum EventActionTypeEnum
    {
        MANUAL,
        MOVP,
        MOVN,
        PORCESS,
        AUTOMATIC,
        TEST,
        EXCEPTION,
    }


}

