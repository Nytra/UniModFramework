using System;

namespace UniModFramework;

public enum Feature
{
    PrePatching
}

public enum RequirementType
{
    Required,
    Optional
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FeatureRequirementAttribute : Attribute
{
    public Feature Feature;
    public RequirementType RequirementType;
    public FeatureRequirementAttribute(Feature feature, RequirementType type = RequirementType.Required)
    {
        Feature = feature;
        RequirementType = type;
    }
}