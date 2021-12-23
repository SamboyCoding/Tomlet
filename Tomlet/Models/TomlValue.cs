namespace Tomlet.Models
{
    public abstract class TomlValue
    {
        public TomlCommentData Comments { get; } = new();
        
        public abstract string StringValue
        {
            get;
        }

        /// <summary>
        /// The value that should be used to represent this instance when it is written to a TOML file.
        /// </summary>
        public abstract string SerializedValue
        {
            get;
        }
    }
}