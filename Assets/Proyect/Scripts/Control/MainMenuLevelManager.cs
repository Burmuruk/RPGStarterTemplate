using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MainMenuLevelManager : MonoBehaviour
{
    [SerializeField] GameObject slotsContainer;
    [SerializeField] GameObject mainMenu;
    JsonSavingWrapper savingWrapper;
    SavingUI savingUI;

    private void Awake()
    {
        savingWrapper = FindObjectOfType<JsonSavingWrapper>();
        savingUI = FindObjectOfType<SavingUI>();
        savingUI.OnSlotAdded += AddSlot;

        savingWrapper.onSceneLoaded += () => FindObjectOfType<LevelManager>().SetPaths() ;
        savingWrapper.OnLoaded += RestoreSlotData;
        FindAvailableSlots();

        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            var data = CaptureLevelData();

            savingWrapper.Save(data["Slot"].ToObject<int>(), data);
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            //ScreenCapture.)
            savingWrapper.Load(GetSlotId());
        }
    }

    public void ShowSlots()
    {
        FindAvailableSlots();
        slotsContainer.SetActive(!slotsContainer.activeSelf);
    }

    public void AddSlot(int slot)
    {
        ShowMenu(false);

        savingWrapper.Load(slot);
    }

    public void LoadSlot(int slot)
    {
        ShowMenu(false);

        
        savingWrapper.Load(slot);
    }

    private void FindAvailableSlots()
    {
        savingWrapper.LookForSlots(out int slotsCount, out int autoSaveCount);

        savingUI.EnableCurrentSlots(slotsCount, autoSaveCount);
    }

    private void ShowMenu(bool shouldShow)
    {
        mainMenu.SetActive(shouldShow);
        slotsContainer.SetActive(false);
    }

    private int GetSlotId()
    {
        return FindObjectOfType<LevelManager>().GetSlotData().Id;
    }

    private JObject CaptureLevelData()
    {
        var slotData = FindObjectOfType<LevelManager>().GetSlotData();

        JObject data = new JObject();
        data["Slot"] = slotData.Id;
        data["BuildIdx"] = slotData.BuildIdx;
        data["TimePlayed"] = slotData.PlayedTime;
        data["MembersCount"] = FindObjectOfType<PlayerManager>().Players.Count;

        return data;
    }

    private void RestoreSlotData(JObject slotData)
    {
        Debug.Log("SlotData to be Updated");
        var data = new SlotData(
            slotData["Slot"].ToObject<int>(),
            slotData["BuildIdx"].ToObject<int>(),
            slotData["TimePlayed"].ToObject<float>());

        FindObjectOfType<LevelManager>().SaveSlotData(data);

        Debug.Log("SlotData Updated");
    }
}
