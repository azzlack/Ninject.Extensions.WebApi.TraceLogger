namespace Ninject.Extensions.WebApi.TraceLogger
{
    using System;

    /// <summary>
    /// Trace performance record.
    /// </summary>
    public class PerformanceRecord
    {
        /// <summary>
        /// Gets or sets the request id.
        /// </summary>
        /// <value>
        /// The request id.
        /// </value>
        public Guid RequestId { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds.
        /// </summary>
        /// <value>
        /// The milliseconds.
        /// </value>
        public double Milliseconds { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string Operation { get; set; }
    }
}