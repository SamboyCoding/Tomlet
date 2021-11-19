namespace Tomlet
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class TomlPropertyAttribute : System.Attribute
    {
        private readonly string _mapFrom;

        public TomlPropertyAttribute(string mapFrom)
        {
            this._mapFrom = mapFrom;
        }

        public string GetMappedString()
        {
            return _mapFrom;
        }
    }
}