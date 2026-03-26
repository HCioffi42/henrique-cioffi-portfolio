# Stage 1: Build
# Uses Node.js to install dependencies and build the static assets.
FROM node:20-alpine AS build
WORKDIR /app

# Copies package files and installs dependencies using a clean slate.
COPY package*.json ./
RUN npm install

# Copies the source code and builds the production-ready bundle.
COPY . .
RUN npm run build

# Stage 2: Runtime
# Uses Nginx to serve the static files generated in the previous stage.
FROM nginx:stable-alpine AS final

# Copies the build output to the default Nginx public folder.
COPY --from=build /app/dist /usr/share/nginx/html

# Copies a custom Nginx configuration to handle React Router (SPA) routing.
# We will create this file in the next step.
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]