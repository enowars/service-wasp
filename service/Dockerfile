FROM debian:latest

WORKDIR /service

RUN apt-get update
RUN apt-get install -y --no-install-recommends apt-utils
RUN apt-get install -y apt-transport-https
RUN apt-get install -y wget
RUN apt-get install -y gpg
# RUN apt-get install -y libssl-dev

# Install dotnet sdk
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/debian/9/prod.list
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list
RUN apt-get update
RUN apt-get install -y dotnet-sdk-2.1

USER root
EXPOSE 5000
EXPOSE 5001

CMD rm -r WASP/bin WASP/obj || true && \
    dotnet clean && \
    dotnet restore && \
    dotnet build && \
    dotnet run --project WASP
    