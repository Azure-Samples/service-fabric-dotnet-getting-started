// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.Service
{
    using System.Web.Http;
    using Microsoft.Practices.Unity;
    using Microsoft.ServiceFabric.Data;
    using Unity.WebApi;
    using global::WordCountService.Controllers;

    /// <summary>
    /// Configures dependency injection for Controllers using a Unity container. 
    /// </summary>
    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config, IReliableStateManager stateManager)
        {
            UnityContainer container = new UnityContainer();

            // The default controller needs a state manager to perform operations.
            // Using the DI container, we can inject it as a dependency.
            // This allows use to write unit tests against the controller using a mock state manager.
            container.RegisterType<DefaultController>(
                new TransientLifetimeManager(),
                new InjectionConstructor(stateManager));

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}