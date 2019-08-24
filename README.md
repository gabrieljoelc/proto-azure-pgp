Tried using terraform registry module but it had a bug where it defaulted to Linux but it didn't set a required field (reserved)

I opened a PR for that and used my local version to create the resources. I discovered that web jobs are not supported for Linux app services...

I destroyed those resouces and created a windows app service

I followed this [post](https://blogs.msdn.microsoft.com/benjaminperkins/2017/03/07/how-to-deploy-a-net-core-console-application-to-azure-webjob/) to deploy the web job:
1. `dotnet publish`ed
1. Add `run.cmd` with the command to execute the console app
1. Zip the publish directory
1. Upload the zip file to the webjob in the Azure portal

Next steps:
1. Fix the hellow world version of the continuous web job getting stuck on "Pending Restart" - I think it's because it doesn't loop. it just terminates after printing hello world
1. Add code from this the ssl [docs](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-ssl-cert-load) to the console app
1. Try running on the app service
1. If that doesn't work, import the cert from key vault?
