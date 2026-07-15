using System;

namespace UniModFramework;

[AttributeUsage(AttributeTargets.Class)]
public class PatchAttribute : Attribute
{
    public Type Type;
    public string MethodName;
    public PatchAttribute(Type type, string methodName)
    {
        Type = type;
        MethodName = methodName;
    }
}