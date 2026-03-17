FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FlowCare/FlowCare.csproj", "FlowCare/"]
RUN dotnet restore "FlowCare/FlowCare.csproj"
COPY . .
WORKDIR "/src/FlowCare"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FlowCare.dll"]