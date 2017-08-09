# This script can be used to update a parameter that exists in the ApplicationManifest.xml file
# This helps if you have a service already running and you want to make changes (other than partition
# information. You may also want to pass in, as a parameter the application name and version but below
# it is hard coded

$parameters = @{}
$parameters.Add("WebService_AppPath","App1")

$applicationName = "fabric:/JobCreatorDemo-NDS"
$applicationVer = "1.0.0"
Start-ServiceFabricApplicationUpgrade -ApplicationName $applicationName -ApplicationTypeVersion $applicationVer `
-Monitored -FailureAction Rollback -UpgradeDomainTimeoutSec 360 -HealthCheckRetryTimeoutSec 10 -ApplicationParameter $parameters -Force 