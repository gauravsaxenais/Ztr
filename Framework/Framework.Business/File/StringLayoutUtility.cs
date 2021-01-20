namespace ZTR.Framework.Business.File
{
    using System;
    using System.Reflection;

    /// <summary>
    /// TrimInputMode
    /// </summary>
    public enum TrimInputMode
    {
        /// <summary>
        /// The no trim
        /// </summary>
        NoTrim,
        /// <summary>
        /// The trim
        /// </summary>
        Trim,
        /// <summary>
        /// The trim start
        /// </summary>
        TrimStart,
        /// <summary>
        /// The trim end
        /// </summary>
        TrimEnd
    }

    /// <summary>
    /// https://www.codeproject.com/Articles/16143/Handling-Fixed-width-Flat-Files-with-NET-Custom-At
    /// </summary>
    public abstract class StringLayoutUtility
    {
        private TrimInputMode _trimInput = TrimInputMode.NoTrim;
        private char _paddingChar = ' ';

        /// <summary>
        /// Gets or sets the trim input.
        /// </summary>
        /// <value>
        /// The trim input.
        /// </value>
        public TrimInputMode TrimInput
        {
            get { return _trimInput; }
            set { _trimInput = value; }
        }

        /// <summary>
        /// Gets or sets the padding character.
        /// </summary>
        /// <value>
        /// The padding character.
        /// </value>
        public char PaddingChar
        {
            get { return _paddingChar; }
            set { _paddingChar = value; }
        }

        /// <summary>
        /// Parses the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Parse(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                foreach (PropertyInfo property in GetType().GetProperties())
                {
                    foreach (Attribute attribute in property.GetCustomAttributes(true))
                    {
                        if (attribute is StringLayoutAttribute stringLayoutAttribute)
                        {
                            string tmp = string.Empty;
                            if (stringLayoutAttribute.StartPosition <= input.Length - 1)
                            {
                                tmp = input.Substring(stringLayoutAttribute.StartPosition, Math.Min(stringLayoutAttribute.EndPosition - stringLayoutAttribute.StartPosition + 1, input.Length - stringLayoutAttribute.StartPosition));
                            }

                            switch (_trimInput)
                            {
                                case TrimInputMode.Trim:
                                    tmp = tmp.Trim();
                                    break;
                                case TrimInputMode.TrimStart:
                                    tmp = tmp.TrimStart();
                                    break;
                                case TrimInputMode.TrimEnd:
                                    tmp = tmp.TrimEnd();
                                    break;
                            }

                            property.SetValue(this, tmp, null);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = string.Empty;

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                foreach (Attribute attribute in property.GetCustomAttributes(false))
                {
                    if (attribute is StringLayoutAttribute stringLayoutAttribute)
                    {
                        string propertyValue = (string)property.GetValue(this, null);
                        if (stringLayoutAttribute.StartPosition > 0 && result.Length < stringLayoutAttribute.StartPosition)
                        {
                            result = result.PadRight(stringLayoutAttribute.StartPosition, _paddingChar);
                        }

                        string left = string.Empty;
                        string right = string.Empty;

                        if (stringLayoutAttribute.StartPosition > 0)
                        {
                            left = result.Substring(0, stringLayoutAttribute.StartPosition);
                        }

                        if (result.Length > stringLayoutAttribute.EndPosition + 1)
                        {
                            right = result[(stringLayoutAttribute.EndPosition + 1)..];
                        }

                        if (propertyValue.Length < stringLayoutAttribute.EndPosition - stringLayoutAttribute.StartPosition + 1)
                        {
                            propertyValue = propertyValue.PadRight(stringLayoutAttribute.EndPosition - stringLayoutAttribute.StartPosition + 1, _paddingChar);
                        }

                        result = left + propertyValue + right;
                    }

                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsValid();
    }
}
