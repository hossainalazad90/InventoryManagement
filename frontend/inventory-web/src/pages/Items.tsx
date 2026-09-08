import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import type { Item } from "../models/Item";
import { items } from "../services/itemService";
export function Items() {
  const [data, setData] = useState<Item[]>([]),
    [search, setSearch] = useState(""),
    [error, setError] = useState("");
  const load = (term = search) =>
    items
      .list(term)
      .then(setData)
      .catch((e) => setError(e.message));
  useEffect(() => {
    items
      .list()
      .then(setData)
      .catch((e) => setError(e.message));
  }, []);
  async function remove(id: number) {
    if (confirm("Delete this item?")) {
      try {
        await items.remove(id);
        load();
      } catch (e) {
        setError((e as Error).message);
      }
    }
  }
  return (
    <>
      <header>
        <div>
          <p className="eyebrow">Master data</p>
          <h2>Items</h2>
        </div>
        <Link className="button" to="/items/new">
          Add item
        </Link>
      </header>
      <div className="toolbar">
        <input
          placeholder="Search item code or name"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button onClick={() => load()}>Search</button>
      </div>
      {error && <p className="error">{error}</p>}
      <section className="panel">
        <table>
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Category</th>
              <th>Unit</th>
              <th>Reorder level</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data.map((x) => (
              <tr key={x.id}>
                <td>{x.itemCode}</td>
                <td>{x.itemName}</td>
                <td>{x.categoryName}</td>
                <td>{x.unitName}</td>
                <td>{x.reorderLevel}</td>
                <td>
                  <span className={x.isActive ? "badge" : "badge muted"}>
                    {x.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td>
                  <Link to={`/items/${x.id}`}>Edit</Link>{" "}
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
