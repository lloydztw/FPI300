namespace JetEazy.Interface
{
    /// <summary>
    /// 所有需要用 Timer_Tick 的 class 都繼承自此介面
    /// </summary>
    public interface IxTickable
    {
        void Tick();
    }
}
