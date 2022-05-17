using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev
{
    public class LogTracer
    {
        public delegate void OnLineAdded(string str);
        public static event OnLineAdded onLineAdded;
        public static List<string> logData = new List<string>();
        public static bool captureLog = true;

        private static bool started;
        


        public static void start()
        {
            if (started)
                return;
            UnityMainThreadDispatcher.init();

            started = true;
            Application.logMessageReceived += onLogMessage;
        }


        private static void onLogMessage(string condition, string stackTrace, LogType type)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            if (!captureLog)
                return;
            add(condition, true);
            if (type != LogType.Log)
                add(stackTrace, false);
        }


        public static void add(string str, bool showTime)
        {
            try
            {
                //var tm = (long) (Time.realtimeSinceStartup * 1000);
                //var str = showTime ? tm + ": " + str_ : str_;
                logData.Add(str);

                onLineAdded?.Invoke(str);
            }
            catch (Exception)
            {
                //Debug.LogWarning(e);
            }

            //if (logging)
            //    addLine(str);
        }
    }
}