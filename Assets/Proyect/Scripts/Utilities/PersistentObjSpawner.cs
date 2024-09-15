using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Utilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PersistentObjSpawner : MonoBehaviour, IJsonSaveable, ISerializationCallbackReceiver
{
    [SerializeField] List<GameObject> persistentObjectsPref;
    [SerializeField] private int _id = 0;
    bool hasSpawned = false;

    private void Awake()
    {
        TrySpawnObjects();
    }

    private void TrySpawnObjects()
    {
        if (TemporalSaver.TryLoad(_id, out object data))
            hasSpawned = (bool)data;

        if (hasSpawned) return;

        SpawnObjects();

        hasSpawned = true;
        TemporalSaver.Save(_id, true);
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
        execution = SavingExecution.System;
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
