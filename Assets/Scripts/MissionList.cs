using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissionList : MonoBehaviour
{
    public Text taskText;
    public static MissionList Instance { get; private set; }
    private List<Mission> tasks = new List<Mission>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        tasks.Add(new Mission { description = "Place candle on the table", isCompleted = false });
        tasks.Add(new Mission { description = "Place plates on the table", isCompleted = false });

        RefreshQuestLog();
    }

    public void UpdateTask(int taskIndex, bool isCompleted)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            var task = tasks[taskIndex];
            task.isCompleted = isCompleted;
            tasks[taskIndex] = task;

            RefreshQuestLog();
        }
    }

    private void RefreshQuestLog()
    {
        string logText = "";
        foreach (var task in tasks)
        {
            logText += task.description + " [" + (task.isCompleted ? "X" : " ") + "]\n";
        }

        taskText.text = logText;
    }
}