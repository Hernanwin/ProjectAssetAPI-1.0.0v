"use client";

import * as React from "react";
import {
  Activity,
  AlertTriangle,
  Monitor,
  RefreshCw,
  ShieldAlert,
  Wifi,
  WifiOff,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { DashboardData } from "@/lib/api";

const AUTO_REFRESH_MS = 15_000;

function formatUtcTimestamp(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return iso;
  }

  return new Intl.DateTimeFormat("es-ES", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: false,
    timeZone: "UTC",
  }).format(date) + " UTC";
}

function getStatusBadge(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "online") {
    return (
      <Badge className="bg-emerald-600 text-white hover:bg-emerald-600">
        <Wifi className="size-3" />
        Online
      </Badge>
    );
  }
  if (normalized === "maintenance") {
    return (
      <Badge variant="secondary">
        <Activity className="size-3" />
        Maintenance
      </Badge>
    );
  }
  return (
    <Badge variant="destructive">
      <WifiOff className="size-3" />
      Offline
    </Badge>
  );
}

function getSeverityBadge(severity: string) {
  const normalized = severity.toLowerCase();
  if (normalized === "critical" || normalized === "high") {
    return <Badge variant="destructive">{severity}</Badge>;
  }
  if (normalized === "medium") {
    return <Badge variant="secondary">{severity}</Badge>;
  }
  return <Badge variant="outline">{severity}</Badge>;
}

