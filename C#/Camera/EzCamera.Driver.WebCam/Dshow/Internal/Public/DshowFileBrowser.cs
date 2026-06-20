#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace JetEazy.DShow.Api
{
    public class DshowFileBrowser
    {
        public static string OpenVideoFileDialog(string path)
        {
            using (var dlg = new OpenFileDialog())
            {
                // 設定對話框的標題
                dlg.Title = "選擇一個 MKV 或 MP4 影片檔案";

                // 設定篩選器 (Filter)。格式為 "描述|副檔名"
                // 可以有多個篩選器，用 '|' 分隔。
                // 多個副檔名用 ';' 分隔。
                dlg.Filter = "MKV 影片檔案 (*.mkv)|*.mkv|MP4 影片檔案 (*.mp4)|*.mp4|所有檔案 (*.*)|*.*";

                // 設定預設的篩選器索引 (1-based):
                //  1 代表 "MKV 影片檔案 (*.mkv)"，
                //  2 代表 "MP4 影片檔案 (*.mp4)"，
                //  3 代表 "所有檔案 (*.*)"。
                dlg.FilterIndex = 1;

                // 設定是否在返回前恢復當前目錄
                dlg.RestoreDirectory = false;

                // 設定是否允許選擇多個檔案 (這裡我們只需要一個)
                dlg.Multiselect = false;

                // 可以設定初始目錄，例如使用者文件目錄
                if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
                {
                    dlg.InitialDirectory = path;
                }

                // 顯示檔案選擇對話框
                var result = dlg.ShowDialog();

                // 檢查使用者是否點擊了 "開啟" 按鈕
                if (result == DialogResult.OK)
                {
                    // 獲取選擇的檔案路徑
                    //string selectedFilePath = dlg.FileName;

                    // 在這裡你可以對 selectedFilePath 進行進一步處理
                    //MessageBox.Show($"你選擇的 MP4 檔案是:\n{selectedFilePath}", "檔案已選擇", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 範例：將檔案路徑顯示在一個 Label 或 TextBox 中
                    // 如果你有一個名為 'lblSelectedFile' 的 Label:
                    // lblSelectedFile.Text = selectedFilePath;

                    return dlg.FileName;
                }
            }
            return null;
        }
    }
}
