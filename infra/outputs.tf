output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "service_plan_name" {
  description = "The name of the Azure Service Plan"
  value       = azurerm_service_plan.main.name
}

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
