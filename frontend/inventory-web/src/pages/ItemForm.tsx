import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import type { Category } from "../models/Category";
import type { Unit } from "../models/Unit";
import { master } from "../services/masterService";
import { items } from "../services/itemService";
export function ItemForm() {
  const { id } = useParams(),
    nav = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]),
    [units, setUnits] = useState<Unit[]>([]),
    [error, setError] = useState("");
  const [form, setForm] = useState({
    itemCode: "",
    itemName: "",
    categoryId: 0,
    unitId: 0,
    reorderLevel: 0,
    isActive: true,
  });
  useEffect(() => {
    Promise.all([master.categories(), master.units()]).then(([c, u]) => {
      setCategories(c);
      setUnits(u);
    });
    if (id)
      items
        .get(+id)
        .then((x) =>
          setForm({
            itemCode: x.itemCode,
            itemName: x.itemName,
            categoryId: x.categoryId,
            unitId: x.unitId,
            reorderLevel: x.reorderLevel,
            isActive: x.isActive,
          }),
        )
        .catch((e) => setError(e.message));
  }, [id]);
  const set = (key: string, value: string | number | boolean) =>
    setForm((p) => ({ ...p, [key]: value }));
  async function submit(e: React.FormEvent) {
    e.preventDefault();
    try {
      await items.save(form, id ? +id : undefined);
      nav("/items");
    } catch (e) {
      setError((e as Error).message);
    }
  }
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Master data</p>
          <h2>{id ? "Edit item" : "New item"}</h2>
        </div>
        <Link to="/items">Cancel</Link>
      </header>
      <form className="panel form" onSubmit={submit}>
        {error && <p className="error">{error}</p>}
        <label>
          Item code
          <input
            required
            value={form.itemCode}
            onChange={(e) => set("itemCode", e.target.value)}
          />
        </label>
        <label>
          Item name
          <input
            required
            value={form.itemName}
            onChange={(e) => set("itemName", e.target.value)}
          />
        </label>
        <label>
          Category
          <select
            required
            value={form.categoryId}
            onChange={(e) => set("categoryId", +e.target.value)}
          >
            <option value="0">Select category</option>
            {categories.map((x) => (
              <option value={x.id} key={x.id}>
                {x.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Unit
          <select
            required
            value={form.unitId}
            onChange={(e) => set("unitId", +e.target.value)}
          >
            <option value="0">Select unit</option>
            {units.map((x) => (
              <option value={x.id} key={x.id}>
                {x.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Reorder level
          <input
            type="number"
            min="0"
            step="0.001"
            value={form.reorderLevel}
            onChange={(e) => set("reorderLevel", +e.target.value)}
          />
        </label>
        <label className="check">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(e) => set("isActive", e.target.checked)}
          />{" "}
          Active item
        </label>
        <div>
          <button className="button" type="submit">
            Save item
          </button>
        </div>
      </form>
    </>
  );
}
