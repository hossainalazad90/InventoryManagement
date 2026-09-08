import { NavLink, Outlet } from "react-router-dom";
const links = [
  ["/", "Dashboard"],
  ["/items", "Items"],
  ["/stock", "Stock"],
  ["/transactions", "Transactions"],
  ["/reports", "Reports"],
];
export function Layout() {
  return (
    <div className="app-shell">
      <aside>
        <h1>Stockwise</h1>
        <p>Inventory management</p>
        <nav>
          {links.map(([to, label]) => (
            <NavLink key={to} to={to} end={to === "/"}>
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
