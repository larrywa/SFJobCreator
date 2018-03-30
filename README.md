# How Not to use Default Services

Service Fabric is an ideal platform for microservices making it easy to package, deploy, and scale. As a developer, there are a lot of solutions that you don't have to code – Service Fabric just does them for you. Plus, Service Fabric is also pretty easy to try out considering its complexity. Using the Web Platform installer to get Service Fabric, reboot and open up Visual Studio and within minutes you can create and deploy some great default projects and just build from there. But when it's time to actually start upgrading your application and scaling your application, you may start noticing some issues. Especially if you modify the ApplicationManifest.xml for <DefaultSevices> and attempt an upgrade. Something happens, or rather nothing happens, it may look like your upgrade will take a very long time and may seemingly never complete.

The idea behind Default Services is that whenever you deploy an application package, the services you see in Default Services will automatically get started during deployment. Default Services settings in your application are there to help you get started and once you are ready for prime time, that's not the best way to manage your solution at scale. The recommendation instead is to use PowerShell to create your initial services after a clean deploy.

Why again did they put this default stuff in there if you are not supposed to use it? That's what I asked, but if you really think about this and what would make for best practice in the cloud, it makes a lot of sense. I like to think about it this way:

> Default Services == Quick Start

> PowerShell == Scale

## Using Powershell to Scale

Using PowerShell to scale your application instead of a text file makes a lot of sense, but it's not just that. If you continue to use Default Services to scale your application, it will teach you and your organization the "wrong model" for the cloud in several ways:

- It confuses service instance information with app type information
- It makes you think that app type/version upgrades are the way to change service instance parameters (and they are not)
- It makes you think that service instance information is maintained in the application type definition (since that’s where they get introduced to it)
- It doesn't teach your organization the APIs that they will need to do most real work (new/update)
- Using Default Services doesn't teach people that the set of services and their properties (as well as the set of applications and their properties) are dynamic and that services and app can be created and modified on the fly.

> The product group introduced us to an example on GitHub that does not use Default Services here: https://github.com/Azure-Samples/service-fabric-dotnet-data-aggregation

It is quite complex so Larry Wall (MSFT Premier Consultant) trimmed down the functionality in another app to illustrate solving the Default Services, or rather, not Default Services with this demo app here:
https://github.com/dkj0/SFJobCreator.

# Some things to note:

- If you open up ApplicationManifest.xml, you will see no <DefaultServices> section, which means that you need another way to start the services.
- The ApplicationManifest.xml is updated but nothing else in the app itself is touched.
     
There are a couple of PS scripts under the JobCreatorDemo-NDS\Scripts folder for you to check out:
- ApplicationPackageUpgrade.ps1 - this allows you to take an updated application package (config, code etc.) and update the version of the application that you have in your cluster. In this example file, the only thing that has changed is the ApplicationManifest.xml file. You could, however, update services or anything within the application and deploy it. You will find a sub-directory under the .\Scripts directory that contains the updated ApplicationManifest.xml. You would need to change the parameter names in the code to match your own app, or pass in parameters.
     
- Cleanup.ps1 - This script will clean up applications that have been installed in your cluster.
ConfigParamUpgrade.ps1 - This script can be used to update parameters that exist inside the ApplicationManifest.xml file.
Deployment.ps1 - This is the primary script to do the application deployment. You would need to change the application names and other parameters prior to running this.

# Deploying

To deploy using this method, just change the appropriate parameter values in Deploy.ps1, right-click on it in Visual Studio and select ‘Execute as Script’. I like to run it in PowerShell ISE just to see how it works, but when you get it working, that’s the fastest way to test from Visual Studio.

The only challenge with this model is debugging locally. If you keep the <DefaultServices> way of doing things, you can just put a breakpoint in your code and hit F5 and hit your breakpoint, but using no default services, you are instead going to have to attach the Visual Studio debugger to the process that's running. For example if you were running the JobCreatorDemo on your local cluster, you would see both WorkService.exe and WebService.exe running that you can attach to.

<hr>
Special thanks to Larry Wall for his help with this example.
https://github.com/dkj0/SFJobCreator


