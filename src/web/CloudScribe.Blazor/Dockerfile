FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/web/CloudScribe.Blazor/CloudScribe.Blazor.csproj", "src/web/CloudScribe.Blazor/"]
COPY ["src/shared/CloudScribe.Contracts/CloudScribe.Contracts.csproj", "src/shared/CloudScribe.Contracts/"]
COPY ["src/shared/CloudScribe.SharedKernel/CloudScribe.SharedKernel.csproj", "src/shared/CloudScribe.SharedKernel/"]

RUN dotnet restore "src/web/CloudScribe.Blazor/CloudScribe.Blazor.csproj"

COPY . .

WORKDIR "/src/src/web/CloudScribe.Blazor"
RUN dotnet build "CloudScribe.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CloudScribe.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

USER app
ENTRYPOINT ["dotnet", "CloudScribe.Blazor.dll"]