output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.cloudscribe.name
}

output "aks_cluster_name" {
  description = "The name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.cloudscribe.name
}

output "aks_cluster_id" {
  description = "The ID of the AKS cluster"
  value       = azurerm_kubernetes_cluster.cloudscribe.id
}

output "aks_kubeconfig" {
  description = "Kubeconfig for the AKS cluster"
  value       = azurerm_kubernetes_cluster.cloudscribe.kube_config_raw
  sensitive   = true
}

output "aks_cluster_fqdn" {
  description = "The FQDN of the AKS cluster"
  value       = azurerm_kubernetes_cluster.cloudscribe.fqdn
}

output "aks_node_resource_group" {
  description = "The auto-generated resource group which contains the resources for this managed Kubernetes cluster"
  value       = azurerm_kubernetes_cluster.cloudscribe.node_resource_group
}

output "acr_name" {
  description = "The name of the Azure Container Registry"
  value       = azurerm_container_registry.cloudscribe.name
}

output "acr_login_server" {
  description = "The login server URL for the Azure Container Registry"
  value       = azurerm_container_registry.cloudscribe.login_server
}

output "acr_admin_username" {
  description = "The admin username for the Azure Container Registry"
  value       = azurerm_container_registry.cloudscribe.admin_username
  sensitive   = true
}

output "acr_admin_password" {
  description = "The admin password for the Azure Container Registry"
  value       = azurerm_container_registry.cloudscribe.admin_password
  sensitive   = true
}

output "public_ip_address" {
  description = "The static public IP address (if created)"
  value       = var.create_public_ip ? azurerm_public_ip.cloudscribe[0].ip_address : null
}

output "kube_config_command" {
  description = "Command to configure kubectl"
  value       = "az aks get-credentials --resource-group ${azurerm_resource_group.cloudscribe.name} --name ${azurerm_kubernetes_cluster.cloudscribe.name}"
}
