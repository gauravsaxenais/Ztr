using System;
using System.Collections.Generic;
using System.Linq;
using ZTR.Framework.Business;

namespace Business.Parsers.Core.Models
{
    /// <summary>
    /// Config Read Model.
    /// </summary>
    public class ConfigTOML 
    {
        /// <summary>
        /// Gets or sets the Base.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public string BaseToml { get; set; }


        /// <summary>
        /// Gets or sets the Valid.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public string ViewToml { get; set; }


    }
}
