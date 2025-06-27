using Burmuruk.Tesis.Control;
using System;

namespace Burmuruk.Tesis.Inventory
{
    public interface IUsable
    {
        void Use(Character character, object args, Action callback);
    }
}
