using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace JetEazy.BasicSpace
{
    public enum TimeUnitEnum
    {
        ms,
        sec,
        min,
        hour,
        day,
        week,
    }

    public class ProcessClass
    {
        public string Alias = "";
        public string RelateString = "";
        public bool IsOn = false;
        public int NextDuriation = 0;

        int myProcessID = 0;
        TimeUnitEnum myTimUnit = TimeUnitEnum.ms;

        JzTimes myTimer = new JzTimes();
        public TimeUnitEnum TimeUnit
        {
            get
            {
                return myTimUnit;
            }
            set
            {
                myTimUnit = value;
            }
        }

        public int ID
        {
            get
            {
                return myProcessID;
            }
            set
            {
                myProcessID = value;
                myTimer.Cut();
            }
        }
        public void Start(string Str)
        {
            RelateString = Str;
            //Start();
            IsOn = true;
            ID = 5;
        }
        public void Start()
        {
            Start("");
        }
        public void Stop()
        {
            IsOn = false;
        }
        public void Continue()
        {
            IsOn = true;
        }
        public void Pause()
        {
            IsOn = false;
        }
        public bool IsTimeup
        {
            get
            {
                bool ret = false;

                switch (TimeUnit)
                {
                    case TimeUnitEnum.ms:
                        ret = myTimer.msDuriation > NextDuriation;
                        break;
                    case TimeUnitEnum.sec:
                        ret = myTimer.secDuriation > NextDuriation;
                        break;
                    case TimeUnitEnum.min:
                        ret = myTimer.minDuriation > NextDuriation;
                        break;
                }

                return ret;
            }
        }
    }


    public class JzTimes
    {
        private int mCutCout = 0;
        private DateTime mCutDateTime = DateTime.Now;
        public DateTime ProcessStartTime = DateTime.Now;
        public JzTimes()
        {

        }
        public JzTimes(int sec)
        {
            ProcessStartTime = ProcessStartTime.AddSeconds(sec);
        }
        public string ProcessDuriation()
        {
            TimeSpan ts =  DateTime.Now.Subtract(ProcessStartTime);
            return ts.TotalSeconds.ToString("0.00") + " sec";
        }
        public static string DuriationString(int Val)
        {
            return String.Format("{0:HH:mm:ss}", DateTime.Now.AddSeconds((double)Val).Subtract(DateTime.Now)).Substring(0, 8);
        }
        public static string DateString
        {
            get
            {
                return DateTime.Now.ToString("yyyy/MM/dd");
            }
        }

        public static string TimeString
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss");
            }
        }
        public static string DateTimeSerialStringFFF
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }
        }
        public static string DateTimeSerialString
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }
        public static string TimeSerialString
        {
            get
            {
                return DateTime.Now.ToString("HHmmss");
            }
        }
        public static string DateSerialString
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd");
            }
        }
        public static int GetWeekNo(DateTime rDateTime)
        {
            CultureInfo culinfo = CultureInfo.CreateSpecificCulture("no");
            Calendar cal = culinfo.Calendar;
            int weekno = cal.GetWeekOfYear(rDateTime, culinfo.DateTimeFormat.CalendarWeekRule, culinfo.DateTimeFormat.FirstDayOfWeek);

            return weekno;

        }
        public static string DateTimeString
        {
            get
            {
                return DateString + " " + TimeString;
            }
        }
        public static string DateAdd(double Days)
        {
            return DateTime.Today.AddDays(Days).ToString("yyyy/MM/dd");
        }
        public static string DateAdd(DateTime rDatetime, double Days)
        {
            return rDatetime.AddDays(Days).ToString("yyyy/MM/dd");
        }

        public static DateTime SecAdd(DateTime rDatetime, double secs)
        {
            return rDatetime.AddSeconds(secs);
        }
        public static string SecAddStr(DateTime rDatetime, double secs)
        {
            return rDatetime.AddSeconds(secs).ToString("HH:mm:ss");
        }
        public static DateTime StrToDateTime(string DateTimeStr)
        {
            return DateTime.Parse(DateTimeStr);
        }
        public static string DateTimeToStr(DateTime DT)
        {
            return String.Format("{0:yyyyMMddhhmmss}", DT);
        }
        public static String DateStr(DateTime rDateTime)
        {
            return rDateTime.ToString("yyyy/MM/dd");
        }
        public static String TimeStr(DateTime rDateTime)
        {
            return rDateTime.ToString("HH:mm:ss");
        }
        public static String TimeStrEx(DateTime rDateTime)
        {
            return rDateTime.ToString("HH:mm") + ":" + rDateTime.ToString("ss").Substring(0, 1) + "0";
        }
        public static void Delay(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public void Cut()
        {
            mCutDateTime = DateTime.Now;
            mCutCout = 0;
        }

        int storems = 0;
        public void Store()
        {
            storems = msDuriation;
        }
        public string StoreSecond()
        {
            string Str = (((double)storems) / 1000).ToString("0.000");

            return Str += " sec";
        }
        public DateTime CutDateTime
        {
            get
            {
                return mCutDateTime;
            }
        }
        public bool Cut(int Count)
        {
            bool ret = true;

            if (mCutCout < Count)
            {
                mCutCout++;
                mCutDateTime = DateTime.Now;
                ret = false;
            }

            return ret;
        }
        public long Duriation(TimeUnitEnum TimeUnit)
        {
            long ltmp = 0;

            switch (TimeUnit)
            {
                case TimeUnitEnum.ms:
                    ltmp = (long)DateTime.Now.Subtract(mCutDateTime).TotalMilliseconds;
                    break;
                case TimeUnitEnum.sec:
                    ltmp = (long)DateTime.Now.Subtract(mCutDateTime).TotalSeconds;
                    break;
                case TimeUnitEnum.min:
                    ltmp = (long)DateTime.Now.Subtract(mCutDateTime).TotalMinutes;
                    break;
                case TimeUnitEnum.hour:
                    ltmp = (long)DateTime.Now.Subtract(mCutDateTime).TotalHours;
                    break;
                case TimeUnitEnum.day:
                    ltmp = (long)DateTime.Now.Subtract(mCutDateTime).TotalDays;
                    break;
            }
            return ltmp;
        }

        public bool IsDirectDefined(int rsec)
        {
            DateTime dt = DateTime.Now;

            return (CutDateTime.AddSeconds(rsec).ToLongTimeString() == dt.ToLongTimeString()
                    || CutDateTime.AddSeconds(rsec + 1) < dt);
        }
        public int msDuriation
        {
            get
            {
                return (int)Duriation(TimeUnitEnum.ms);
            }
        }
        public int secDuriation
        {
            get
            {
                return (int)Math.Abs(Duriation(TimeUnitEnum.sec));
            }
        }
        public float secFloat
        {
            get
            {
                return (float)Math.Abs(Duriation(TimeUnitEnum.ms)) / 1000;
            }
        }
        public int minDuriation
        {
            get
            {
                return (int)Duriation(TimeUnitEnum.min);
            }
        }
    }
}
