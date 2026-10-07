using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traveller106;

namespace NeedleX.ProcessSpace
{
    public class FocusProcess : BaseProcess
    {
        #region ACCESS_TO_OTHER_PROCESSES
        System.Diagnostics.Stopwatch m_Stopwatch = new System.Diagnostics.Stopwatch();
        #endregion

        #region SINGLETON
        static FocusProcess _singleton = null;
        private FocusProcess()
        {
        }
        #endregion

        public static FocusProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new FocusProcess();
                return _singleton;
            }
        }

        public override void Tick()
        {
            if (!IsValidPlcScanned())
                return;

            var Process = this;

            //if (Process.IsOn)
            //{
            //    switch (Process.ID)
            //    {
            //        case 5:

            //            Process.TimeUnit = TimeUnitEnum.ms;
            //            Process.NextDuriation = 100;
            //            Process.ID = 10;

            //            focusList.Clear();

            //            d1 = 112.15f;
            //            d2 = 112.25f;
            //            offset = 0.01f;

            //            float.TryParse(txt1.Text.Trim(), out d1);
            //            float.TryParse(txt2.Text.Trim(), out d2);
            //            float.TryParse(txtoffset.Text.Trim(), out offset);

            //            CommonLogClass.Instance.LogMessage("开始位置=" + d1.ToString());
            //            CommonLogClass.Instance.LogMessage("结束位置=" + d2.ToString());
            //            CommonLogClass.Instance.LogMessage("间隔=" + offset.ToString());

            //            m_filenamepath = Traveller106.Universal.DEBUGRESULTPATH + "\\" + JzTimes.DateTimeSerialStringFFF;
            //            if (!System.IO.Directory.Exists(m_filenamepath))
            //                System.IO.Directory.CreateDirectory(m_filenamepath);
            //            //d1 = float.Parse(txt1.Text.Trim());
            //            //d2 = float.Parse(txt2.Text.Trim());
            //            //offset = float.Parse(txtoffset.Text.Trim());


            //            break;

            //        case 10:
            //            if (Process.IsTimeup)
            //            {
            //                if (d1 < d2)
            //                {
            //                    GetAxis(2).Go(d1, 0);
            //                    //d1 -= offset;
            //                    //CommonLogClass.Instance.LogMessage("当前位置=" + d1.ToString());
            //                    Process.NextDuriation = 300;
            //                    Process.ID = 15;
            //                }
            //                else
            //                {
            //                    Process.NextDuriation = 300;
            //                    Process.ID = 20;
            //                }

            //            }
            //            break;
            //        case 15:
            //            if (Process.IsTimeup)
            //            {
            //                float _posCur = (float)GetAxis(2).GetPos();
            //                if (IsInRange(_posCur, d1, 0.0085f) && GetAxis(2).IsOK)
            //                {
            //                    Process.NextDuriation = 100;
            //                    Process.ID = 10;

            //                    CamHeight.Snap();
            //                    Bitmap bmp = new Bitmap(CamHeight.GetSnap());

            //                    Mat source = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            //                    double _score = GetImgQualityScore(source, cboMode.SelectedIndex);
            //                    DrawText(bmp, $"(Method:{cboMode.Text.Trim()}) Score:{_score.ToString("0.000")}");
            //                    pictureBox1.Image = bmp;

            //                    bmp.Save(m_filenamepath + "\\pos_" + d1.ToString("0.0000") + ".png",
            //                        System.Drawing.Imaging.ImageFormat.Png);

            //                    CommonLogClass.Instance.LogMessage("位置=" + d1.ToString());
            //                    CommonLogClass.Instance.LogMessage($"(Method:{cboMode.Text.Trim()}) Score:{_score.ToString("0.000")}");
            //                    focusItemClass focus = new focusItemClass();
            //                    focus.fCurrentPos = d1;
            //                    focus.dScore = _score;
            //                    focusList.Add(focus);

            //                    //bmp.Dispose();
            //                    source.Dispose();

            //                    d1 += offset;
            //                }

            //            }
            //            break;
            //        case 20:
            //            if (Process.IsTimeup)
            //            {
            //                if (GetAxis(2).IsOK)
            //                {
            //                    //Process.Stop();
            //                    focusList.Sort((item1, item2) => { return item1.dScore > item2.dScore ? -1 : 1; });
            //                    if (focusList.Count > 0)
            //                    {
            //                        GetAxis(2).Go(focusList[0].fCurrentPos, 0);
            //                        CommonLogClass.Instance.LogMessage($"(Method:{cboMode.Text.Trim()}) " +
            //                            $"(Current Pos:{focusList[0].fCurrentPos.ToString("0.000")}) " +
            //                            $"Max Score:{focusList[0].dScore.ToString("0.000")}");

            //                        Process.NextDuriation = 300;
            //                        Process.ID = 25;
            //                    }
            //                    else
            //                    {
            //                        Process.Stop();
            //                    }
            //                }
            //            }
            //            break;
            //        case 25:
            //            if (Process.IsTimeup)
            //            {
            //                float _posCur = (float)GetAxis(2).GetPos();
            //                if (IsInRange(_posCur, focusList[0].fCurrentPos, 0.0085f) && GetAxis(2).IsOK)
            //                {
            //                    Process.Stop();

            //                    m_Running = false;

            //                    CamHeight.Snap();
            //                    Bitmap bmp = new Bitmap(CamHeight.GetSnap());
            //                    //DrawText(bmp, $"(Method:{cboMode.Text.Trim()}) Score:{_score.ToString("0.000")}");
            //                    pictureBox1.Image = bmp;
            //                }
            //            }
            //            break;
            //    }
            //}
        }
    }
}
