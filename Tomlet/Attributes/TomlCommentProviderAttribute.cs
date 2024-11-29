using System;
using System.Linq;

namespace Tomlet.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class TomlCommentProviderAttribute : Attribute
{
    private readonly Type _provider;
    private readonly object[] _args;
    private readonly Type[] _constructorParamsType;

    public string GetComment()
    {
        var constructor = _provider.GetConstructor(_constructorParamsType) ??
                          throw new ArgumentException("Fail to get a constructor matching the parameters");
        var instance = constructor.Invoke(_args) as ICommentProvider ??
                       throw new Exception("Fail to create an instance of the provider");
        return instance.GetComment();
    }

    public TomlCommentProviderAttribute(Type provider, object[] args)
    {
        if (!typeof(ICommentProvider).IsAssignableFrom(provider))
        {
            throw new ArgumentException("Provider must implement ICommentProvider");
        }

        _provider = provider;
        _args = args ?? new object[] { };
        _constructorParamsType = args?.Select(a => a.GetType()).ToArray() ?? new Type[] { };
    }
}