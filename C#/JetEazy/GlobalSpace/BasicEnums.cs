namespace JetEazy
{
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

    public enum DBStatusEnum
    {
        ADD,
        MODIFY,
        NONE,
        DELETE,
    }

    public enum DataTableEnum : int
    {
        COUNT = 4,

        ACCDB = 0,
        ESSDB = 1,
        RUNDB = 2,
        RCPDB = 3,
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
}

