namespace ZTR.Framework.Service
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// RawRequestBodyAttribute
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ModelBinderAttribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RawRequestBodyAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawRequestBodyAttribute"/> class.
        /// </summary>
        public RawRequestBodyAttribute()
            : base(typeof(RawRequestBodyModelBinder))
        {
        }

        /// <summary>
        /// Gets the binding source.
        /// </summary>
        /// <value>
        /// The binding source.
        /// </value>
        public override BindingSource BindingSource => BindingSource.Body;
    }
}
