import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { transactions } from "../services/transactionService";
import type { Transaction } from "../models/Transaction";
const names: Record<number, string> = { 1: "Receive", 2: "Issue", 3: "Return" };
export function StockTransactions() {
  const [rows, setRows] = useState<Transaction[]>([]),
    [error, setError] = useState("");
  const load = () =>
    transactions
      .list()
      .then(setRows)
      .catch((e) => setError(e.message));
  useEffect(() => {
    load();
  }, []);
  async function remove(id: number) {
    if (confirm("Delete and reverse this transaction?"))
      try {
        await transactions.remove(id);
        load();
      } catch (e) {
        setError((e as Error).message);
      }
  }
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Operations</p>
          <h2>Stock transactions</h2>
        </div>
        <Link className="button" to="/transactions/new">
          New transaction
        </Link>
      </header>
      {error && <p className="error">{error}</p>}
      <section className="panel">
        <table>
          <thead>
            <tr>
              <th>No.</th>
              <th>Date</th>
              <th>Type</th>
              <th>Store</th>
              <th>Lines</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((x) => (
              <tr key={x.id}>
                <td>{x.transactionNo}</td>
                <td>{new Date(x.transactionDate).toLocaleDateString()}</td>
                <td>{names[x.transactionType]}</td>
                <td>{x.storeName}</td>
                <td>{x.details.length}</td>
                <td>
                  <Link to={`/transactions/${x.id}`}>Edit</Link>{" "}
                  <button className="link danger" onClick={() => remove(x.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </>
  );
}
