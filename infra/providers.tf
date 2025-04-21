# Azure Provider source and version being used
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.97.0" # or whatever is latest stable
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}
