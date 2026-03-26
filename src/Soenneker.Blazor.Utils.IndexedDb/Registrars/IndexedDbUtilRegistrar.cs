using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Registrars;

namespace Soenneker.Blazor.Utils.IndexedDb.Registrars;

/// <summary>
/// Registration for the interop and utility services.
/// </summary>
public static class IndexedDbUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IIndexedDbInterop"/> and <see cref="IIndexedDbUtil"/> as scoped services.
    /// </summary>
    public static IServiceCollection AddIndexedDbUtilAsScoped(this IServiceCollection services)
    {
        services.AddResourceLoaderAsScoped()
                .TryAddScoped<IIndexedDbInterop, IndexedDbInterop>();

        services.TryAddScoped<IIndexedDbUtil, IndexedDbUtil>();

        return services;
    }
}
