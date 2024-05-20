
using Newtonsoft.Json.Linq;

namespace Burmuruk.Tesis.Saving
{
    public interface IJsonSaveable
    {
        JToken CaptureAsJToken();
        void RestoreFromJToken(JToken state);
    }
}
