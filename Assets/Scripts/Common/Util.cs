using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEngine;
using Engine.Utility;

class SpanUtil
{
    static long start_tick=0;

    static public void Start(string tag = null)
    {
        start_tick = DateTime.UtcNow.Ticks;

        if(tag != null)
        {
            Log.Info("Span Start: " + tag);
        }
    }

    static public void Stop(string tag)
    {
        Log.Info("Span Stop, " + tag + ":  " + (DateTime.UtcNow.Ticks - start_tick)/10000);
        start_tick = DateTime.UtcNow.Ticks;
    }
}
