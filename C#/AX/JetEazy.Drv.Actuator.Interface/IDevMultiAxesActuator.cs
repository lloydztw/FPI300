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
    using X = QMath.QVector;

    public interface IDevMotionActuator : IDevActuator
    {
        // Position and Speed
        X Pos { get; }
        X Speed { get; }

        // Motor Operations (async)
        void Home();
        void MoveTo(X pos);

        // Safety Check
        bool IsSafeToHome();
        bool IsSafeToMoveTo(X pos);
    }
}

