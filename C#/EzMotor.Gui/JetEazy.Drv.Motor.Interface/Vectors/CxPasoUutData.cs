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


namespace Paso
{
    public class CxPasoUutData
    {
        public PasoCasetteTowerID HostCasetteTowerID;
        public int HostCasetteCellIndex;
        public PasoUutStatus Status;
        public object Tag;

        public CxPasoUutData()
        {
            HostCasetteTowerID = PasoCasetteTowerID.IN;
            HostCasetteCellIndex = -1;
            Status = PasoUutStatus.Unknown;
        }
        public void Clear()
        {
            HostCasetteTowerID = PasoCasetteTowerID.IN;
            HostCasetteCellIndex = -1;
            Status = PasoUutStatus.Empty;
        }
        public virtual CxPasoUutData Clone()
        {
            CxPasoUutData obj = new CxPasoUutData();
            obj.HostCasetteTowerID = HostCasetteTowerID;
            obj.HostCasetteCellIndex = HostCasetteCellIndex;
            obj.Status = Status;
            obj.Tag = Tag;
            return obj;
        }
    }
}
