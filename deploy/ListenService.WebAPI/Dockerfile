FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件（利用 Docker 缓存层）
COPY ["ListenService/ListenService.WebAPI/ListenService.WebAPI.csproj", "ListenService/ListenService.WebAPI/"]
COPY ["ListenService/ListenService.Domain/ListenService.Domain.csproj", "ListenService/ListenService.Domain/"]
COPY ["ListenService/ListenService.Infrastrucure/ListenService.Infrastrucure.csproj", "ListenService/ListenService.Infrastrucure/"]
COPY ["Commons/MyJWT/MyJWT.csproj", "Commons/MyJWT/"]

RUN dotnet restore "ListenService/ListenService.WebAPI/ListenService.WebAPI.csproj"

# 复制全部源码
COPY . .

RUN dotnet publish "ListenService/ListenService.WebAPI/ListenService.WebAPI.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# 容器内监听 8080 端口
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ListenService.WebAPI.dll"]
