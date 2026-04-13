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
    public interface IDrvMotorIo
    {
        bool ServoON { get; set; }

        bool IsEmergencyButtonPressed { get; }

        bool IsBrakeON { get; }

        /// <summary>
        /// 剎車使用 function 型態, 比較強韌
        /// </summary>
        bool LockBrake(int checkLoops = 0);

        /// <summary>
        /// 剎車使用 function 型態, 比較強韌
        /// </summary>
        bool ReleaseBrake(int checkLoops = 0);

    }
}
