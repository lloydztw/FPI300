using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using JetEazy;
using JetEazy.DBSpace;
using JetEazy.BasicSpace;

namespace JetEazy.ControlSpace
{
    class EventIDClass
    {
        string ID = "";
        string[] Explain;
        public EventIDClass(string Str)
        {
            string[] strs = Str.Split(',');

            ID = strs[0];

            Explain = new string[strs.Length - 1];

            Explain[0] = strs[1];
            Explain[1] = strs[2];
        }
        public EventIDClass(EventIDClass eventid)
        {
            ID = eventid.ID;

            Explain = new string[eventid.Explain.Length];

            Explain[0] = eventid.Explain[0];
            Explain[1] = eventid.Explain[1];
        }
        public string GetExplain(int Index)
        {
            return Explain[Index];
        }
        public string GetID
        {
            get
            {
                return ID;
            }
        }
    }



    class EventItemClass
    {
        string Date = "";
        string Time = "";

        string ACCNAME = "";
        public string REPLACESTR = "";

        EventActionTypeEnum Action = EventActionTypeEnum.MANUAL;

        EventIDClass EventID;

        public EventItemClass(EventIDClass eventid, EventActionTypeEnum action)
        {
            Date = JzTimes.DateString;
            Time = JzTimes.TimeString;

            Action = action;

            EventID = new EventIDClass(eventid);
        }
        public bool IsAlarm
        {
            get
            {
                return EventID.GetID[0] == 'A';
            }
        }
        public bool IsWarn
        {
            get
            {
                return EventID.GetID[0] == 'W';
            }
        }
        public string GetID
        {
            get
            {
                return EventID.GetID;
            }

        }
        public override string ToString()
        {
            return ToString(ACCNAME);
        }
        public string ToString(string accname, string replacestr)
        {
            REPLACESTR = replacestr;

            return ToString(' ', accname);
        }
        public string ToString(string accname)
        {   
            return ToString(' ',accname);
        }
        public string ToString(char devchar,string accname)
        {
            string Str = "";

            Str += Date + devchar;
            Str += Time + devchar;
            Str += accname + devchar;
            Str += Action.ToString().PadRight(10) + devchar;
            Str += EventID.GetID + devchar;
            Str += EventID.GetExplain(0).Replace("%",REPLACESTR)+"      ";
            Str += EventID.GetExplain(1).Replace("%", REPLACESTR);

            ACCNAME = accname;

            return Str;
        }
    }



    public class EventClass
    {
        #region CONFIGURATION_組態常數
        const int REALALARMCOUNT = 8;
        const int EVENTCOUNT = 8;
        #endregion

        #region PRIVATE_DATA

        Label lblRealtimeAlarm;
        //ListBox lsbAlarm;
        //ListBox lsbEvent;

        //StreamWriter TableSw;
        StreamWriter EventSw;
        //StreamWriter DataSw;
        StreamWriter WarningSw;

        FileStream fs;

        string LastSavePath = "";
        string LastAlarmSavePath = "";

        string RealtimeString = "";

        List<EventIDClass> EventIDList = new List<EventIDClass>();

        List<EventItemClass> EventList = new List<EventItemClass>();
        List<EventItemClass> AlarmList = new List<EventItemClass>();
        List<EventItemClass> RealtimeAlarmList = new List<EventItemClass>();

        JzTimes DataLogTime = new JzTimes();
        JzTimes ProcessLogTime = new JzTimes(-1000);

        JzToolsClass JzTools = new JzToolsClass();
        string logPath = "D:\\EVENTLOG";

        #endregion


        public EventClass(string eventfile, string logPath = null)
        {
            //@LETIAN: 2022/07/05 加入 logPath 初始化設定.
            // 讓上層可以 統合管理所有 LOG 資料夾
            if (!string.IsNullOrEmpty(logPath))
                this.logPath = logPath;

            Initial(eventfile);
        }

        void Initial(string eventfile)
        {
            string Str = "";

            JzTools.ReadData(ref Str, eventfile);

            Str = Str.Replace(Environment.NewLine, "@");

            string[] strs = Str.Split('@');

            EventIDList.Clear();

            foreach (string str in strs)
            {
                EventIDList.Add(new EventIDClass(str));
            }

            //TableSw = File.AppendText(logPath + "\\TABLE.jdb");
            //TableSw.AutoFlush = true;

            //WarningSw = File.AppendText(logPath + "\\WARNINGTABLE.jdb");
            //WarningSw.AutoFlush = true;

            //@Gaara 2022/07/15 事件的操作LOG 由其他CLASS記錄 這裏取消
            //Tick(true);
        }

