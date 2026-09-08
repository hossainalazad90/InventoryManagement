const API_URL = import.meta.env.VITE_API_URL;
export type ApiResponse<T> = {
  success: boolean;
  message?: string;
  data?: T;
  errors?: Record<string, string[]>;
};
export async function api<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const response = await fetch(`${API_URL}${endpoint}`, {
    headers: { "Content-Type": "application/json", ...options.headers },
    ...options,
  });
  const body = (await response.json().catch(() => ({}))) as ApiResponse<T>;
  if (!response.ok || !body.success)
    throw new Error(
      body.message ||
        Object.values(body.errors ?? {})
          .flat()
          .join(" ") ||
        "Request failed",
    );
  return body.data as T;
}
export function query(
  params: Record<string, string | number | boolean | undefined>,
) {
  const q = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== "") q.set(k, String(v));
  });
  return q.toString() ? `?${q}` : "";
}
