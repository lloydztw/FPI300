using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("LaserAlignDX")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LaserAlignDX")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("268ec7ef-0e73-4d58-88c9-5ed04e8b8645")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
//可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
//通过使用 "*"，如下所示:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("2.6.1.1")]
[assembly: AssemblyFileVersion("2.6.1.1")]


/*
 * 待办
 * 1.AOI检测功能待加
 * 2.尺寸功能待加
 * 3.飞拍的功能测试待观察
 * 4.整盘识别后根据真空吸嘴重合度判断产品是否可吸
 * 
 * 20250724
 * 1.隐藏一些不必要的控件
 * 2.加入每个属性的注释
 * 3.打包目前的程序
 * 
 * 
 * 20250711
 * 1.AOI检测功能待加
 * 2.尺寸功能待加
 * 3.飞拍的功能测试待观察
 * 4.整盘识别后根据真空吸嘴重合度判断产品是否可吸
 * 
 * 
 * 20250625
 * 1.加入图片格式转换位8bit
 * 2.加入模板框可以单个删除功能
 * 
 * 
 * 20250617
 * 1.加入Strip图片结果存图及单机事件
 * 
 * 
 * 20250510
 * 1.加入mapping设定检测与不检测
 * 2.测试有判断结果
 * 
 * 
 * 20250415
 * 1.切图加快 用AU的部分功能
 * 2.V3小颗粒定位用HIK 还有外扩功能
 * 
 * 20250310
 * 1.加入DPS灯的控制
 * 2.板边码加入ZIXING读取
 * 3.目前只有V9显示的框直接画到DISPLAY
 * 
 * 
 * 20250228
 * 1.加入板边码读取
 * 2.显示的方框用DS控件
 * 
 * 
 * 20241125
 * 1.加入参数中宽度高度尺寸卡控功能可设置上下限及标准值
 * 2.加入设定中四边判断直角的度数公差
 * 3.优化计算的时间
 * 4.优化自动测试中抓图超时问题
 * 
 * 
 * 20241121
 * 1.加入V9全域判断方向
 * 
 * 
 * 20241024
 * 1.解决切换参数的问题
 * 2.加入抓边模式可任意切换
 * 
 * 
 * 20240927
 * 1.加入tcp通讯 切换参数和mapping
 * 2.加入定点画图
 * 
 * 
 * 20240925
 * 1.加入灯光控制
 * 2.优化计算时间
 * 
 */
