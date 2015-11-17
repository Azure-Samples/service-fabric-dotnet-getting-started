---
services: service-fabric
platforms: dotnet
author: seanmck
---

# Service Fabric Getting Started Samples

This repository contains a set of simple sample projects to help you get started with Service Fabric. If you are looking for larger samples, please take a look at some of the [other Service Fabric samples][service-fabric-samples] available in the Azure samples gallery.

## How the samples are organized

The samples are divided into two groups based on the [Service Fabric programming model][service-fabric-programming-models] that they focus on: either Reliable Actors or Reliable Services. Note that most real applications will include a mixture of the two programming models so once you have reviewed these samples, you are encouraged to look at the more complex samples which combine the two frameworks.

## Actor samples
### VoicemailBox

VoicemailBox provides an example of a simple actor, representing a voicemail account. The actor interface defines methods for adding, deleting, and querying messages and for managing the account's greeting. The application includes a web UI accessible at http://&lt;clusteraddress&gt;:8081/voicemailbox/ and a set of web APIs that can be used to interact with the actor methods.

### VisualObjects

The VisualObjects sample provides a visual representation of how actors provide full encapsulation of behavior and state.

It also provides a clear demonstration of how Service Fabric performs rolling upgrades as the behavior of the shapes can be seen to gradually change as the upgrade proceeds across the upgrade domains in the cluster. To see how to perform a rolling upgrade using the Visual Objects sample, see the [application upgrade tutorial][app-upgrade-tutorial].

**Important note:** The VisualObjects web service depends on two JavaScript files that are not directly included in the sample due the licensing restrictions. Follow the instructions in the Readme to learn how to set up the sample.

## Service Samples
### Chatter

Chatter is a single-room chat application. It includes a web UI built in ASP.NET 5 and accessible at http://&lt;clusteraddress&gt;:8081/Chatter/. User messages are sent to a stateful service for persistence.

### ClusterMonitor

ClusterMonitor shows how to use the REST APIs provided by Service Fabric to query the state of the cluster and its running applications. It is also useful as a tool for learning how concepts like partitioning and failover work. The application's web UI is accessible at http://&lt;clusteraddress&gt;:8081/ClusterMonitor/ and shows the cluster with all deployed replicas represented within the node that is currently hosting them.

To see how Service Fabric automatically rebalances replicas in the cluster when a node fails, do the following:

1. Deploy an application to your cluster. This could be a sample application or one you have built.
2. Deploy the ClusterMonitor sample.
3. Launch Service Fabric Explorer by navigating to http://&lt;clusteraddress&gt;:19080/Explorer.
4. Choose a node and click Actions > Deactivate (Restart).

### WordCount

WordCount provides an introduction to using reliable collections and to partitioning stateful services. A client-side JavaScript function generates random five-character strings, which are then sent to the application via an ASP.NET WebAPI to be counted. The stateless web service resolves the endpoint for the stateful service's partition based on the first character of the string. The stateful service maintains a backlog of words to count in a `ReliableQueue` and then keeps track of their count in a `ReliableDictionary`. The total count, plus a per-partition count, are shown in the web UI at http://&lt;clusteraddress&gt;:8081/WordCount/.

## Deploying the samples

With the exception of VisualObjects, which requires a small amount of setup, all of the samples can be loaded and deployed immediately using Visual Studio. To deploy on the local cluster, you can simply hit F5 to debug the sample. If you'd like to try publishing the sample to an Azure cluster:

1. Right-click on the application project in Solution Explorer and choose Publish.
2. Sign-in to the Microsoft account associated with your Azure subscription.
3. Choose the cluster you'd like to deploy to.

## More information

The [Service Fabric documentation][service-fabric-docs] includes a rich set of tutorials and conceptual articles, which serve as a good complement to the samples.

<!-- Links -->

[service-fabric-samples]: http://aka.ms/servicefabricsamples
[service-fabric-programming-models]: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-choose-framework/
[app-upgrade-tutorial]: https://azure.microsoft.com/en-us/documentation/articles/service-fabric-application-upgrade-tutorial/
[service-fabric-docs]: http://aka.ms/servicefabricdocs
