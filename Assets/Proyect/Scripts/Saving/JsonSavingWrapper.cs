using Burmuruk.Tesis.Stats;
using System.Collections;
using UnityEngine;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "miGuardado-";
        const string defaultAutoSaveFile = "miAutoGuardado-";

        //private IEnumerator Start()
        //{
        //    yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
        //}

        public void Save(int slot)
        {
            if (slot == 0)
            {
                int id = System.DateTime.Now.Second + System.DateTime.Now.Hour + System.DateTime.Now.Year;
                GetComponent<JsonSavingSystem>().Save(defaultAutoSaveFile + id);
            }
            else
            {
                GetComponent<JsonSavingSystem>().Save(defaultSaveFile + slot);
            }
        }

        public void Load(int slot)
        {
            FindObjectOfType<BuffsManager>().RemoveAllBuffs();

            if (slot == 0)
            {
                GetComponent<JsonSavingSystem>().Load(defaultAutoSaveFile + slot); 
            }
            else
            {
                GetComponent<JsonSavingSystem>().Load(defaultSaveFile + slot);
            }
        }

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                Save(1);
            }

            if (Input.GetKeyUp(KeyCode.L))
            {
                Load(1);
            }
        }
    }
}