FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件（利用 Docker 缓存层）
COPY ["WordService/WordService.WebAPI/WordService.WebAPI.csproj", "WordService/WordService.WebAPI/"]
COPY ["WordService/WordService.Domain/WordService.Domain.csproj", "WordService/WordService.Domain/"]
COPY ["WordService/WordService.Infrastrucure/WordService.Infrastrucure.csproj", "WordService/WordService.Infrastrucure/"]
COPY ["Commons/MyJWT/MyJWT.csproj", "Commons/MyJWT/"]
COPY ["Commons/Infrastructure/Infrastructure.csproj", "Commons/Infrastructure/"]

RUN dotnet restore "WordService/WordService.WebAPI/WordService.WebAPI.csproj"

# 复制全部源码
COPY . .

RUN dotnet publish "WordService/WordService.WebAPI/WordService.WebAPI.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# 容器内监听 8080 端口
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WordService.WebAPI.dll"]
