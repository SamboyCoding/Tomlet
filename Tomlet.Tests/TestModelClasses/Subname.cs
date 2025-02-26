using System;

namespace Tomlet.Tests.TestModelClasses;

public class Subname
{
    public string a;
    public string b;

    protected bool Equals(Subname other)
    {
        return a == other.a && b == other.b;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Subname)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(a, b);
    }
}