using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tomlet.Tests.TestModelClasses;

namespace Tomlet.Tests.TestDataGenerators;

public class EnumerableSerializerDataGenerator : IEnumerable<object[]>
{
    private readonly string[] _emptyStringArray = {string.Empty, string.Empty, string.Empty, string.Empty, string.Empty};

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new StringEnumerableWrapper { Array = _emptyStringArray } , TestResources.ArrayOfEmptyStringTestOutput };
        yield return new object[] { new StringEnumerableWrapper { Array = _emptyStringArray.ToList() } , TestResources.ArrayOfEmptyStringTestOutput };
        yield return new object[] { new StringEnumerableWrapper { Array = new HashSet<string>(_emptyStringArray) } , TestResources.HashSetOfEmptyStringTestOutput };
        yield return new object[] { new StringEnumerableWrapper { Array = new LinkedList<string>(_emptyStringArray) } , TestResources.ArrayOfEmptyStringTestOutput };
        yield return new object[] { new StringEnumerableWrapper { Array = new Queue<string>(_emptyStringArray) } , TestResources.ArrayOfEmptyStringTestOutput };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}