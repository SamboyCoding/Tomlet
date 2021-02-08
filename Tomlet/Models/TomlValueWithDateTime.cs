using System;

namespace Tomlet.Models
{
    public interface TomlValueWithDateTime
    {
        public abstract DateTime Value { get; }
    }
}