# SFJobCreator

The Microsoft Service Fabric product group introduced us to an example on GitHub that does not use Default Services here: 

This is a sample Service Fabric application taken from https://github.com/Azure-Samples/service-fabric-dotnet-data-aggregation.

It is quite complex so Larry trimmed down the functionality in another app to illustrate solving the Default Services, or rather, not Default Services and we've posted the modified demo app here. Some things to note: 

If you open up ApplicationManifest.xml, you will see no <DefaultServices> section, which means that you need another way to start the services.

The ApplicationManifest.xml is updated but nothing else in the app itself is touched. 

There are a couple of PS scripts under the JobCreatorDemo-NDS\Scripts folder for you to check out:
     
ApplicationPackageUpgrade.ps1
      - this allows you to take an updated application package (config, code etc.) and update the version of the application that you have in your cluster. In this example file, the only thing that has changed is the ApplicationManifest.xml file. You could, however, update services or anything within the application and deploy it. You will find a sub-directory under the .\Scripts directory that contains the updated ApplicationManifest.xml. You would need to change the parameter names in the code to match your own app, or pass in parameters.

Cleanup.ps1
      - This script will clean up applications that have been installed in your cluster.

ConfigParamUpgrade.ps1
      - This script can be used to update parameters that exist inside the ApplicationManifest.xml file.

Deployment.ps1
      - This is the primary script to do the application deployment. You would need to change the application names and other parameters prior to running this.

To deploy using this method, just change the appropriate parameter values in Deploy.ps1, right-click on it in Visual Studio and select ‘Execute as Script’. I like to run it in PowerShell ISE just to see how it works, but when you get it working, that’s the fastest way to test from Visual Studio. 

The only challenge with this model is debugging locally. If you keep the <DefaultServices> way of doing things, you can just put a breakpoint in your code and hit F5 and hit your breakpoint, but using no default services, you are instead going to have to attach the Visual Studio debugger to the process that's running. For example if you were running the JobCreatorDemo on your local cluster, you would see both WorkService.exe and WebService.exe running that you can attach to.


