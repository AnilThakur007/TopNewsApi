namespace ViewModel
{
    public class UtilityModel
    {
    }

    public class CacheSettings
    {
        public int AbsoluteExpirationMinutes { get; set; }
        public int SlidingExpirationMinutes { get; set; }
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int MaxStories { get; set; }
    }
}
