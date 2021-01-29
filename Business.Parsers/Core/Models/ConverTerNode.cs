using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using ZTR.Framework.Business;

namespace Business.Parsers.Core.Models
{
    /// <summary>
    /// Config Read Model.
    /// </summary>
    internal class ConverterNode
    {
       
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the block.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public HtmlNode TagNode { get; set; }
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public HtmlNode SerialNode { get; set; }
    }
}
