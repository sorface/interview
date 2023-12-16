FROM nginx

COPY nginx.conf /etc/nginx
COPY mime.types /etc/nginx

RUN mkdir -p /etc/ssl/
RUN mkdir -p /home/html/

EXPOSE 8088

CMD ["nginx", "-g", "daemon off;"]