FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish -c Release --self-contained true -o /build

# Copy to runtime container
FROM ghcr.io/enowars/enochecker-dotnet:nightly
COPY --from=build /build /WaspChecker

RUN ls /WaspChecker
CMD ["/WaspChecker/WaspChecker.dll"]
