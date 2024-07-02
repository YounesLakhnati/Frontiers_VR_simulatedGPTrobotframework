using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class Log : MonoBehaviour
{
    private List<LogEntry> _logEntries;
    public InputActionReference exportAction;
    public AudioSource source;

    private void Awake()
    {
        exportAction.action.performed += ExportLog;
    }

    private void OnDestroy()
    {
        exportAction.action.performed -= ExportLog;
    }

    public Log()
    {
        _logEntries = new List<LogEntry>();
    }

    public void AddEntry(Role role, string message)
    {
        _logEntries.Add(new LogEntry(role, message));
    }

    public string GetLog()
    {
        StringBuilder logBuilder = new StringBuilder();
        foreach (var logEntry in _logEntries)
        {
            logBuilder.AppendLine($"[{logEntry.TimeStamp}] {logEntry.Role}: {logEntry.Message}");
        }
        return logBuilder.ToString();
    }

    public string GetLogByRole(Role role)
    {
        StringBuilder logBuilder = new StringBuilder();
        foreach (var logEntry in _logEntries)
        {
            if (logEntry.Role == role)
            {
                logBuilder.AppendLine($"[{logEntry.TimeStamp}] {logEntry.Role}: {logEntry.Message}");
            }
        }
        return logBuilder.ToString();
    }

    public string GetLastSentencesUpToUserRequest()
    {
        StringBuilder logBuilder = new StringBuilder();

        for (int i = _logEntries.Count - 1; i >= 0; i--)
        {
            var logEntry = _logEntries[i];
            logBuilder.Insert(0, $"[{logEntry.TimeStamp}] {logEntry.Role}: {logEntry.Message}\n");

            // If we encounter a user message, we stop
            if (logEntry.Role == Role.User)
            {
                break;
            }
        }

        return logBuilder.ToString();
    }

    public void ExportLog(InputAction.CallbackContext ctx)
    {
        StringBuilder logBuilder = new StringBuilder();
        foreach (var logEntry in _logEntries)
        {
            logBuilder.AppendLine($"[{logEntry.TimeStamp}] {logEntry.Role}: {logEntry.Message}");
        }
        string dateTimeString = DateTime.Now.ToString("ddMMyyyy_HHmmss");
        string fileName = dateTimeString + "log.txt";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, logBuilder.ToString());
        source.Play();
    }    
}