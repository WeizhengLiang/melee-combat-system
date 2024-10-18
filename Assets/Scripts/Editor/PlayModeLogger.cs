using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[InitializeOnLoad]
public class PlayModeLogger
{
    private static string logFilePath = "Assets/PlayModeLog.txt";
    private static StringWriter logWriter;

    static PlayModeLogger()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            StartLogging();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            StopLogging();
        }
    }

    private static void StartLogging()
    {
        logWriter = new StringWriter();
        Application.logMessageReceived += LogCallback;
        Debug.Log("Play Mode started. Logging begins.");
    }

    private static void StopLogging()
    {
        Application.logMessageReceived -= LogCallback;
        SaveLogToFile();
        logWriter.Close();
        logWriter = null;
        Debug.Log("Play Mode ended. Log saved to " + logFilePath);
    }

    private static void LogCallback(string condition, string stackTrace, LogType type)
    {
        logWriter.WriteLine($"[{DateTime.Now}] [{type}] {condition}");
        if (type == LogType.Exception)
        {
            logWriter.WriteLine(stackTrace);
        }
    }

    private static void SaveLogToFile()
    {
        File.WriteAllText(logFilePath, logWriter.ToString());
    }
}