namespace Business.Parsers.Core.Models
{
    using HtmlAgilityPack;
    using System.Collections.Generic;

    /// <summary>
    /// Config Read Model.
    /// </summary>
    internal class ConverterNode
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the block.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public IEnumerable<HtmlNode> TagNode { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public HtmlNode SerialNode { get; set; }
    }
}
