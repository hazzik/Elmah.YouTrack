using System;
using System.Collections;
using TryParsers;

namespace Elmah.YouTrack
{
    public class Config
    {
        public Config(string host)
        {
            if (host == null) throw new ArgumentNullException("host");
            Host = host;
        }

        public string Host { get; private set; }

        public int Port { get; set; }

        public bool UseSsl
        {
            get; set;
        }

        public static Config FromDictionary(IDictionary dictionary)
        {
            var host = (string) dictionary[ConfigKeys.Host];
            if (string.IsNullOrEmpty(host))
            {
                throw new ApplicationException("The \"host\" setting is not found in configuration");
            }

            return new Config(host)
            {
                Port = TryParse.Int32(dictionary.Get(ConfigKeys.Port)).GetValueOrDefault(80),
                UseSsl = TryParse.Boolean(dictionary.Get(ConfigKeys.UseSsl)).GetValueOrDefault(),
                Path = dictionary.Get(ConfigKeys.Path),
                Username = dictionary.Get(ConfigKeys.Username),
                Password = dictionary.Get(ConfigKeys.Passwrod),
                Project = dictionary.Get(ConfigKeys.Project)
            };
        }

        public string Project { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public string Path { get; set; }
    }
}