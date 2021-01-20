namespace ZTR.Framework.Business
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Abstract class for request handlers. Added logging.
    /// </summary>
    /// <seealso cref="ZTR.Framework.Business.IManager" />
    public abstract class Manager : IManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Manager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected Manager(ILogger logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            Logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected ILogger Logger { get; private set; }
    }
}
