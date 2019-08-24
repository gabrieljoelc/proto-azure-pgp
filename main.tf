provider "azurerm" {
  version = ">= 1.6.0"
}

terraform {
  required_version = ">= 0.11.0"
}


# ###############
# required values
# ###############

variable "name" {
  description = "The name of the web app"
}

variable "resource_group_name" {
  description = "The name of the resource group in which the resources will be created."
}

# ###############
# optional values
# ###############

variable "location" {
  description = "Region where the resources are created."
  default     = "westus"
}

variable "plan_settings" {
  type        = "map"
  description = "Definition of the dedicated plan to use"

  default = {
    #kind     = "Linux" # Linux or Windows
    kind     = "Windows" # Linux or Windows
    size     = "S1"
    capacity = 1
    tier     = "Standard"
    #reserved     = true
    reserved     = false
  }
}

variable "service_plan_name" {
  description = "The name of the App Service Plan, default = $web_app_name"
  default     = ""
}

variable "app_settings" {
  description = "A key-value pair of App Settings"
  default     = {}
}

variable "site_config" {
  description = "A key-value pair for Site Config"
  type        = "list"

  default = []
}

#resource "azurerm_resource_group" "webapp" {
#  name     = "${var.resource_group_name == "" ? replace(var.name, "/[^a-z0-9]/", "RG") : var.resource_group_name}"
data "azurerm_resource_group" "webapp" {
  name     = "${var.resource_group_name}"
}

resource "azurerm_app_service_plan" "webserviceplan" {
  name                = "${var.service_plan_name == "" ? replace(var.name, "/[^a-z0-9]/", "") : var.service_plan_name}"
  location            = "${data.azurerm_resource_group.webapp.location}"
  resource_group_name = "${data.azurerm_resource_group.webapp.name}"
  
  kind                = "${var.plan_settings["kind"]}"

  reserved            = "${var.plan_settings["reserved"]}"
  
  sku {
    tier     = "${var.plan_settings["tier"]}"
    size     = "${var.plan_settings["size"]}"
    capacity = "${var.plan_settings["capacity"]}"
  }
}

resource "azurerm_app_service" "webapp" {
  name                = "${var.name}"
  location            = "${data.azurerm_resource_group.webapp.location}"
  resource_group_name = "${data.azurerm_resource_group.webapp.name}"
  app_service_plan_id = "${azurerm_app_service_plan.webserviceplan.id}"
  site_config         = "${var.site_config}"
  app_settings        = "${var.app_settings}"
}

#module "webapp" {
#  #source  = "rahulkhengare/webapp/azurerm"
#  source = "git@github.com:gabrieljoelc/terraform-azurerm-webapp.git?ref=linux-reserved-true-fix"
#  version = "0.1.0"
#  name                = "mdu-proto-pgp-job"
#  resource_group_name = "proto"
#  location = "centralus"
#}
