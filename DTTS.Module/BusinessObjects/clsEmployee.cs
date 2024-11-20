using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Persistent.Base.General;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;
using System.Collections.Generic;
using DevExpress.ExpressApp.Utils;
using System.Security.Cryptography;
using System.Text;
using DevExpress.ExpressApp.DC;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.ConditionalAppearance;

namespace DTTS.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Nhân viên")]
    [ImageName("BO_Employee")]
    [Appearance("RedPriceObject", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "Position.PositionLevel=0", Context = "ListView", BackColor = "Red",
        FontColor = "Maroon", Priority = 2)]
    public partial class Employee : HRCategory, ISecurityUser, IAuthenticationStandardUser, ISecurityUserWithRoles, IOperationPermissionProvider  // BaseObject,
    {
        #region ISecurityUser Members
        private bool isActive = true;
        public bool IsActive
        {
            get { return isActive; }
            set { SetPropertyValue("IsActive", ref isActive, value); }
        }
        private string userName = String.Empty;
        [RuleRequiredField("EmployeeUserNameRequired", DefaultContexts.Save)]
        [RuleUniqueValue("EmployeeUserNameIsUnique", DefaultContexts.Save,
            "The login with the entered user name was already registered within the system.")]
        public string UserName
        {
            get { return userName; }
            set { SetPropertyValue("UserName", ref userName, value); }
        }
        #endregion

        #region IAuthenticationStandardUser Members
        private bool changePasswordOnFirstLogon;
        public bool ChangePasswordOnFirstLogon
        {
            get { return changePasswordOnFirstLogon; }
            set
            {
                SetPropertyValue("ChangePasswordOnFirstLogon", ref changePasswordOnFirstLogon, value);
            }
        }

        private bool IsMD5(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return false;
            }

            return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
        }

        public string EncodePassword(string originalPassword)
        {
            if (originalPassword == null)
                return String.Empty;
            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = ASCIIEncoding.Default.GetBytes(originalPassword);
            encodedBytes = md5.ComputeHash(originalBytes);

            //Convert encoded bytes back to a 'readable' string
            string retStr = BitConverter.ToString(encodedBytes);
            retStr = retStr.Replace("-", "");
            return retStr.ToLower();
        }

        private string storedPassword;
        [Browsable(false), Size(SizeAttribute.Unlimited), Persistent, SecurityBrowsable]
        protected string StoredPassword
        {
            get { return storedPassword; }
            set { storedPassword = value; }
        }
        public bool ComparePassword(string password)
        {
            bool retCompare = false;
            if (String.IsNullOrEmpty(password))
                retCompare = String.IsNullOrEmpty(storedPassword);
            else
            {
                retCompare = EncodePassword(password).CompareTo(storedPassword) == 0;
                //2024: bổ sung check để so sánh trực tiếp với chuỗi password MD5 truyền vào
                if(!retCompare && IsMD5(password))
                {
                    retCompare = password.CompareTo(storedPassword) == 0; //password là chuỗi MD5
                }
                //--------------------------------------------------------------------------
            }

            return retCompare;
            //return SecurityUserBase.ComparePassword(this.storedPassword, password);
        }
        public void SetPassword(string password)
        {
            //this.storedPassword = new PasswordCryptographer().GenerateSaltedPassword(password);
            this.storedPassword = EncodePassword(password);
            OnChanged("StoredPassword");
        }

        #endregion

        #region ISecurityUserWithRoles Members
        IList<ISecurityRole> ISecurityUserWithRoles.Roles
        {
            get
            {
                IList<ISecurityRole> result = new List<ISecurityRole>();
                foreach (EmployeeRole role in EmployeeRoles)
                {
                    result.Add(role);
                }
                return result;
            }
        }
        #endregion

        #region Employees-EmployeeRoles - Tham chieu giua Roles, bo sung
        [Association("Employees-EmployeeRoles")]
        [RuleRequiredField("EmployeeRoleIsRequired", DefaultContexts.Save,
            TargetCriteria = "IsActive",
            CustomMessageTemplate = "An active employee must have at least one role assigned")]
        public XPCollection<EmployeeRole> EmployeeRoles
        {
            get
            {
                return GetCollection<EmployeeRole>("EmployeeRoles");
            }
        }
        #endregion

        #region IOperationPermissionProvider Members
        IEnumerable<IOperationPermission> IOperationPermissionProvider.GetPermissions()
        {
            return new IOperationPermission[0];
        }
        IEnumerable<IOperationPermissionProvider> IOperationPermissionProvider.GetChildren()
        {
            return new EnumerableConverter<IOperationPermissionProvider, EmployeeRole>(EmployeeRoles);
        }
        #endregion

        #region HRCategory
        [XafDisplayName("Cấp trên")]
        protected override ITreeNode Parent
        {
            get
            {
                return Department;
            }
        }

        protected override IBindingList Children
        {
            get
            {
                return new BindingList<object>();
            }
        }

        [XafDisplayName("Tên đơn vị")]
        public override string Name
        {
            get
            {
                return Title;
            }
        }

        [XafDisplayName("Phân loại")]
        public override string HRType
        {
            get
            {
                return "Employee";
            }
        }

        #endregion

        public Employee(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            if (Session.IsNewObject(this))
            {
                this.isActive = true;
                this.DateIn = DateTime.Now;

            }
        }

        [Association("Task-AssignedEmp")]
        [XafDisplayName("Danh sách công việc được giao")]
        public XPCollection<Task> Tasks
        {
            get { return GetCollection<Task>(nameof(Tasks)); }
        }

        #region OnSaving: Thiết lập mật khẩu mặc định
        protected override void OnSaving()
        {
            base.OnSaving();
            if (Session.IsNewObject(this))
            {
                //Thiết lập mật khẩu mặc định = username
                if (string.IsNullOrEmpty(this.storedPassword))
                {
                    this.SetPassword(this.userName);
                }
            }
        }
        #endregion  

        private string fullname;
        [XafDisplayName("Họ tên NV")]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Fullname
        {
            get { return fullname; }
            set { SetPropertyValue("Fullname", ref fullname, value); }
        }

        public string Title
        {
            get
            {
                string title = string.Format("{0} ({1})", this.Fullname, this.userName);

                return title;
            }
        }

        #region prop MiddleName - Tách lấy Họ lót, tên,...
        [XafDisplayName("Họ lót")]
        public string MiddleName
        {
            get {
                if (this.fullname == null || this.fullname.Length <= 0)
                    return "";
                var names = this.fullname.Split(' ');
                if (names.Length <= 1) return "";

                return names[1];
            }
        }

        [XafDisplayName("Tên")]
        public string FirstName
        {
            get
            {
                if (this.fullname == null || this.fullname.Length <= 0)
                    return "";
                var names = this.fullname.Split(' ');
                if (names.Length <= 1) return "";

                return names[names.Length-1];
            }
        }

        [XafDisplayName("Họ")]
        public string LastName
        {
            get
            {
                if (this.fullname == null || this.fullname.Length <= 0)
                    return "";
                var names = this.fullname.Split(' ');
                if (names.Length <= 0) return "";

                return names[0];
            }
        }
        #endregion

        private string email;
        [XafDisplayName("Email")]
        [RuleRequiredField(DefaultContexts.Save)]
        public string Email
        {
            get { return email; }
            set { SetPropertyValue("Email", ref email, value); }
        }

        private string addressEmp;
        [XafDisplayName("Địa chỉ")]
        public string AddressEmp
        {
            get { return addressEmp; }
            set { SetPropertyValue(nameof(AddressEmp), ref addressEmp, value); }
        }

        private string phoneEmp;
        [XafDisplayName("Số điện thoại NV")]
        [RuleRegularExpression(DefaultContexts.Save, @"^\d{1,10}$", CustomMessageTemplate = "Số điện thoại chỉ được chứa số và tối đa 10 chữ số.")]
        [RuleRequiredField(DefaultContexts.Save, CustomMessageTemplate = "Số điện thoại là bắt buộc.")]
        public string PhoneEmp
        {
            get { return phoneEmp; }
            set { SetPropertyValue(nameof(PhoneEmp), ref phoneEmp, value); }
        }

        private Employee manager;
        //[DataSourceProperty("Department.Employees", DataSourcePropertyIsNullMode.SelectAll)]
        [DataSourceCriteria("Position.PositionLevel >= 100")]
        [XafDisplayName("Quản lý trực tiếp")]
        public Employee Manager
        {
            get { return manager; }
            set { SetPropertyValue("Manager", ref manager, value); }
        }

        private string notes;
        [Size(4096)]
        [XafDisplayName("Ghi chú")]
        public string Notes
        {
            get { return notes; }
            set { SetPropertyValue("Notes", ref notes, value); }
        }

        #region OLD: 1-n, mỗi người thuộc 1 phòng ban
        //OLD: 1-n, mỗi người thuộc 1 phòng ban
        private Department department;
        [RuleRequiredField(DefaultContexts.Save)]
        [Association("Department-Employees", typeof(Department))]
        [XafDisplayName("Phòng ban/bộ phận")]
        public Department Department
        {
            get { return department; }
            set { SetPropertyValue("Department", ref department, value); }
        }
        //END OLD: 1-n, mỗi người thuộc 1 phòng ban */
        #endregion

        private Position position;
        [XafDisplayName("Chức vụ")]
        public Position Position
        {
            get { return position; }
            set { SetPropertyValue("Position", ref position, value); }
        }

        private DateTime datein;
        private DateTime dateout;

        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Ngày vào làm")]
        public DateTime DateIn
        {
            get { return datein; }
            set { SetPropertyValue("DateIn", ref datein, value); }
        }

        [XafDisplayName("Ngày thôi việc")]
        public DateTime DateOut
        {
            get { return dateout; }
            set { SetPropertyValue("DateOut", ref dateout, value); }
        }

        //// Danh sách công việc được giao
        //[Association("Employee-AssignedTasks")]
        //[XafDisplayName("Danh sách công việc được giao")]
        //public XPCollection<AssignTo> AssignedTasks
        //{
        //    get { return GetCollection<AssignTo>(nameof(AssignedTasks)); }
        //}
    }

    #region Department - Phong ban/Bo phan cua Employee
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Đơn vị trực thuộc")]
    [ImageName("BO_Department")]
    public class Department : HRCategory
    {
        private string title;
        private string office;
        private Organization organization;

        #region HRCategory
        protected override ITreeNode Parent
        {
            get
            {
                if (managerDepartment != null)
                    return managerDepartment;
                else
                    return organization;
            }
        }
        protected override IBindingList Children
        {
            get
            {
                if (SubordinatesDepartments != null && SubordinatesDepartments.Count > 0)
                    return SubordinatesDepartments;
                else
                    return Employees;
            }
        }

        public override string Name
        {
            get
            {
                return title;
            }
        }

        public override string HRType
        {
            get
            {
                string hType = "Department";
                if (this.GetType() == typeof(Employee))
                {
                    hType = "Employee";
                }
                return hType;
            }
        }

        [Association("Organization-Departments", typeof(Organization)), XafDisplayName("Tổ chức"), ImmediatePostData()]
        public Organization Organization
        {
            get
            {
                return organization;
            }
            set
            {
                SetPropertyValue("Organization", ref organization, value);
            }
        }
        #endregion

        public Department(Session session) : base(session)
        {
            
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            //Fix departmentManagerOid - list
            if (string.IsNullOrEmpty(this.ListManagerDepartmentOid))
            {
                /*
                this.updateListManagerDepartmentOid();

                this.Save();
                Session.CommitTransaction();
                */
            }
        }

        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Tên phòng ban")]
        public string Title
        {
            get { return title; }
            set { SetPropertyValue("Title", ref title, value); }
        }

        [XafDisplayName("Văn phòng làm việc")]
        public string Office
        {
            get { return office; }
            set { SetPropertyValue("Office", ref office, value); }
        }

        [Association("Department-Employees", typeof(Employee))]
        [XafDisplayName("Danh sách nhân viên")]
        public XPCollection Employees
        {
            get { return GetCollection("Employees"); }
        }

        //[NonPersistent, XafDisplayName("Văn bản thuộc phòng/đơn vị")]
        //[Association("DocumentDepartments", UseAssociationNameAsIntermediateTableName = true)]
        //public IList<Document> Documents
        //{
        //    get { return GetList<Document>("Documents"); }
        //}

        private Department managerDepartment;
        [Association("ManagerSubordinates-Department")]
        [XafDisplayName("Đơn vị cấp trên"), DataSourceCriteria("TopOrganization = '@This.Organization'"), ImmediatePostData()]
        public Department ManagerDepartment
        {
            get { return managerDepartment; }
            set { SetPropertyValue("ManagerDepartment", ref managerDepartment, value); }
        }

        [Association("ManagerSubordinates-Department")]
        [XafDisplayName("Đơn vị trực thuộc")]
        public XPCollection<Department> SubordinatesDepartments
        {
            get { return GetCollection<Department>("SubordinatesDepartments"); }
        }

        #region checkIsManager - Đệ quy, Kiểm tra 1 Department có phải là department cấp trên của Department này hay ko
        public bool checkIsManager(Guid departmentOid)
        {
            //1. Nếu department này ko có Cấp trên nào => return false cho nhanh, nó là cao nhất
            if(this.ManagerDepartment == null)
            {
                return false;
            }

            bool isMaster = false;
            Department checkDep = Session.FindObject<Department>(new BinaryOperator("Oid", departmentOid));
            if(checkDep == null)
            {
                return false; //Không tìm thấy
            }
            else if(checkDep == this.ManagerDepartment)
            {
                return true;
            }
            else if(this.ManagerDepartment != null )
            {
                isMaster = this.ManagerDepartment.checkIsManager(departmentOid);
            }

            return isMaster;
        }
        #endregion

        #region ListManagerDepartmentOid - string dùng trong Criteria để tìm các phòng cấp trên cho nhanh
        [Browsable(false)]
        public string ListManagerDepartmentOid
        {
            get; set;
        }
        
        public void updateListManagerDepartmentOid()
        {
            this.ListManagerDepartmentOid = getListManagerDepartmentOid();
        }

        //Đệ quy
        private string getListManagerDepartmentOid()
        {
            //string isUpdated = false;
            if(this.managerDepartment == null)
            {
                return "";
            }

            return string.Format("{0},{1}", this.managerDepartment.Oid.ToString(), this.managerDepartment.getListManagerDepartmentOid());
        }
        #endregion

        #region getLeader() - Lấy lãnh đạo phòng cao nhất
        public Employee getLeader()
        {
            Employee leader = null;
            if (this.Employees == null || this.Employees.Count <= 0)
                return null;
            try { 
                IEnumerator<EmployeeOidHolder> leaders = this.Session.GetObjectsFromQuery<EmployeeOidHolder>(
    @"SELECT TOP 1 emp.[Oid]
      FROM [dbo].[Employee] emp
      INNER JOIN [dbo].[Position] p ON emp.Position = p.Oid
      WHERE [Department] = @p0
      order by p.PositionLevel desc;", new object[] { this.Oid.ToString() }).GetEnumerator();
                if (leaders.MoveNext())
                {
                    leader = this.Session.GetObjectByKey<Employee>(leaders.Current.Oid);
                }
            }
            catch
            {
                return null;
            }
            //foreach (Employee emp in Employees)
            //{
            //    if (leader == null || (emp.Position != null && leader.Position != null && emp.Position.PositionLevel > leader.Position.PositionLevel))
            //    {
            //        leader = emp;
            //    }
            //}
            return leader;
        }

        #endregion

        #region getOrganization - Đệ quy lấy Tổ chức của Đơn vị trực thuộc
        public Organization TopOrganization
        {
            get { return this.getOrganization(); }
        }

        public Organization getOrganization()
        {
            Department managerDepartment = this.managerDepartment;

            if (managerDepartment == null)
                return this.Organization;

            Organization org = managerDepartment.Organization;
            org = this.ManagerDepartment.getOrganization();

            return org;
        }
        #endregion

        #region OnSaving - Fix & updated cho Department
        protected override void OnSaving()
        {
            //1.FixDuplicated Parent nodes - Bỏ Đơn vị tổ chức nếu có chọn Đơn vị cấp trên
            if (this.managerDepartment != null)
                this.Organization = null;
            //2. Luôn luôn cập nhật danh sách Oid các Department cấp trên (kiểu String)
            this.updateListManagerDepartmentOid();
            base.OnSaving();
        }
        #endregion
    }
    #endregion

    #region Task - Công việc
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Công việc")]
    [ImageName("BO_Task")]
    public class Task : BaseObject
    {
        public Task(Session session) : base(session) { }

        private string title;
        private string description;
        private DateTime dueDate;

        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Tên công việc")]
        public string Title
        {
            get { return title; }
            set { SetPropertyValue(nameof(Title), ref title, value); }
        }

        [XafDisplayName("Mô tả công việc")]
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get { return description; }
            set { SetPropertyValue(nameof(Description), ref description, value); }
        }

        [XafDisplayName("Hạn chót")]
        [ModelDefault("EditMask", "d")] // Chỉ định định dạng ngày
        [ModelDefault("DisplayFormat", "{0:MM/dd/yyyy}")] // Định dạng hiển thị
        public DateTime DueDate
        {
            get { return dueDate; }
            set { SetPropertyValue(nameof(DueDate), ref dueDate, value); }
        }

        // Liên kết với AssignTo
        [Association("Task-AssignedEmp")]
        [XafDisplayName("Danh sách nhân viên được giao")]
        public XPCollection<Employee> AssignedEmp
        {
            get { return GetCollection<Employee>(nameof(AssignedEmp)); }
        }
    }
    #endregion

    #region AssignTo - Giao việc
    /*
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Giao việc")]
    [ImageName("BO_AssignTo")]
    public class AssignTo : BaseObject
    {
        public AssignTo(Session session) : base(session) { }

        private Employee employee;
        private Task task;

        [XafDisplayName("Nhân viên")]
        [Association("Employee-AssignedTasks")]
        public Employee Employee
        {
            get { return employee; }
            set { SetPropertyValue(nameof(Employee), ref employee, value); }
        }

        [XafDisplayName("Công việc")]
        [Association("Task-AssignedTo")]
        public Task Task
        {
            get { return task; }
            set { SetPropertyValue(nameof(Task), ref task, value); }
        }
    }
    */
    #endregion

    #region Organization - Cơ quan/tổ chức
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Cơ quan/tổ chức")]
    [ImageName("BO_Department")]
    public class Organization : HRCategory
    {
        private string title;
        private string office;

        protected override ITreeNode Parent
        {
            get
            {
                return null;
            }
        }
        protected override IBindingList Children
        {
            get
            {
                return Departments;
            }
        }

        public override string HRType
        {
            get
            {
                return "Organization";
            }
        }

        private string organizationName;
        [ XafDisplayName("Tên tổ chức")]
        public override string Name
        {
            get
            {
                return organizationName;
            }
        }
        [RuleRequiredField(DefaultContexts.Save), XafDisplayName("Tên tổ chức")]
        public string OrganizationName
        {
            get
            {
                return organizationName;
            }

            set { SetPropertyValue("OrganizationName", ref organizationName, value); }
        }

        [Association("Organization-Departments")] //, DevExpress.Xpo.Aggregated
        [XafDisplayName("Đơn vị trực thuộc")]
        public XPCollection<Department> Departments
        {
            get
            {
                return GetCollection<Department>("Departments");
            }
        }

        public Organization(Session session) : base(session) { }

        [XafDisplayName("Địa điểm")]
        public string Office
        {
            get { return office; }
            set { SetPropertyValue("Office", ref office, value); }
        }

        
    }
    #endregion

    #region HRCategory - Định nghĩa phân cấp của Tổ chức, doanh nghiệp (TreeList)
    [Browsable(false)]
    [NavigationItem(false)]
    [XafDisplayName("Cây đơn vị")]
    public abstract class HRCategory : BaseObject, ITreeNode
    {
        protected abstract ITreeNode Parent
        {
            get;
        }
        protected abstract IBindingList Children
        {
            get;
        }
        public HRCategory(Session session) : base(session) { }

        [XafDisplayName("Tên đơn vị")]
        public abstract string Name
        {
            get;
        }
        [XafDisplayName("Phân loại")]

        public abstract string HRType
        {
            get;
        }

        #region ITreeNode
        IBindingList ITreeNode.Children
        {
            get
            {
                return Children;
            }
        }
        string ITreeNode.Name
        {
            get
            {
                return Name;
            }
        }
        ITreeNode ITreeNode.Parent
        {
            get
            {
                return Parent;
            }
        }
        #endregion
    }
    #endregion

    #region Position - Vi tri/chuc vu cua Employee
    [DefaultClassOptions]
    [XafDefaultProperty("Title")]
    [XafDisplayName("Chức vụ")]
    [ImageName("BO_Position")]
    public class Position : BaseObject
    {
        public Position(Session session) : base(session) { }
        private string title;
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Tên chức vụ")]
        public string Title
        {
            get { return title; }
            set { SetPropertyValue("Title", ref title, value); }
        }

        private int positionLevel; //PositionLevels
        [XafDisplayName("Thứ tự sắp xếp")]
        public int PositionLevel
        {
            get { return positionLevel; }
            set { SetPropertyValue("PositionLevel", ref positionLevel, value); }
        }
    }
    #endregion

    #region EmployeeRole - Merge Employee + Role cua he thong Security cua XAF
    [ImageName("BO_Role")]
    [XafDisplayName("Quyền hệ thống")]
    public class EmployeeRole : SecuritySystemRoleBase
    {
        public EmployeeRole(Session session)
            : base(session)
        {
        }
        [Association("Employees-EmployeeRoles")]
        public XPCollection<Employee> Employees
        {
            get
            {
                return GetCollection<Employee>("Employees");
            }
        }
    }
    #endregion

    #region PositionLevels - Cấp độ của nhân viên
    public enum PositionLevels
    {
        [DevExpress.Xpo.DisplayName("Apprentice")] // Học việc
        apprentice = 0,
        [DevExpress.Xpo.DisplayName("Probationary")] //Thử việc
        probationary = 1,
        [DevExpress.Xpo.DisplayName("Worker")] //Công nhân
        worker  =2,
        [DevExpress.Xpo.DisplayName("Engineers")]
        engineers=3,
        [DevExpress.Xpo.DisplayName("Manager")]
        manager = 4,
        [DevExpress.Xpo.DisplayName("Director")]
        director = 5,
        [DevExpress.Xpo.DisplayName("Special")]
        special=6
    }
    #endregion


    #region EmployeeOidHolder
    [NonPersistent]
    internal class EmployeeOidHolder
    {
        public Guid Oid
        {
            get;set;
        }
    }
    #endregion
}