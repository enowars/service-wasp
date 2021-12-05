FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY WASP.csproj .
RUN apt-get update && \
    apt-get install -y openssl && \
    openssl genrsa -des3 -passout pass:password -out server.pass.key 2048 && \
    openssl rsa -passin pass:password -in server.pass.key -out server.key && \
    rm server.pass.key && \
    openssl req -new -key server.key -out server.csr \
        -subj "/O=BAMBICTF/OU= 🐝🐝🐝 WASP Attack Coordination Dpt Ltd/CN=wasp.com" && \
    openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt && \
    openssl pkcs12 -export -po password -out cert.pfx -inkey server.key -in server.crt && \
    ls -alh
RUN dotnet restore WASP.csproj
COPY . .
RUN dotnet publish WASP.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
EXPOSE 443
COPY --from=build /app .
ENTRYPOINT ["dotnet", "WASP.dll"]
