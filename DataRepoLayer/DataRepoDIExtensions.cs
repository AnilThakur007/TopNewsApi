using Microsoft.Extensions.DependencyInjection;

namespace DataRepoLayer
{
    /// <summary>
    /// Provides extension methods for configuring News api layer services.
    /// </summary>
    public static class DataRepoDIExtensions
    {
        /// <summary>
        /// Adds database/News api layer services to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to which database/news api services will be added.</param>
        /// <returns>The updated service collection with registered database/news api services.</returns>
        public static IServiceCollection AddDatabaseLayerServices(this IServiceCollection services)
        {
            services.AddScoped<IStoryRepository, StoryRepository>();
            return services;
        }
    }
}
