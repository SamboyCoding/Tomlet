using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses
{
    public class ClassWithNonSerializableAttributes
    {
        public string SerializedString { get; set; }

        public int SerializedInt { get; set; }

        [TomlNonSerialized]
        public string NonSerializedProperty { get; set; }

        private string _SerializedField = "Serialized Field";

        [TomlNonSerialized]
        private string _NonSerializedField = "Non-Serialized private field";

    }
}
