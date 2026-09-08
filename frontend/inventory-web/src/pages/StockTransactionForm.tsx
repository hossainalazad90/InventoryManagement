import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import type { Detail } from "../models/Detail";
import type { Item } from "../models/Item";
import type { Store } from "../models/Store";
import type { Unit } from "../models/Unit";
import { master } from "../services/masterService";
import { items } from "../services/itemService";
import { transactions } from "../services/transactionService";
import { stock } from "../services/stockService";

const blank = (): Detail => ({
  id: 0,
  itemId: 0,
  quantity: 1,
  unitId: 0,
  remarks: "",
  detailDate: new Date().toISOString().slice(0, 10),
  isChecked: true,
});
export function StockTransactionForm() {
  const { id } = useParams(),
    nav = useNavigate();
  const [catalog, setCatalog] = useState<Item[]>([]),
    [stores, setStores] = useState<Store[]>([]),
    [units, setUnits] = useState<Unit[]>([]),
    [error, setError] = useState(""),
    [saving, setSaving] = useState(false),
    [deleted, setDeleted] = useState<number[]>([]);
  const [form, setForm] = useState({
    transactionDate: new Date().toISOString().slice(0, 10),
    transactionType: 1,
    storeId: 0,
    remarks: "",
    details: [blank()],
  });
  useEffect(() => {
    Promise.all([items.list(), master.stores(), master.units()]).then(
      ([i, s, u]) => {
        setCatalog(i);
        setStores(s);
        setUnits(u);
      },
    );
    if (id)
      transactions
        .get(+id)
        .then((x) =>
          setForm({
            transactionDate: x.transactionDate.slice(0, 10),
            transactionType: x.transactionType,
            storeId: x.storeId,
            remarks: x.remarks ?? "",
            details: x.details.map((d) => ({
              ...d,
              detailDate: d.detailDate?.slice(0, 10),
            })),
          }),
        )
        .catch((e) => setError(e.message));
  }, [id]);
  function update(
    index: number,
    key: keyof Detail,
    value: string | number | boolean,
  ) {
    setForm((p) => {
      const details = p.details.map((d, i) =>
        i === index ? { ...d, [key]: value } : d,
      );
      if (key === "itemId") {
        const item = catalog.find((x) => x.id === value);
        if (item) details[index].unitId = item.unitId;
      }
      return { ...p, details };
    });
  }
  function remove(i: number) {
    setForm((p) => {
      const removed = p.details[i];
      if (removed.id) setDeleted((x) => [...x, removed.id]);
      return { ...p, details: p.details.filter((_, n) => n !== i) };
    });
  }
  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    if (
      !form.storeId ||
      !form.details.length ||
      form.details.some((d) => !d.itemId || !d.unitId || d.quantity <= 0)
    ) {
      setError("Complete every transaction row with a positive quantity.");
      return;
    }
    try {
      setSaving(true);
      if (form.transactionType === 2) {
        for (const d of form.details) {
          const available = await stock.balance(d.itemId, form.storeId);
          if (d.quantity > available.availableQuantity)
            throw new Error(
              `${available.itemName}: only ${available.availableQuantity} available.`,
            );
        }
      }
      const payload = id
        ? {
            transactionId: +id,
            ...form,
            details: form.details,
            deletedDetailIds: deleted,
          }
        : form;
      await transactions.save(payload, id ? +id : undefined);
      nav("/transactions");
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setSaving(false);
    }
  }
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Operations</p>
          <h2>{id ? "Edit transaction" : "New transaction"}</h2>
        </div>
        <Link to="/transactions">Cancel</Link>
      </header>
      <form onSubmit={submit}>
        {error && <p className="error">{error}</p>}
        <section className="panel form header-form">
          <label>
            Date
            <input
              required
              type="date"
              value={form.transactionDate}
              onChange={(e) =>
                setForm((p) => ({ ...p, transactionDate: e.target.value }))
              }
            />
          </label>
          <label>
            Type
            <select
              value={form.transactionType}
              onChange={(e) =>
                setForm((p) => ({ ...p, transactionType: +e.target.value }))
              }
            >
              <option value="1">Receive</option>
              <option value="2">Issue</option>
              <option value="3">Return</option>
            </select>
          </label>
          <label>
            Store
            <select
              required
              value={form.storeId}
              onChange={(e) =>
                setForm((p) => ({ ...p, storeId: +e.target.value }))
              }
            >
              <option value="0">Select store</option>
              {stores.map((x) => (
                <option key={x.id} value={x.id}>
                  {x.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Remarks
            <input
              value={form.remarks}
              onChange={(e) =>
                setForm((p) => ({ ...p, remarks: e.target.value }))
              }
            />
          </label>
        </section>
        <section className="panel">
          <div className="section-head">
            <h3>Transaction details</h3>
            <button
              type="button"
              onClick={() =>
                setForm((p) => ({ ...p, details: [...p.details, blank()] }))
              }
            >
              Add row
            </button>
          </div>
          <table className="grid">
            <thead>
              <tr>
                <th>Item</th>
                <th>Qty</th>
                <th>Unit</th>
                <th>Date</th>
                <th>Remarks</th>
                <th>Include</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {form.details.map((d, i) => (
                <tr key={`${d.id}-${i}`}>
                  <td>
                    <select
                      value={d.itemId}
                      onChange={(e) => update(i, "itemId", +e.target.value)}
                    >
                      <option value="0">Select item</option>
                      {catalog.map((x) => (
                        <option key={x.id} value={x.id}>
                          {x.itemCode} · {x.itemName}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <input
                      type="number"
                      min="0.001"
                      step="0.001"
                      value={d.quantity}
                      onChange={(e) => update(i, "quantity", +e.target.value)}
                    />
                  </td>
                  <td>
                    <select
                      value={d.unitId}
                      onChange={(e) => update(i, "unitId", +e.target.value)}
                    >
                      {units.map((x) => (
                        <option key={x.id} value={x.id}>
                          {x.name}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <input
                      type="date"
                      value={d.detailDate ?? ""}
                      onChange={(e) => update(i, "detailDate", e.target.value)}
                    />
                  </td>
                  <td>
                    <input
                      value={d.remarks ?? ""}
                      onChange={(e) => update(i, "remarks", e.target.value)}
                    />
                  </td>
                  <td>
                    <input
                      type="checkbox"
                      checked={d.isChecked}
                      onChange={(e) => update(i, "isChecked", e.target.checked)}
                    />
                  </td>
                  <td>
                    <button
                      type="button"
                      className="link danger"
                      onClick={() => remove(i)}
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
        <button className="button" disabled={saving} type="submit">
          {saving ? "Saving…" : "Save transaction"}
        </button>
      </form>
    </>
  );
}
