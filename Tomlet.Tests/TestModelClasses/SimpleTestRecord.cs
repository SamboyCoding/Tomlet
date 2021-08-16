using System;

namespace Tomlet.Tests.TestModelClasses
{
    public record SimpleTestRecord
    {
        public string MyString { get; init; }
        public float MyFloat { get; init; }
        public bool MyBool { get; init; }
        public DateTime MyDateTime { get; init; }
    }
}
