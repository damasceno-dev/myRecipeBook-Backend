Running the project: 
Set the environment variables of the corresponding files using the examples:

| Example File                                           | Real File                                       |
|--------------------------------------------------------|-------------------------------------------------|
| MyRecipeBook.API/appsettings.Development.json          | MyRecipeBook.API/appsettings.Production.json    |
| MyRecipeBook.API/API.Example.env                       | MyRecipeBook.API/API.env                        |
| MyRecipeBook.Infrastructure/Infrastructure.Example.env | MyRecipeBook.Infrastructure/Infrastructure.env  |

Putting the project online for the first time:
1) Deploy the infrastructure needed. Set up a repository similar to https://github.com/damasceno-dev/myRecipeBook-Infrastructure
2) Run the Deploy workflow to deploy every aws resource needed by this project.
3) This workflow outputs the RDS endpoint, the ECR url, S3 bucket name and SQS url
4) Fill them in the corresponding variable:

| Name              | Variable                                   | File                                            |
|-------------------|--------------------------------------------|-------------------------------------------------|
| RDS endpoint      | ConnectionString.DefaultConnection (Host)  | MyRecipeBook.API/appsettings.Production.json    |
| S3 bucket name    | AWS_S3_BUCKET_NAME                         | MyRecipeBook.Infrastructure/Infrastructure.env  |
| SQS url           | AWS_SQS_DELETE_USER_URL                    | MyRecipeBook.Infrastructure/Infrastructure.env  |
| ECR url           | AWS_ECR_URL                                | MyRecipeBook.Infrastructure/Infrastructure.env  |

5) Deploy the docker image of this project in AWS ECR using 'Deploy to Private Amazon ECR' yml (auto runs on every commit)
6) Deploy the app runner workflow in the same repository mentioned in step 1
7) The App Runner URL output from the previous step is the url of the online project

Continuous Delivery:
After the project is online, every push will push this new image to Amazon ECR,
and its going to be automatically used by the Amazon App Runner to put the last image online
