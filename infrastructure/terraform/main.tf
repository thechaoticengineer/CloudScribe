terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Resource Group
resource "azurerm_resource_group" "cloudscribe" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}

# Azure Container Registry
resource "azurerm_container_registry" "cloudscribe" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.cloudscribe.name
  location            = azurerm_resource_group.cloudscribe.location
  sku                 = var.acr_sku
  admin_enabled       = true

  tags = var.tags
}

# AKS Cluster
resource "azurerm_kubernetes_cluster" "cloudscribe" {
  name                = var.aks_cluster_name
  location            = azurerm_resource_group.cloudscribe.location
  resource_group_name = azurerm_resource_group.cloudscribe.name
  dns_prefix          = var.aks_dns_prefix
  kubernetes_version  = var.kubernetes_version

  default_node_pool {
    name                = "default"
    node_count          = var.node_count
    vm_size             = var.vm_size
    os_disk_size_gb     = var.os_disk_size_gb
    enable_auto_scaling = var.enable_auto_scaling
    min_count           = var.enable_auto_scaling ? var.min_node_count : null
    max_count           = var.enable_auto_scaling ? var.max_node_count : null
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "standard"
    network_policy    = "azure"
  }

  tags = var.tags
}

# Role assignment for AKS to pull from ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  principal_id                     = azurerm_kubernetes_cluster.cloudscribe.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.cloudscribe.id
  skip_service_principal_aad_check = true
}

# Public IP for Keycloak LoadBalancer
resource "azurerm_public_ip" "keycloak" {
  count               = var.create_public_ip ? 1 : 0
  name                = "${var.aks_cluster_name}-keycloak-ip"
  location            = azurerm_resource_group.cloudscribe.location
  resource_group_name = azurerm_kubernetes_cluster.cloudscribe.node_resource_group
  allocation_method   = "Static"
  sku                 = "Standard"

  tags = merge(var.tags, {
    Service = "Keycloak"
  })
}

# Public IP for Blazor LoadBalancer
resource "azurerm_public_ip" "blazor" {
  count               = var.create_public_ip ? 1 : 0
  name                = "${var.aks_cluster_name}-blazor-ip"
  location            = azurerm_resource_group.cloudscribe.location
  resource_group_name = azurerm_kubernetes_cluster.cloudscribe.node_resource_group
  allocation_method   = "Static"
  sku                 = "Standard"

  tags = merge(var.tags, {
    Service = "Blazor"
  })
}
