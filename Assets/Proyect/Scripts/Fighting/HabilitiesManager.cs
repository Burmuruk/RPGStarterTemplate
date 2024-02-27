using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Fighting
{
    class HabilitiesManager : MonoBehaviour
    {
        

        Movement.Movement m_movement;
        StatsManager stats;

        public static readonly Dictionary<HabilityType, Action<object>> habilitiesList = new()
        {
            { HabilityType.Dash, Dash },
            { HabilityType.Jump, Jump },
            { HabilityType.StealHealth, StealLife },
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

    public enum HabilityType
    {
        None,
        Dash,
        Jump,
        StealHealth
    }
}
