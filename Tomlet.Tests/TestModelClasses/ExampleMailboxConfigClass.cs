namespace Tomlet.Tests.TestModelClasses;

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