        public void Initial(Label lblrealtimealarm)
        {
            lblRealtimeAlarm = lblrealtimealarm;
        }

        public void Initial(ListBox lsbalarm, ListBox lsbevent)
        {
            //lsbAlarm = lsbalarm;
            //lsbEvent = lsbevent;
        }

        public void Initial(ListBox lsbevent)
        {
            //lsbEvent = lsbevent;
        }

        public void GenEvent(string ID, EventActionTypeEnum action, string replaceVal, AccClass acc)
        {
            foreach (EventIDClass eventid in EventIDList)
            {
                if (eventid.GetID == ID)
                {
                    AddEvent(eventid, action, replaceVal, acc);
                    break;
                }
            }
        }


        #region PRIVATE_DATA_NOT_USED

        string SaveLogName = "";

        //StreamWriter ProcessEventSw;
        //StreamWriter ProcessDataSw;
        //StreamWriter ProcessReportSw;

        string ReportPath = "";

        #endregion


        #region NOT_USED_CODE

        //public void StartProcessLog(string logname,string tabledata)
        //{
        //    string SavePath = INI.LOG_PATH + "\\" + JzTimes.DateSerialString;

        //    if (!Directory.Exists(SavePath))
        //        Directory.CreateDirectory(SavePath);

        //    ReportPath = SavePath;

        //    SaveLogName = JzTimes.TimeSerialString + "-" + logname;

        //    ProcessEventSw = File.AppendText(SavePath + "\\" + SaveLogName + ".txt");

        //    ProcessDataSw = File.AppendText(SavePath + "\\" + SaveLogName + ".log");

        //    ProcessEventSw.AutoFlush = true;
        //    ProcessDataSw.AutoFlush = true;
        //    //ProcessReportSw.AutoFlush = true;

        //    TableSw.WriteLine(tabledata + "," + SaveLogName + ".csv");

        //    //ProcessReportSw.WriteLine(processdata);

        //}

        //public void StopLog(string processstopdata)
        //{
        //    ProcessEventSw.Close();
        //    ProcessDataSw.Close();

        //    //ProcessReportSw.WriteLine(processstopdata);
        //    //ProcessReportSw.Close();
        //    JzTools.SaveData(processstopdata, ReportPath + "\\" + SaveLogName + ".csv");

        //    SaveLogName = "";
        //}

        #endregion


        void AddEvent(EventIDClass eventid, EventActionTypeEnum action, string replaceVal, AccClass acc)
        {
            bool IsAlarmAlive = false;

            EventItemClass eventitem = new EventItemClass(eventid, action);

            eventitem.ToString(acc.NAME, replaceVal);

            if (eventitem.IsAlarm || eventitem.IsWarn)
            {
                foreach (EventItemClass evtitem in AlarmList)
                {
                    if (evtitem.GetID == eventid.GetID && evtitem.REPLACESTR == replaceVal)
                    {
                        IsAlarmAlive = true;

                        break;
                    }
                }
                if (!IsAlarmAlive)
                {
                    TriggerAlarm(true);

                    AlarmList.Add(eventitem);
                    //lsbAlarm.Items.Add(eventitem.ToString(acc.NAME,replaceVal));

                    RealtimeAlarmList.Add(eventitem);

                    if (RealtimeAlarmList.Count > REALALARMCOUNT)
                    {
                        RealtimeAlarmList.RemoveAt(0);
                    }

                    RealtimeString = "";

                    foreach (EventItemClass evtitem in RealtimeAlarmList)
                    {
                        RealtimeString += evtitem.ToString(acc.NAME) + Environment.NewLine;
                    }

                    lblRealtimeAlarm.Text = RealtimeString;

                }
            }

            if (!IsAlarmAlive)
            {
                //if (!IsWarnLive)
                {
                    EventList.Add(eventitem);
                    //lsbEvent.Items.Add(eventitem.ToString(acc.NAME, replaceVal));

                    if (EventList.Count > EVENTCOUNT)
                    {
                        EventList.RemoveAt(0);
                        //lsbEvent.Items.RemoveAt(0);
                    }

                    string Str = eventitem.ToString(',', acc.NAME);

                    if (EventSw != null)
                        EventSw.WriteLine(Str);
                    //EventSw.Flush();


                    if (eventitem.GetID.Substring(0, 1) == "A" || eventitem.GetID.Substring(0, 1) == "W")
                    {
                        string SaveAWPath = logPath + "\\ALARM.LOG\\" + JzTimes.DateSerialString;

                        if (!Directory.Exists(SaveAWPath))
                            Directory.CreateDirectory(SaveAWPath);

                        if (SaveAWPath != LastAlarmSavePath)
                        {
                            if (WarningSw != null)
                                WarningSw.Close();

                            WarningSw = File.AppendText(SaveAWPath + "\\" + DateTime.Now.ToString("yyyyMMdd_HH") + ".log.csv");
                            WarningSw.AutoFlush = true;

                            LastAlarmSavePath = SaveAWPath;
                        }

                        WarningSw.WriteLine(Str);
                    }

                    //if (SaveLogName != "")
                    //    ProcessEventSw.WriteLine(Str);
                }
            }
        }

