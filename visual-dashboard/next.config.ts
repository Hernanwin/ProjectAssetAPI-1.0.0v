import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Agrega esta línea para permitir que tu red local acceda a los recursos
  allowedDevOrigins: ['192.168.0.28'],
  
  // Si tenías otras configuraciones aquí, mantenlas
};

export default nextConfig;