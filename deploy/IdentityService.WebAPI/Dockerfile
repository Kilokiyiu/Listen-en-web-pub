FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件（利用 Docker 缓存层）
COPY ["IdentitySerivce/IdentityService.WebAPI/IdentityService.WebAPI.csproj", "IdentitySerivce/IdentityService.WebAPI/"]
COPY ["IdentitySerivce/IdentitySerivce.Domain/IdentitySerivce.Domain.csproj", "IdentitySerivce/IdentitySerivce.Domain/"]
COPY ["IdentitySerivce/IdentitySerivce.Infrastructure/IdentitySerivce.Infrastructure.csproj", "IdentitySerivce/IdentitySerivce.Infrastructure/"]
COPY ["Commons/MyJWT/MyJWT.csproj", "Commons/MyJWT/"]
COPY ["Commons/MyEventController/MyEventController.csproj", "Commons/MyEventController/"]

RUN dotnet restore "IdentitySerivce/IdentityService.WebAPI/IdentityService.WebAPI.csproj"

# 复制全部源码
COPY . .

RUN dotnet publish "IdentitySerivce/IdentityService.WebAPI/IdentityService.WebAPI.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# 容器内监听 8080 端口
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IdentityService.WebAPI.dll"]