        public void RemoveAlarm()
        {
            int i = AlarmList.Count - 1;
            
            int j = 0;

            string RemoveStr = "";

            bool IsAll = true;// lsbAlarm.SelectedItems.Count == 0;

            while (i > -1)
            {
                if (IsAll)
                    //if (lsbAlarm.GetSelected(i) || IsAll)
                {
                    j = 0;

                    RemoveStr = "";

                    foreach (EventItemClass eventitem in RealtimeAlarmList)
                    {
                        if (eventitem.GetID == AlarmList[i].GetID)
                        {
                            RemoveStr = j.ToString() + ",";
                        }

                        j++;
                    }

                    if (RemoveStr.Length > 0)
                    {
                        RemoveStr = JzTools.RemoveLastChar(RemoveStr, 1);

                        string[] strs = RemoveStr.Split(',');

                        j = strs.Length - 1;

                        while (j > -1)
                        {
                            RealtimeAlarmList.RemoveAt(int.Parse(strs[j]));
                            j--;
                        }
                    }

                    //lsbAlarm.Items.RemoveAt(i);
                    AlarmList.RemoveAt(i);
                }

                i--;
            }

            RealtimeString = "";

            foreach (EventItemClass evtitem in RealtimeAlarmList)
            {
                RealtimeString += evtitem.ToString() + Environment.NewLine;
            }

            lblRealtimeAlarm.Text = RealtimeString;

            if (RealtimeString == "")
                OnAlarm(false);
        }
        /// <summary>
        /// 獲取警報的數量
        /// </summary>
        /// <returns>返回數量</returns>
        public int GetAlarmCount()
        {
            return RealtimeAlarmList.Count;
        }

        #region PRIVATE_DATA_II
        string LastLogTime = "";
        string LastDataLogTime = "";
        string LastAlarmLogTime = "";
        #endregion


        public void Tick(bool IsDirect)
        {

            if ((ProcessLogTime.IsDirectDefined(1) || IsDirect) && !LastLogTime.Equals(JzTimes.DateTimeSerialString))
            {
                LastLogTime = JzTimes.DateTimeSerialString;

                string SavePath = logPath + "\\" + JzTimes.DateSerialString;
                //string SavePath = INI.LOG_PATH + "\\" + JzTimes.DateSerialString;

                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);


                if (SavePath != LastSavePath)
                {
                    if (EventSw != null)
                        EventSw.Close();

                    //FileStream EventFS = new FileStream(SavePath + "\\DAILY.txt", FileMode.Append, FileAccess.Write, FileShare.Read);
                    //EventSw = new StreamWriter(EventFS);

                    EventSw = File.AppendText(SavePath + "\\" + DateTime.Now.ToString("yyyyMMdd_HH") + ".log.csv");
                    //EventSw = File.AppendText(SavePath + "\\DAILY.txt");
                    EventSw.AutoFlush = true;

                    //if (DataSw != null)
                    //    DataSw.Close();

                    //DataSw = File.AppendText(SavePath + "\\DAILY.log");
                    //DataSw.AutoFlush = true;

                    LastSavePath = SavePath;
                }
            }
        }


        //當有Input Trigger時，產生OnTrigger
        public delegate void AlarmHandler(bool IsBuzzer);
        public event AlarmHandler TriggerAlarm;
        void OnAlarm(bool IsBuzzer)
        {
            if (TriggerAlarm != null)
            {
                TriggerAlarm(IsBuzzer);
            }
        }
    }
}
