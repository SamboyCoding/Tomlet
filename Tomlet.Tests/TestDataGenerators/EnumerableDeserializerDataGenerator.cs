using System.Collections;
using System.Collections.Generic;
using Tomlet.Tests.TestModelClasses;

namespace Tomlet.Tests.TestDataGenerators;

public class EnumerableDeserializerDataGenerator : IEnumerable<object[]>
{
    private const int ExpectedCount = 5;
    private readonly string _expectedValue = string.Empty;

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { typeof(StringArrayWrapper), TestResources.ArrayOfEmptyStringTestInput, ExpectedCount, _expectedValue };
        yield return new object[] { typeof(StringEnumerableWrapper), TestResources.ArrayOfEmptyStringTestInput, ExpectedCount, _expectedValue };
        yield return new object[] { typeof(StringListWrapper), TestResources.ArrayOfEmptyStringTestInput, ExpectedCount, _expectedValue };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}