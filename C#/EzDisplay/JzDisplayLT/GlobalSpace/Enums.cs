namespace JzDisplay
{
    public enum ShapeEnum : int
    {
        ANY = -1,

        COUNT = 10,

        RECT = 0,
        CIRCLE = 1,
        POLY = 2,
        CAPSULE = 3,
        RING = 4,
        ORING = 5,
        RECTRECT = 6,
        HEXHEX = 7,
        RECTO = 8,
        HEXO = 9,
    }
    public enum ShapeDefineEnum
    {
        RECT,
        CIRCLE,
        POLY,
        CAPSULE,
        RING,
        ORING,
        RECTRECT,
        HEXHEX,
        RECTO,
        HEXO,
    }
    public enum MoverOpEnum : int
    {
        SELECT,
        ADD,
        DEL,

        MOUSEUP,

        //For Adjust Image Position

        READYTOMOVE,
    }
    public enum ShapeOpEnum : int
    {
        ADDSHAPE,
        DELSHAPE,
        REVISESHAPE,
    }
    public enum DisplayTypeEnum
    {
        SHOW,   //僅顯示
        ADJUST, //調整位置
        NORMAL, //所有功能全開
        CAPTRUE,    //取像
        //SELECT,//仅选中
    }
    public enum ShowModeEnum : int
    {
        NORMAL = 0,
        BORDERSHOW = 1,
        MAINSHOW = 2,
    }




}
