using System;

namespace Tomlet.Models;

public interface ITomlValueWithDateTime
{
    public DateTime Value { get; }
}