# ─────────────────────────────────────────────
# Configuration – Update to match your setup
# ─────────────────────────────────────────────

$solutionPath = "./src/Docplanner.Api.sln"
$projectPath = "./src/Docplanner.Api/Docplanner.Api.csproj"
$publishDir = "./publish"
$resourceGroup = "docplanner-dev-rg"
$appServiceName = "dralia-api-app"
$subscriptionName = "Pay as you go"

# Resolve the absolute path for the publish directory
$publishAbsoluteDir = (Resolve-Path $publishDir).Path

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
# Deploy to Azure App Service
# ─────────────────────────────────────────────

Write-Host "🚀 Deploying to Azure App Service '$appServiceName' using ZIP file..."
az webapp deploy --resource-group $resourceGroup --name $appServiceName --src-path $zipFilePath --type zip
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Deployment failed."
    exit 1
}

Write-Host "✅ Deployment completed successfully!"
