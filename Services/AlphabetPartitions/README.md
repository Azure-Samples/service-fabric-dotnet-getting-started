## Run this sample

Alphabet partitions is an intro to partitioning stateful services in Service Fabric. It uses letters of the alphabet as partition keys into a stateful service with 26 partitions - one for each letter of the alphabet.

To run this services:

1. Open the .sln solution file in Visual Studio 2015
2. Press F5 to run

You can access the application in a web browser by going to:

**http://localhost:8081/alphabetpartitions?lastname=test**

Try different values for lastname to see data get sent to different partitions.

## Next Steps

- [Read more about partitioning](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-concepts-partitioning)
 