FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ["MyRecipeBook.API/", "MyRecipeBook.API/"]
COPY ["MyRecipeBook.Application/", "MyRecipeBook.Application/"]
COPY ["MyRecipeBook.Communication/", "MyRecipeBook.Communication/"]
COPY ["MyRecipeBook.Domain/", "MyRecipeBook.Domain/"]
COPY ["MyRecipeBook.Exception/", "MyRecipeBook.Exception/"]
COPY ["MyRecipeBook.Infrastructure/", "MyRecipeBook.Infrastructure/"]

# ✅ Explicitly copy env files to production
COPY MyRecipeBook.API/API.env MyRecipeBook.API/API.env
COPY MyRecipeBook.API/API.env MyRecipeBook.API/API.env
WORKDIR MyRecipeBook.API/

RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "MyRecipeBook.API.dll"]