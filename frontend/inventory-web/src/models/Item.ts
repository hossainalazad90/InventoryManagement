export type Item = {
  id: number;
  itemCode: string;
  itemName: string;
  categoryId: number;
  categoryName: string;
  unitId: number;
  unitName: string;
  reorderLevel: number;
  isActive: boolean;
};
