export type Balance = {
  itemId: number;
  itemCode: string;
  itemName: string;
  storeId: number;
  storeName: string;
  unitId: number;
  unitName: string;
  availableQuantity: number;
  reorderLevel: number;
  isBelowReorderLevel: boolean;
};
