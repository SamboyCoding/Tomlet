namespace Tomlet.Exceptions
{
    public class NoTomlKeyException : TomlExceptionWithLine
    {
        public NoTomlKeyException(int lineNumber) : base(lineNumber) { }

        public override string Message => $"Expected a TOML key on line {LineNumber}, but found an equals sign ('=').";
    }
}