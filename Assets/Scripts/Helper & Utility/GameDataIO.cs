using UnityEngine;
using Survivor.Core;
using System.IO;

public static class GameDataIO
{
    //NAMING PREFS
    private const string LOCAL_DATA_DIRECTORY_PREFIX = "LOCAL_";
    private const string UNIVERSAL_DATA_DIRECTORY_NAME = "UNIVERSAL";
    private const string DATA_FILE_EXTENSION = ".dat";
    private const string UNIVERSAL_DATA_FILE_PREFIX = "UD_";
    private const string LOCAL_DATA_FILE_PREFIX = "LD_";
    //DIRECTORY PATHS
    private static string UniversalDataDirectory { get { return Application.persistentDataPath + "/" + UNIVERSAL_DATA_DIRECTORY_NAME; } }
    private static string LocalDataDirectory { get { return UniversalDataDirectory + $"{LOCAL_DATA_DIRECTORY_PREFIX}{GameManager.GameIndex}/"; } }
    //FILE PATHS
    private static string UniversalFileName(string id) =>  $"{UNIVERSAL_DATA_FILE_PREFIX}{id}{DATA_FILE_EXTENSION}";
    private static string LocalFileName(string id) => $"{LOCAL_DATA_FILE_PREFIX}{id}{DATA_FILE_EXTENSION}";

    public static void WriteLocalFIle(string id, string data) =>WriteData(LocalDataDirectory, LocalFileName(id), data);
    public static void WriteUniversalFile(string id, string data) => WriteData(UniversalDataDirectory, UniversalFileName(id), data);
    public static string ReadLocalFile(string id) => ReadData(LocalDataDirectory, LocalFileName(id));
    public static string ReadUniversalFile(string id) =>ReadData(UniversalDataDirectory, UniversalFileName(id));

    private static void WriteData(string directoryPath, string fileName, string data) 
    {
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        string filePath = directoryPath + fileName;
        if (File.Exists(filePath)) File.Delete(filePath);
        File.WriteAllText(filePath, data);
    }
    private static string ReadData(string directoryPath, string fileName) 
    {
        string filePath = directoryPath + fileName;
        if (!File.Exists(filePath)) return null;
        return File.ReadAllText(filePath);
    }
}
