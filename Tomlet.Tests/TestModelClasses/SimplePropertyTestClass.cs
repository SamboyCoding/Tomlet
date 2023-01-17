using System;
using System.Diagnostics.CodeAnalysis;

//Class defines Equals but not GetHashCode 
#pragma warning disable 659 

namespace Tomlet.Tests.TestModelClasses
{
    public class SimplePropertyTestClass
    {
        public string MyString { get; set; }
        public float MyFloat { get; set; }
        public bool MyBool { get; set; }
        public DateTime MyDateTime { get; set; }

        //Properties without getters are ignored, so this should be ignored
        public string PropWithNoGetter
        {
            set
            {
                //Do nothing
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
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
