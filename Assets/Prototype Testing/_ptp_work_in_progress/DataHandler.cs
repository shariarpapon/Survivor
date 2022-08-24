using System.IO;
using UnityEngine;
using Survivor.Core;

public class DataHandler : MonoBehaviour
{
    public static void WriteData(string data, string dataId) 
    {
        return;
    }

    public static string ReadData(string dataId) 
    {
        return null;
    }

    public static string GetGlobalDataPath(string id) 
    {
        return Application.persistentDataPath + $"gbdata_{id}.svd";
    }
}
