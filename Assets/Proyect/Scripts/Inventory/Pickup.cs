using System.Collections;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] GameObject item;

        public int ID { get; set; }
        public GameObject Prefab { get => item; }
    }
}