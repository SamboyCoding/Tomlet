namespace Tomlet.Tests.TestModelClasses
{
    public class ClassWhichOverwritesFieldUsingNewKeyword : BaseClass
    {
        public new string OverwrittenField;
        public string NotOverwrittenSubclassField;
    }

    public class BaseClass
    {
        public string OverwrittenField = "default value";
        public string NotOverwrittenSuperclassField;
    }
}