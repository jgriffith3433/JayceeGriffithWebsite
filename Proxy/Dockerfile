FROM nginx:1.23.3
EXPOSE 80
COPY ["./conf/default.conf","/etc/nginx/conf.d/default.conf"]
CMD ["nginx", "-g", "daemon off;"]