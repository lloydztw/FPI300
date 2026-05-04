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


namespace EzAoiEmptyTrayInspector.Model
{
    /// <summary>
    /// 空盤 像測參數
    /// </summary>
    public class JxTrayVisionSettings : JxContainer
    {
        public JxTempMatchSettings Match = new JxTempMatchSettings(null, "吸嘴比對設定");
        public JxEnum<MirrorMode> Mirror = new JxEnum<MirrorMode>("Miror", description: "鏡像");
        public JxRotAngleSettings RotAngle = new JxRotAngleSettings();
        public JxBool Inverse = new JxBool("Inverse", "反相處理", false);
        public JxInt OutGridBlocThreshold = JxInt.C255("OG Threshold", "區塊門限", 0);
        public JxBool FindAllFailBlocs = new JxBool("FindAllBlocs", "明確找出所有異常區塊", true);
        public JxInt OutGridBlocMinSize = new JxInt("OutGridBlocMinSize", "外圍區塊最小邊長 (pixel)", 550);

        public JxTrayVisionSettings()
        {
            Name = $"Tray Vision";
            Description = $"空盤 像測參數";
        }
        public override void OnBindingSubItems()
        {
            //>>> 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                //Mirror,       // 保留擴充
                //RotAngle,     // 保留擴充
                Match,
                Inverse,
                OutGridBlocThreshold,
                FindAllFailBlocs,
                OutGridBlocMinSize,
            });
            base.OnBindingSubItems();
        }
    }
}
