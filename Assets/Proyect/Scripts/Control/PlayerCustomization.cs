using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Customization", order = 4)]
    public class PlayerCustomization : ScriptableObject
    {
        [SerializeField]
        Color[] colors;
        [SerializeField]
        int[] defaultColors;

        public Color[] Colors { get => colors; }
        public Color[] DefaultColors
        {
            get
            {
                var newColors = new List<Color>();

                foreach (var color in defaultColors)
                {
                    newColors.Add(colors[color]);
                }

                return newColors.ToArray();
            }
        }
    }
}