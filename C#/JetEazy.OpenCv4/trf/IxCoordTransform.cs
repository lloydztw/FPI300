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

using System;
using System.Drawing;
using JetEazy.QMath;

namespace JetEazy.Aoi.TransformD
{
    public interface IxCoordTransform : ICloneable, IDisposable
    {
        QVector ToLocal(QVector ptWorld, QVector vMotor = null);
        QVector ToWorld(QVector ptLocal, QVector vMotor = null);
        PointF ToLocal(PointF ptWorld, QVector vMotor = null);
        PointF ToWorld(PointF ptLocal, QVector vMotor = null);
        RectangleF ToLocal(RectangleF ptWorld, QVector vMotor = null);
        RectangleF ToWorld(RectangleF ptLocal, QVector vMotor = null);

        void GetCalibrationPoints(out QVector[,] pointsInView, out QVector[,] pointsInWorld);
        void SetCalibrationPoints(QVector[,] pointsInView, QVector[,] pointsInWorld);
        bool BuildTransformFormula();

        void Load(string strIniFileName);
        void Save(string strIniFileName);
    }
}
