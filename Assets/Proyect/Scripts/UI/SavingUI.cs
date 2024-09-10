using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject[] slots;
    [SerializeField] GameObject[] autoSave;
    [SerializeField] GameObject btnAddMore;
    [SerializeField] GameObject btnLoad;

    int curSlots = 0;
    int curAutoSave = 0;
    int curSlot = 1;

    public event Action<int> OnSlotAdded;

    void Start()
    {
        //DontDestroyOnLoad(this);
    }

    public void EnableCurrentSlots(int slotsCount, int autoSaveCount)
    {
        EnableSlots(slotsCount, slots);
        EnableSlots(autoSaveCount, autoSave);
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
        OnSlotAdded?.Invoke(curSlots + 1);
    }

    private void EnableSlots(int count, GameObject[] slots)
    {
        int i = 0;
        foreach (var slot in slots)
        {
            if (i++ < count)
            {
                slot.SetActive(true);
            }
            else
            {
                slot.SetActive(false);
            }
        }
    }
}
