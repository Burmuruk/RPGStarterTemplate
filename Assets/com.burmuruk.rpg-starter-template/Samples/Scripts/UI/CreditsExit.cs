using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.UI
{
    public class CreditsExit : MonoBehaviour
    {
        [SerializeField] GameObject mainButtons;
        [SerializeField] GameObject credtis;

        public void HideCredits()
        {
            mainButtons.SetActive(true);
            credtis.SetActive(false);
        }
    } 
}
