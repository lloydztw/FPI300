using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using JetEazy;
using JetEazy.ControlSpace;
using JetEazy.BasicSpace;

namespace Eazy_Project_III.OPSpace.ResultSpace
{
    public enum Result_EA : int
    {
        Allinone = 0,
        KBAOI = 1,
        KBHeight = 2,
        KBOffset = 3,
        KBGap = 4,

        Audix = 5,
        AudixDfly = 6,
        R32 = 7,
        RXX = 8,
        R15 = 9,
        R9 = 10,
        R3 = 11,
        R1 = 12,
        R5 = 13,
        C3 = 14,
        MAIN_SD = 15,
        MAIN_X6=16,
        /// <summary>
        /// 点胶第三站
        /// </summary>
        DISPENSING = 17,
    };

    public class DupItemClass
    {
        string ID = "";
        int Count = 0;
        DateTime LastTime;

        public DupItemClass(string id)
        {
            ID = id;
            Count = 1;
            LastTime = DateTime.Now;
        }
        public bool CheckDup(string id)
        {
            bool ret = false;

            if (ID == id)
            {
                Count++;

                if (Count < 3)
                    LastTime = DateTime.Now;

                ret = true;
            }
            return ret;
        }
        public bool ChekcOverTime()
        {
            bool IsOverTime = false;

            if (DateTime.Now.Subtract(LastTime).TotalSeconds > 90)
            {
                IsOverTime = true;
                LastTime = DateTime.Now;
            }
            else
                IsOverTime = false;

            return Count == 2 || (Count > 2 && IsOverTime);
        }

        public int ChekcTimes()
        {
            return Count;
        }
    }
    public class DupClass
    {
        List<DupItemClass> DupItemList = new List<DupItemClass>();

        public DupClass()
        {

        }
        public bool CheckIsOK(string id)
        {
            bool IsOKToCountinue = false;

            int i = 0;

            if (DupItemList.Count == 0)
            {
                IsOKToCountinue = true;
            }
            else
            {
                while (i < DupItemList.Count)
                {
                    if (DupItemList[i].CheckDup(id))
                    {
                        if (DupItemList[i].ChekcOverTime())
                            IsOKToCountinue = true;

                        break;
                    }
                    i++;
                }

                if (i == DupItemList.Count)
                    IsOKToCountinue = true;
            }

            return IsOKToCountinue;
        }

        public int CheckTime(string id)
        {
            int i = 0;
            int ret = 0;

            if (DupItemList.Count == 0)
            {
                Check(id, false);
            }
            else
            {
                while (i < DupItemList.Count)
                {
                    if (DupItemList[i].CheckDup(id))
                    {
                        ret = DupItemList[i].ChekcTimes();
                        break;
                    }
                    i++;
                }
            }

            return ret;
        }
        public void CheckTime(string id, bool ispass)
        {
            int i = DupItemList.Count - 1;

            while (i > -1)
            {
                if (DupItemList[i].CheckDup(id))
                {
                    if (ispass)
                        DupItemList.RemoveAt(i);
                    break;
                }
                i--;
            }
        }

        public void Check(string id, bool ispass)
        {
            bool IsDup = false;

            int i = DupItemList.Count - 1;

            while (i > -1)
            {
                if (DupItemList[i].CheckDup(id))
                {
                    if (ispass)
                        DupItemList.RemoveAt(i);

                    IsDup = true;
                    break;
                }
                i--;
            }
            if (!IsDup)
            {
                DupItemList.Add(new DupItemClass(id));
            }
        }
    }

    [Serializable]
    public abstract class GeoResultClass
    {
        protected VersionEnum VERSION;
        protected OptionEnum OPTION;
        public string RELATECOLORSTR = "";

        protected Result_EA myResultEA;
        protected JzTimes myJztimes;

        public int DirIndex;
        protected string DebugSavePath;
        protected string DebugDirPath;
        protected string[] Dirs;
        public string DebugStringNow;
        public string LastDirPath;
        protected string DebugRecipeSaveName;

        public ProcessClass MainProcess;

