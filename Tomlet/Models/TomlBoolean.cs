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

        public bool Value => _value;

        public override string StringValue => Value ? bool.TrueString : bool.FalseString;
    }
}