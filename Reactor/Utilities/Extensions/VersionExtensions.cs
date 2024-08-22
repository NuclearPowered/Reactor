using SemanticVersioning;

namespace Reactor.Utilities.Extensions;

/// <summary>
/// Provides extension methods for <see cref="SemanticVersioning.Version"/>.
/// </summary>
public static class VersionExtensions
{
    /// <summary>
    /// Gets the provided <paramref name="version"/> without the build string (everything after the + symbol like the commit hash is stripped).
    /// </summary>
    /// <param name="version">The <see cref="SemanticVersioning.Version"/>.</param>
    /// <returns>The <paramref name="version"/> without build.</returns>
    public static Version WithoutBuild(this Version version)
    {
        return new Version(version.Major, version.Minor, version.Patch, version.PreRelease);
    }
}
