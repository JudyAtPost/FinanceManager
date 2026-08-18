FROM node:22-alpine AS build
WORKDIR /app

# Trust any corporate CA certs dropped in docker-certs/ (e.g. for TLS-inspecting proxies). No-op if empty.
COPY docker-certs/ /usr/local/share/ca-certificates/
RUN find /usr/local/share/ca-certificates -name "*.crt" -exec cat {} \; > /tmp/extra-ca-certs.pem
ENV NODE_EXTRA_CA_CERTS=/tmp/extra-ca-certs.pem

COPY package.json ./
RUN npm install

COPY . .
RUN npm run build

FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/personal-finance-web/browser /usr/share/nginx/html

EXPOSE 80