namespace LightMapper.DependencyInjection;

/// <summary>
/// When applied to the assembly, emits an extension method that registers all generated
/// <see cref="ILightMapper{TSource, TDestination}"/> implementations as singletons.
/// The consuming project must reference the Microsoft.Extensions.DependencyInjection package (or a metapackage that includes it).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GenerateLightMapperServiceRegistrationsAttribute : Attribute
{
}
