namespace ZTR.Framework.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Business;
    using FluentAssertions;
    using Xunit;

    public static class AssertExtensions
    {
        public static void EquivalentWithMissingMembers<T1, T2>(IEnumerable<T1> actualValues, IEnumerable<T2> expectedValues)
        {
            actualValues.Should().BeEquivalentTo(
                expectedValues,
                options => options.ExcludingMissingMembers());
        }

        public static void Equivalent<T1, T2>(IEnumerable<T1> actualValues, IEnumerable<T2> expectedValues)
        {
            actualValues.Should().BeEquivalentTo(expectedValues);
        }

        public static void EquivalentWithMissingMembers<T1, T2>(T1 actualValue, T2 expectedValue)
        {
            actualValue.Should().BeEquivalentTo(
                expectedValue,
                options => options.ExcludingMissingMembers());
        }

        public static void Equivalent<T1, T2>(T1 actualValue, T2 expectedValue)
        {
            actualValue.Should().BeEquivalentTo(expectedValue);
        }

        public static void ContainsErrorCode<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, TErrorCode errorCode, params TErrorCode[] errorCodes)
            where TErrorCode : struct, Enum
        {
            ContainsErrorCode(errorRecords, errorCodes.Prepend(errorCode));
        }

        public static void ContainsErrorCode<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<TErrorCode> errorCodes)
            where TErrorCode : struct, Enum
        {
            var result = ErrorCode(errorRecords, errorCodes);
            Assert.True(result, "Result does not contain the error code.");
        }

        public static void NotContainsErrorCode<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, TErrorCode errorCode, params TErrorCode[] errorCodes)
            where TErrorCode : struct, Enum
        {
            NotContainsErrorCode(errorRecords, errorCodes.Prepend(errorCode));
        }

        public static void NotContainsErrorCode<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<TErrorCode> errorCodes)
            where TErrorCode : struct, Enum
        {
            var result = ErrorCode(errorRecords, errorCodes);
            Assert.False(result, "Result contains the error code.");
        }

        public static void ContainsOrdinalPosition<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, int ordinalPosition, params int[] ordinalPositions)
            where TErrorCode : struct, Enum
        {
            ContainsOrdinalPosition(errorRecords, ordinalPositions.Prepend(ordinalPosition));
        }

        public static void ContainsOrdinalPosition<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<int> ordinalPositions)
            where TErrorCode : struct, Enum
        {
            var result = OrdinalPosition(errorRecords, ordinalPositions);
            Assert.True(result, "Result does not contain the ordinal position.");
        }

        public static void NotContainsOrdinalPosition<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, int ordinalPosition, params int[] ordinalPositions)
            where TErrorCode : struct, Enum
        {
            NotContainsOrdinalPosition(errorRecords, ordinalPositions.Prepend(ordinalPosition));
        }

        public static void NotContainsOrdinalPosition<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<int> ordinalPositions)
            where TErrorCode : struct, Enum
        {
            var result = OrdinalPosition(errorRecords, ordinalPositions);
            Assert.False(result, "Result does contain the ordinal position.");
        }

        private static bool ErrorCode<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<TErrorCode> errorCodes)
            where TErrorCode : struct, Enum
        {
            var result = errorRecords.SelectMany(x => x.Errors.Select(y => y.ErrorCode)).Where(z => errorCodes.Contains(z)).Distinct();
            return result.Count() == errorCodes.Distinct().Count();
        }

        private static bool OrdinalPosition<TErrorCode>(ErrorRecords<TErrorCode> errorRecords, IEnumerable<int> ordinalPositions)
            where TErrorCode : struct, Enum
        {
            var result = errorRecords.Select(x => x.OrdinalPosition).Where(y => ordinalPositions.Contains(y)).Distinct();
            return result.Count() == ordinalPositions.Distinct().Count();
        }
    }
}
