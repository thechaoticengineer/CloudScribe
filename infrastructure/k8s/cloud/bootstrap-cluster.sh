#!/bin/bash

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}=== CloudScribe Cluster Bootstrap ===${NC}"
echo "This script will:"
echo "  1. Detect AKS cluster configuration"
echo "  2. Install nginx Ingress Controller"
echo "  3. Wait for External IP"
echo "  4. Install cert-manager"
echo "  5. Wait for cert-manager to be ready"
echo "  6. Apply Ingress resources"
echo ""

if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}ERROR: kubectl is not installed${NC}"
    exit 1
fi

if ! command -v helm &> /dev/null; then
    echo -e "${RED}ERROR: helm is not installed${NC}"
    echo "Install helm from: https://helm.sh/docs/intro/install/"
    exit 1
fi

if [ ! -f ".secrets.env" ]; then
    echo -e "${RED}ERROR: .secrets.env not found${NC}"
    echo "Please create it from .secrets.env.example"
    exit 1
fi

echo -e "${YELLOW}[1/5] Detecting AKS cluster configuration...${NC}"

MC_RESOURCE_GROUP=$(kubectl get nodes -o jsonpath='{.items[0].metadata.labels.kubernetes\.azure\.com/cluster}' 2>/dev/null)

if [ -z "$MC_RESOURCE_GROUP" ]; then
    echo -e "${YELLOW}Warning: Could not auto-detect MC resource group from cluster.${NC}"
    echo -e "${YELLOW}Checking if running on AKS...${NC}"

    NODE_RG=$(kubectl get nodes -o jsonpath='{.items[0].spec.providerID}' 2>/dev/null | cut -d'/' -f5)

    if [ -n "$NODE_RG" ]; then
        MC_RESOURCE_GROUP=$NODE_RG
        echo -e "${GREEN}✓ Detected MC resource group: $MC_RESOURCE_GROUP${NC}"
    else
        echo -e "${RED}ERROR: Could not detect Azure resource group.${NC}"
        echo "Please ensure you're connected to an AKS cluster."
        exit 1
    fi
else
    echo -e "${GREEN}✓ Detected MC resource group: $MC_RESOURCE_GROUP${NC}"
fi

echo ""
echo -e "${YELLOW}[2/5] Installing nginx Ingress Controller...${NC}"

if helm list -n ingress-nginx | grep -q nginx-ingress; then
    echo "nginx-ingress already installed, skipping..."
else
    helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx 2>/dev/null || true
    helm repo update

    helm install nginx-ingress ingress-nginx/ingress-nginx \
      --namespace ingress-nginx \
      --create-namespace \
      --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-resource-group"="$MC_RESOURCE_GROUP" \
      --set controller.service.externalTrafficPolicy=Local \
      --set controller.config.use-forwarded-headers="true" \
      --set controller.config.compute-full-forwarded-for="true" \
      --set controller.config.proxy-buffer-size="16k"

    echo -e "${GREEN}✓ nginx Ingress Controller installed${NC}"
fi

echo ""
echo -e "${YELLOW}[3/5] Waiting for nginx Ingress Controller to get External IP...${NC}"
echo "This may take 2-3 minutes..."

TIMEOUT=300
ELAPSED=0
while [ $ELAPSED -lt $TIMEOUT ]; do
    INGRESS_IP=$(kubectl get service nginx-ingress-ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")

    if [ -n "$INGRESS_IP" ]; then
        echo -e "${GREEN}✓ Got Ingress IP: $INGRESS_IP${NC}"
        break
    fi

    echo -n "."
    sleep 5
    ELAPSED=$((ELAPSED + 5))
done

if [ -z "$INGRESS_IP" ]; then
    echo -e "${RED}ERROR: Timeout waiting for Ingress IP${NC}"
    echo "Check status: kubectl get service -n ingress-nginx"
    exit 1
fi

echo ""
echo -e "${YELLOW}[4/6] Installing cert-manager...${NC}"

if helm list -n cert-manager | grep -q cert-manager; then
    echo "cert-manager already installed, skipping..."
else
    helm repo add jetstack https://charts.jetstack.io 2>/dev/null || true
    helm repo update

    helm install cert-manager jetstack/cert-manager \
      --namespace cert-manager \
      --create-namespace \
      --version v1.14.2 \
      --set installCRDs=true

    echo -e "${GREEN}✓ cert-manager installed${NC}"
fi

echo ""
echo -e "${YELLOW}[5/6] Waiting for cert-manager to be ready...${NC}"
kubectl wait --for=condition=ready pod -l app.kubernetes.io/instance=cert-manager -n cert-manager --timeout=120s

echo ""
echo -e "${YELLOW}[6/6] Applying Ingress resources...${NC}"
./apply-ingress.sh

echo ""
echo -e "${GREEN}=== BOOTSTRAP COMPLETE ===${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Build and push Docker images"
echo "2. Run: ./deploy.sh"
echo ""
echo -e "${YELLOW}Your services will be at:${NC}"
echo "  - Blazor:   https://blazor.$INGRESS_IP.nip.io"
echo "  - API:      https://api.$INGRESS_IP.nip.io"
echo "  - Keycloak: https://auth.$INGRESS_IP.nip.io"
