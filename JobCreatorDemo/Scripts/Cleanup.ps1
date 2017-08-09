# This script is used to remove the application from your cluster (local or in Azure) and also remove the app from the image store
# (local or in Azure). If you are running this local, uncomment the local image store name and comment out the fabric:ImageStore
# You will need to put in the appropriate cluster endpoint information

# Application name
$applicationName = "fabric:/JobCreatorDemo-NDS"
$applicationName2 = "fabric:/JobCreatorDemo-NDS2"

# Local image store
#$imageStoreConnectionString = "file:C:\SfDevCluster\Data\ImageStoreShare"

# Azure cluster image store
$imageStoreConnectionString = "fabric:ImageStore"

# Cluster endpoint
$clusterEndpoint = ""
#$clusterEndpoint = "localhost:19000"
Connect-ServiceFabricCluster $clusterEndpoint

Get-ServiceFabricApplication -ApplicationName $applicationName | Remove-ServiceFabricApplication -Force -ForceRemove
Get-ServiceFabricApplicationType -ApplicationTypeName $applicationName | Unregister-ServiceFabricApplicationType -Force

Get-ServiceFabricApplication -ApplicationName $applicationName2 | Remove-ServiceFabricApplication -Force -ForceRemove
Get-ServiceFabricApplicationType -ApplicationTypeName $applicationName2 | Unregister-ServiceFabricApplicationType -Force


#$folders = Get-ServiceFabricImageStoreContent -Path -ImageStoreConnectionString $imageStoreConnectionString -RemoteRelativePath "\"

$folders = Get-ServiceFabricImageStoreContent -ImageStoreConnectionString $imageStoreConnectionString -RemoteRelativePath "\"

if($folders -ne "Invalid location or unable to retrieve image store content")
{
    foreach($folder in $folders)
    {
        Remove-ServiceFabricApplicationPackage -ApplicationPackagePathInImageStore $folder.StoreRelativePath `
        -ImageStoreConnectionString $imageStoreConnectionString -Confirm:$false   
    }
}
