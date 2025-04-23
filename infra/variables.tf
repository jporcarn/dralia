variable "resource_group_name" {
  type    = string
  default = "docplanner-rg"
}

variable "location" {
  type    = string
  default = "westeurope"
}

variable "project_name" {
  type    = string
  default = "dralia-api"
}

variable "environment" {
  type    = string
  default = "Development"
}

variable "service_version" {
  description = "The tag being deployed, set via TF_VAR_service_version"
  type        = string
  # default     = "unknown" # Default value if TF_VAR_service_version is not set
}
variable "credentials_username" {
  description = "The username for the Availability API, set via TF_VAR_credentials_username"
  type        = string
}

variable "credentials_password" {
  description = "The password for the Availability API, set via TF_VAR_credentials_password"
  type        = string
}
