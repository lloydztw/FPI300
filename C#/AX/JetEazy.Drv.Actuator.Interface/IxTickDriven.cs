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


namespace JetEazy.Actuactor
{
    public interface IxTickDriven
    {
        //void Accept(IxTickSource src);
        //int NextDuration { get; }
        //bool Enabled { get; set; }
        void Tick();
    }
}
