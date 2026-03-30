# Stage 1: Build
# Uses Node.js to install dependencies and build the static assets.
FROM node:20-alpine AS build
WORKDIR /app

# Copies package files and installs dependencies using a clean slate.
COPY package*.json ./
RUN npm install

# Copies the source code and builds the production-ready bundle.
COPY . .

# HC: Receives the application version from the CI/CD pipeline and exposes it as an environment variable so the Vite build process can embed it into the static files.
ARG APP_VERSION
ENV VITE_APP_VERSION=$APP_VERSION

RUN npm run build

# Stage 2: Runtime
# Uses Nginx to serve the static files and manage the reverse proxy.
FROM nginx:stable-alpine AS final
WORKDIR /usr/share/nginx/html

# HC: Removes the default Nginx configuration to avoid conflicts with our custom rules.
RUN rm /etc/nginx/conf.d/default.conf

# HC: Copies the custom configuration that handles SPA routing and Backend Proxying.
# This is the key piece for making the API accessible via the /api prefix.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copies the build output from the previous stage to the Nginx public folder.
COPY --from=build /app/dist .

# Exposes port 80 for incoming HTTP traffic.
EXPOSE 80

# Starts Nginx in the foreground to keep the container running.
CMD ["nginx", "-g", "daemon off;"]