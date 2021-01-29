using System.Collections.Generic;
using System.Linq;
using ZTR.Framework.Business;

namespace Business.Parsers.Core.Models
{
    /// <summary>
    /// Config Read Model.
    /// </summary>
    internal class ConfigTag
    {
        IEnumerable<string> _tags;
        IEnumerable<string> _inline;
        public ConfigTag(string tags)
        {
            var t = tags.Replace("html:", string.Empty).RemoveNewline().Split(',');
            _tags = t.Where(o => o.EndsWith("@tag")).Select(o => o.Split("@")[0].RemoveNewline());
            _inline = t.Where(o => o.EndsWith("@inline")).Select(o => o.Split("@")[0].RemoveNewline());
        }
        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public IEnumerable<string> Tags => _tags;


        /// <summary>
        /// Gets or sets the block.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public IEnumerable<string> Inline => _inline;
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }
    }
}
