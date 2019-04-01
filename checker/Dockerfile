FROM ubuntu:18.04
LABEL maintainer="ENOFLAG <mail@enoflag.de>"

# Install python3 and uswgi
RUN apt-get update
RUN apt-get install -y python3 python3-dev python3-pip nginx

# use python3
RUN pip3 install uwsgi

# copy our files in.
COPY ./ /checker
WORKDIR /checker

RUN pip3 install -r requirements.txt

COPY ./nginx.conf /etc/nginx/sites-enabled/default

EXPOSE 5002

# run it all.
CMD service nginx start && uwsgi --ini ./uswgi.ini
