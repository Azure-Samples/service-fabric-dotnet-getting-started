#Deploy SimpleWebServer.exe locally
Connect-ServiceFabricCluster localhost:19000

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath '..\Service Fabric Package\SingleApplication' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'WebServer'

Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'WebServer'

New-ServiceFabricApplication -ApplicationName 'fabric:/WebServer' -ApplicationTypeName 'WebServerType' -ApplicationTypeVersion 1.0 

New-ServiceFabricService -ApplicationName 'fabric:/WebServer' -ServiceName 'fabric:/WebServer/WebServerService' -ServiceTypeName 'WebServer' -PartitionSchemeSingleton -Stateless -InstanceCount 1








#Deploy SimpleWebServer.exe to Azure
Connect-ServiceFabricCluster yourclusterendpoint

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath '..\Service Fabric Package\SingleApplication' -ImageStoreConnectionString 'fabric:imagestore' -ApplicationPackagePathInImageStore 'WebServer'

Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'WebServer'

New-ServiceFabricApplication -ApplicationName 'fabric:/WebServer' -ApplicationTypeName 'WebServerType' -ApplicationTypeVersion 1.0

New-ServiceFabricService -ApplicationName 'fabric:/WebServer' -ServiceName 'fabric:/WebServer/WebServerService' -ServiceTypeName 'WebServer' -PartitionSchemeSingleton -Stateless -InstanceCount 1
