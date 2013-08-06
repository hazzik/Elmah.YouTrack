using System.Collections;

namespace Elmah.YouTrack
{
    internal static class DictionaryEx
    {
        public static string Get(this IDictionary dictionary, string key)
        {
            return (string) dictionary[key];
        }
    }
}