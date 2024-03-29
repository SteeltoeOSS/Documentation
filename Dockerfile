# example --build-arg="METADATA_IMAGE_VERSION=2.5.5-3.2.6"
ARG METADATA_IMAGE_VERSION
FROM steeltoe.azurecr.io/documentation-metadata:${METADATA_IMAGE_VERSION} AS build
WORKDIR /docs
COPY . .
RUN docfx build -o /built-docs --globalMetadataFiles main-site.json

FROM nginx:1.19
RUN rm -rf /usr/share/nginx/html
COPY --from=build /built-docs/_site /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker/ /
COPY *.env /etc/documentation-site/
