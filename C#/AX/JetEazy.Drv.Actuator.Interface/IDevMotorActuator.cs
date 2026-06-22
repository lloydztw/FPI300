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


using JetEazy.Drivers.Motor;

namespace JetEazy.Actuactor
{
    public interface IDevMotorActuator : IDevActuator
    {
        int LastErrorCode { get; }

        IDrvMotorAxis Motor { get; }

        /// <summary>
        /// Current Position
        /// </summary>
        double Pos { get; }

        /// <summary>
        /// Current Speed
        /// </summary>
        double Speed { get; }

        void Home();
        void Move(double speed);
        void MoveTo(double pos, double speed);

        // Safety Check
        //>>> bool IsSafeToMove(double speed);
        //>>> bool IsSafeToMoveTo(double pos);
    }
}

