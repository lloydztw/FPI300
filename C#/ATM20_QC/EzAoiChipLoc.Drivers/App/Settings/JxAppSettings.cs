#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LeTian.JxProps;

namespace EzAoiChipLocQC
{
    /// <summary>
    /// App 全域參數設定
    /// </summary>
    public class JxAppSettings : JxContainer
    {
        public JxBool LoginEnabled = new JxBool("Login Enabled", description: "使用登入帳號");
        public JxBool OutputResultImageFile = new JxBool("Output Result Image", description: "生成檢測結果圖檔");
        public JxPathFile OutputDataPath = new JxPathFile("Output Data Path", Global.APP_PATH.DumpPath, description: "輸出資料夾", isPathOnly: true);
        public JxVisionSource VisionSrc0 = new JxVisionSource(0);
        public JxVisionSource VisionSrc1 = new JxVisionSource(1);
        public JxVisionSource GetVisionSrc(int id)
        {
            if (id == 0) return VisionSrc0;
            else if (id == 1) return VisionSrc1;
            else return null;
        }

        public JxAppSettings() : base("AppSetings", "APP系統設定")
        {
            LoginEnabled.Value = true;
        }

        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                LoginEnabled,
                OutputResultImageFile,
                OutputDataPath,
                VisionSrc0,
                VisionSrc1,
            });
            base.OnBindingSubItems();
        }
        public override string NormalizeFile(string fileName)
        {
            //指定默認的保存檔案名稱
            if (string.IsNullOrEmpty(fileName))
                fileName = Global.APP_PATH.IniFile;
            return fileName;
        }
    }


    public class JxVisionSource : JxContainer
    {
        public JxPathFile ImgFile = new JxPathFile("ImgFile", description: "影像來源檔");

        public JxVisionSource(int id) : base($"VisionSrc{id + 1}", $"影像來源 #{id + 1} (隱藏)")
        {
        }
        public JxVisionSource()
        {
        }

        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                ImgFile,
                //Mirror,
            });
            base.OnBindingSubItems();
        }
    }
}
