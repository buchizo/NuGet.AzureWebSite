using System.Data.Services;
using System.ServiceModel.Activation;
using System.Web.Routing;
using Ninject;
using NuGet.AzureWebSite.DataServices;
using NuGet.AzureWebSite.Infrastructure;
using NuGet.AzureWebSite.Publishing;
using RouteMagic;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NuGet.AzureWebSite.Sample.NuGetRoutes), "Start")]

namespace NuGet.AzureWebSite.Sample {
    public static class NuGetRoutes {
        public static void Start() {
            MapRoutes(RouteTable.Routes);
        }

        private static void MapRoutes(RouteCollection routes) {
            // The default route is http://{root}/nuget/Packages
            var factory = new DataServiceHostFactory();
            var serviceRoute = new ServiceRoute("nuget", factory, typeof(Packages));
            serviceRoute.Defaults = new RouteValueDictionary { { "serviceType", "odata" } };
            serviceRoute.Constraints = new RouteValueDictionary { { "serviceType", "odata" } };
            routes.Add("nuget", serviceRoute);
        }

        private static PackageService CreatePackageService() {
            return NinjectBootstrapper.Kernel.Get<PackageService>();
        }
    }
}
