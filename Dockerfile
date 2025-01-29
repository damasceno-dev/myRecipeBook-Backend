FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ["MyRecipeBook.API/", "MyRecipeBook.API/"]
COPY ["MyRecipeBook.Application/", "MyRecipeBook.Application/"]
COPY ["MyRecipeBook.Communication/", "MyRecipeBook.Communication/"]
COPY ["MyRecipeBook.Domain/", "MyRecipeBook.Domain/"]
COPY ["MyRecipeBook.Exception/", "MyRecipeBook.Exception/"]
COPY ["MyRecipeBook.Infrastructure/", "MyRecipeBook.Infrastructure/"]

WORKDIR MyRecipeBook.API/

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "MyRecipeBook.API.dll"]