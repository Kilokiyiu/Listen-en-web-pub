FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["KaoyanService/KaoyanService.WebAPI/KaoyanService.WebAPI.csproj", "KaoyanService/KaoyanService.WebAPI/"]
COPY ["KaoyanService/KaoyanService.Domain/KaoyanService.Domain.csproj", "KaoyanService/KaoyanService.Domain/"]
COPY ["KaoyanService/KaoyanService.Infrastructure/KaoyanService.Infrastructure.csproj", "KaoyanService/KaoyanService.Infrastructure/"]
COPY ["Commons/MyJWT/MyJWT.csproj", "Commons/MyJWT/"]
COPY ["Commons/Infrastructure/Infrastructure.csproj", "Commons/Infrastructure/"]
COPY ["Commons/CommonInit/CommonInit.csproj", "Commons/CommonInit/"]
COPY ["Commons/DomainCommonInterface/DomainCommonInterface.csproj", "Commons/DomainCommonInterface/"]

RUN dotnet restore "KaoyanService/KaoyanService.WebAPI/KaoyanService.WebAPI.csproj"

COPY . .

RUN dotnet publish "KaoyanService/KaoyanService.WebAPI/KaoyanService.WebAPI.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KaoyanService.WebAPI.dll"]
