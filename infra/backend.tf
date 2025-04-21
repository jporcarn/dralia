terraform {
  backend "azurerm" {
    resource_group_name  = "docplanner-store-dev-rg"
    storage_account_name = "draliastorejjppnn"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}
