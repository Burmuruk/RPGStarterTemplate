using Burmuruk.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Utilities
{
    public class PoolCoolDownAction<T>
    {
        private Queue<CoolDownAction> timers = new();
        private List<(T character, Coroutine coroutine, CoolDownAction cd)> runningTimers = new();
    }
}
