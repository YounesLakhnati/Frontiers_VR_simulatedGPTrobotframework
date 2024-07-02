using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogEntry 
{
    public Role Role { get; set; }
    public string Message { get; set; }
    public string TimeStamp { get; set; }

    public LogEntry(Role role, string message)
    {
        Role = role;
        Message = message;
        TimeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
