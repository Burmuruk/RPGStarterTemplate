using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Burmuruk.Tesis.Stats.BasicStats;

public class SavingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject slotsContainer;
    [SerializeField] GameObject mainMenu;
    [SerializeField] SlotUI[] slots;
    [SerializeField] SlotUI[] autoSaves;
    [SerializeField] GameObject btnAddMore;
    [SerializeField] GameObject btnLoad;

    JsonSavingWrapper savingWrapper;

    int curSlots = 0;

    [Serializable]
    private struct SlotUI
    {
        [SerializeField] GameObject item;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI timePlayed;
        [SerializeField] TextMeshProUGUI membersCount;
        [SerializeField] Image picture;

        public GameObject GameObject { get { return item; } }
        public string Title { get => title.text; set => title.text = value; }
        public string PlayedTime { get => timePlayed.text; set => timePlayed.text = value; }
        public int MembersCount { get => Int32.Parse(membersCount.text); set => membersCount.text = value.ToString(); }
        public Sprite Sprite { get => picture.sprite; set => picture.sprite = value; }
    }

    public event Action<int> OnSlotAdded;

    private void Awake()
    {
        savingWrapper = FindObjectOfType<JsonSavingWrapper>();

        EnableCurrentSlots(savingWrapper.FindAvailableSlots());
    }

    public void ShowSlots()
    {
        EnableCurrentSlots(savingWrapper.FindAvailableSlots());
        slotsContainer.SetActive(!slotsContainer.activeSelf);
    }

    public void LoadSlot(int slot)
    {
        ShowMenu(false);

        savingWrapper.Load(slot);
    }

    public void EnableCurrentSlots(List<(int id, JObject slotData)> slots)
    {
        DisableSlots();
        EnableSlots(slots, out int slotsCount);
        
        curSlots = slotsCount;

        if (slotsCount >= 3)
        {
            btnAddMore.SetActive(false);
        }
        else if (slotsCount > 0)
        {
            btnAddMore.SetActive(true);
            btnLoad.SetActive(true);
        }
        else
        {
            btnLoad.SetActive(false);
        }
    }

    public void AddSlot()
    {
        ShowMenu(false);

        savingWrapper.Load(curSlots + 1);
    }

    private void ShowMenu(bool shouldShow)
    {
        mainMenu.SetActive(shouldShow);
        slotsContainer.SetActive(false);
    }

    private void DisableSlots()
    {
        foreach (var slot in slots)
        {
            slot.GameObject.SetActive(false);
        }

        foreach (var autosave in autoSaves)
        {
            autosave.GameObject.SetActive(false);
        }
    }

    private void EnableSlots(List<(int id, JObject slotData)> slots, out int slotsCount)
    {
        slotsCount = 0;

        foreach (var slot in slots)
        {
            SlotUI curSlot = default;
            if (slot.id < 0)
            {
                curSlot = autoSaves[slot.id * -1];
            }
            else
            {
                curSlot = this.slots[slot.id - 1];
                ++slotsCount;
            }

            curSlot.Title = "Guardado " + slot.id;
            curSlot.PlayedTime = slot.slotData["TimePlayed"].ToString();
            curSlot.MembersCount = slot.slotData["MembersCount"].ToObject<int>();
            curSlot.GameObject.SetActive(true);
        }
    }
}
