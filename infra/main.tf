
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# web app
resource "azurerm_service_plan" "main" {
  name                = "${var.project_name}-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  os_type  = "Linux"
  sku_name = "B1"
}

resource "azurerm_linux_web_app" "main" {
  name                = "${var.project_name}-app"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    "WEBSITES_PORT"          = "8080"
    "ASPNETCORE_ENVIRONMENT" = var.environment # Development or Production"
  }

  https_only = true
}

