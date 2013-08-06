using System;
using System.Collections;
using System.Text;
using YouTrackSharp.Infrastructure;
using YouTrackSharp.Issues;

namespace Elmah.YouTrack
{
    public class YouTrackErrorLog : ErrorLog
    {
        private readonly Config _config;

        public YouTrackErrorLog(IDictionary config)
        {
            _config = Config.FromDictionary(config);
        }

        public override string Name
        {
            get { return "YouTrack error log"; }
        }

        public override string Log(Error error)
        {
            var management = CreateIssueManagement(_config);

            var issue = BuildIssue(error);
            return management
                .CreateIssue(issue);
        }

        private static IssueManagement CreateIssueManagement(Config config)
        {
            var connection = new Connection(config.Host, config.Port, config.UseSsl, config.Path);
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
            {
                connection.Authenticate(config.Username, config.Password);
            }

            return new IssueManagement(connection);
        }

        private Issue BuildIssue(Error error)
        {
            dynamic issue = new Issue();
            issue.summary = error.Message;
            issue.description = ToDescription(error);
            issue.project = _config.Project;
            issue.type = "Exception";
            return issue;
        }

        private static string ToDescription(Error error)
        {
            var sb = new StringBuilder();

            sb.AppendLine()
                .AppendFormat("Exception = \"{0}\"", error.Exception)
                .AppendLine();

            return sb.ToString();
        }

        public override ErrorLogEntry GetError(string id)
        {
            throw new NotSupportedException();
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            throw new NotSupportedException();
        }
    }
}