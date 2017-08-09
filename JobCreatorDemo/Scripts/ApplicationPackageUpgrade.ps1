# This script shows how to update an application package
# In this example, the application package only consists of ApplicationManifest.xml, but it could also contain 
# sub-folders with updated service config and code

$cloud = $true

# name of the folder where you are keeping your upgraded ApplicationManifest.xml file
$upgradeConfigFolder = "JobCreatorDemo-NDSConfigOnlyPackage"

# this is the name of the folder that would appear in your image store
$imageStoreFolder = "JobCreatorDemo-NDSV2"

# full name of the application - needs to be the same name as what is already deployed
$applicationName = "fabric:/JobCreatorDemo-NDS"

# new application version
$applicationVer = "2.0.0"

if($cloud)
{
    $imageStoreConnectionString = "fabric:ImageStore"
}
else
{
    $imageStoreConnectionString = "file:C:\SfDevCluster\Data\ImageStoreShare"
}

$scriptPath = Get-Item(Convert-Path($MyInvocation.MyCommand.Path))
$scriptDirectory = Get-Item(Convert-Path($scriptPath.PSParentPath))

$upgradePackagePath = "$scriptDirectory\$upgradeConfigFolder"

Test-ServiceFabricApplicationPackage -ApplicationPackagePath $upgradePackagePath -ImageStoreConnectionString $imageStoreConnectionString
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath $upgradePackagePath -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $imageStoreFolder
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $imageStoreFolder


Start-ServiceFabricApplicationUpgrade -ApplicationName $applicationName -ApplicationTypeVersion $applicationVer `
-Monitored -FailureAction Rollback -Force 
