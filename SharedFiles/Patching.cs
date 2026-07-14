using System;

namespace UniModFramework;

public class UniPatchAttribute : Attribute
{
    public Type Type;
    public string MethodName;
    public UniPatchAttribute(Type type, string methodName)
    {
        Type = type;
        MethodName = methodName;
    }
}