namespace Tomlet.Exceptions
{
    public class TomlTableLockedException : TomlExceptionWithLine
    {
        private readonly string _key;

        public TomlTableLockedException(int lineNumber, string key) : base(lineNumber)
        {
            _key = key;
        }

        public override string Message => $"TOML table is locked (e.g. defined inline), cannot add or update key {_key} to it on line {LineNumber}";
    }
}