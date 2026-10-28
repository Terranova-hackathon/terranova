FROM node:22-alpine

WORKDIR /app

COPY package.json ./
RUN npm install

# Source is bind-mounted in dev; this COPY is a fallback for production builds
COPY . .

EXPOSE 8080

CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--port", "8080", "--poll", "1000"]
