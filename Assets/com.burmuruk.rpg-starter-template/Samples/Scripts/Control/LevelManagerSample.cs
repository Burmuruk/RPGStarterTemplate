using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Burmuruk.RPGStarterTemplate.Control.Samples
{
    public class LevelManagerSample : LevelManager
    {
        [SerializeField] public string sceneName;
        //public void Update()
        //{
        //    if (Input.GetKeyUp(KeyCode.K))
        //    {
        //        var data = CaptureLevelData();

        //        savingWrapper.Save(data["Slot"].ToObject<int>(), data);
        //    }

        //    if (Input.GetKeyUp(KeyCode.L))
        //    {


        //        TemporalSaver.RemoveAllData();
        //        savingWrapper.Load(GetSlotData().Id);
        //    }
        //}

        public void ChangeMenu()
        {
            savingWrapper.AddNewAutoSaveSlot(CaptureLevelData(), true);

            gameManager.EnableUI(true);
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        //protected override void LoadMainMenu()
        //{
        //    FindObjectOfType<PersistentObjSpawner>().Restart();
        //    Time.timeScale = 1;
        //    var menu = SceneManager.GetSceneByName("MenuCharactersScene");
        //    if (menu != null && menu.isLoaded)
        //        SceneManager.UnloadSceneAsync(menu);
        //    PersistentObjects.ClearAndChangeScene("MainMenuSample");
        //}

        //public void GoToMainMenu()
        //{
        //    FindObjectOfType<Fader>().FadeIn();
        //    savingWrapper.AddNewAutoSaveSlot(CaptureLevelData(), true);
        //    while (playerManager.Players.Count > 0)
        //        playerManager.RemoveMember(playerManager.Players[0]);
        //    //FindObjectOfType<PersistentObjSpawner>().Restart();
        //    itemsToDestroy.ForEach(obj => Destroy(obj));
        //    Time.timeScale = 1;
        //    var menu = SceneManager.GetSceneByName("MenuCharactersScene");
        //    if (menu != null && menu.isLoaded)
        //        SceneManager.UnloadSceneAsync(menu);
        //    gameManager.GoToMainMenu();
        //    //PersistentObjects.ClearAndChangeScene(0);

        //    //SynchronizationContext context = SynchronizationContext.Current;
        //    //WaitFaid().ContinueWith((antecedent) =>
        //    //{
        //    //    context.Post(_ => LoadMainMenu(), null);
        //    //}, TaskContinuationOptions.ExecuteSynchronously);
        //}

        //private Task<bool> WaitFaid()
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    var timer = new System.Timers.Timer(1000)
        //    { AutoReset = false };
        //    timer.Elapsed += delegate
        //    {
        //        timer.Dispose();
        //        tcs.SetResult(true);
        //    };
        //    timer.Start();
        //    return tcs.Task;
        //}

        //protected virtual void LoadMainMenu()
        //{
        //    FindObjectOfType<PersistentObjSpawner>().Restart();
        //    itemsToDestroy.ForEach(obj => Destroy(obj));
        //    Time.timeScale = 1;
        //    var menu = SceneManager.GetSceneByName("MenuCharactersScene");
        //    if (menu != null)
        //        SceneManager.UnloadSceneAsync(menu);
        //    PersistentObjects.ClearAndChangeScene(0);
        //}
    }
}
