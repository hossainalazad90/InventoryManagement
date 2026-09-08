import type { Transaction } from "../models/Transaction";
import { api } from "./api";

export const transactions = {
  list: () => api<Transaction[]>("/stock-transactions"),
  get: (id: number) => api<Transaction>(`/stock-transactions/${id}`),
  save: (payload: unknown, id?: number) =>
    api<Transaction>(`/stock-transactions${id ? `/${id}` : ""}`, {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(payload),
    }),
  remove: (id: number) =>
    api<unknown>(`/stock-transactions/${id}`, { method: "DELETE" }),
};
