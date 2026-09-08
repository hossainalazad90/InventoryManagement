import type { Category } from "../models/Category";
import type { Store } from "../models/Store";
import type { Unit } from "../models/Unit";
import { api } from "./api";

export const master = {
  categories: () => api<Category[]>("/categories?activeOnly=true"),
  units: () => api<Unit[]>("/units?activeOnly=true"),
  stores: () => api<Store[]>("/stores?activeOnly=true"),
};
