variable "prefix" {}
variable "vpc_cidr_block" {
  default = "10.0.0.0/16"
}

variable "bucket_name" {
  description = "Name of the S3 bucket"
  type        = string
}

variable "versioning" {
  description = "Enable versioning for the S3 bucket"
  type        = bool
  default     = false # No versioning by default
}