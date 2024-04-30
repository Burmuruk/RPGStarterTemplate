using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    public void Save(int id)
    {
        //var items = FindObjectsOfType<ISaveable>();

        //foreach (var item in items)
        //{
        //    item.Save();
        //}
    }

    public void Load(int id)
    {

    }

    public void DisplayFileInfo(int id)
    {

    }
}

public interface ISaveable
{
    bool IsPersistentData { get; }
    int ID { get; }
    object Save();
    void Load(object args);
}