        //protected AlbumClass AlbumWork;
        protected CCDCollectionClass CCDCollection;
        //protected bool IsDirect = false;
        protected TestMethodEnum TestMethod = TestMethodEnum.BUTTON;
        protected bool IsNoUseCCD = false;
        protected int EnvIndex = 0;
        protected bool IsPass = false;
        public int[] DelayTime = new int[10];
        protected bool IsCheckSnPass = false;

        public bool IsStopNormalTick = false;

        protected DupClass DUP;

        //Create by Gaara 2020/10/21
        /// <summary>
        /// 存放初始的图片 用于hive上传的资料
        /// </summary>
        public List<Bitmap> listBmpHiveTemp = new List<Bitmap>();
        //Bitmap m_bmptemp = new Bitmap(1, 1);
        //public void SetAlbumWorkBmps(AlbumClass eAlbum)
        //{
        //    listBmpHiveTemp.Clear();
        //    string strPath = "D:\\LOA\\HIVETEMP";
        //    if (!Directory.Exists(strPath))
        //        Directory.CreateDirectory(strPath);
        //    foreach (PageClass page in eAlbum.ENVList[0].PageList)
        //    {
        //        //m_bmptemp.Dispose();
        //        Bitmap m_bmptemp = new Bitmap(page.GetbmpRUN()); //Bitmap m_bmptemp = new Bitmap(CCDCollection.GetBMP(page.CamIndex, false));
        //        listBmpHiveTemp.Add(m_bmptemp);
        //        m_bmptemp.Save(strPath + "\\" + page.PageRunNo + ".png");
        //        //m_bmptemp.Dispose();
        //    }
        //}

        //Create by Gaara 2019/12/26
        /// <summary>
        /// 存放測試結束後，產生的相機圖片及截圖，路徑供Hive調用
        /// </summary>
        public string Path_Hive_Pictures = "";
        /// <summary>
        /// 存放測試結束後，產生的單片數據，完整路徑名稱供Hive調用
        /// </summary>
        public string FullPathName_Hive_Reports = "";

        //public WorkStatusCollectionClass RunStatusCollection = new WorkStatusCollectionClass();

        /// <summary>
        /// 準備要開始的一些資料
        /// </summary>
        public abstract void GetStart(CCDCollectionClass ccocollection, TestMethodEnum testmethod, bool isnouseccd);
        //public abstract void GetStart(AlbumClass albumwork, CCDCollectionClass ccocollection, TestMethodEnum testmethod, bool isnouseccd);
        /// <summary>
        /// 
        /// </summary>
        public abstract void Tick();
        public abstract void GenReport();
        protected abstract void MainProcessTick();
        /// <summary>
        /// 若OperationIndex為-1時則是回復到最初的樣子
        /// </summary>
        /// <param name="operationindex"></param>
        public abstract void ResetData(int operationindex);
        public abstract void SetDelayTime();
        public abstract void FillProcessImage();
        public void SetSaveDirectory(string debugsavepath)
        {
            DebugSavePath = debugsavepath;
        }
        public void RefreshDebugSrcDirectory(string debugsrcpath)
        {
            DebugDirPath = debugsrcpath;

            Dirs = Directory.GetDirectories(DebugDirPath);
            DirIndex = 0;
        }

        int debugindex = 0;
        List<string> listPath = new List<string>();
        public string GetDirPath(string debugPath)
        {
            List<string> listpath = new List<string>();
            DirectoryInfo root = new DirectoryInfo(debugPath);
            foreach (DirectoryInfo d in root.GetDirectories())
            {
                listpath.Add(d.Name);
            }

            if (debugindex >= listpath.Count)
                debugindex = 0;
            string name = listpath[debugindex];
            debugindex++;

            return name;
        }


        /// <summary>
        /// 返回路经
        /// </summary>
        /// <returns></returns>
        public string GetLastDirPath(string debugsrcpath)
        {
            DebugDirPath = debugsrcpath;

            Dirs = Directory.GetDirectories(DebugDirPath);
            if (DirIndex >= Dirs.Length)
                DirIndex = 0;

            string Str = Dirs[DirIndex];
            LastDirPath = Str;
            return LastDirPath;
        }
        public string GetDebugDirectory()
        {
            string Str = Dirs[DirIndex];

            LastDirPath = Str;

            string[] strs;
            strs = Str.Split('\\');

            DebugStringNow = "(" + DirIndex.ToString("000") + ") " + strs[strs.Length - 1];

            DirIndex += 1;

            if (DirIndex == Dirs.Length)
            {
                RefreshDebugSrcDirectory(DebugDirPath);
            }

            return DebugStringNow;
        }

