# MyRecipeBook Backend

This repository is a submodule of the [MyRecipeBook Full-Stack Application](https://github.com/damasceno-dev/myRecipeBook), containing the Terraform configurations for AWS resources.

This project is part of Welisson Arley .NET course on udemy: [.NET Core: um curso orientado para o mercado de trabalho](https://www.udemy.com/course/net-core-curso-orientado-para-mercado-de-trabalho)

**My Recipe Book** - an application for cooking enthusiasts who love to share recipes! My Recipe Book is designed to make your kitchen life easier, helping you organize, manage your recipes, and make your culinary experience more enjoyable.

This project consists of an **API** developed in **.NET** for managing culinary recipes. The **API** allows users to register by providing their name, email, and password. After registration, users can create, edit, filter, and delete recipes. Each recipe must include a title, ingredients, and instructions. Additionally, users have the option to add preparation time, difficulty level, and an illustrative image to the recipe.

The **API** supports **PostgreSQL** as the database option, providing flexibility in the data storage environment. **CI/CD** pipeline configuration and integration with **SonarCloud** ensure continuous code analysis, promoting more robust and secure development.

Following **Domain-Driven Design (DDD)** and **SOLID** principles, the project architecture aims to maintain a modular and sustainable design. Data validation is performed using **FluentValidation**, ensuring that all data inputs meet the established criteria.

To ensure code quality, **unit and integration tests** are implemented. The use of **dependency injection** promotes better modularity and code testability, facilitating maintenance and project evolution.

Other technologies and practices adopted include **Entity Framework** for object-relational mapping andimplementation of **JWT & Refresh Token** for secure authentication. Database migrations are managed to ensure controlled evolution of the data schema. Additionally, the use of **Git** and the **GitFlow** branching strategy helps in organizing and controlling code versions.

## Technologies Used

![badge-dot-net]
![badge-rider]
![badge-postgres]
![badge-swagger]
![badge-github-pipelines]
![badge-google]
![badge-openai]
![badge-sonarcloud]

## Running the project: 
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

### Continuous Delivery:
After the project is online, every push will push this new image to Amazon ECR,
and its going to be automatically used by the Amazon App Runner to put the last image online

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=bugs)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=coverage)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=YOUR_PROJECT_KEY)

<!-- Sonarcloud -->
[sonarcloud-dashboard]: https://sonarcloud.io/summary/overall?id=YOUR_PROJECT_KEY
[sonarcloud-qualityGate]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=alert_status
[sonarcloud-bugs]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=bugs
[sonarcloud-vulnerabilities]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=vulnerabilities
[sonarcloud-code-smells]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=code_smells
[sonarcloud-coverage]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=coverage
[sonarcloud-duplicated-lines]: https://sonarcloud.io/api/project_badges/measure?project=YOUR_PROJECT_KEY&metric=duplicated_lines_density

<!-- Badges -->
[badge-dot-net]: https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge
[badge-rider]: https://img.shields.io/badge/Rider-000000?logo=rider&logoColor=fff&style=for-the-badge
[badge-postgres]: https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=fff&style=for-the-badge
[badge-swagger]: https://img.shields.io/badge/Swagger-85EA2D?logo=swagger&logoColor=000&style=for-the-badge
[badge-github-pipelines]: https://img.shields.io/badge/GitHub%20Pipelines-2088FF?logo=githubactions&logoColor=fff&style=for-the-badge
[badge-google]: https://img.shields.io/badge/Google-4285F4?logo=google&logoColor=fff&style=for-the-badge
[badge-openai]: https://img.shields.io/badge/OpenAI-412991?logo=openai&logoColor=fff&style=for-the-badge
[badge-sonarcloud]: https://img.shields.io/badge/SonarCloud-F3702A?logo=sonarcloud&logoColor=fff&style=for-the-badge
