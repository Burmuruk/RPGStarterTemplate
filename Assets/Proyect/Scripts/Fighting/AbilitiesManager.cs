using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    class AbilitiesManager : MonoBehaviour
    {
        Movement.Movement m_movement;

        public static readonly Dictionary<AbilityType, Action<object, Action>> habilitiesList = new()
        {
            { AbilityType.Dash, Dash },
            { AbilityType.Jump, Jump },
            { AbilityType.StealHealth, StealLife },
        };

        public static void Dash(object args, Action callback)
        {
            //Vector3 dir = (Vector3) direction;

            //stats.Speed += 2;
            print("In Dash!!!");
        }

        public static void Jump(object args, Action callback)
        {

        }

        public static void StealLife(object args, Action callback)
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
