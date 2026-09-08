import type { Detail } from "./Detail";

export type Transaction = {
  id: number;
  transactionNo: string;
  transactionDate: string;
  transactionType: number;
  storeId: number;
  storeName: string;
  remarks?: string;
  details: Detail[];
};
