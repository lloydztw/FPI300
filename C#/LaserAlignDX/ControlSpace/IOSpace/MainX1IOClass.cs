
using JetEazy.ControlSpace;

namespace VsCommon.ControlSpace.IOSpace
{
    public class MainX1IOClass : GeoIOClass
    {
        public enum ADRMainX1 : int
        {
            COUNT = 9,

            ADR_Heart = 0,
            ADR_LineScanRecipe = 1,
            ADR_SoftwareReady = 2,
            ADR_LineScanStart = 3,
            ADR_LineScanReady = 4,
            ADR_LineScanDone = 5,
            ADR_LineScanResult = 6,
            ADR_LineScannWhatFor = 7,
            ADR_LineScanBarcode = 8,
        }

        public MainX1IOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            ADDRESSARRAY = new AddressClass[(int)ADRMainX1.COUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {
            ADDRESSARRAY[(int)ADRMainX1.ADR_Heart] = new AddressClass(ReadINIValue("Operation Address", "Heart", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanRecipe] = new AddressClass(ReadINIValue("Operation Address", "LineScanRecipe", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_SoftwareReady] = new AddressClass(ReadINIValue("Operation Address", "SoftwareReady", "", INIFILE));

            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanStart] = new AddressClass(ReadINIValue("Operation Address", "LineScanStart", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanReady] = new AddressClass(ReadINIValue("Operation Address", "LineScanReady", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanDone] = new AddressClass(ReadINIValue("Operation Address", "LineScanDone", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanResult] = new AddressClass(ReadINIValue("Operation Address", "LineScanResult", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScannWhatFor] = new AddressClass(ReadINIValue("Operation Address", "ADR_LineScannWhatFor", "", INIFILE));

            ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanBarcode] = new AddressClass(ReadINIValue("Operation Address", "ADR_LineScanBarcode", "0:Gvl_LinescanPC.sLineScanCode", INIFILE));



        }
        public override void SaveData()
        {

        }


        public bool Heart
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_Heart];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_Heart];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        public string LineScanRecipe
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanRecipe];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanRecipe];
                PLC[address.SiteNo].WriteVari(address.Address0, value);
            }
        }
        /// <summary>
        /// 启动软件的ready
        /// </summary>
        public bool SoftwareReady
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_SoftwareReady];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_SoftwareReady];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }

        /// <summary>
        /// PLC->PC 马达移动到开始位通知pc信号
        /// </summary>
        public bool LineScanStart
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanStart];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
        }
        /// <summary>
        /// PC->PLC 线扫准备就绪
        /// </summary>
        public bool LineScanReady
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanReady];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanReady];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        /// <summary>
        /// PC->PLC完成信号和结果一起给
        /// </summary>
        public bool LineScanDone
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanDone];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanDone];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        /// <summary>
        /// 1：ok 2:ng
        /// </summary>
        public string LineScanResult
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanResult];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanResult];
                PLC[address.SiteNo].WriteVari(address.Address0, value);
            }
        }
        /// <summary>
        /// 0:正常 1:校正
        /// </summary>
        public int ADR_LineScannWhatFor
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScannWhatFor];
                int iret = 0;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScannWhatFor];
            //    PLC[address.SiteNo].WriteVari(address.Address0, value);
            //}
        }
        public string ADR_LineScanBarcode
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanBarcode];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScanBarcode];
                PLC[address.SiteNo].WriteVari(address.Address0, value);
            }
        }

    }
}
