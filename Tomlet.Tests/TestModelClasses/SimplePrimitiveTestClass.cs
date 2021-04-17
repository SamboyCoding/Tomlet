using System;

namespace Tomlet.Tests.TestModelClasses
{
    public class SimplePrimitiveTestClass
    {
        public string MyString;
        public float MyFloat;
        public bool MyBool;
        public DateTime MyDateTime;

        protected bool Equals(SimplePrimitiveTestClass other)
        {
            return MyString == other.MyString && MyFloat.Equals(other.MyFloat) && MyBool == other.MyBool && MyDateTime.Equals(other.MyDateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SimplePrimitiveTestClass) obj);
        }
    }
}