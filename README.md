# Dralia
## Doctor slots API

## Deploy to Azure

deploy-api.ps1
PS>
$securePassword = Read-Host "Enter Password" -AsSecureString
.\deploy-api.ps1 -Username "techuser" -Password $securePassword
