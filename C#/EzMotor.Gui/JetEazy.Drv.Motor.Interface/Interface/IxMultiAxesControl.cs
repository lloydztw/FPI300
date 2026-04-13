/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20081201 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

using System;

namespace JetEazy.Drivers.Motor
{
    public interface IxMultiAxesControl : IDisposable
    {
        IDrvMotorAxis[] Axes { get; }
        MotorUnitMode Unit { get; }
        bool IsContinuousMove { get; }
        object Ext { get; }

        bool MoveTo(double[] arrPos, double Vstart = 0, double Vmax = 0, double Tacc = 0, double Tdec = 0);
        bool IsReadyForNextContinuousCmd();
    }
}
