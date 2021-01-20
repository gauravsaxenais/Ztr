namespace ZTR.Framework.Service
{
    using Microsoft.AspNetCore.Mvc;

    public sealed class QueryRouteAttribute : RouteAttribute
    {
        private const string TemplateBase = "api/query/";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRouteAttribute"/> class.
        /// </summary>
        public QueryRouteAttribute()
            : base(TemplateBase + "[controller]")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRouteAttribute"/> class.
        /// </summary>
        /// <param name="templateSuffix">The template suffix.</param>
        public QueryRouteAttribute(string templateSuffix)
            : base(TemplateBase + templateSuffix)
        {
        }
    }
}
