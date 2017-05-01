#region References

using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Mvc;
using TimePilot.Service.Project;
using TimePilot.DataAccess;
using TimePilot.DataAccess.Repository;

#endregion

namespace TimePilot.App_Start
{
    public class Bootstrapper
    {
        public static IUnityContainer Initialize()
        {
            var container = BuildUnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        }

        private static void RegisterTypes(IUnityContainer container)
        {
            //Business layer services
            container.RegisterType<IProjectService, ProjectService>();
            container.RegisterType<IProjectRepository, ProjectRepository>();

        }
    }
}