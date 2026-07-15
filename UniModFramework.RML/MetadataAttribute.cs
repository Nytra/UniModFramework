public class MetadataAttribute : Attribute
{
    public string GUID;
    public string Name;
    public string Version;
    public string Author;
    public string Link;
    public MetadataAttribute(string GUID, string Name, string Version, string Author, string Link)
    {
        this.GUID = GUID;
        this.Name = Name;
        this.Version = Version;
        this.Author = Author;
        this.Link = Link;
    }
}