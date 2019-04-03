FROM ubuntu:18.04
LABEL maintainer="ENOFLAG <mail@enoflag.de>"

# Install python3 and uswgi
RUN apt-get update
RUN apt-get install -y python3 python3-dev python3-pip nginx git

# use python3
RUN pip3 install uwsgi

# copy our files in.
WORKDIR /checker
COPY ./requirements.txt /checker/requirements.txt
RUN pip3 install -r requirements.txt
COPY . .



#ENTRYPOINT ["/bin/bash"]
ENTRYPOINT [ "uwsgi", "--uid", "uwsgi", \
               "--socket", "0.0.0.0:3031", \
               "--protocol", "uwsgi", \
               "--ini", "/checker/uwsgi.ini" ]