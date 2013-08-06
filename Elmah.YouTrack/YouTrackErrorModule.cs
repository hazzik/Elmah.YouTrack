using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
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
            catch (Exception ex)
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
            issue.description = BuildDescription(error);
            issue.project = _config.Project;
            issue.type = "Exception";
            return issue;
        }

        private static string BuildDescription(Error error)
        {
            var sb = new StringBuilder();

            Write(sb, "ApplicationName", error.ApplicationName);
            Write(sb, "ErrorType", error.Type);
            Write(sb, "Time", error.Time);
            Write(sb, "HostName", error.HostName);
            Write(sb, "Source", error.Source);
            Write(sb, "User", error.User);
            Write(sb, "StatusCode", error.StatusCode);
            Write(sb, "WebHostHtmlMessage", error.WebHostHtmlMessage);

            var number = 1;
            var exceptions = GetExceptions(error.Exception);
            foreach (var exception in exceptions)
            {
                sb
                    .AppendFormat("-- EXCEPTION #{0}/{1} [{2}]", number++, exceptions.Count, exception.GetType().Name)
                    .AppendLine()
                    .AppendLine("Message = \"")
                    .AppendLine(exception.Message)
                    .AppendLine("\"")
                    .AppendFormat("ClassName = \"{0}\"", exception.GetType().FullName)
                    .AppendLine();

                Write(sb, "Data", exception.Data);

                sb.AppendLine("StackTraceString = \"")
                    .AppendLine(exception.StackTrace)
                    .AppendLine("\"")
                    .AppendLine();
            }

            Write(sb, "Server Variables", error.ServerVariables);
            Write(sb, "Query String", error.QueryString);
            Write(sb, "Form", error.Form);
            Write(sb, "Cookies", error.Cookies);

            return sb.ToString();
        }

        private static void Write(StringBuilder sb, string name, NameValueCollection value)
        {
            if (value.Count == 0) return;

            sb.AppendLine()
                .AppendFormat("{{cut {0}}}", name).AppendLine()
                .AppendFormat("-- {0}", name).AppendLine();

            Write(sb, value);

            sb.AppendLine("{cut}")
                .AppendLine();
        }

        private static void Write(StringBuilder sb, string name, object value)
        {
            if (value != null && value.ToString() != string.Empty)
                sb.AppendFormat("{0} = \"{1}\"", name, value).AppendLine();
        }

        private static void Write(StringBuilder sb, string name, IDictionary value)
        {
            foreach (DictionaryEntry entry in value.Keys)
            {
                sb.AppendFormat("{2}.{0} = \"{1}\"", entry.Key, entry.Value, name).AppendLine();
            }
        }

        private static void Write(StringBuilder sb, NameValueCollection collection)
        {
            foreach (string key in collection)
            {
                sb.AppendFormat("{0} = \"{1}\"", key, collection[key]).AppendLine();
            }
        }

        private static IList<Exception> GetExceptions(Exception exception)
        {
            var stack = new Stack<Exception>();
            while (exception != null)
            {
                stack.Push(exception);
                exception = exception.InnerException;
            }
            return stack.ToArray();
        }

        private static IDictionary GetConfig()
        {
            return (IDictionary) ConfigurationManager.GetSection("elmah/youtrack");
        }
    }
}
