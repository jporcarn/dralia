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
