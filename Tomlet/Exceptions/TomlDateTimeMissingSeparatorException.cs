namespace Tomlet.Exceptions
{
    public class TomlDateTimeMissingSeparatorException : TomlExceptionWithLine
    {
        public TomlDateTimeMissingSeparatorException(int lineNumber) : base(lineNumber)
        {
        }

        public override string Message => $"Found a date-time on line {LineNumber} which is missing a separator (T, t, or a space) between the date and time.";
    }
}