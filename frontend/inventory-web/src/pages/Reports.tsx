import { useState } from "react";
import { query } from "../services/api";
const base = import.meta.env.VITE_API_URL;
export function Reports() {
  const [kind, setKind] = useState("stock-movement"),
    [from, setFrom] = useState(
      new Date(new Date().setMonth(new Date().getMonth() - 1))
        .toISOString()
        .slice(0, 10),
    ),
    [to, setTo] = useState(new Date().toISOString().slice(0, 10));
  function download(format: string) {
    window.open(
      `${base}/reports/${kind}/export${query({ format, fromDate: from, toDate: to })}`,
      "_blank",
      "noopener",
    );
  }
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Reporting</p>
          <h2>Inventory reports</h2>
        </div>
      </header>
      <section className="panel form report">
        <label>
          Report
          <select value={kind} onChange={(e) => setKind(e.target.value)}>
            <option value="stock-movement">Stock movement</option>
            <option value="transaction-details">Transaction details</option>
          </select>
        </label>
        <label>
          From
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
          />
        </label>
        <label>
          To
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
          />
        </label>
        <div>
          <p className="muted">
            Exports use the backend RDLC report generator.
          </p>
          <button type="button" onClick={() => download("pdf")}>
            Export PDF
          </button>{" "}
          <button type="button" onClick={() => download("excel")}>
            Export Excel
          </button>
        </div>
      </section>
    </>
  );
}
