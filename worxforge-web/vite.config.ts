import { defineConfig } from "vitest/config";
import { loadEnv } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import tailwindcss from "@tailwindcss/vite";
import { fileURLToPath, URL } from "node:url";

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), "");
  const apiProxyTarget = environment.VITE_API_PROXY_TARGET || "https://localhost:7235";

  return {
    resolve: {
      alias: {
        "@": fileURLToPath(new URL("./src", import.meta.url))
      }
    },
    plugins: [
      react(),
      babel({
        presets: [reactCompilerPreset()]
      }),
      tailwindcss()
    ],
    server: {
      proxy: {
        "/api": {
          target: apiProxyTarget,
          changeOrigin: true,
          secure: false
        }
      }
    },
    test: {
      environment: "jsdom",
      globals: true,
      pool: "forks",
      setupFiles: "./src/test/setup.ts"
    }
  };
});
