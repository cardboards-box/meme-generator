FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish "./src/MemeGenerator.Web/MemeGenerator.Web/MemeGenerator.Web.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-add-repository ppa:quamotion/ppa && apt-get update && apt-get install -y libgdiplus
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MemeGenerator.Web.dll"]
