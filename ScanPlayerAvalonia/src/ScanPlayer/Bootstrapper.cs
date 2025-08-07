using ScanPlayer.Models;
using Splat;

namespace ScanPlayer
{
    internal static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver? services = null, IReadonlyDependencyResolver? resolver = null)
        {
            var container = services ?? Locator.CurrentMutable;

            // Services
            container.RegisterLazySingleton(() => new Workspace());
        }
    }
}
