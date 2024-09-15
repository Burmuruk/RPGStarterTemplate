
using Newtonsoft.Json.Linq;

namespace Burmuruk.Tesis.Saving
{
    public interface IJsonSaveable
    {
        JToken CaptureAsJToken(out SavingExecution execution);
        void RestoreFromJToken(JToken state);
    }
}
