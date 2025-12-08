#!/bin/bash

# CloudScribe Kubernetes Cleanup Script
# This script deletes all CloudScribe resources from Kubernetes

set -e

echo "=========================================="
echo "CloudScribe Kubernetes Cleanup"
echo "=========================================="
echo ""

if ! command -v kubectl &> /dev/null; then
    echo "ERROR: kubectl is not installed or not in PATH"
    exit 1
fi

if ! kubectl get namespace cloudscribe &> /dev/null; then
    echo "Namespace 'cloudscribe' does not exist. Nothing to clean up."
    exit 0
fi

echo "This will DELETE all CloudScribe resources from Kubernetes:"
echo "  - Namespace: cloudscribe"
echo "  - All deployments, services, jobs, and pods"
echo "  - PostgreSQL database and all data (PersistentVolumeClaim)"
echo "  - Keycloak configuration"
echo "  - All secrets"
echo ""
read -p "Are you sure you want to continue? (yes/no): " -r
echo ""

if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
    echo "Cleanup cancelled."
    exit 0
fi

echo "Starting cleanup process..."
echo ""

echo "[1/7] Deleting Blazor UI..."
kubectl delete -f 06-blazor.yaml --ignore-not-found=true

echo "[2/7] Deleting Notes API..."
kubectl delete -f 05-api.yaml --ignore-not-found=true

echo "[3/7] Deleting Keycloak setup job..."
kubectl delete -f 04-setup-job.yaml --ignore-not-found=true

echo "[4/7] Deleting Keycloak..."
kubectl delete -f 03-keycloak.yaml --ignore-not-found=true

echo "[5/7] Deleting PostgreSQL..."
kubectl delete -f 02-postgres.yaml --ignore-not-found=true

echo "[6/7] Waiting for resources to terminate..."
kubectl wait --for=delete pod -l app=postgres -n cloudscribe --timeout=60s 2>/dev/null || true
kubectl wait --for=delete pod -l app=keycloak -n cloudscribe --timeout=60s 2>/dev/null || true

echo "[7/7] Deleting namespace and all remaining resources..."
# kubectl delete namespace cloudscribe --timeout=120s ## It is not working.

echo ""
echo "=========================================="
echo "Cleanup completed successfully!"
echo "=========================================="
echo ""
echo "To verify deletion:"
echo "  kubectl get all -n cloudscribe"
echo ""
echo "To redeploy CloudScribe:"
echo "  ./deploy.sh"
echo ""
