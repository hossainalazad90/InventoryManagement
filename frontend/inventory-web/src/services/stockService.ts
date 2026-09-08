import type { Balance } from "../models/Balance";
import { api, query } from "./api";

export const stock = {
  balances: () => api<Balance[]>("/stock/balances"),
  balance: (itemId: number, storeId: number) =>
    api<Balance>(`/stock${query({ itemId, storeId })}`),
};
