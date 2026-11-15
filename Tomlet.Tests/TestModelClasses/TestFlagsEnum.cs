using System;

namespace Tomlet.Tests.TestModelClasses;

[Flags]
public enum TestFlagsEnum
{
    None = 0,
    Flag1 = 1,
    Flag2 = 2,
    Flag3 = 4
}