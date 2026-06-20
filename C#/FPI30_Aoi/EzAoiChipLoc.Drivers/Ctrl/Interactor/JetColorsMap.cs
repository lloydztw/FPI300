using OpenCvSharp;
using System;
using System.Drawing;


namespace EzDualMatch
{
    public class JetColorsMap
    {
        #region PRIVATE_DATA
        Color[] _colors;
        #endregion

        #region PRIVATE_STATIC_FUNCTION
        static Color GetJetColor(int index, int totalNumber)
        {
            // 將 index 映射到 0 到 255 的範圍
            int grayValue = (int)((index / (double)(totalNumber - 1)) * 255);
            grayValue = Math.Min(grayValue, 255);  // 確保值在 0 到 255 之間

            // 建立一個單像素的灰階圖像
            using (Mat grayMat = new Mat(1, 1, MatType.CV_8UC1, new Scalar(grayValue)))

            // 應用 Jet 色彩映射
            using (Mat colorMat = new Mat())
            {
                Cv2.ApplyColorMap(grayMat, colorMat, ColormapTypes.Jet);

                // 取得對應的 BGR 顏色
                Vec3b bgrColor = colorMat.Get<Vec3b>(0, 0);

                // 將 BGR 轉換為 RGB 並回傳作為 Color 結構
                return Color.FromArgb(bgrColor.Item2, bgrColor.Item1, bgrColor.Item0);
            }
        }
        #endregion

        public JetColorsMap(int totalColors)
        {
            _colors = new Color[totalColors];
            for(int i = 0; i < totalColors; i++)
                _colors[i] = GetJetColor(i, totalColors);
        }
        public int TotalColors
        {
            get => _colors.Length;
        }
        public Color this[int idx]
        {
            get { return _colors[idx]; }
        }
    }
}
