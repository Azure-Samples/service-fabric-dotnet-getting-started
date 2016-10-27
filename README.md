---
services: service-fabric
platforms: dotnet
author: seanmck
---

# Service Fabric Getting Started Samples

This repository contains a set of simple sample projects to help you get started with Service Fabric. If you are looking for larger samples, please take a look at some of the [other Service Fabric samples][service-fabric-samples] available in the Azure samples gallery.

## How the samples are organized

The samples are divided by the category and [Service Fabric programming model][service-fabric-programming-models] that they focus on: Reliable Actors, Reliable Services, custom application orchestration, and Service Fabric management tasks. Note that most real applications will include a mixture of the concepts and programming models so once you have reviewed these samples, you are encouraged to look at the more complex samples which combine the two frameworks.

## Actor samples
### VoicemailBox

VoicemailBox provides an example of a simple actor, representing a voicemail account. The actor interface defines methods for adding, deleting, and querying messages and for managing the account's greeting. The application includes a web UI accessible at http://&lt;clusteraddress&gt;:8081/voicemailbox/ and a set of web APIs that can be used to interact with the actor methods.

### VisualObjects

The VisualObjects sample provides a visual representation of how actors provide full encapsulation of behavior and state.

It also provides a clear demonstration of how Service Fabric performs rolling upgrades as the behavior of the shapes can be seen to gradually change as the upgrade proceeds across the upgrade domains in the cluster. To see how to perform a rolling upgrade using the Visual Objects sample, see the [application upgrade tutorial][app-upgrade-tutorial].

## Service Samples
### AlphabetPartitions

AlphabetPartitions provides an example of creating a partitioned, scaled-out service with 26 partitions, one for each letter in the alphabet. The partitionKey to call a particular partition from the Web service to the Processing service is created by simply looking that the first letter in the lastname parameter that is supplied in the URL.

To demonstrate this service, open up a browser and type http://localhost:8081/alphabetpartitions?lastname=FUSSELL

In the reply you should see something similar to below, with a different partition ID and service replica address

Result: User FUSSELL successfully added

Partition key: 'Microsoft.ServiceFabric.Services.Client.ServicePartitionKey' generated from the first letter 'F' of input value 'FUSSELL'.

Processing service partition ID: 446847de-9131-4670-bc02-408d5bafb5bd.
 
Processing service replica address: Http://localhost:8089/446847de-9131-4670-bc02-408d5bafb5bd/131036145073838836-396b611d-9b91-4d78-8393-c3217221c422 

The partitionKey generation is in the Web.cs file in the solution. This is simple approach to partitioning your compute and data. Typically you perform a hash function on some data from the client to choose a particular partition. See the "Scale Apps" section in the documentation for more information.  

### Chatter

Chatter is currently being upgraded to ASP.NET Core 1.0 and will return soon.

### WordCount

WordCount provides an introduction to using reliable collections and to partitioning stateful services. A client-side JavaScript function generates random five-character strings, which are then sent to the application via an ASP.NET WebAPI to be counted. The stateless web service resolves the endpoint for the stateful service's partition based on the first character of the string. The stateful service maintains a backlog of words to count in a `ReliableQueue` and then keeps track of their count in a `ReliableDictionary`. The total count, plus a per-partition count, are shown in the web UI at http://&lt;clusteraddress&gt;:8081/WordCount/.

### WCFService

#### Calculator.Service
Calculator Service provides a way to integrate your existing WCF Services with service fabric framework .This shows how to create Stateless Service using communication as WCF Http binding.

#### Calculator.Client
This shows how to create WCF Client and how to connect the the service endpoint. 
CalculatorClient is a Wcf Client uses ClientFactoryBase which in turn provides various features like resolving endpoints during Service Failover  , ExceptionHandling and maintains a cache of communication
clients and attempts to reuse the clients for requests to the same service endpoint.
It is using BasicHttpBinding . Any WCF binding can be chosen. It should be compatible with service binding.

## Guest Executables Samples
### SimpleApplication

The simple guest executable sample shows how to take an arbitrary EXE that is not built on Actors, Services, or any Service Fabric APIs and deploy it to a Service Fabric cluster. The EXE it uses is SimpleWebServer.exe, which is a simple web server that listens on port 8080 and returns the machine name based on a query string parameter.

## Management Samples
### ClusterMonitor

ClusterMonitor shows how to use the REST APIs provided by Service Fabric to query the state of the cluster and its running applications. It is also useful as a tool for learning how concepts like partitioning and failover work. The application's web UI is accessible at http://&lt;clusteraddress&gt;:8081/cluster/ and shows the cluster with all deployed replicas represented within the node that is currently hosting them.

To see how Service Fabric automatically rebalances replicas in the cluster when a node fails, do the following:

1. Deploy an application to your cluster. This could be a sample application or one you have built.
2. Deploy the ClusterMonitor sample.
3. Launch Service Fabric Explorer by navigating to http://&lt;clusteraddress&gt;:19080/Explorer.
4. Choose a node and click Actions > Deactivate (Restart).

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
