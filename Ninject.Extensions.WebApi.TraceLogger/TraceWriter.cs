namespace Ninject.Extensions.WebApi.TraceLogger
{
    using System;
    using System.Web.Http;
    using System.Web.Http.Tracing;

    /// <summary>
    /// An instance of the currently registered TraceWriter
    /// </summary>
    public class TraceWriter
    {
        /// <summary>
        /// The singleton.
        /// </summary>
        private static ITraceWriter traceWriter;

        /// <summary>
        /// Prevents a default instance of the <see cref="TraceWriter" /> class from being created.
        /// </summary>
        private TraceWriter()
        {
        }

        /// <summary>
        /// Gets the current instance of the ITraceWriter service.
        /// </summary>
        /// <value>
        /// The ITraceWriter instance.
        /// </value>
        public static ITraceWriter Instance
        {
            get
            {
                if (traceWriter == null)
                {
                    var currentTraceWriter = GlobalConfiguration.Configuration.Services.GetTraceWriter();

                    if (currentTraceWriter == null)
                    {
                        throw new NullReferenceException("No concrete implementation of ITraceWriter has been registered!");
                    }

                    return traceWriter = currentTraceWriter;
                }

                return traceWriter;
            }
        }
    }
}