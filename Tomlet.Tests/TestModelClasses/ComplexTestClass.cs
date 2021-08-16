using System;
using System.Collections.Generic;
using System.Linq;

//Class defines Equals but not GetHashCode 
#pragma warning disable 659 

namespace Tomlet.Tests.TestModelClasses
{
    public class ComplexTestClass
    {
        public string TestString;
        public SubClassTwo SubClass2;
        public List<SubClassOne> ClassOnes = new();

        public class SubClassOne
        {
            public string SubKeyOne;

            protected bool Equals(SubClassOne other)
            {
                return SubKeyOne == other.SubKeyOne;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((SubClassOne) obj);
            }
        }

        public class SubClassTwo
        {
            public string SubKeyOne;
            public DateTimeOffset SubKeyTwo;
            public int SubKeyThree;
            public float SubKeyFour;

            protected bool Equals(SubClassTwo other)
            {
                return SubKeyOne == other.SubKeyOne && SubKeyTwo.Equals(other.SubKeyTwo) && SubKeyThree == other.SubKeyThree && SubKeyFour.Equals(other.SubKeyFour);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((SubClassTwo) obj);
            }
        }

        protected bool Equals(ComplexTestClass other)
        {
            return TestString == other.TestString && Equals(SubClass2, other.SubClass2) && ClassOnes.SequenceEqual(other.ClassOnes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComplexTestClass) obj);
        }
    }
}