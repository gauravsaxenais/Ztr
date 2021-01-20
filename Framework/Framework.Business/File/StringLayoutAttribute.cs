namespace ZTR.Framework.Business.File
{
    using System;

    /// <summary>
    /// StringLayoutAttribute
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class StringLayoutAttribute : Attribute
    {
        private int _startPosition;
        private int _endPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLayoutAttribute"/> class.
        /// </summary>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <exception cref="ArgumentException">StartPosition must be > 0 and EndPosition must be greater than or equal to StartPosition</exception>
        public StringLayoutAttribute(int startPosition, int endPosition)
        {
            if (startPosition >= 0 && endPosition >= startPosition)
            {
                _startPosition = startPosition;
                _endPosition = endPosition;
            }
            else
            {
                throw new ArgumentException("StartPosition must be > 0 and EndPosition must be greater than or equal to StartPosition");
            }
        }

        /// <summary>
        /// Gets or sets the start position.
        /// </summary>
        /// <value>
        /// The start position.
        /// </value>
        public int StartPosition
        {
            get { return _startPosition; }
            set { _startPosition = value; }
        }

        /// <summary>
        /// Gets or sets the end position.
        /// </summary>
        /// <value>
        /// The end position.
        /// </value>
        public int EndPosition
        {
            get { return _endPosition; }
            set { _endPosition = value; }
        }
    }
}
