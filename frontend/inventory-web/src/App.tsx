import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { Dashboard } from "./pages/Dashboard";
import { Items } from "./pages/Items";
import { ItemForm } from "./pages/ItemForm";
import { Stock } from "./pages/Stock";
import { StockTransactions } from "./pages/StockTransactions";
import { StockTransactionForm } from "./pages/StockTransactionForm";
import { Reports } from "./pages/Reports";
export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="items" element={<Items />} />
          <Route path="items/new" element={<ItemForm />} />
          <Route path="items/:id" element={<ItemForm />} />
          <Route path="stock" element={<Stock />} />
          <Route path="transactions" element={<StockTransactions />} />
          <Route path="transactions/new" element={<StockTransactionForm />} />
          <Route path="transactions/:id" element={<StockTransactionForm />} />
          <Route path="reports" element={<Reports />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
