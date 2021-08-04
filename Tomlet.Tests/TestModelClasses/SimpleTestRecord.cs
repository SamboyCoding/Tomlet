using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
