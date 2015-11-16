$dp0 = Split-Path -parent $PSCommandPath
$SolutionDir = (Get-Item "$dp0\..\").FullName
$applicationPath = "$SolutionDir\pkg\Debug"

Connect-ServiceFabricCluster

Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $applicationPath -ApplicationPackagePathInImageStore "VisualObjectsApplication" -ImageStoreConnectionString fabric:ImageStore

Register-ServiceFabricApplicationType -ApplicationPathInImageStore "VisualObjectsApplication"
 
Start-ServiceFabricApplicationUpgrade -applicationName fabric:/VisualObjectsApplication -ApplicationTypeVersion 2.0.0.0 -UnmonitoredAuto
