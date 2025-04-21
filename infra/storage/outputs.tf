output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "storage_account_name" {
  description = "The name of the storage account"
  value       = azurerm_storage_account.tfstate.name
}

output "storage_container_name" {
  description = "The name of the storage container"
  value       = azurerm_storage_container.tfstate.name
}

output "storage_account_primary_endpoint" {
  description = "The primary endpoint for the storage account"
  value       = azurerm_storage_account.tfstate.primary_blob_endpoint
}
