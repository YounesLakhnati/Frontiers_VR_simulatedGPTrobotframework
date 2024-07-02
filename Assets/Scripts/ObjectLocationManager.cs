using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLocationManager : MonoBehaviour
{
    public static ObjectLocationManager Instance { get; private set; }
    public List<GameObject> allObjects;
    public List<GameObject> locations;
    public List<GameObject> grabbables;
    public List<GameObject> lockedObjects;
    private Dictionary<string, Transform> objectLocations = new Dictionary<string, Transform>();

    private Dictionary<string, Vector3> originalPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Quaternion> originalRotations = new Dictionary<string, Quaternion>();

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
        allObjects.AddRange(locations);
        allObjects.AddRange(grabbables);
        allObjects.AddRange(lockedObjects);

        foreach (GameObject obj in allObjects)
        {
            RegisterObject(obj.name, obj.transform);
        }

        foreach (GameObject obj in allObjects)
        {
            Position positionScript = obj.GetComponent<Position>();

            if (positionScript == null)
            {
                Debug.LogWarning($"Object {obj.name} does not have a Position component. Adding one.");
                positionScript = obj.AddComponent<Position>();
            }
        }
    }

    public void RegisterObject(string name, Transform transform)
    {
        if (objectLocations.ContainsKey(name))
        {
            Debug.LogWarning($"Object with name {name} is already registered. Overwriting existing entry.");
        }

        objectLocations[name] = transform;

        // Assume each object has a script with an OnTransformChanged event we can subscribe to
        var objectScript = transform.gameObject.GetComponent<Position>();
        if (objectScript != null)
        {
            objectScript.OnTransformChanged += UpdateObjectTransform;
        }

        originalPositions[name] = transform.position;
        originalRotations[name] = transform.rotation;
    }

    public void UnregisterObject(string name)
    {
        if (objectLocations.ContainsKey(name))
        {
            var objectScript = objectLocations[name].gameObject.GetComponent<Position>();
            if (objectScript != null)
            {
                objectScript.OnTransformChanged -= UpdateObjectTransform;
            }

            objectLocations.Remove(name);
        }
        else
        {
            Debug.LogWarning($"No object with name {name} is registered.");
        }
    }

    public Vector3 GetObjectTransform(string name)
    {
        if (name == "User_pointing_location")
        {
            return AgentController.Instance.GetPointPosition().point;
        }
        else if (!objectLocations.ContainsKey(name))
        {
            Debug.LogError($"No object with name {name} is registered.");
            return Vector3.zero;
        } else
        {
            return objectLocations[name].position;
        } 
    }

    private void UpdateObjectTransform(GameObject obj)
    {
        if (objectLocations.ContainsKey(obj.name))
        {
            objectLocations[obj.name] = obj.transform;
        }
    }

    public List<string> GetLocations()
    {
        List<string> locationList = new List<string>(objectLocations.Keys);
        //locationList.Add("User_pointing_location"); //on/offswitch fuer user_pointing_location

        return locationList;
    }

    //public List<string> GetLocations()
    //{
    //    List<string> list = new List<string>();
    //    foreach (GameObject obj in locations)
    //    {
    //        list.Add(obj.name);
    //    }
    //    return list;
    //}

    public List<string> GetGrabbables()
    {
        List<string> list = new List<string>();
        foreach (GameObject obj in grabbables)
        {
            list.Add(obj.name);
        }
        return list;
    }

    public List<string> GetLockedObjects()
    {
        List<string> list = new List<string>();
        foreach (GameObject obj in lockedObjects)
        {
            list.Add(obj.name);
        }
        return list;
    }

    public void ResetPositions()
    {
        foreach (var entry in originalPositions)
        {
            if (objectLocations.ContainsKey(entry.Key))
            {
                objectLocations[entry.Key].position = entry.Value;
            }
            else
            {
                Debug.LogWarning($"Object {entry.Key} was not found in the scene.");
            }
        }
        foreach (var entry in originalRotations)
        {
            if (objectLocations.ContainsKey(entry.Key))
            {
                objectLocations[entry.Key].rotation = entry.Value;
            }
            else
            {
                Debug.LogWarning($"Object {entry.Key} was not found in the scene.");
            }
        }
    }
}