namespace ZTR.Framework.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Bogus;
    using EnsureThat;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServicesExtensions
    {
        public static IServiceCollection AddFakers(this IServiceCollection services, Assembly assembly, params Assembly[] assemblies)
        {
            EnsureArg.IsNotNull(assembly, nameof(assembly));

            return AddFakers(services, assemblies.Prepend(assembly));
        }

        public static IServiceCollection AddFakers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(assemblies, nameof(assemblies));

            var fakers = assemblies.ToList().SelectMany(x => x.GetTypes()
                .Where(t => typeof(IFakerTInternal).IsAssignableFrom(t) && !t.IsAbstract));

            foreach (var faker in fakers)
            {
                services.AddScoped(faker);
            }

            return services;
        }
    }
}
