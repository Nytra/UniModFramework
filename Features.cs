namespace ModFramework;

public enum Feature
{
    PrePatching
}

public class FeatureRequirementAttribute : Attribute
{
    public Feature Feature;
    public FeatureRequirementAttribute(Feature feature)
    {
        Feature = feature;
    }
}