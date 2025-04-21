resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# storage account for tfstate
resource "azurerm_storage_account" "tfstate" {
  name                     = "draliastorejjppnn" # must only consist of lowercase letters and numbers, and must be between 3 and 24 characters long
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

}

resource "azurerm_storage_container" "tfstate" {
  name                  = "tfstate"
  storage_account_name  = azurerm_storage_account.tfstate.name
  container_access_type = "private"
}
