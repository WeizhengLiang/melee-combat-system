using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// Automatically logs play mode messages to a file for debugging and analysis
/// </summary>
[InitializeOnLoad]
public class PlayModeLogger
{
    private static string logFilePath = "Assets/PlayModeLog.txt";
    private static StringWriter logWriter;

    /// <summary>
    /// Initializes the logger and subscribes to play mode state changes
    /// </summary>
    static PlayModeLogger()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    /// <summary>
    /// Handles play mode state changes and manages logging accordingly
    /// </summary>
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

    /// <summary>
    /// Starts the logging process when entering play mode
    /// </summary>
    private static void StartLogging()
    {
        logWriter = new StringWriter();
        Application.logMessageReceived += LogCallback;
        Debug.Log("Play Mode started. Logging begins.");
    }

    /// <summary>
    /// Stops the logging process and saves the log file when exiting play mode
    /// </summary>
    private static void StopLogging()
    {
        Application.logMessageReceived -= LogCallback;
        SaveLogToFile();
        logWriter.Close();
        logWriter = null;
        Debug.Log("Play Mode ended. Log saved to " + logFilePath);
    }

    /// <summary>
    /// Callback for handling log messages and writing them to the log file
    /// </summary>
    private static void LogCallback(string condition, string stackTrace, LogType type)
    {
        logWriter.WriteLine($"[{DateTime.Now}] [{type}] {condition}");
        if (type == LogType.Exception)
        {
            logWriter.WriteLine(stackTrace);
        }
    }

    /// <summary>
    /// Saves the accumulated log messages to a file
    /// </summary>
    private static void SaveLogToFile()
    {
        File.WriteAllText(logFilePath, logWriter.ToString());
    }
}