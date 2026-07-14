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