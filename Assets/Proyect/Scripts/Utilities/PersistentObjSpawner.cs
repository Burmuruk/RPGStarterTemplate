using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PersistentObjSpawner : MonoBehaviour
{
    [SerializeField] List<GameObject> persistentObjectsPref;
    [SerializeField] private int _id = 0;
    bool hasSpawned = false;

    public int Id
    {
        get
        {
            if (_id == 0)
                _id = GetHashCode();

            return _id;
        }
    }

    public void TrySpawnObjects()
    {
        if (TemporalSaver.TryLoad(Id, out object data))
            hasSpawned = (bool)data;

        if (hasSpawned) return;

        SpawnObjects();

        hasSpawned = true;
        TemporalSaver.Save(Id, true);
    }

    private void SpawnObjects()
    {
        foreach (var obj in persistentObjectsPref)
        {
            var newObj = Instantiate(obj);
            DontDestroyOnLoad(newObj);
        }
    }

    public JToken CaptureAsJToken(out SavingExecution execution)
    {
        execution = SavingExecution.Admin;
        JObject state = new JObject();

        state["HasSpawned"] = hasSpawned;

        return state;
    }

    public void RestoreFromJToken(JToken state)
    {
        hasSpawned = (state as JObject)["HasSpawned"].ToObject<bool>();

        TrySpawnObjects();
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (_id == 0)
            _id = GetHashCode();

        //SerializedObject serializedObject = new(this);
        //SerializedProperty property = serializedObject.FindProperty("_id");
        //property.intValue = _id;
        //serializedObject.ApplyModifiedProperties(); 
#endif
    }

    public void OnAfterDeserialize() { }
}
