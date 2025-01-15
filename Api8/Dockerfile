# 使用包含 .NET SDK 的標準映像檔來建構應用程式
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Api8.csproj", "."]
RUN dotnet restore "Api8.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Api8.csproj" -c Release -o /app/build

# 發佈應用程式
FROM build AS publish
RUN dotnet publish "Api8.csproj" -c Release -o /app/publish

# 使用帶有 ICU 的 Chiseled Runtime 映像作為最終映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-extra AS final
WORKDIR /app

# 複製發佈檔案
COPY --from=publish /app/publish .

# 設定環境變數啟用完整全球化支援（確保 ICU 功能正常）
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 設定啟動命令
ENTRYPOINT ["dotnet", "Api8.dll"]