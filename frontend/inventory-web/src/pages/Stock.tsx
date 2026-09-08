import { useEffect, useState } from "react";
import type { Balance } from "../models/Balance";
import { stock } from "../services/stockService";
export function Stock() {
  const [rows, setRows] = useState<Balance[]>([]),
    [error, setError] = useState("");
  useEffect(() => {
    stock
      .balances()
      .then(setRows)
      .catch((e) => setError(e.message));
  }, []);
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Availability</p>
          <h2>Stock balances</h2>
        </div>
      </header>
      {error && <p className="error">{error}</p>}
      <section className="panel">
        <table>
          <thead>
            <tr>
              <th>Item</th>
              <th>Store</th>
              <th>Unit</th>
              <th>Available</th>
              <th>Reorder level</th>
              <th>Health</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((x) => (
              <tr key={`${x.itemId}-${x.storeId}`}>
                <td>
                  {x.itemCode} · {x.itemName}
                </td>
                <td>{x.storeName}</td>
                <td>{x.unitName}</td>
                <td>{x.availableQuantity}</td>
                <td>{x.reorderLevel}</td>
                <td>
                  <span
                    className={
                      x.isBelowReorderLevel ? "badge warning" : "badge"
                    }
                  >
                    {x.isBelowReorderLevel ? "Reorder" : "Healthy"}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </>
  );
}
