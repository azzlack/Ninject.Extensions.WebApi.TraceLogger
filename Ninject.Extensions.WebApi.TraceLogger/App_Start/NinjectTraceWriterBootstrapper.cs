using Ninject.Extensions.WebApi.TraceLogger.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NinjectTraceWriterBootstrapper), "Start")]

namespace Ninject.Extensions.WebApi.TraceLogger.App_Start
{
    using System.Web.Http;
    using System.Web.Http.Tracing;

    /// <summary>
    /// Bootstrapper for the Ninject TraceWriter
    /// </summary>
    public class NinjectTraceWriterBootstrapper
    {
        /// <summary>
        /// Executes before the application starts.
        /// </summary>
        public static void Start()
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NinjectTraceLogger());
        }
    }
}