﻿namespace Tomlet
{
    public class TomlSerializerOptions
    {
        public static TomlSerializerOptions Default = new();
        public bool OverrideConstructorValues { get; set; } = false;
    }
}