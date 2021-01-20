namespace ZTR.Framework.Service
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// JobRouteAttribute
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RouteAttribute" />
    public class JobRouteAttribute : RouteAttribute
    {
        private const string TemplateBase = "api/job/";

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRouteAttribute"/> class.
        /// </summary>
        public JobRouteAttribute()
            : base(TemplateBase + "[controller]")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobRouteAttribute"/> class.
        /// </summary>
        /// <param name="templateSuffix">The template suffix.</param>
        public JobRouteAttribute(string templateSuffix)
            : base(TemplateBase + templateSuffix)
        {
        }
    }
}
