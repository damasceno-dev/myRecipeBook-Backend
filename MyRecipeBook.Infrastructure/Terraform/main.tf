terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.40.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
  profile = "damasc-user"
}

# module "vpc" {
#   source         = "./modules/vpc"
#   prefix         = var.prefix
#   vpc_cidr_block = var.vpc_cidr_block
# }

module "s3" {
  source = "./modules/s3"
  prefix = var.prefix
  bucket_name = var.bucket_name
  versioning  = var.versioning
}