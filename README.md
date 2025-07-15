# Bikya

# 📦 Bikya - Online Product Exchange & Selling Platform

Bikya is a full-stack web application built with ASP.NET Core (.NET 8) and Angular 19 that enables users to **sell, buy, and exchange products**. Admins can manage product approvals, while users can add products with images, track their submissions, and view their own listings.

---

## 🚀 Technologies Used

### Backend
- .NET 8 Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Role-based Authorization (Admin/User)

### Frontend
- Angular 19
- RxJS, Angular Material
- Lazy Loading & Guards
- Form Validation + Reactive Forms

### Others
- IIS / Kestrel Hosting
- Image Upload (wwwroot)
- AutoMapper
- Swagger (for testing APIs)

---

## 🧱 Main System Components (Entities)

| Entity         | Description                                |
|----------------|--------------------------------------------|
| `User`         | Registered user with roles (Admin/User)    |
| `Product`      | Item listed for sale or exchange           |
| `Category`     | Product categories                         |
| `ProductImage` | Images related to a product                |
| `Shipping`     | (Optional) Shipping details if required    |

---

## 🔁 Business Logic & Workflow

### 🔹 User Flow
1. Register or Login
2. Create Product (with/without images)
3. Upload Main & Additional Images
4. Product goes into review (IsApproved = false)
5. Track own products
6. Once approved, product is visible to others

### 🔹 Admin Flow
1. Login as Admin
2. View all products
3. Approve/Reject submissions
4. Manage listings

---

## 🛠️ How to Run the Project

### Backend

```bash
git clone https://github.com/your-org/bikya.git
cd Bikya/Bikya.API
dotnet ef database update
dotnet run

### Frontend
