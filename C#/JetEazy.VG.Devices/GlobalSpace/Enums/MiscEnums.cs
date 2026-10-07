namespace JetEazy
{
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

    public enum OPDataTableEnum : int
    {
        COUNT = 2,

        ALBUMDB = 0,
        ENVDB = 1,

    }

    public enum LayoutEnum
    {
        L1440X900,
        L1280X800,
    }
}
