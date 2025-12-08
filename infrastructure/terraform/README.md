# CloudScribe - Terraform Infrastructure

This directory contains Terraform configurations for provisioning Azure Kubernetes Service (AKS) infrastructure for CloudScribe.

## What Gets Provisioned

- **Azure Resource Group** - Container for all Azure resources
- **Azure Kubernetes Service (AKS)** - Managed Kubernetes cluster
- **Azure Container Registry (ACR)** - Private Docker registry
- **Role Assignments** - AKS permissions to pull images from ACR
- **Public IP** (optional) - Static IP for LoadBalancer services

## Prerequisites

1. **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Terraform** - [Install Terraform](https://www.terraform.io/downloads) (>= 1.0)
3. **kubectl** - [Install kubectl](https://kubernetes.io/docs/tasks/tools/)

### Azure Authentication

Login to Azure:

```bash
az login
```

Set your subscription (if you have multiple):

```bash
az account list --output table
az account set --subscription "Your-Subscription-ID"
```

## Quick Start

### 1. Configure Variables

Copy the example variables file and customize it:

```bash
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` and adjust values as needed:

```hcl
resource_group_name = "rg-cloudscribe"
location            = "westeurope"
aks_cluster_name    = "aks-cloudscribe"
acr_name            = "acrcloudscribe123"  # Must be globally unique!
```

**Important**: `acr_name` must be globally unique across all of Azure and contain only alphanumeric characters.

### 2. Initialize Terraform

```bash
terraform init
```

### 3. Plan Deployment

Review what will be created:

```bash
terraform plan
```

### 4. Apply Configuration

Create the infrastructure:

```bash
terraform apply
```

Type `yes` when prompted to confirm.

This process takes approximately 5-10 minutes.

### 5. Configure kubectl

After successful deployment, connect to your AKS cluster:

```bash
az aks get-credentials --resource-group rg-cloudscribe --name aks-cloudscribe
```

Verify connection:

```bash
kubectl get nodes
```

## Deployment Configuration

### Node Pool Sizing

Default configuration:
- **Node Count**: 2
- **VM Size**: Standard_D2s_v3 (2 vCPUs, 8 GB RAM)
- **OS Disk**: 30 GB

For production workloads, consider:
- Larger VM sizes (Standard_D4s_v3 or higher)
- Auto-scaling enabled
- Multiple node pools

### Auto-scaling

To enable auto-scaling, update `terraform.tfvars`:

```hcl
enable_auto_scaling = true
min_node_count      = 1
max_node_count      = 5
```

### Container Registry SKUs

- **Basic** - Development/testing (included in default config)
- **Standard** - Production workloads with higher throughput
- **Premium** - Geo-replication, content trust, private link

## Deploying CloudScribe to AKS

After infrastructure is provisioned:

### 1. Push Images to ACR

Get ACR credentials:

```bash
# Login to ACR
az acr login --name acrcloudscribe

# Or get admin credentials
terraform output -raw acr_admin_username
terraform output -raw acr_admin_password
```

Tag and push your Docker images:

```bash
# Tag images
docker tag ghcr.io/thechaoticengineer/cloudscribe-api:latest acrcloudscribe.azurecr.io/cloudscribe-api:latest
docker tag ghcr.io/thechaoticengineer/cloudscribe-blazor:latest acrcloudscribe.azurecr.io/cloudscribe-blazor:latest

# Push to ACR
docker push acrcloudscribe.azurecr.io/cloudscribe-api:latest
docker push acrcloudscribe.azurecr.io/cloudscribe-blazor:latest
```

### 2. Update Kubernetes Manifests

Update image references in `infrastructure/k8s/05-api.yaml` and `infrastructure/k8s/06-blazor.yaml`:

```yaml
image: acrcloudscribe.azurecr.io/cloudscribe-api:latest
```

### 3. Deploy to AKS

```bash
cd ../k8s
./deploy.sh
```

Or manually:

```bash
kubectl apply -f 01-base.yaml
kubectl apply -f 02-postgres.yaml
kubectl apply -f 03-keycloak.yaml
kubectl apply -f 04-setup-job.yaml
kubectl apply -f 05-api.yaml
kubectl apply -f 06-blazor.yaml
```

### 4. Get Public IP

If you created a public IP for LoadBalancer:

```bash
terraform output public_ip_address
```

Or get the LoadBalancer IP assigned by AKS:

```bash
kubectl get service cloudscribe-blazor-service -n cloudscribe
```

## Useful Commands

### View Outputs

```bash
terraform output                              # All outputs
terraform output aks_cluster_name             # Specific output
terraform output -raw acr_login_server        # Raw value without quotes
```

### Get kubeconfig

```bash
terraform output -raw kube_config_command | bash
```

### Update Infrastructure

After modifying `.tf` files:

```bash
terraform plan    # Review changes
terraform apply   # Apply changes
```

### Destroy Infrastructure

**Warning**: This deletes all resources and data!

```bash
terraform destroy
```

## Cost Optimization

### Development Environment

For minimal costs during development:

```hcl
node_count     = 1
vm_size        = "Standard_B2s"  # Burstable, lower cost
acr_sku        = "Basic"
```

### Production Environment

For production workloads:

```hcl
node_count          = 3
vm_size             = "Standard_D4s_v3"
enable_auto_scaling = true
min_node_count      = 3
max_node_count      = 10
acr_sku             = "Standard"  # or Premium
```

## Troubleshooting

### ACR Name Already Exists

If you get an error about ACR name already being taken:

```bash
# Change acr_name in terraform.tfvars to something unique
acr_name = "acrcloudscribe<yourname>123"
```

### kubectl Not Working

Re-fetch credentials:

```bash
az aks get-credentials --resource-group rg-cloudscribe --name aks-cloudscribe --overwrite-existing
```

### AKS Can't Pull from ACR

The role assignment might need time to propagate. Wait a few minutes and try again, or verify:

```bash
az role assignment list --scope $(terraform output -raw acr_id)
```

## State Management

Currently using local state (`terraform.tfstate`). For production/team environments, configure remote state:

### Azure Storage Backend

Create backend configuration in `backend.tf`:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "stterraformstate123"
    container_name       = "tfstate"
    key                  = "cloudscribe.tfstate"
  }
}
```

## Next Steps

- Set up CI/CD pipeline to deploy to AKS
- Configure Azure Entra ID integration
- Set up monitoring with Azure Monitor
- Configure ingress controller for better routing
- Set up cert-manager for TLS certificates

## Resources

- [Azure AKS Documentation](https://docs.microsoft.com/en-us/azure/aks/)
- [Azure Container Registry Documentation](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Terraform AzureRM Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
