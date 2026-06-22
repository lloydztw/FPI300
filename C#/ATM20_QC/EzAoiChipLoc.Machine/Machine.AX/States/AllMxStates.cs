/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2012/03/22 The class is created by LeTian Chang
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

namespace AX.Machine.States
{
    public class MxStates
    {
        public MxState Init = new S_Init();
        public MxState Ready = new S_Ready();
        public MxState MotorHome = new S_MotorMoving("Home");
        public MxState MotorMove = new S_MotorMoving("Move");
        public MxState MotorMoveTo = new S_MotorMoving("MoveTo");
        public MxState MultiMoveTo = new S_MotorMoving("MultiMoveTo");
        public MxState ContiMove = new S_MotorMoving("ContiMove");
        public MxState MotorRecover = new S_MotorMoving("Recovery");
        public MxState MotorError = new S_MotorError();

        // Singleton
        public readonly static MxStates Instance = new MxStates();
    }
}
