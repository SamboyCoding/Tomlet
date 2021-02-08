namespace Tomlet.Models
{
    public class TomlBoolean : TomlValue
    {
        public static readonly TomlBoolean TRUE = new(true);
        public static readonly TomlBoolean FALSE = new(false);
        
        private bool _value;
        private TomlBoolean(bool value)
        {
            _value = value;
        }

        public static TomlBoolean ValueOf(bool b) => b ? TRUE : FALSE;

        public bool Value => _value;

        public override string StringValue => Value ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant();
    }
}