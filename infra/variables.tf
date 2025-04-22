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

variable "latest_tag" {
  description = "The tag being deployed, set via TF_VAR_LATEST_TAG"
  type        = string
  default     = "unknown" # Default value if TF_VAR_LATEST_TAG is not set
}
