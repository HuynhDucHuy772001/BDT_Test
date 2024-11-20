# Một số lưu ý khi sử dụng DevExpress XAF .NET Web forms và Core WebApi

**Kiến trúc:**
- Web sử dụng .Net 4.5.2 (có thể xài 4.9)
- WebAPI sử dụng .Net Core 6.0
- Link Document chuẩn của Devexpress: _https://docs.devexpress.com/eXpressAppFramework/112670/expressapp-framework?p=netframework_

**Khái niệm trong XAF**
- Khái niệm XPO: là bộ công cụ để tương tác với database => eXpress Persistent Objects (or **XPO**) is an Object-Relational Mapping (**ORM**) tool for .NET that handles all aspects of database creation and object persistence, allowing you to concentrate on your application’s business logic rather than database complexities.
  -> https://docs.devexpress.com/eXpressAppFramework/112600/business-model-design-orm/business-model-design-with-xpo
- Session (XPO), ObjectSpace (XAF Application): ObjectSpace chính là thư viện API viết sẵn mà XAF cung cấp để tương tác với DB thay vì phải tương tác với ORM hay DBMS.
  -> https://docs.devexpress.com/eXpressAppFramework/113708/data-manipulation-and-business-logic
- Datasource: sử dụng trong View, Grid, Editor nói chung.
- CriteriaOperator: sử dụng để lọc, chỉ cho XPO select dữ liệu cần thiết, được quyền truy cập.
  -> https://docs.devexpress.com/CoreLibraries/DevExpress.Data.Filtering.CriteriaOperator
- Khái nhiệm ViewController (DetailView, ListView, ReportView, DashboardView): https://docs.devexpress.com/eXpressAppFramework/112611/ui-construction/views?p=netframework
- Khái niệm Theme, Template: https://docs.devexpress.com/eXpressAppFramework/112611/ui-construction/views?p=netframework;

**CRUD**
- Cấu trúc bảng, model thể hiện trong DTTS.Module.BusinessObjects, sau này BusninessObject gọi tắt là BO (trong DevExpress BO là XPO);
  + Dự án sử dụng phương thức Code-First: các file BO lưu trong này, code BO để XAF tạo DB;
  + DB First chỉ dùng khi cần sinh code cho BO;
- Hiển thị tiêu chuẩn:
  + Đặt tên, validate, định dạng chữ/label: sử dụng Attribute (XafDisplayName, Browsable, Conditional Appearance Rule (Attribute & Criterial);
    -> _https://docs.devexpress.com/eXpressAppFramework/113286/conditional-appearance?p=netframework_
  + Sử dụng file Model (DTTS.Web/Model)
- Hiển thị nâng cao, giao diện nâng cao:
  + Sử dụng ViewController: truy suất Editor để điều chỉnh hiển thị;
  + Sử dụng Custom Property Editor để thay thế các Editor mặc định;
  + Sử dụng file .css (áp đặt customstyle);
  + Sử dụng kỹ thuật customize theme/template:
    . How to: Customize an ASP.NET Web Forms Template: https://docs.devexpress.com/eXpressAppFramework/113460/ui-construction/templates/in-webforms/how-to-customize-an-asp-net-template?p=netframework
    . How to: Adjust the Size and Style of Pop-up Dialogs: https://docs.devexpress.com/eXpressAppFramework/113456/ui-construction/templates/in-webforms/how-to-adjust-the-size-and-style-of-pop-up-dialogs-asp-net?p=netframework
- Action Container (nơi chứa các các button): https://docs.devexpress.com/eXpressAppFramework/112610/ui-construction/action-containers?p=netframework

**Không CRUD mà chỉ cần tạo View**
- Để XAF tạo view mà ko sinh ra DB, sử dụng thuộc tính [NonPersistant]
- Trong thư mục Web hoàn toàn có thể tạo các file .aspx như ASP.NET thông thường.

