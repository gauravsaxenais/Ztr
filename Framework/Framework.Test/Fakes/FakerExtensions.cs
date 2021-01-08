namespace ZTR.Framework.Test.Fakes
{
    using System;

    public static class FakerExtensions
    {
        public static DateTimeOffset Truncate(this DateTimeOffset dateTime)
        {
            return dateTime.Truncate(TimeSpan.FromMilliseconds(1));
        }

        public static DateTimeOffset Truncate(this DateTimeOffset dateTime, TimeSpan timeSpanTruncation)
        {
            if (dateTime == DateTimeOffset.MinValue || dateTime == DateTimeOffset.MaxValue)
            {
                return dateTime; // do not modify guard values
            }
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpanTruncation.Ticks));
        }
    }
}
