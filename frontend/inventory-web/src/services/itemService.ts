import type { Item } from "../models/Item";
import { api, query } from "./api";

export const items = {
  list: (search = "") => api<Item[]>(`/items${query({ search })}`),
  get: (id: number) => api<Item>(`/items/${id}`),
  save: (
    payload: Omit<Item, "id" | "categoryName" | "unitName">,
    id?: number,
  ) =>
    api<Item>(`/items${id ? `/${id}` : ""}`, {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(payload),
    }),
  remove: (id: number) => api<unknown>(`/items/${id}`, { method: "DELETE" }),
};
