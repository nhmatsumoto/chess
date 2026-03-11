# Build Stage
FROM node:20-slim AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Runtime Stage
FROM node:20-slim
WORKDIR /app

COPY --from=build /app/dist ./dist
COPY --from=build /app/package*.json ./
COPY --from=build /app/node_modules ./node_modules

EXPOSE 5173

# Qwik dev server usually runs on 5173, but for production we might need a simple server or just run preview
# Since we want to "run" it, let's use the dev server for simplicity in this context, 
# or a simple static server. For Qwik City, we usually deploy to a cloud provider.
# Let's use `npm run preview` as a stand-in for a simple production-like run.
CMD ["npm", "run", "preview", "--", "--host"]
