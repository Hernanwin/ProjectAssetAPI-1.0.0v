import { NextResponse } from "next/server";
import { fetchDashboardData } from "@/lib/api";

export const dynamic = "force-dynamic";

export async function GET() {
  const data = await fetchDashboardData();
  return NextResponse.json(data, {
    headers: {
      "Cache-Control": "no-store",
    },
  });
}
