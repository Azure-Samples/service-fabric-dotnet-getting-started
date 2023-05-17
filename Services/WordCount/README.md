## Run this sample

Alphabet partitions is an intro to partitioning stateful services in Service Fabric. It uses letters of the alphabet as partition keys into a stateful service with 26 partitions - one for each letter of the alphabet.

To run this services:

1. Open the .sln solution file in Visual Studio 2019 or 2022
2. Install latest runtime and SDK from here: https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-get-started
3. Set WordCount as Startup Project
4. Press F5 to run

You can access the application in a web browser by going to:

**http://localhost:8081/WordCount/


Try different values for lastname to see data get sent to different partitions.

## Next Steps

- [Read more about ReliableServices and  Reliable Collections ](https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-introduction)
 