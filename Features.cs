namespace UniModFramework;

public enum Feature
{
    PrePatching
}

public enum FeatureRequirementType
{
    Required,
    Optional
}

public class FeatureRequirementAttribute : Attribute
{
    public Feature Feature;
    public FeatureRequirementType RequirementType;
    public FeatureRequirementAttribute(Feature feature, FeatureRequirementType type = FeatureRequirementType.Required)
    {
        Feature = feature;
        RequirementType = type;
    }
}