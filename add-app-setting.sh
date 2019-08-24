# from https://docs.microsoft.com/en-us/azure/app-service/app-service-web-ssl-cert-load#make-the-certificate-accessible
az webapp config appsettings set --name $AZURE_APP_SERVICE --resource-group $AZURE_RESOURCE_GROUP --settings WEBSITE_LOAD_CERTIFICATES=$WEBSITE_LOAD_CERTIFICATES
