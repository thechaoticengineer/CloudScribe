#!/bin/bash

# CloudScribe Kubernetes Deployment Script
# This script deploys all CloudScribe resources to Kubernetes

set -e

echo "=========================================="
echo "CloudScribe Kubernetes Deployment"
echo "=========================================="
echo ""

if ! command -v kubectl &> /dev/null; then
    echo "ERROR: kubectl is not installed or not in PATH"
    exit 1
fi

if [ ! -f ".secrets.env" ]; then
    echo "ERROR: .secrets.env file not found"
    echo "Please create .secrets.env with the following variables:"
    echo "  POSTGRES_PASSWORD=<your-password>"
    echo "  POSTGRES_CONNECTION_STRING=<your-connection-string>"
    echo "  KEYCLOAK_ADMIN_USER=<admin-username>"
    echo "  KEYCLOAK_ADMIN_PASS=<admin-password>"
    echo "  OIDC_CLIENT_SECRET=<client-secret>"
    exit 1
fi

echo "Starting deployment process..."
echo ""

echo "[1/7] Creating namespace..."
kubectl apply -f 01-base.yaml

echo "[2/7] Setting up Kubernetes secrets..."
./setup-secrets.sh

echo "[3/7] Deploying PostgreSQL database..."
kubectl apply -f 02-postgres.yaml

echo "Waiting for PostgreSQL to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n cloudscribe --timeout=120s


echo "[4/7] Deploying Keycloak..."
kubectl apply -f 03-keycloak.yaml

echo "Waiting for Keycloak to be ready (this may take a few minutes)..."
kubectl wait --for=condition=ready pod -l app=keycloak -n cloudscribe --timeout=300s


echo "Waiting for Keycloak to fully initialize..."
sleep 10


echo "[5/7] Running Keycloak configuration job..."
kubectl apply -f 04-setup-job.yaml

echo "Waiting for Keycloak setup job to complete..."
kubectl wait --for=condition=complete job/keycloak-setup -n cloudscribe --timeout=120s


echo "[6/7] Deploying CloudScribe Notes API..."
kubectl apply -f 05-api.yaml

echo "Waiting for API to be ready..."
kubectl wait --for=condition=ready pod -l app=cloudscribe-api -n cloudscribe --timeout=120s


echo "[7/7] Deploying CloudScribe Blazor UI..."
kubectl apply -f 06-blazor.yaml

echo "Waiting for Blazor UI to be ready..."
kubectl wait --for=condition=ready pod -l app=cloudscribe-blazor -n cloudscribe --timeout=120s

echo ""
echo "=========================================="
echo "Deployment completed successfully!"
echo "=========================================="
echo ""


echo "Service Information:"
echo "--------------------"
kubectl get services -n cloudscribe cloudscribe-blazor-service

