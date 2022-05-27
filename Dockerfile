FROM steeltoe.azurecr.io/documentation-metadata:3.2.0-2.5.5-3 AS build
WORKDIR /docs
COPY . .
RUN docfx build -o build --globalMetadataFiles devhost.json

FROM nginx:1.19
COPY --from=build /docs/build/_site /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
