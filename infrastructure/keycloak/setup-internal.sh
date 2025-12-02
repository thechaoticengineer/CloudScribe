#!/bin/bash
set -e
set -x

KEYCLOAK_URL="http://keycloak:8080"
KCADM="/opt/keycloak/bin/kcadm.sh"
SECRET=${KEYCLOAK_CLIENT_SECRET:-dev-secret}

until $KCADM config credentials --server $KEYCLOAK_URL --realm master --user "$KEYCLOAK_ADMIN" --password "$KEYCLOAK_ADMIN_PASSWORD" > /dev/null 2>&1; do
  sleep 3
done

if ! $KCADM get realms/cloudscribe > /dev/null 2>&1; then
    $KCADM create realms -s realm=cloudscribe -s enabled=true
fi

if ! $KCADM get clients -r cloudscribe -q clientId=cloudscribe-web | grep "cloudscribe-web" > /dev/null; then
    $KCADM create clients -r cloudscribe \
      -s clientId=cloudscribe-web \
      -s secret=$SECRET \
      -s enabled=true \
      -s clientAuthenticatorType=client-secret \
      -s "redirectUris=[\"https://localhost:7283/signin-oidc\"]" \
      -s "webOrigins=[\"+\"]" \
      -s publicClient=false \
      -s serviceAccountsEnabled=true \
      -s standardFlowEnabled=true \
      -s directAccessGrantsEnabled=true
fi

if ! $KCADM get users -r cloudscribe -q username=testuser | grep "testuser" > /dev/null; then
    $KCADM create users -r cloudscribe -s username=testuser -s enabled=true
    $KCADM set-password -r cloudscribe --username testuser --new-password test
fi

if ! $KCADM get users -r cloudscribe -q username=testuser | grep "testuser2" > /dev/null; then
    $KCADM create users -r cloudscribe -s username=testuser2 -s enabled=true
    $KCADM set-password -r cloudscribe --username testuser2 --new-password test
fi

echo "CONFIGURATION FINISHED"