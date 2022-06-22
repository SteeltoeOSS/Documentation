FROM steeltoe.azurecr.io/documentation-metadata:3.2.0-2.5.5-6 AS build
WORKDIR /docs
COPY . .
RUN docfx build -o build --globalMetadataFiles main-site.json

FROM nginx:1.19
RUN rm -rf /usr/share/nginx/html
COPY --from=build /docs/_site /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker/ /
COPY *.env /etc/documentation-site/
