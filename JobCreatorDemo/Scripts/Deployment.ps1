#Note that this file is duplicative of what is present in Deploy-ServiceFabricApplication which is included
#in every Service Fabric project in VS by default. This shows real dynamic service creation, rather
#than relying on default services as the visual studio tooling does. It has the advantage of being able
#to be used outside of the VS environment. That said, it does not naturally understand environment profiles
#or the application parameters mechanism provided via the VS tooling. It is meant to serve as an example
#of manual application and service creation and configuration. 

$cloud = $true
$cloudAddress = ""
$constrainedNodeTypes = $false

$scriptPath = Get-Item(Convert-Path($MyInvocation.MyCommand.Path))
$scriptDirectory = Get-Item(Convert-Path($scriptPath.PSParentPath))
$appDirectory = Get-Item(Convert-Path($scriptDirectory.PSParentPath))
$rootName = $appDirectory.FullName

# This is the folder on your machine where the application package can be found
$packagePath = "$rootName\pkg\Debug\"
# Name of the folder that will appear in the image store
$appPackageInImageStore = "JobCreatorDemo-NDS"

# This is the partition range for the stateful service
$lowkey = "-9223372036854775808"
$highkey = "9223372036854775807" 

$appName = "fabric:/JobCreatorDemo-NDS"
$appName2 = "fabric:/JobCreatorDemo-NDS2"
$appType = "JobCreatorDemo-NDSType"
$appInitialVersion = "1.0.0"

# This is the name of the node type you are creating
# When you create a cluster, you will name the nodetype, that is what 
# you put here. If you have multiple node types, you will need to expand on 
# the number of node types
$frontendNodeType = "feworker"

# Cloud or local settings
if($cloud)
{
    $clusterAddress = $cloudAddress+":19000"
    $webServiceInstanceCount = -1
    $statefulPartitionCount = 5
    $imageStoreConnectionString = "fabric:ImageStore"
 }
else
{
    $clusterAddress = "localhost:19000"
    $webServiceInstanceCount = 1
    $statefulPartitionCount = 2
    $imageStoreConnectionString = "file:C:\SfDevCluster\Data\ImageStoreShare"
}

# if you have implemented service constraints...
if($constrainedNodeTypes)
{
    $webServiceConstraint = "NodeType == $frontendNodeType"
    $statefulServiceConstraint = "NodeType == $frontendNodeType"
     
}
else
{
    $webServiceConstraint = ""
    $statefulServiceConstraint = ""
}

# names and types of services in the app
$webServiceType = "WebServiceType"
$webServiceName = "WebService"

$statefulServiceType = "WorkServiceType"
$statefulServiceName = "WorkService"
$statefulServiceReplicaCount = 3

# These parameters end up in the ApplicationManifest.xml file
$parameters = @{}
$parameters.Add("WebService_AppPath","App1")

Write-Host "Connecting to $clusterAddress"

Connect-ServiceFabricCluster $clusterAddress

# The test is seeing if you aready have a package deployed. You should see an error if everyting is working correctly, meaning
# the test failed because it could not find the application package
Test-ServiceFabricApplicationPackage -ApplicationPackagePath $packagePath -ImageStoreConnectionString $imageStoreConnectionString

# Copy the package to the Image store
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $packagePath -ImageStoreConnectionString $imageStoreConnectionString `
-ApplicationPackagePathInImageStore $appPackageInImageStore

# Register the package on the image store
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $appPackageInImageStore

# Remove the package after it is registered
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $appPackageInImageStore

#
# Create App 1
#
New-ServiceFabricApplication -ApplicationName $appName -ApplicationTypeName $appType -ApplicationTypeVersion $appInitialVersion `
-ApplicationParameter $parameters

#create web api
New-ServiceFabricService -ServiceTypeName $webServiceType -Stateless -ApplicationName $appName -ServiceName "$appName/$webServiceName" `
-PartitionSchemeSingleton -InstanceCount $webServiceInstanceCount -PlacementConstraint $webServiceConstraint

#create stateful service
New-ServiceFabricService -ServiceTypeName $statefulServiceType -Stateful -HasPersistedState -ApplicationName $appName `
-ServiceName "$appName/$statefulServiceName" -PartitionSchemeUniformInt64 -MinReplicaSetSize $statefulServiceReplicaCount `
-TargetReplicaSetSize $statefulServiceReplicaCount -PartitionCount $statefulPartitionCount -LowKey $lowkey -HighKey $highkey `
-PlacementConstraint $statefulServiceConstraint 

#
# Create App 2
#
# You can't simply update a parameter, you have to remove it and add it back with a new value
$parameters.Remove("WebService_AppPath")
$parameters.Add("WebService_AppPath","App2")

New-ServiceFabricApplication -ApplicationName $appName2 -ApplicationTypeName $appType -ApplicationTypeVersion $appInitialVersion `
-ApplicationParameter $parameters

#create web api
New-ServiceFabricService -ServiceTypeName $webServiceType -Stateless -ApplicationName $appName2 -ServiceName "$appName2/$webServiceName" `
-PartitionSchemeSingleton -InstanceCount $webServiceInstanceCount -PlacementConstraint $webServiceConstraint

#create stateful service
New-ServiceFabricService -ServiceTypeName $statefulServiceType -Stateful -HasPersistedState -ApplicationName $appName2 `
-ServiceName "$appName2/$statefulServiceName" -PartitionSchemeUniformInt64 -MinReplicaSetSize $statefulServiceReplicaCount `
-TargetReplicaSetSize $statefulServiceReplicaCount -PartitionCount $statefulPartitionCount -LowKey $lowkey -HighKey $highkey `
-PlacementConstraint $statefulServiceConstraint 