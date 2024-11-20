using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DTTS.Module.BusinessObjects;

namespace DTTS.Module.DatabaseUpdate {
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
#if !RELEASE
            #region EmployeeRole - Khoi tao Role/User cho EmployeeRole
            ///Tài khoản Default (Default)
            //EmployeeRole defaultEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
            //    new BinaryOperator("Name", "default"));
            //if (defaultEmployeeRole == null)
            //{
            //    defaultEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
            //    defaultEmployeeRole.Name = "Default";
            //    defaultEmployeeRole.IsAdministrative = false;
            //    defaultEmployeeRole.Save();
            //}

            /////Tài khoản Thủ quỷ (Cashier)
            //EmployeeRole cashierEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
            //    new BinaryOperator("Name", "Cashier"));
            //if (cashierEmployeeRole == null)
            //{
            //    cashierEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
            //    cashierEmployeeRole.Name = "Cashier";
            //    cashierEmployeeRole.IsAdministrative = false;
            //    cashierEmployeeRole.Save();
            //}
            ///Tài khoản Giám đốc (Director)
            EmployeeRole directorEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
                new BinaryOperator("Name", "Director"));
            if (directorEmployeeRole == null)
            {
                directorEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
                directorEmployeeRole.Name = "Director";
                directorEmployeeRole.IsAdministrative = false;
                directorEmployeeRole.Save();
            }

            ///Tài khoản Nhân viên (Staff)
            EmployeeRole staffEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
                new BinaryOperator("Name", "Staff"));
            if (staffEmployeeRole == null)
            {
                staffEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
                staffEmployeeRole.Name = "Staff";
                staffEmployeeRole.IsAdministrative = false;
                staffEmployeeRole.Save();
            }

            ///Tài khoản Văn thư (Officer)
            //EmployeeRole officerEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
            //    new BinaryOperator("Name", "Officer"));
            //if (officerEmployeeRole == null)
            //{
            //    officerEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
            //    officerEmployeeRole.Name = "Officer";
            //    officerEmployeeRole.IsAdministrative = false;
            //    officerEmployeeRole.Save();
            //}


            //EmployeeRole TaskerRole = ObjectSpace.FindObject<EmployeeRole>(
            //    new BinaryOperator("Name", "Tasker"));
            //if (TaskerRole == null)
            //{
            //    TaskerRole = ObjectSpace.CreateObject<EmployeeRole>();
            //    TaskerRole.Name = "Tasker";
            //    TaskerRole.IsAdministrative = false;
            //    TaskerRole.Save();
            //}

            ///Administrator
            EmployeeRole adminEmployeeRole = ObjectSpace.FindObject<EmployeeRole>(
                new BinaryOperator("Name", SecurityStrategy.AdministratorRoleName));
            if (adminEmployeeRole == null)
            {
                adminEmployeeRole = ObjectSpace.CreateObject<EmployeeRole>();
                adminEmployeeRole.Name = SecurityStrategy.AdministratorRoleName;
                adminEmployeeRole.IsAdministrative = true;
                adminEmployeeRole.Save();
            }

            Employee adminEmployee = ObjectSpace.FindObject<Employee>(
                new BinaryOperator("UserName", "admin"));
            if (adminEmployee == null)
            {
                adminEmployee = ObjectSpace.CreateObject<Employee>();
                adminEmployee.UserName = "admin";
                adminEmployee.SetPassword("admin");
                adminEmployee.EmployeeRoles.Add(adminEmployeeRole);
            }

            ObjectSpace.CommitChanges(); //This line persists created object(s).
            #endregion

#endif
        }
        public override void UpdateDatabaseBeforeUpdateSchema() {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }
        
    }
}
