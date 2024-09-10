using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;

    public void ShowMenu(bool shouldShow)
    {
        mainMenu.SetActive(shouldShow);
    }

    public void HideCredits()
    {

    }

    public void LoadGame(int idx)
    {

    }
}
