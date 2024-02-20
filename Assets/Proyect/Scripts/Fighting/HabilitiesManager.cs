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

        public static List<(string, Action<object>)> habilitiesList = new()
        {
            { ("Dash", Dash) },
            { ("Jump", Jump) },
            { ("StealLife", StealLife) },
        };

        public HabilitiesManager()
        {
            habilitiesList = new();
        }

        public static void Dash(object direction)
        {
            Vector3 dir = (Vector3) direction;

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
}
