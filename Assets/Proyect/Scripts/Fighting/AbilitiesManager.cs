using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Fighting
{
    class AbilitiesManager : MonoBehaviour
    {
        

        Movement.Movement m_movement;
        StatsManager stats;

        public static readonly Dictionary<AbilityType, Action<object>> habilitiesList = new()
        {
            { AbilityType.Dash, Dash },
            { AbilityType.Jump, Jump },
            { AbilityType.StealHealth, StealLife },
        };

        public static void Dash(object direction)
        {
            //Vector3 dir = (Vector3) direction;

            //stats.Speed += 2;
            print("In Dash!!!");
        }

        public static void Jump(object direction)
        {

        }

        public static void StealLife(object direction)
        {

        }
    }

    public enum AbilityType
    {
        None,
        Dash,
        Jump,
        StealHealth
    }
}
