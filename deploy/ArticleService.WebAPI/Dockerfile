FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件（利用 Docker 缓存层）
COPY ["ArticleService/ArticleService.WebAPI/ArticleService.WebAPI.csproj", "ArticleService/ArticleService.WebAPI/"]
COPY ["ArticleService/ArticleService.Domain/ArticleService.Domain.csproj", "ArticleService/ArticleService.Domain/"]
COPY ["ArticleService/ArticleService.Infrastructure/ArticleService.Infrastructure.csproj", "ArticleService/ArticleService.Infrastructure/"]
COPY ["Commons/MyJWT/MyJWT.csproj", "Commons/MyJWT/"]

RUN dotnet restore "ArticleService/ArticleService.WebAPI/ArticleService.WebAPI.csproj"

# 复制全部源码
COPY . .

RUN dotnet publish "ArticleService/ArticleService.WebAPI/ArticleService.WebAPI.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# 容器内监听 8080 端口
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ArticleService.WebAPI.dll"]
