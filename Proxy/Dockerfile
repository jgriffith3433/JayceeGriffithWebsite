FROM nginx:1.23.3
EXPOSE 80
EXPOSE 443
COPY ["./conf/default.conf","/etc/nginx/conf.d/default.conf"]
COPY ./ssl /etc/nginx/ssl
CMD ["nginx", "-g", "daemon off;"]