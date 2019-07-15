---
page_type: sample
description: The project contains an application demonstrating the concepts needed to get started building highly-available, distributed applications.
languages:
- csharp
products:
- azure
---

# Service Fabric Getting Started Sample
This repository contains an introductory sample project for [Microsoft Azure Service Fabric](https://azure.microsoft.com/services/service-fabric/). The sample project contains a single application with multiple services demonstrating the basic concepts needed to get you started building highly-available, scalable, distributed applications.

 > *Looking for the older getting started samples? They're in the [classic branch](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/tree/classic).*

More info on Service Fabric:
 - [Documentation](https://docs.microsoft.com/azure/service-fabric/)
 - [More sample projects](https://azure.microsoft.com/resources/samples/?service=service-fabric)
 - [Service Fabric open source home repo](https://github.com/azure/service-fabric)

## Building and deploying

First, [set up your development environment](https://docs.microsoft.com/azure/service-fabric/service-fabric-get-started) with [Visual Studio 2017](https://www.visualstudio.com/vs/). Make sure you have at least version **15.1** of Visual Studio 2017 installed.

 > *Looking for the Visual Studio 2015 version? It's in the [vs2015 branch](https://github.com/Azure-Samples/service-fabric-dotnet-getting-started/tree/vs2015).*

This sample application can be built and deployed immediately using Visual Studio 2017. To deploy on the local cluster, you can simply hit F5 to debug the sample. If you'd like to try publishing the sample to an Azure cluster:

1. Right-click on the application project in Solution Explorer and choose Publish.
2. Sign-in to the Microsoft account associated with your Azure subscription.
3. Choose the cluster you'd like to deploy to.
    * Ensure that your cluster has the reverse proxy feature enabled, [more info](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reverseproxy#setup-and-configuration)
4. Add a Load Balancing Rule to map the inbound port 8081 to the same port on the backend pool, [more info](https://docs.microsoft.com/en-us/azure/load-balancer/load-balancer-get-started-internet-portal#create-a-probe-lb-rule-and-nat-rules)

## About this sample application

The sample application contains several services, each demonstrating how to use key parts of Service Fabric.

### Web Service

This is a stateless front-end web service using [ASP.NET Core in a Reliable Service](https://docs.microsoft.com/azure/service-fabric/service-fabric-reliable-services-communication-aspnetcore). This service demonstrates a basic front-end service that acts as a gateway for users into your application. It presents a single-page application UI and an HTTP API to interact with the rest of the application. This is the only service that exposes an endpoint to the Internet for users to interact with, and all user ingress to the application comes through this service.

#### Key concepts

 - Stateless public-facing ASP.NET Core service using WebListener
 - Communicating with other services in a variety of ways:
    - Over HTTP to another service using the Service Fabric Reverse Proxy
    - RPC calls with Service Remoting
    - RPC calls to a Reliable Actor service
 - Communicating with partitioned services, including partition key generation, querying, and aggregation.
 - Using the Service Fabric cluster querying APIs
 - Dependency injection of Service Fabric components into an ASP.NET Core application

### Guest EXE Backend Service

This is a simple [guest executable service](https://docs.microsoft.com/azure/service-fabric/service-fabric-deploy-existing-app) that is not built on Reliable Services, Reliable Actors, or any Service Fabric APIs. It is a pre-compiled EXE that is packaged up and run on Service Fabric as-is. The EXE is a simple web server that returns the machine name of the node it is currently running on.

#### Key concepts

 - How to take an existing executable and host it on Service Fabric.

### Stateless Backend Service
This is a stateless back-end service using [Service Remoting](https://docs.microsoft.com/azure/service-fabric/service-fabric-reliable-services-communication-remoting). This service demonstrates a basic back-end service that can be used for processing work with an RPC API to check the status of that work.

#### Key concepts
 - Stateless internal-only service
 - Setting up Service Remoting

### Stateful Backend Service
This is a stateful back-end service using ASP.NET Core. This service demonstrates the use of [Reliable Collections](https://docs.microsoft.com/azure/service-fabric/service-fabric-reliable-services-reliable-collections) in a stateful service. It presents an internal-only HTTP API for other services to interact with.

#### Key concepts
 - Stateful internal-only ASP.NET Core service using Kestrel
 - Reliable Collections
 - Service partitioning
 - Dependency injection of IReliableStateManager for use in MVC controllers

### Actor Backend Service
This is a back-end [Reliable Actor]() service. This service demonstrates how to set up a Reliable Actor service and use its features to implement a simple process service using the Virtual Actor model.

#### Key concepts

 - Reliable Actors with persisted state
 - Reminders and basic Reliable Actor lifecycle


---
*This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.*
