#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using Amir.StateMachine;

namespace JetEazy.Actuactor
{
    public interface IDevActuator : IStateMachine, IxTickDriven
    {
        int ID { get; }

        string Name { get; }

        /// <summary>
        /// 準備就緒
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// 移動中
        /// </summary>
        bool IsMoving { get; }
        
        /// <summary>
        /// 一般異常 (可復位)
        /// </summary>
        bool IsException { get; }

        /// <summary>
        /// 嚴重異常 (必須重開機)
        /// </summary>
        bool IsFatalError { get; }

        /// <summary>
        /// 重置
        /// </summary>
        /// void Reset();

        /// <summary>
        /// 正常停止
        /// </summary>
        void Stop();

        /// <summary>
        /// 立即強制停止
        /// </summary>
        void EmgStop();

        /// <summary>
        /// Tick Driven
        /// </summary>
        //void Tick();
    }
}
