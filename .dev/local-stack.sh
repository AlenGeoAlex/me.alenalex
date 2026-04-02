#!/bin/bash
# localstack-bucket.sh

ENDPOINT="http://localhost:4566"
REGION="us-east-1"
AWS_ACCESS_KEY_ID="test"
AWS_SECRET_ACCESS_KEY="test"

export AWS_ACCESS_KEY_ID AWS_SECRET_ACCESS_KEY

usage() {
  echo "Usage: $0 <create|delete> <bucket-name>"
  exit 1
}

[[ $# -ne 2 ]] && usage

ACTION=$1
BUCKET=$2

case $ACTION in
  create)
    aws --endpoint-url "$ENDPOINT" --region "$REGION" s3 mb "s3://$BUCKET"
    echo "Created bucket: $BUCKET"
    ;;
  delete)
    aws --endpoint-url "$ENDPOINT" --region "$REGION" s3 rb "s3://$BUCKET" --force
    echo "Deleted bucket: $BUCKET"
    ;;
  *)
    usage
    ;;
esac
