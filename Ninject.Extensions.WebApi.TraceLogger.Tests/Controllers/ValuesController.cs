namespace Ninject.Extensions.WebApi.TraceLogger.Tests.Controllers
{
    using System;
    using System.Web.Http;
    using System.Web.Http.Tracing;

    public class ValuesController : ApiController
    {
        /// <summary>
        /// Returns "Hello World"
        /// </summary>
        /// <example>api/values</example>
        /// <returns>A sample string.</returns>
        public string Get()
        {
            return "Hello World!";
        }


        /// <summary>
        /// Generates a trace based on the id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// A string if trace was successful.
        /// </returns>
        /// <example>api/values/implementation</example>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the 'id' parameter contains an invalid value.</exception>
        public string Get(string id)
        {
            if (id == "interface")
            {
                TraceWriter.Instance.Info(this.Request, "Test", "Trace using interface");

                return "Traced using interface";
            }

            if (id == "implementation")
            {
                this.Configuration.Services.GetTraceWriter().Info(this.Request, "Test", "Trace using implementation");

                return "Traced using implementation";
            }

            throw new ArgumentOutOfRangeException("id", "id must be either 'interface' or 'implementation'");
        }
    }
}