FROM nginx:latest

WORKDIR /usr/share/documentation/www

COPY _site ./

RUN chown -R app:app /usr/share/documentation/www
RUN rm /etc/nginx/sites-enabled/default
RUN rm -f /etc/service/nginx/down
COPY documentation.conf /etc/nginx/sites-enabled/