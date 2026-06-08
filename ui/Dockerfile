# ─── Stage 1: Build ──────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Cap Node's heap so the install/build survive in a memory-constrained Docker VM.
ENV NODE_OPTIONS=--max-old-space-size=2048

COPY package.json package-lock.json* ./
# --maxsockets 1 limits parallel extraction (lower peak memory); skip audit/fund noise.
RUN npm ci --no-audit --no-fund --maxsockets 1

COPY . .
RUN npm run build

# ─── Stage 2: Serve with nginx ───────────────────────────────────────────────
FROM nginx:alpine AS runtime

# Angular's application builder emits the browser bundle under dist/<project>/browser
COPY --from=build /app/dist/ui/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
