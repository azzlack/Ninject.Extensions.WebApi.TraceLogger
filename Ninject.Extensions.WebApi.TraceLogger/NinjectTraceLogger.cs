namespace Ninject.Extensions.WebApi.TraceLogger
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Tracing;

    using Ninject.Extensions.Logging;

    /// <summary>
    /// TraceWriter implementation using the Ninject logging extension.
    /// </summary>
    public class NinjectTraceLogger : ITraceWriter
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// All traces of Kind 'Begin'
        /// </summary>
        private readonly List<TraceRecord> beginTraces;

        /// <summary>
        /// Initializes a new instance of the <see cref="NinjectTraceLogger" /> class.
        /// </summary>
        public NinjectTraceLogger()
        {
            var kernel = new StandardKernel();

            this.logger = kernel.Get<ILoggerFactory>().GetCurrentClassLogger();

            this.beginTraces = new List<TraceRecord>();
        }

        /// <summary>
        /// Determines whether the specified category is enabled.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="level">The level.</param>
        /// <returns>
        ///   <c>true</c> if the specified category is enabled; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnabled(string category, TraceLevel level)
        {
            return true;
        }

        /// <summary>
        /// Traces the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="category">The category.</param>
        /// <param name="level">The level.</param>
        /// <param name="traceAction">The trace action.</param>
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            // Create trace record
            var record = new TraceRecord(request, category, level);

            // Execute trace action delegate
            if (traceAction != null) 
            {
                traceAction(record);
            }

            // Calculate performance
            if (record.Kind == TraceKind.Begin)
            {
                this.beginTraces.Add(record);
            }

            // Log trace
            this.LogTrace(record);
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="record">The trace record.</param>
        public virtual void LogTrace(TraceRecord record)
        {
            var message = string.Format("[{0}] {1}: {2} {3} {4}", record.Category, record.Kind, record.Request.Method, record.Request.RequestUri, string.IsNullOrEmpty(record.Message) ? string.Empty : " - " + record.Message);

            switch (record.Level)
            {
                case TraceLevel.Info:
                    this.logger.Info(message);
                    break;
                case TraceLevel.Debug:
                    this.logger.Debug(message);
                    break;
                case TraceLevel.Warn:
                    this.logger.Warn(message);
                    break;
                case TraceLevel.Error:
                    this.logger.Error(message);
                    break;
                case TraceLevel.Fatal:
                    this.logger.Fatal(message);
                    break;
            }

            // Add exception message if present
            if (record.Exception != null)
            {
                this.logger.Error(string.Format("{0}: {1}", record.Exception.Message, record.Exception.StackTrace));

                if (record.Exception.InnerException != null)
                {
                    this.logger.Error(string.Format("{0}: {1}", record.Exception.InnerException.Message, record.Exception.InnerException.StackTrace));    
                }
            }

            // Write to system.diagnostics as well
            System.Diagnostics.Trace.WriteLine(message, record.Category);

            // Calculate performance
            if (record.Kind == TraceKind.End)
            {
                var begin = this.beginTraces.FirstOrDefault(r =>
                        (record.RequestId == r.RequestId && record.Category == r.Category &&
                         record.Operation == r.Operation && record.Operator == r.Operator));


                if (begin != null)
                {
                    // Log performance
                    this.logger.Info(string.Format("[{0}] {1}: {2} {3} - Request processing time: {4} s", record.Category, record.Kind, record.Request.Method, record.Request.RequestUri, record.Timestamp - begin.Timestamp));

                    // Remove begintrace
                    this.beginTraces.Remove(begin);
                }
            }
        }
    }
}
