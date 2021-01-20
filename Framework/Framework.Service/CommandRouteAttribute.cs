namespace ZTR.Framework.Service
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// CommandRouteAttribute
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RouteAttribute" />
    public class CommandRouteAttribute : RouteAttribute
    {
        private const string TemplateBase = "api/command/";

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouteAttribute"/> class.
        /// </summary>
        public CommandRouteAttribute()
            : base(TemplateBase + "[controller]")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouteAttribute"/> class.
        /// </summary>
        /// <param name="templateSuffix">The template suffix.</param>
        public CommandRouteAttribute(string templateSuffix)
            : base(TemplateBase + templateSuffix)
        {
        }
    }
}
