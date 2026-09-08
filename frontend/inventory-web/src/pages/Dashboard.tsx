import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import type { Balance } from "../models/Balance";
import { stock } from "../services/stockService";
export function Dashboard() {
  const [rows, setRows] = useState<Balance[]>([]);
  useEffect(() => {
    stock
      .balances()
      .then(setRows)
      .catch(() => {});
  }, []);
  const low = rows.filter((x) => x.isBelowReorderLevel);
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Overview</p>
          <h2>Inventory dashboard</h2>
        </div>
        <Link className="button" to="/transactions/new">
          New transaction
        </Link>
      </header>
      <div className="stats">
        <article>
          <b>{rows.length}</b>
          <span>Stock positions</span>
        </article>
        <article>
          <b>{low.length}</b>
          <span>Need reordering</span>
        </article>
        <article>
          <b>{new Set(rows.map((x) => x.storeId)).size}</b>
          <span>Active stores</span>
        </article>
      </div>
      <section className="panel">
        <h3>Items below reorder level</h3>
        {low.length ? (
          <table>
            <thead>
              <tr>
                <th>Item</th>
                <th>Store</th>
                <th>Available</th>
                <th>Reorder level</th>
              </tr>
            </thead>
            <tbody>
              {low.map((x) => (
                <tr key={`${x.itemId}-${x.storeId}`}>
                  <td>
                    {x.itemCode} · {x.itemName}
                  </td>
                  <td>{x.storeName}</td>
                  <td>{x.availableQuantity}</td>
                  <td>{x.reorderLevel}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className="muted">No low-stock items.</p>
        )}
      </section>
    </>
  );
}
