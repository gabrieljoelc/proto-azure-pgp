# Deploy

Pre-requesites:
- terraform
- az CLI
- dotnet CLI
- \*nix os or something for zip command in `deploy.sh`
- zip command

1. Create resources:
```
terraform apply -var='app_settings={"APPLICATION_ID":"123","KEY_VAULT_URL":"https://mykeyvault.vault.azure.com","VAULT_KEY_PREFIX":"boknows","WEBSITE_LOAD_CERTIFICATES":"456"}'
```
2. Deploy code:
```bash
AZURE_RESOURCE_GROUP=my-resource-group AZURE_APP_SERVICE=my-app-service ./deploy.sh
```
or with environment variables:
```
AZURE_RESOURCE_GROUP=my-resource-group AZURE_APP_SERVICE=my-app-service ./deploy.sh
```

# Solution Progress

Tried using terraform registry module but it had a bug where it defaulted to Linux but it didn't set a required field (reserved)

I opened a PR for that and used my local version to create the resources. I discovered that web jobs are not supported for Linux app services...

I destroyed those resouces and created a windows app service

I followed this [post](https://blogs.msdn.microsoft.com/benjaminperkins/2017/03/07/how-to-deploy-a-net-core-console-application-to-azure-webjob/) to deploy the web job:
1. `dotnet publish`ed
1. Add `run.cmd` with the command to execute the console app
1. Zip the publish directory
1. Upload the zip file to the webjob in the Azure portal

## Steps as of 2019-08-24:
~1. Fix the hellow world version of the continuous web job getting stuck on "Pending Restart" - I think it's because it doesn't loop. it just terminates after printing hello world~
1. Add code from this the ssl [docs](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-ssl-cert-load) to the console app
1. Try running on the app service
1. If that doesn't work, import the cert from key vault?
  1. I did import the cert so we could fetch it from the app service
1. Try to fetch secrets from key vault using the cert

## Figure out why I'm getting:
```
[08/24/2019 22:24:22 > 580242: ERR ] Unhandled Exception: Microsoft.Azure.KeyVault.Models.KeyVaultErrorException: Client address is not authorized and caller is not a trusted service.
[08/24/2019 22:24:22 > 580242: ERR ] Client address: <ip address of app service i guess>
[08/24/2019 22:24:22 > 580242: ERR ] Caller: appid=ad725530-d6b5-442a-91a1-9e7acabe1a39;oid=74207611-1b8d-48e6-90db-db7f25fddaca;iss=https://sts.windows.net/e86bf018-a44a-45cc-9613-ab3643ec944a/
[08/24/2019 22:24:22 > 580242: ERR ] Vault: mdu-prod-eus-ftp-kv;location=eastus
[08/24/2019 22:24:22 > 580242: ERR ]    at Microsoft.Azure.KeyVault.KeyVaultClient.GetSecretWithHttpMessagesAsync(String vaultBaseUrl, String secretName, String secretVersion, Dictionary`2 customHeaders, CancellationToken cancellationToken)
[08/24/2019 22:24:22 > 580242: ERR ]    at Microsoft.Azure.KeyVault.KeyVaultClientExtensions.GetSecretAsync(IKeyVaultClient operations, String vaultBaseUrl, String secretName, CancellationToken cancellationToken)
[08/24/2019 22:24:22 > 580242: ERR ]    at proto_azure_pgp.Program.Main(String[] args) in /Users/gabe/dev/proto-azure-pgp/Program.cs:line 42
[08/24/2019 22:24:22 > 580242: ERR ]    at proto_azure_pgp.Program.<Main>(String[] args)
```
1. Added app service as AD identity in App Service > Identity
1. Add access policy for that specific identity - Got the same error
1. Tried adding the app service to the VNET the KV is in. Got this error:
  - > Legacy Cmak generation is not supported for gateway id <path to VNET gateway i think?> when vpn client protocol IkeV2 is configured. Please use vpn profile package option instead."
4. Found this note on Key Vault "Firewalls and virtual networks" section near the "Allow trusted Microsoft service to bypass this firewall option":
> This setting is related to firewall only. In order to access this key vault, the trusted service must also be given explicit permissions in the Access policies section.
  - I need to keep this in mind because originally I was try and avoid step 2. I need to see if there's a more scalable way to do it though (e.g. add app service to security group or principal or something?)
