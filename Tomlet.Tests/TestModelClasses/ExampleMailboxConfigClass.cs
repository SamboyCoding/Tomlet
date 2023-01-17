using System.Diagnostics.CodeAnalysis;
using Tomlet.Attributes;

namespace Tomlet.Tests.TestModelClasses;

[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ExampleMailboxConfigClass
{
    [TomlInlineComment("The name of the mailbox")]
    public string mailbox;
    [TomlInlineComment("Your username for the mailbox")]
    public string username;
    [TomlInlineComment("The password you use to access the mailbox")]
    public string password;

    [TomlPrecedingComment("The rules for the mailbox follow")]
    public Rule[] rules { get; set; }

    public class Rule
    {
        public string address;
        public string[] blocked;
        public string[] allowed;
    }
}