namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using EnsureThat;
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the managers.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="serviceLifetime">The service lifetime.</param>
        /// <returns></returns>
        public static IServiceCollection AddManagers(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArgExtensions.HasItems(assemblies, nameof(assemblies));

            return services.RegisterValidators(assemblies, serviceLifetime).RegisterManagers(assemblies, serviceLifetime);
        }

        /// <summary>
        /// Adds the managers.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="serviceLifetime">The service lifetime.</param>
        /// <returns></returns>
        public static IServiceCollection AddManagers(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(assembly, nameof(assembly));

            return AddManagers(services, new[] { assembly }, serviceLifetime);
        }

        private static IServiceCollection RegisterManagers(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime)
        {
            var managers = assemblies.SelectMany(x => x.ExportedTypes
                .Where(t => typeof(IManager).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract));

            foreach (var manager in managers)
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(manager);
                        break;

                    case ServiceLifetime.Scoped:
                        services.AddScoped(manager);
                        break;

                    case ServiceLifetime.Transient:
                        services.AddTransient(manager);
                        break;

                    default:
                        throw new InvalidOperationException("The service lifetime provided while registering managers is not supported.");
                }
            }

            return services;
        }

        private static IServiceCollection RegisterValidators(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime)
        {
            var validators = assemblies.SelectMany(x => x.ExportedTypes
                .Where(t => typeof(IValidator).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract));

            foreach (var validator in validators)
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(validator);
                        break;

                    case ServiceLifetime.Scoped:
                        services.AddScoped(validator);
                        break;

                    case ServiceLifetime.Transient:
                        services.AddTransient(validator);
                        break;

                    default:
                        throw new InvalidOperationException("The service lifetime provided while registering managers is not supported.");
                }
            }

            return services;
        }
    }
}
