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
}

module "sqs" {
  source = "./modules/sqs"
  prefix                       = var.prefix
  delay_seconds                = 0
  message_retention_seconds    = 345600
  visibility_timeout_seconds   = 30
  receive_wait_time_seconds    = 10
  max_receive_count            = 5
  dlq_message_retention_seconds = 1209600
}
