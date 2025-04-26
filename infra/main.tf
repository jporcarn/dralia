locals {
  tags = {
    author = "Josep Porcar Nadal"
    email  = "jjppnn@hotmail.com"
  }
}

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = local.tags
}

# web app
resource "azurerm_service_plan" "main" {
  name                = "${var.project_name}-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  os_type  = "Linux"
  sku_name = "B1"

  tags = local.tags
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
    "WEBSITES_PORT"                          = "8080"
    "ASPNETCORE_ENVIRONMENT"                 = var.environment          # Development or Production"
    "AVAILABILITYAPI__CREDENTIALS__USERNAME" = var.credentials_username # Set the username from the credentials map
    "AVAILABILITYAPI__CREDENTIALS__PASSWORD" = var.credentials_password # Set the password from the credentials map
    "SERVICE_VERSION"                        = var.service_version      # Set the service version from the latest tag
  }

  https_only = true

  tags = merge(local.tags, { version = var.service_version }) # Combine fixed and dynamic tags set from TF_VAR_service_version
}

# # Angular static site
resource "azurerm_static_web_app" "spa" {
  name                = "dralia-spa"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  tags = merge(local.tags, { version = var.service_version }) # Combine fixed and dynamic tags set from TF_VAR_service_version
}

