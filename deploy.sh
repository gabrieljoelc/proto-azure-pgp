dotnet publish --configuration Release -o .deploy/app_data/Jobs/Continuous/pgp
cp run.cmd .deploy/app_data/Jobs/Continuous/pgp
# this is the only way archive only the contents of .deploy so that it isn't the root directory when deployed (see https://askubuntu.com/a/743860/812363)
cd .deploy ; zip -r ../deploy.zip . * ; cd ..
az webapp deployment source config-zip -g $AZURE_RESOURCE_GROUP -n $AZURE_APP_SERVICE --src ./deploy.zip