export function DashboardClient({ initialData }: { initialData: DashboardData }) {
  const [data, setData] = React.useState<DashboardData>(initialData);
  const [isRefreshing, setIsRefreshing] = React.useState(false);
  const [autoRefresh, setAutoRefresh] = React.useState(true);

  const refresh = React.useCallback(async () => {
    setIsRefreshing(true);
    try {
      const response = await fetch("/api/dashboard", { cache: "no-store" });
      if (response.ok) {
        const nextData = (await response.json()) as DashboardData;
        setData(nextData);
      }
    } catch {
      setData((prev) => ({
        ...prev,
        error: "No se pudo actualizar el estado desde el servidor local.",
      }));
    } finally {
      setIsRefreshing(false);
    }
  }, []);

  React.useEffect(() => {
    if (!autoRefresh) {
      return;
    }

    const timer = setInterval(() => {
      void refresh();
    }, AUTO_REFRESH_MS);

    return () => clearInterval(timer);
  }, [autoRefresh, refresh]);

  const onlineCount = data.labs.filter(
    (lab) => lab.status.toLowerCase() === "online"
  ).length;
  const offlineCount = data.labs.length - onlineCount;

  return (
    <main className="p-6 md:p-8 max-w-7xl w-full mx-auto space-y-8">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b pb-4">
        <div>
          <h1 className="scroll-m-20 text-3xl font-semibold tracking-tight">
            Panel de Monitoreo de Dispositivos
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Última actualización: {formatUtcTimestamp(data.fetchedAtUtc)}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant={autoRefresh ? "secondary" : "outline"}
            size="sm"
            onClick={() => setAutoRefresh((prev) => !prev)}
          >
            <Activity className="size-4" />
            {autoRefresh ? "Auto-refresco: ON (15s)" : "Auto-refresco: OFF"}
          </Button>
          <Button
            variant="default"
            size="sm"
            onClick={() => void refresh()}
            disabled={isRefreshing}
          >
            <RefreshCw className={`size-4 ${isRefreshing ? "animate-spin" : ""}`} />
            Actualizar
          </Button>
        </div>
      </div>

      {data.error && (
        <div
          role="alert"
          className="flex items-start gap-3 rounded-lg border border-destructive/40 bg-destructive/10 p-4 text-sm text-destructive"
        >
          <AlertTriangle className="size-5 shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold">Error de comunicación con la API</p>
            <p className="mt-0.5">{data.error}</p>
          </div>
        </div>
      )}

      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card size="sm">
          <CardHeader>
            <CardDescription>Total de dispositivos</CardDescription>
            <CardTitle className="text-2xl font-bold flex items-center gap-2">
              <Monitor className="size-5 text-muted-foreground" />
              {data.labs.length}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card size="sm">
          <CardHeader>
            <CardDescription>En línea</CardDescription>
            <CardTitle className="text-2xl font-bold text-emerald-600 flex items-center gap-2">
              <Wifi className="size-5" />
              {onlineCount}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card size="sm">
          <CardHeader>
            <CardDescription>Desconectados / Otros</CardDescription>
            <CardTitle className="text-2xl font-bold text-destructive flex items-center gap-2">
              <WifiOff className="size-5" />
              {offlineCount}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card size="sm">
          <CardHeader>
            <CardDescription>Eventos de seguridad</CardDescription>
            <CardTitle className="text-2xl font-bold flex items-center gap-2">
              <ShieldAlert className="size-5 text-muted-foreground" />
              {data.securityEvents.length}
            </CardTitle>
          </CardHeader>
        </Card>
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">
          Dispositivos registrados
        </h2>

        {data.labs.length === 0 ? (
          <div className="text-center p-10 border rounded-xl bg-muted/20">
            <p className="text-lg font-medium text-muted-foreground">
              {data.error
                ? "No se pudieron cargar los dispositivos mientras la API no responda."
                : "No hay dispositivos registrados todavía. Ejecuta ProjectAssetAgent para reportar un equipo."}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {data.labs.map((lab) => (
              <Card
                key={lab.id}
                className="shadow-sm hover:shadow-md transition-shadow"
              >
                <CardHeader className="pb-2">
                  <div className="flex items-center justify-between gap-2">
                    <CardTitle className="text-lg text-primary truncate">
                      {lab.hostname}
                    </CardTitle>
                    {getStatusBadge(lab.status)}
                  </div>
                  <CardDescription>ID #{lab.id}</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-col space-y-2.5 text-sm">
                    <div className="flex justify-between items-center border-b pb-2">
                      <span className="text-muted-foreground">IP identificada</span>
                      <span className="font-mono font-medium">{lab.ipAddress}</span>
                    </div>
                    <div className="flex justify-between items-start gap-4 border-b pb-2">
                      <span className="text-muted-foreground shrink-0">
                        Sistema Operativo
                      </span>
                      <span className="font-mono text-right text-xs font-medium">
                        {lab.operatingSystem}
                      </span>
                    </div>
                    <div className="flex justify-between items-center border-b pb-2">
                      <span className="text-muted-foreground">Último reporte</span>
                      <span className="font-mono text-xs">
                        {formatUtcTimestamp(lab.lastSeenUtc)}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-muted-foreground">Eventos seguridad</span>
                      <Badge variant={lab.securityEventCount > 0 ? "destructive" : "outline"}>
                        {lab.securityEventCount}
                      </Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </section>

      <section className="space-y-4">
        <h2 className="text-xl font-semibold tracking-tight">
          Eventos de seguridad recientes
        </h2>

        <Card>
          <CardContent className="pt-2">
            {data.securityEvents.length === 0 ? (
              <p className="text-sm text-muted-foreground py-6 text-center">
                No se registran eventos de seguridad.
              </p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Severidad</TableHead>
                    <TableHead>Dispositivo</TableHead>
                    <TableHead>Tipo de evento</TableHead>
                    <TableHead>Descripción</TableHead>
                    <TableHead>Fecha (UTC)</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.securityEvents.map((evt) => (
                    <TableRow key={evt.id}>
                      <TableCell>{getSeverityBadge(evt.severity)}</TableCell>
                      <TableCell className="font-mono text-xs">
                        {evt.hostname ?? `Asset #${evt.assetId}`}
                      </TableCell>
                      <TableCell className="font-medium">{evt.eventType}</TableCell>
                      <TableCell className="max-w-md whitespace-normal">
                        {evt.description}
                      </TableCell>
                      <TableCell className="font-mono text-xs">
                        {formatUtcTimestamp(evt.createdAtUtc)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </section>
    </main>
  );
}
