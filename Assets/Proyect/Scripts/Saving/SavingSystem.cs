using Burmuruk.Tesis.Movement.PathFindig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    GeneralInfo generalInfo;

    struct GeneralInfo
    {
        public readonly int ID;
        string location;
        int playersCount;
        float timePlaying;

        public GeneralInfo(int id)
        {
            ID = 0;
            location = "";
            playersCount = 0;
            timePlaying = 0;
        }
    }

    public void Save(string saveFile)
    {
        string path = GetPathFromSaveFile(saveFile);

        using (FileStream stream = File.Open(path, FileMode.Create))
        {
            byte[] bytes = Encoding.UTF8.GetBytes("¡Hola Mundo!");
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public void Load(string saveFile)
    {
        string path = GetPathFromSaveFile(saveFile);

        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
        }
    }

    private string GetPathFromSaveFile(string saveFile)
    {
        //return Path.Combine( Application.persistentDataPath, saveFile + ".sav");
        return null;
    }

    public void GetFileInfo(int id)
    {

    }
}

public interface ISaveable
{
    int ID { get; }
    object Save();
    void Load(object args);
}
