#!/bin/bash
# init-aws.sh
awslocal s3 mb s3://bloggi

awslocal s3api put-bucket-cors --bucket bloggi --cors-configuration '{
  "CORSRules": [
    {
      "AllowedOrigins": ["*"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE", "HEAD"],
      "AllowedHeaders": ["*"],
      "ExposeHeaders": ["ETag"]
    }
  ]
}'

echo "Bucket created with CORS."
