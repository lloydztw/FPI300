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
using System.ComponentModel;

namespace EzAoiChipLocQC.Model
{
    public enum LightChannelEnum : int
    {
        [Description("紅光")]
        Red = 1,
        [Description("白光")]
        White = 2,
        [Description("紅白光")]
        Red_White = 3,
    };

    public class JzLightSettings : JxContainer
    {
        public JxEnum<LightChannelEnum> Channel = new JxEnum<LightChannelEnum>("Channel", "光源通道");
        public JxInt Intensity = JxInt.C255("Intensity", "光源亮度", 255);

        public JzLightSettings() : base("Ligth Control", "光源控制")
        {
            Channel.Value = LightChannelEnum.Red_White;
        }
        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Channel,
                Intensity,
            });
            base.OnBindingSubItems();
        }
    }
}