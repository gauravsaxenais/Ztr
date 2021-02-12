using System;
using System.Collections.Generic;
using System.Linq;
using ZTR.Framework.Business;

namespace Business.Parsers.Core.Models
{
    /// <summary>
    /// Config Read Model.
    /// </summary>
    internal class ConfigMap : IEquatable<ConfigMap>
    {
        public ConfigMap(string mapping)
        {
            var t = mapping.Replace("map:", string.Empty).RemoveNewline().Split(':');
            From = t[0]?.Trim().Replace(" ", string.Empty);
            To = t[1].Trim();
        }
        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public string From { get; private set; }


        /// <summary>
        /// Gets or sets the block.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public string To { get; private set; }
  

        public bool Equals(ConfigMap other)
        {
            if (From == other.From && To == other.To)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            int f = From.GetHashCode();
            int t = To.GetHashCode();

            return f ^ t;
        }
    }
}
