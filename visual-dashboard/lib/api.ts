export interface VisualLab {
  id: number;
  hostname: string;
  ipAddress: string;
  operatingSystem: string;
  status: string;
  lastSeenUtc: string;
  createdAtUtc: string;
  securityEventCount: number;
}

export interface VisualSecurityEvent {
  id: number;
  assetId: number;
  hostname: string | null;
  eventType: string;
  severity: string;
  description: string;
  createdAtUtc: string;
}

export interface DashboardData {
  labs: VisualLab[];
  securityEvents: VisualSecurityEvent[];
  error: string | null;
  fetchedAtUtc: string;
}

function getApiConfig() {
  const baseUrl = (process.env.API_BASE_URL ?? "http://localhost:5000").replace(/\/+$/, "");
  const apiKey =
    process.env.API_KEY ??
    (process.env.NODE_ENV !== "production" ? "dev-local-api-key-change-in-prod" : "");

  return { baseUrl, apiKey };
}

function buildHeaders(apiKey: string): HeadersInit {
  const headers: Record<string, string> = {
    Accept: "application/json",
  };
  if (apiKey) {
    headers["X-Api-Key"] = apiKey;
  }
  return headers;
}

export async function fetchDashboardData(): Promise<DashboardData> {
  const { baseUrl, apiKey } = getApiConfig();
  const headers = buildHeaders(apiKey);
  const fetchedAtUtc = new Date().toISOString();

  try {
    const [labsRes, securityRes] = await Promise.all([
      fetch(`${baseUrl}/api/VisualLabs`, { headers, cache: "no-store" }),
      fetch(`${baseUrl}/api/VisualSecurity`, { headers, cache: "no-store" }),
    ]);

    if (labsRes.status === 401 || securityRes.status === 401) {
      return {
        labs: [],
        securityEvents: [],
        error: "Error de autenticación (401): verifica que API_KEY coincida con ProjectAssetAPI.",
        fetchedAtUtc,
      };
    }

    if (!labsRes.ok) {
      return {
        labs: [],
        securityEvents: [],
        error: `No se pudieron obtener los dispositivos (HTTP ${labsRes.status}).`,
        fetchedAtUtc,
      };
    }

    const labs = (await labsRes.json()) as VisualLab[];
    const securityEvents = securityRes.ok
      ? ((await securityRes.json()) as VisualSecurityEvent[])
      : [];

    return {
      labs,
      securityEvents,
      error: null,
      fetchedAtUtc,
    };
  } catch (error) {
    const detail = error instanceof Error ? error.message : "Error desconocido";
    return {
      labs: [],
      securityEvents: [],
      error: `No se pudo conectar con ProjectAssetAPI en ${baseUrl} (${detail}).`,
      fetchedAtUtc,
    };
  }
}
