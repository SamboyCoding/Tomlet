using System.Diagnostics.CodeAnalysis;

namespace Tomlet.Tests.TestModelClasses;

[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ExampleMailboxConfigClass
{
    public string mailbox;
    public string username;
    public string password;

    public Rule[] rules;

    public class Rule
    {
        public string address;
        public string[] blocked;
        public string[] allowed;
    }
}