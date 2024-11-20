# git-httt-dtts

# Dự án Ban Dân tộc: Modul Hệ thống thông tin Dân tộc thiểu số

**1. Cấu hình Solutions cho VS:**

- NuGet:
  - Newtonsoft.Json (Author: James Newton-King)
  - Microsoft.AspNetCore.Mvc.NewtonsoftJson -> chọn version 6.0.36 (dành cho WebAPI)

- References:
  - System.Net.Http

**2. File Web.Config:**
- Tạo DB, khai báo trong dòng connectionString (lưu ý user tạm mở quyền dbcreater và sysadmin (ServerRoles)

**3. Defaults:**
Tài khoản Admin: admin/admin
