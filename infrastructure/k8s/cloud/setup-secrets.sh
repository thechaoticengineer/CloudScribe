#!/bin/bash

NAMESPACE="cloudscribe"
SECRET_NAME="cloudscribe-secrets"
ENV_FILE=".secrets.env"

if [ ! -f "$ENV_FILE" ]; then
    echo "ERROR: File $ENV_FILE not found."
    exit 1
fi

echo "Checking namespace '$NAMESPACE'..."
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

echo "Removing old secret if exists..."
kubectl delete secret $SECRET_NAME -n $NAMESPACE --ignore-not-found

echo "Creating new secret '$SECRET_NAME'..."
kubectl create secret generic $SECRET_NAME \
    -n $NAMESPACE \
    --from-env-file=$ENV_FILE

echo "DONE! Secrets updated."