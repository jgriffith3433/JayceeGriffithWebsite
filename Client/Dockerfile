FROM node:14-alpine as build
WORKDIR /app

RUN npm install -g @angular/cli

COPY ./package.json .
RUN npm install
COPY . .
RUN npm run build

FROM nginx:1.23.3 as runtime
EXPOSE 9000
COPY --from=build /app/dist/client /usr/share/nginx/html
COPY ["./conf/default.conf","/etc/nginx/conf.d/default.conf"]
CMD ["nginx", "-g", "daemon off;"]
