# General outputs
output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "service_plan_name" {
  description = "The name of the Azure Service Plan"
  value       = azurerm_service_plan.main.name
}

# API outputs
output "web_app_name" {
  description = "The name of the Azure Linux Web App"
  value       = azurerm_linux_web_app.main.name
}

output "web_app_default_hostname" {
  description = "The default hostname of the Azure Linux Web App"
  value       = azurerm_linux_web_app.main.default_hostname
}

output "web_app_https_only" {
  description = "Indicates if HTTPS is enabled for the web app"
  value       = azurerm_linux_web_app.main.https_only
}

output "service_version" {
  value = var.service_version
}

output "credentials_username" {
  value = var.credentials_username
}

# Angular static site outputs
output "static_site_default_hostname" {
  description = "The default hostname of the Azure Static Web App."
  value       = azurerm_static_web_app.spa.default_host_name
}

output "static_site_id" {
  description = "The ID of the Azure Static Web App."
  value       = azurerm_static_web_app.spa.id
}

output "static_site_name" {
  description = "The name of the Azure Static Web App."
  value       = azurerm_static_web_app.spa.name
}

output "static_site_location" {
  description = "The location of the Azure Static Web App."
  value       = azurerm_static_web_app.spa.location
}
