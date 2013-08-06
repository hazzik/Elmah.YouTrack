using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using YouTrackSharp.Infrastructure;
using YouTrackSharp.Issues;

namespace Elmah.YouTrack
{
    public class YouTrackErrorModule : HttpModuleBase, IExceptionFiltering
    {
        private Config _config;

        protected override bool SupportDiscoverability
        {
            get { return true; }
        }

        public event ExceptionFilterEventHandler Filtering;

        protected override void OnInit(HttpApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            var config = GetConfig();
            if (config == null)
                return;

            _config = Config.FromDictionary(config);

            application.Error += OnError;
            ErrorSignal.Get(application).Raised += OnErrorSignaled;
        }

        private void OnError(object sender, EventArgs e)
        {
            var context = ((HttpApplication) sender).Context;
            OnError(context.Server.GetLastError(), context);
        }

        private void OnErrorSignaled(object sender, ErrorSignalEventArgs args)
        {
            OnError(args.Exception, args.Context);
        }

        private void OnError(Exception e, HttpContext context)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            var args = new ExceptionFilterEventArgs(e, context);
            OnFiltering(args);
            if (args.Dismissed)
                return;

            var error = new Error(e, context);
            if (_config.ReportAsynchronously)
                ReportErrorAsync(error);
            else
                ReportError(error);
        }

        private void OnFiltering(ExceptionFilterEventArgs args)
        {
            var handler = Filtering;
            if (handler == null)
                return;
            handler(this, args);
        }

        private void ReportErrorAsync(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");
            ThreadPool.QueueUserWorkItem(ReportError, error);
        }

        private void ReportError(object state)
        {
            try
            {
                ReportError((Error) state);
            }
            catch (SmtpException ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private void ReportError(Error error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            var management = CreateIssueManagement(_config);

            var issue = BuildIssue(error);
            management.CreateIssue(issue);
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

        private static IDictionary GetConfig()
        {
            return (IDictionary) ConfigurationManager.GetSection("elmah/youtrack");
        }
    }
}
