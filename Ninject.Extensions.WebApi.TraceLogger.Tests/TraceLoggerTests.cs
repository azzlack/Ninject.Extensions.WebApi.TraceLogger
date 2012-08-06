using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ninject.Extensions.WebApi.TraceLogger.Tests
{
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.SelfHost;
    using System.Web.Http.Tracing;

    using NUnit.Framework;

    using Ninject.Extensions.Logging;
    using Ninject.Extensions.Logging.Log4net.Infrastructure;
    using Ninject.Web.WebApi;

    using log4net.Config;

    [TestFixture]
    public class TraceLoggerTests
    {
        /// <summary>
        /// The base address.
        /// </summary>
        private const string BaseAddress = "http://localhost";

        /// <summary>
        /// The base port.
        /// </summary>
        private int basePort;

        /// <summary>
        /// The webapi server
        /// </summary>
        private HttpSelfHostServer server;

        /// <summary>
        /// The webapi client
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int BasePort
        {
            get
            {
                return this.basePort;
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            // Start standalone iis express instance if port has not been defined
            if (this.basePort == 0)
            {
                // Get port
                this.basePort = this.GetAvailableTcpPort();
            } 

            // Create server configuration
            var config = new HttpSelfHostConfiguration(new Uri(BaseAddress + ":" + this.BasePort + "/"));

            // Configure dependencyresolver
            var kernel = new StandardKernel();

            kernel.Bind<ILoggerFactory>().ToConstant<Log4NetLoggerFactory>(new Log4NetLoggerFactory());
            config.DependencyResolver = new NinjectDependencyResolver(kernel);
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

            config.Services.Replace(typeof(ITraceWriter), new NinjectTraceLogger());
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NinjectTraceLogger());
            
            // Set api routes
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Configure log4net
            XmlConfigurator.Configure();

            // Create server
            this.server = new HttpSelfHostServer(config);

            this.server.OpenAsync().Wait();

            Debug.WriteLine("Server is up and running on '" + BaseAddress  + ":" + this.BasePort + "/" + "'");
        }

        [SetUp]
        public void Setup()
        {
            this.client = new HttpClient() { BaseAddress = new Uri(BaseAddress + ":" + this.BasePort + "/") };
        }

        [Test]
        public void Get_WhenWebApiIsRunning_ShouldReturnHelloWorld()
        {
            var result = this.client.GetStringAsync("api/values").Result;

            Assert.That(result == "\"Hello World!\"");
        }

        [Test]
        public void Trace_WhenUsingInterface_ShouldNotThrowException()
        {
            try
            {
                var result = this.client.GetStringAsync("api/values/interface").Result;

                Assert.That(result == "\"Traced using interface\"");
            }
            catch (AggregateException ex)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Trace_WhenUsingResolver_ShouldNotThrowException()
        {
            try 
            {
                var result = this.client.GetStringAsync("api/values/implementation").Result;

                Assert.That(result == "\"Traced using implementation\"");
            }
            catch (AggregateException ex)
            {
                Assert.Fail();
            }
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            this.server.CloseAsync().Wait();

            Debug.WriteLine("Server has been shut down.");
        }

        /// <summary>
        /// Gets an available TCP port.
        /// </summary>
        /// <returns>A port number.</returns>
        private int GetAvailableTcpPort()
        {
            var globalIpProperties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = globalIpProperties.GetActiveTcpConnections();

            var availablePorts = Enumerable.Range(1024, 64510).Where(x => activeTcpConnections.All(c => c.LocalEndPoint.Port != x)).ToList();

            var port = availablePorts[new Random().Next(availablePorts.Count())];

            return port;
        }
    }
}
