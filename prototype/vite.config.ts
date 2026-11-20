import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: { alias: { "@": path.resolve(__dirname, "src") } },
  // Wspólny adres z API, dzięki temu ciasteczko SameSite=Strict działa bez CORS.
  server: { proxy: { "/api": "http://localhost:5080" } },
});
