using System.Text.RegularExpressions;
using Tomlet.Exceptions;
using Tomlet.Models;

namespace Tomlet
{
    internal static class TomlDateTimeUtils
    {
        private static readonly Regex DATE_TIME_REGEX = new(
            @"^(?:(\d+)-(0[1-9]|1[012])-(0[1-9]|[12]\d|3[01]))?([\sTt])?(?:([01]\d|2[0-3]):([0-5]\d):([0-5]\d|60)(\.\d+)?((?:[Zz])|(?:[\+|\-](?:[01]\d|2[0-3])(?::[0-6][0-9])?(?::[0-6][0-9])?))?)?$",
            RegexOptions.Compiled
        );

        internal static TomlValue? ParseDateString(string input, int lineNumber)
        {
            //All groups can be empty.
            //Group 1 - Year
            //Group 2 - Month
            //Group 3 - Day
            //Group 4 - Date/Time separator. If empty string and both date and time present, syntax error.
            //Group 5 - Hour
            //Group 6 - Minute
            //Group 7 - Second
            //Group 8 - Milliseconds
            //Group 9 - Time zone - if not present, this is a local (date)time. If present without a date or without a time, syntax error.
            var match = DATE_TIME_REGEX.Match(input);

            //If year is present, whole date has to be by the regex.
            var hasYear = !match.Groups[1].Value.IsNullOrWhiteSpace();
            var hasSeparator = !string.IsNullOrEmpty(match.Groups[4].Value);
            var hasHour = !match.Groups[5].Value.IsNullOrWhiteSpace();
            var hasTimezone = !match.Groups[9].Value.IsNullOrWhiteSpace();

            if (hasYear && hasHour && !hasSeparator)
                throw new TomlDateTimeMissingSeparatorException(lineNumber);

            if (hasSeparator && (!hasHour || !hasYear))
                throw new TomlDateTimeUnnecessarySeparatorException(lineNumber);

            if (hasTimezone && (!hasHour || !hasYear))
                throw new TimeOffsetOnTomlDateOrTimeException(lineNumber, match.Groups[9].Value);

            if (!hasYear)
                return TomlLocalTime.Parse(input);

            if (!hasHour)
                return TomlLocalDate.Parse(input);

            if (!hasTimezone)
                return TomlLocalDateTime.Parse(input);

            return TomlOffsetDateTime.Parse(input);
        }
    }
}