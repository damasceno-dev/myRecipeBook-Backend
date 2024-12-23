variable "prefix" {
  description = "Prefix for resource names"
  type        = string
}

variable "vpc_cidr_block" {
  default = "10.0.0.0/16"
}