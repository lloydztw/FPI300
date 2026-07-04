using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Traveller.Sim
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // 1. 定義目標程式與工作目錄路徑
                // 使用 AppDomain.CurrentDomain.BaseDirectory 可以讓這個啟動器放哪裡都能跑
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string targetExe = Path.Combine(baseDir, "FPI30AOIX3.exe");

                // 檢查主程式是否存在，避免閃退找不到原因
                if (!File.Exists(targetExe))
                {
                    MessageBox.Show($"Can not find： {targetExe}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. 設定啟動參數
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = targetExe;
                startInfo.Arguments = "SIM";                        // 帶入你的 SIM 參數
                startInfo.WorkingDirectory = baseDir;               // 設定工作目錄 (相當於 cd ...)

                // 3. 關鍵設定：完全隱藏黑色視窗
                startInfo.UseShellExecute = false;                  // 必須為 false 才能隱藏視窗
                startInfo.CreateNoWindow = true;                    // 不建立新視窗
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;  // 隱藏視窗

                // 4. 啟動程式
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Simulation Failed To Start：\n\r{ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
