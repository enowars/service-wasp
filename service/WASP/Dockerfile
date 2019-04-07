FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY WASP/WASP.csproj WASP/
RUN dotnet restore WASP/WASP.csproj
COPY . .
WORKDIR /src/WASP
RUN dotnet build WASP.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish WASP.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WASP.dll"]
