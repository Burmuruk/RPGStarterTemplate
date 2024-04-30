using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public Color PlayerColor {  get; set; }
    public Gender PlayerGender { get; set; }

    public enum Gender
    {
        None,
        Male,
        Female
    }
}
