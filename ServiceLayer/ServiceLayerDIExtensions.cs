using DataRepoLayer;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceLayer
{
    /// <summary>
    /// Provides extension methods for configuring dependency injection in the service layer.
    /// </summary>
    public static class ServiceLayerDIExtensions
    {
        /// <summary>
        /// Registers the services required for the service layer with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the services are added.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> with the service layer services registered.
        /// </returns>
        /// <remarks>
        /// This method adds database layer services by calling <see cref="DataRepoDIExtensions.AddDatabaseLayerServices(IServiceCollection)"/>.
        /// It also registers <see cref="IStoriesService"/> with its implementation <see cref="StoriesService"/>.
        /// </remarks>
        public static IServiceCollection AddServiceLayerServices(this IServiceCollection services)
        {
            DataRepoDIExtensions.AddDatabaseLayerServices(services);
            services.AddScoped<IStoriesService, StoriesService>();
            return services;
        }
    }
}
