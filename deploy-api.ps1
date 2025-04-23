# ─────────────────────────────────────────────
# Configuration – Update to match your setup
# ─────────────────────────────────────────────
param (
    [string]$Username,
    [SecureString]$Password
)


$solutionPath = "./src/Docplanner.Api.sln"
$projectPath = "./src/Docplanner.Api/Docplanner.Api.csproj"
$publishDir = "./publish"
$resourceGroup = "docplanner-dev-rg"
$appServiceName = "dralia-api-app"
$subscriptionName = "Pay as you go"
$infraDir = "./infra" # Path to the Terraform configuration directory

# Convert SecureString to Plain Text for Azure CLI
$PlainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
)

# Debugging output
# Write-Host "Plain Password: $PlainPassword"

# ─────────────────────────────────────────────
# Azure Login
# ─────────────────────────────────────────────

Write-Host "🔐 Logging in to Azure..."
az login
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Azure login failed. Aborting."
    exit 1
}

Write-Host "🎯 Setting subscription to '$subscriptionName'..."
az account set --subscription "$subscriptionName"
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Failed to set subscription. Aborting."
    exit 1
}

# ─────────────────────────────────────────────
# Deploy Infrastructure with Terraform
# ─────────────────────────────────────────────

Write-Host "🌍 Deploying infrastructure with Terraform..."

# Initialize Terraform
Write-Host "🔧 Initializing Terraform..."
# Move to the Terraform directory
Set-Location $infraDir
terraform init
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Terraform initialization failed. Aborting."
    exit 1
}

# Set Terraform environment variables
$env:TF_VAR_service_version = "local"
$env:TF_VAR_credentials_username = $Username
$env:TF_VAR_credentials_password = $PlainPassword

# Debugging output
Write-Host "TF_VAR_service_version: $env:TF_VAR_service_version"
Write-Host "TF_VAR_credentials_username: $env:TF_VAR_credentials_username"
Write-Host "TF_VAR_credentials_password: $env:TF_VAR_credentials_password"

Write-Host "🔍 Running Terraform plan to verify TF_VAR_* variable values..."
terraform plan
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Terraform plan failed. Aborting."
    exit 1
}

# Apply Terraform configuration using TF_VAR_* variable values...
Write-Host "🚀 Applying Terraform configuration using TF_VAR_* variable values..."
terraform apply -auto-approve
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Terraform apply failed. Aborting."
    exit 1
}

Write-Host "✅ Infrastructure deployed successfully!"

# Return to the original directory
Set-Location ..

# ─────────────────────────────────────────────
# Build the solution
# ─────────────────────────────────────────────

Write-Host "🧱 Building solution..."
dotnet build $solutionPath --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build failed. Aborting."
    exit 1
}

# ─────────────────────────────────────────────
# Run Unit Tests
# ─────────────────────────────────────────────

Write-Host "🧪 Running unit tests..."
dotnet test $solutionPath --configuration Debug --filter FullyQualifiedName~"*Tests.Unit"
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Unit tests failed. Aborting."
    exit 1
}

# ─────────────────────────────────────────────
# Run Integration Tests
# ─────────────────────────────────────────────

Write-Host "🧪 Running integration tests..."
dotnet test $solutionPath --configuration Debug --filter FullyQualifiedName~"*Tests.Integration"
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Integration tests failed. Aborting."
    exit 1
}

# ─────────────────────────────────────────────
# Publish the project
# ─────────────────────────────────────────────

Write-Host "📦 Publishing project..."
dotnet publish $projectPath --configuration Debug --output $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Publish failed. Aborting."
    exit 1
}

# ─────────────────────────────────────────────
# Create a ZIP file from the publish directory
# ─────────────────────────────────────────────

$zipFilePath = "$publishDir.zip"
Write-Host "📦 Creating ZIP file at '$zipFilePath'..."
if (Test-Path $zipFilePath) {
    Remove-Item $zipFilePath -Force
}
Compress-Archive -Path $publishDir\* -DestinationPath $zipFilePath
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Failed to create ZIP file. Aborting."
    exit 1
}

Write-Host "✅ ZIP file created successfully at '$zipFilePath'."

# ─────────────────────────────────────────────
# Deploy to Azure App Service
# ─────────────────────────────────────────────

Write-Host "🚀 Deploying to Azure App Service '$appServiceName' using ZIP file..."
az webapp deploy --resource-group $resourceGroup --name $appServiceName --src-path $zipFilePath --type zip --verbose
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Deployment failed."
    exit 1
}

Write-Host "✅ Deployment completed successfully!"