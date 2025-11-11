# CloudScribe

A cloud-native note-taking application built with microservices architecture, demonstrating modern DevOps practices and infrastructure automation.

## Overview

CloudScribe is designed as a production-like test environment showcasing containerized microservices deployment to Azure Kubernetes Service (AKS). The project emphasizes infrastructure-as-code, OAuth2/OIDC integration, and cloud-native patterns.

## Architecture

### Services

- **CloudScribe.Notes.API** - REST API for note management (CRUD operations)
- **CloudScribe.Blazor** - Blazor Server frontend application
- **Keycloak** - OAuth2/OIDC provider (test environment)
- **PostgreSQL** - Primary database

### Technology Stack

- **.NET 9.0** - Application runtime
- **Blazor Server** - Interactive web UI
- **Entity Framework Core** - ORM with PostgreSQL provider
- **Keycloak** - Identity and access management
- **PostgreSQL** - Relational database
- **Docker** - Containerization
- **Kubernetes (AKS)** - Orchestration platform

## Deployment Strategy

### Target Environment: Azure Kubernetes Service (AKS)

The application runs entirely within AKS, simulating a test environment with the following characteristics:

- All components containerized and deployed as Kubernetes workloads
- PostgreSQL runs inside the cluster (test environment setup)
- Keycloak deployed as in-cluster OAuth provider
- Easy migration path to Azure Entra ID for production scenarios

### Infrastructure as Code

Multiple IaC approaches will be explored:

- **Terraform** - Cloud infrastructure provisioning
- **Azure Bicep** - Azure-native IaC alternative
- **Kubernetes Manifests** - Application deployment configurations

## Project Goals

1. **Microservices Architecture** - Loosely coupled services with clear boundaries
2. **Cloud-Native Patterns** - Container orchestration, service discovery, configuration management
3. **Security** - OAuth2/OIDC integration with pluggable identity providers
4. **Infrastructure Automation** - Reproducible environments via IaC
5. **DevOps Practices** - CI/CD pipelines, GitOps workflows

## Authentication Flow

**Test Environment:**
- Keycloak as OAuth2/OIDC provider
- JWT token-based authentication
- Easy configuration for local development

**Production Path:**
- Seamless switch to Azure Entra ID (formerly Azure AD)
- Minimal code changes required
- Same OAuth2/OIDC flows

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- kubectl
- Azure CLI (for AKS deployment)
- Terraform or Azure Bicep (for infrastructure)

### Local Development

```bash
# Clone repository
git clone <repository-url>
cd CloudScribe

# Run infrastructure (Keycloak + PostgreSQL)
docker-compose up -d

# Run API
dotnet run --project Src/Services/CloudScribe.Notes.API

# Run Frontend
dotnet run --project Src/Web/CloudScribe.Blazor
```

### Deployment to AKS

```bash
# Provision infrastructure
cd infrastructure/terraform
terraform init
terraform apply

# Deploy applications
kubectl apply -f k8s/
```

## Project Structure

```
CloudScribe/
├── CloudScribe.sln
├── docker-compose.yml
├── infrastructure
│         ├── bicep
│         ├── k8s
│         └── terraform
├── README.md
├── src
│         ├── services
│         │   └── CloudScribe.Notes.API
│         └── web
│             └── CloudScribe.Blazor
└── tests
```

## Roadmap

- [ ] Core API implementation (Notes CRUD)
- [ ] Blazor UI for note management
- [ ] Keycloak integration (OAuth2/OIDC)
- [ ] Entity Framework Core with PostgreSQL
- [ ] Docker containerization
- [ ] Kubernetes manifests
- [ ] Terraform infrastructure provisioning
- [ ] Azure Bicep alternative
- [ ] AKS deployment
- [ ] CI/CD pipeline
- [ ] Entra ID integration option

## License

This project is for educational and demonstration purposes.
