#!/usr/bin/env bash
# Runs ON the EC2 instance, invoked by GitHub Actions via SSM Run Command.
# Usage: deploy.sh <image-tag> <ecr-registry> [aws-region]
set -euo pipefail

IMAGE_TAG="${1:?image tag required}"
ECR_REGISTRY="${2:?ecr registry required}"
AWS_REGION="${3:-eu-central-1}"

cd /opt/workouttracker

# Authenticate Docker to ECR (uses the EC2 instance IAM role, no stored creds).
aws ecr get-login-password --region "$AWS_REGION" \
  | docker login --username AWS --password-stdin "$ECR_REGISTRY"

# Pin the tag + registry into .env so the running stack and any manual `docker compose` agree.
sed -i "s|^IMAGE_TAG=.*|IMAGE_TAG=${IMAGE_TAG}|" .env
sed -i "s|^ECR_REGISTRY=.*|ECR_REGISTRY=${ECR_REGISTRY}|" .env

docker compose -f docker-compose.prod.yml pull
docker compose -f docker-compose.prod.yml up -d

# Reclaim disk from old image layers (EC2 free-tier disks are small).
docker image prune -f

echo "Deployed tag ${IMAGE_TAG}."
