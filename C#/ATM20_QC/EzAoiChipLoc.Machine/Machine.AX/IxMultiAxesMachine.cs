#region AUTHOR
/*
 * <FileName>
 * 
 * Copyright (C) 2023
 * 2023-09-03 revised by LetTian Chang
 * 2012-03-22 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * letian@jeteazy.com
 * lloydz.tw@gmail.com.tw
 * 
 */
#endregion

using JetEazy.Actuactor;
using JetEazy.Drivers.IOCtrl;
using JetEazy.Drivers.Motor;
using System;
using System.Collections.Generic;

namespace AX.Machine
{
    public interface IxMultiAxesMachine : IDevActuator
    {
        //>>> event EventHandler OnMotionCompleted;
        event EventHandler OnTicked;

        int TotalMotorsCount { get; }
        IEnumerable<IDevMotorActuator> IterMotorActuator();
        IEnumerable<IDrvMotorAxis> IterMotor();
        IDevMotorActuator GetMotorAcuator(int axisId);
        IDrvCommonIoCtrl IoCtrl { get; }

        void Home(int axisId = -1);
        void MotorMove(int axisId, double speed);
        void MotorMoveTo(int axisId, double pos, double speed);

        bool SafetyCheckEnabled { get; set; }
        void Load(string iniFileName = null);
        void Save(string iniFileName = null);
    }
}
