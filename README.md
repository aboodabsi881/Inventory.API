# 📦 Inventory Management System

A modern, full-stack **Inventory Management Application** built using **ASP.NET Core MVC** and **ASP.NET Core Web API**. The platform enables seamless product and category management, real-time cart manipulation, wishlisting, and role-based permissions with a clean, responsive UI.

---

## 🚀 Features

* **📦 Product Management:** Browse, create, edit, view details, and delete inventory products.
* **🏷️ Category Management:** Organize products under specific categories with full CRUD capabilities.
* **🛒 Interactive Shopping Cart:**
  * Real-time AJAX item updates (increment/decrement quantities dynamically).
  * Auto-calculating grand total and item subtotals without full page refreshes.
  * Adaptive UI buttons transitioning from `Add to Cart` to a pure Bootstrap `+ / -` quantity stepper.
* **❤️ Favorite List (Wishlist):** One-click AJAX favoriting with dynamic visual feedback across product cards.
* **🖼️ Lightbox Image Viewer:** Click-to-enlarge high-resolution product and category preview modals using **SweetAlert2**.
* **🔐 Anti-Forgery & Validation:** Secure AJAX communications using ASP.NET Core Anti-Forgery tokens.
* **🔒 Role Management:** Admin panel structure for role and permissions setup.

---

## 📊 Dashboard Preview

[![Inventory Dashboard](dashboard_img.png)](dashboard_img.png)

> 💡 *Click the preview image to open the full [Dashboard PDF](dashboard_img.png).*

---

## 🛠️ Tech Stack & Architecture

### **Architecture Layering**
* **`Inventory.API`**: RESTful API handling core business logic, database transactions, and data retrieval.
* **`Inventory.Web`**: ASP.NET Core MVC Client consuming the Web API via `HttpClientFactory`.
* **`Inventory.Core`**: DTOs, ViewModel mappings, and domain interfaces.

### **Technologies Used**
* **Backend:** C# / .NET (ASP.NET Core MVC & Web API)
* **Frontend:** Razor Views (CSHTML), Bootstrap 5, Bootstrap Icons
* **Client Scripting:** jQuery, AJAX, SweetAlert2
* **Data Interchange:** JSON (`System.Text.Json`)

---

## 📁 Project Structure

```text
InventoryApp/
├── dashboard-overview.pdf       # Full dashboard PDF
├── dashboard-preview.png        # Rendered image preview for README
├── Inventory.API/               # RESTful Web API project
│   └── Controllers/             # CartsController, ProductsController, etc.
│
├── Inventory.Core/              # Shared Layer
│   ├── DTOs/                    # Data Transfer Objects
│   └── Interfaces/              # Service & Repository Contracts
│
└── Inventory.Web/               # MVC Client project
    ├── Controllers/             # CartController, ProductsController, CategoriesController
    ├── ViewModels/              # ProductVM, CategoryVM, CartVM
    └── Views/                   # Razor Views (Cart, Products, Categories, Shared)