        public void SetDebugRecipeSaveName(string debugrecipesavename)
        {
            DebugRecipeSaveName = debugrecipesavename;
        }

        public bool TestAndReadData(ref string datastr, string FileName)
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader Srr = new StreamReader(fs, Encoding.Default);

                datastr = Srr.ReadToEnd();

                Srr.Close();
                Srr.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                return false;
            }

        }


        public static void CopyEntireDir(string sourcePath, string destPath)
        {
            //Now Create all of the directories        
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath));
            //Copy all the files & Replaces any files with the same name      
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destPath), true);
        }

        private static void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from))
                CopyFolder(sub + "\\", to + Path.GetFileName(sub) + "\\");

            // 文件
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, to + Path.GetFileName(file), true);
        }

        //public virtual string QFactorySend(JzQFactoryClass eQFactory, QFactoryErrorCode eErrorCode)
        //{
        //    if (eQFactory != null)
        //        return eQFactory.Send(eErrorCode);

        //    return "NULL";
        //}

        public void SaveData(string DataStr, string FileName)
        {
            File.WriteAllText(FileName, DataStr, Encoding.Default);
        }

        //Folder是需要复制的总目录，lastpath是目标目录
        //private void CopyFile(DirectoryInfo Folders, string lastpath)
        //{
        //    //首先复制目录下的文件
        //    foreach (FileInfo fileInfo in Folders.GetFiles())
        //    {
        //        if (fileInfo.Exists)
        //        {
        //            //如果列表有记录的文件，就跳过
        //            if (filePaths.Contains(fileInfo.FullName))
        //                continue;


        //            string filename = fileInfo.FullName.Substring(fileInfo.FullName.LastIndexOf('\\'));

        //            fileInfo.CopyTo(lastpath + filename, true);
        //        }
        //    }

        //    //其次复制目录下的文件夹，并且进行遍历
        //    foreach (DirectoryInfo Folder in Folders.GetDirectories())
        //    {
        //        //如果有记录在列表中，则跳过该目录
        //        if (folderPaths.Contains(Folder.FullName)) continue;
        //        string Foldername = Folder.FullName.Substring(Folder.FullName.LastIndexOf('\\'));
        //        //复制后文件夹目录
        //        string copypath = lastpath + Foldername;
        //        //创建文件夹
        //        if (!Directory.Exists(copypath))
        //            Directory.CreateDirectory(copypath);
        //        //将目录加深，遍历子目录中的文件
        //        lastpath = copypath;
        //        //子目录递归调用，遍历子目录
        //        CopyFile(Folder, lastpath);
        //        //上一个子目录中归来，还原目录深度，循环至下一子目录
        //        lastpath = lastpath.Substring(0, lastpath.LastIndexOf('\\'));




        //    }
        //}

        public delegate void TriggerHandler(ResultStatusEnum resultstatus);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(ResultStatusEnum resultstatus)
        {
            if (TriggerAction != null)
            {
                TriggerAction(resultstatus);
            }
        }

        public delegate void EnvTriggerHandler(ResultStatusEnum resultstatus, int envindex, string operpagestr);
        public event EnvTriggerHandler EnvTriggerAction;
        public void OnEnvTrigger(ResultStatusEnum resultstatus, int envindex, string operpagestr)
        {
            if (EnvTriggerAction != null)
            {
                EnvTriggerAction(resultstatus, envindex, operpagestr);
            }
        }

        public delegate void TriggerOPHandler(ResultStatusEnum resultstatus, string str);
        public event TriggerOPHandler TriggerOPAction;
        public void OnTriggerOP(ResultStatusEnum resultstatus, string str)
        {
            if (TriggerOPAction != null)
            {
                TriggerOPAction(resultstatus, str);
            }
        }


        public delegate void TriggerOPMessIng( string str);
        public event TriggerOPMessIng TriggerOPMess;
        public void OnTriggerMess( string str)
        {
            if (TriggerOPMess != null)
            {
                TriggerOPMess( str);
            }
        }
    }
}
