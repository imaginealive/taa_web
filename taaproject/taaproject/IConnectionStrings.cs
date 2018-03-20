namespace taaproject
{
    public interface IServiceConfigurations
    {
        string DefaultConnection { get; set; }
        string DatabaseName { get; set; }
        string ProjectCollection { get; set; }
        string UserCollection { get; set; }
        string MembershipCollection { get; set; }
        string FeatureCollection { get; set; }
        string StoryCollection { get; set; }
        string TaskCollection { get; set; }
    }
}