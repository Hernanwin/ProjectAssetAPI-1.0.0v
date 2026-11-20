import { DashboardClient } from "@/components/dashboard-client";
import { fetchDashboardData } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const initialData = await fetchDashboardData();
  return <DashboardClient initialData={initialData} />;
}
