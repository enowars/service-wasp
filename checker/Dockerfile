FROM debian:latest

WORKDIR /checker

RUN apt-get update
RUN apt-get install -y --no-install-recommends apt-utils
RUN apt-get install -y apt-transport-https
RUN apt-get install -y git python3 python3-pip

EXPOSE 5002

COPY . /checker

CMD /checker/
    