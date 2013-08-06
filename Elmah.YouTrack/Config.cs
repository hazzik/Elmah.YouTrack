using System;
using System.Collections;
using TryParsers;

namespace Elmah.YouTrack
{
    public class Config
    {
        public Config(string host, string project)
        {
            if (host == null) throw new ArgumentNullException("host");
            if (project == null) throw new ArgumentNullException("project");
            Host = host;
            Project = project;
        }

        public string Host { get; private set; }

        public int Port { get; set; }

        public bool UseSsl { get; set; }

        public string Project { get; private set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public string Path { get; set; }

        public static Config FromDictionary(IDictionary dictionary)
        {
            var url = TryParse.Uri(dictionary.Get(ConfigKeys.Url), UriKind.RelativeOrAbsolute);
            var host = dictionary.Get(ConfigKeys.Host);
            if (url == null && string.IsNullOrEmpty(host))
            {
                throw new ApplicationException("The \"url\" or \"host\" setting is not found in configuration");
            }

            string project = dictionary.Get(ConfigKeys.Project);
            if (string.IsNullOrEmpty(project))
            {
                throw new ApplicationException("The \"project\" setting is not found in configuration");
            }

            if (url != null)
            {
                return new Config(url.Host, project)
                {
                    Port = url.Port,
                    UseSsl = url.Scheme.Contains("https"),
                    Path = url.AbsolutePath,
                    Username = dictionary.Get(ConfigKeys.Username),
                    Password = dictionary.Get(ConfigKeys.Passwrod),
                };
            }
            else
            {
                return new Config(host, project)
                {
                    Port = TryParse.Int32(dictionary.Get(ConfigKeys.Port)).GetValueOrDefault(80),
                    UseSsl = TryParse.Boolean(dictionary.Get(ConfigKeys.UseSsl)).GetValueOrDefault(),
                    Path = dictionary.Get(ConfigKeys.Path),
                    Username = dictionary.Get(ConfigKeys.Username),
                    Password = dictionary.Get(ConfigKeys.Passwrod)
                };
            }
        }
    }
}