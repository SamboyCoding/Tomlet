using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomlet.Tests.TestModelClasses
{
    public class SimplePropertyTestClass
    {
        public string MyString { get; set; }
        public float MyFloat { get; set; }
        public bool MyBool { get; set; }
        public DateTime MyDateTime { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SimplePropertyTestClass @class &&
                   MyString == @class.MyString &&
                   MyFloat == @class.MyFloat &&
                   MyBool == @class.MyBool &&
                   MyDateTime == @class.MyDateTime;
        }
    }
}
