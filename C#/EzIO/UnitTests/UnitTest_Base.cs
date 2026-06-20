#region AUTHOR
/*
 * EzIO UnitTests
 * Copyright (C) 2026
 * 2026-04-05 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System;
using System.Collections.Generic;

namespace EzIO.UnitTests
{
    public class UnitTest_Base
    {
        protected Random _rnd = new Random();

        protected static bool FATEK_IS_SIM = true;
        protected static int FATEK_COM_PORT = 3;

        protected static bool HCFA_IS_SIM = true;
        protected static int HCFA_IP_PORT = 502;

        public static void _TRACE(string msg)
        {
            System.Console.WriteLine(msg);
            //System.Diagnostics.Trace.WriteLine(msg);
        }
        public static void _TRACE(IAddress addr, bool showDual = false)
        {
            if (addr != null)
            {
                if (showDual)
                    _TRACE($"{addr} ({addr.ToCommString()})");
                else
                    _TRACE(addr.ToCommString());
            }
        }
        public static void _TRACE(IoPoint ioPoint)
        {
            if (ioPoint == null)
                return;
            if (ioPoint.Bits == 1)
            {
                string text = ioPoint.Inverted ? "~" : "";
                text = $"{text}{ioPoint.KeyName} == {ioPoint.Data} ({(ioPoint.IsOn ? "ON" : "OFF")})";
                _TRACE(text);
            }
            else
            {
                _TRACE($"{ioPoint.KeyName} == {ioPoint.Data}");
            }
        }
        public static void _TRACE(IEnumerable<IoPoint> ioPoints)
        {
            foreach (IoPoint ioPoint in ioPoints)
            {
                _TRACE(ioPoint);
            }
        }
        public static void _TRACE(IoMemoryBank bank)
        {
            if (bank != null)
                _TRACE($"Bank({bank.AddressBase}, span={bank.Span}, actual={bank.ActualNumber})");
        }
    }
}
