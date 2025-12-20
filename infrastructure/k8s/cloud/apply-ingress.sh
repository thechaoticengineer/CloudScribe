#!/bin/bash

set -e

NAMESPACE="cloudscribe"
ENV_FILE=".secrets.env"

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}=== CloudScribe Ingress Setup ===${NC}"

if [ ! -f "$ENV_FILE" ]; then
    echo -e "${RED}ERROR: File $ENV_FILE not found.${NC}"
    echo "Please create it from .secrets.env.example and fill in the values."
    exit 1
fi

echo -e "${YELLOW}Loading variables from $ENV_FILE...${NC}"
source $ENV_FILE

if [ -z "$LETSENCRYPT_EMAIL" ]; then
    echo -e "${RED}ERROR: LETSENCRYPT_EMAIL is not set in $ENV_FILE${NC}"
    exit 1
fi

echo -e "${YELLOW}Getting nginx Ingress Controller IP...${NC}"
INGRESS_IP=$(kubectl get service nginx-ingress-ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")

if [ -z "$INGRESS_IP" ]; then
    echo -e "${RED}ERROR: Could not get nginx Ingress Controller IP.${NC}"
    echo "Make sure nginx Ingress Controller is installed and has an external IP."
    echo "Run: kubectl get service -n ingress-nginx"
    exit 1
fi

echo -e "${GREEN}✓ Ingress IP: $INGRESS_IP${NC}"
echo -e "${GREEN}✓ Let's Encrypt Email: $LETSENCRYPT_EMAIL${NC}"

echo -e "${YELLOW}Applying cert-manager ClusterIssuers...${NC}"
sed "s/REPLACE_WITH_EMAIL/$LETSENCRYPT_EMAIL/g" 07-cert-manager.yaml | kubectl apply -f -

echo -e "${YELLOW}Applying Ingress resources...${NC}"
sed "s/REPLACE_WITH_IP/$INGRESS_IP/g" 08-ingress.yaml | kubectl apply -f -

echo ""
echo -e "${GREEN}=== SUCCESS ===${NC}"
echo -e "ClusterIssuers and Ingress resources applied!"
echo ""
echo -e "${YELLOW}Your services will be available at:${NC}"
echo -e "  - Blazor:   https://blazor.$INGRESS_IP.nip.io"
echo -e "  - API:      https://api.$INGRESS_IP.nip.io"
echo -e "  - Keycloak: https://auth.$INGRESS_IP.nip.io"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Wait for certificates to be issued (1-5 minutes):"
echo "   kubectl get certificate -n $NAMESPACE -w"
echo ""
echo "2. Check certificate status:"
echo "   kubectl describe certificate cloudscribe-tls-cert -n $NAMESPACE"
echo ""
echo "3. Test HTTPS endpoints:"
echo "   curl -I https://blazor.$INGRESS_IP.nip.io"
