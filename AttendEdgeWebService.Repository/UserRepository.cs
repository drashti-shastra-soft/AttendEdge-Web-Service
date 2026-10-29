using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class UserRepository : IUserRepository
    {
        public Domain.UserLister GetAll(Domain.UserLister mLister)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var query = from u in context.Users
                                join c in context.Companies on u.CompanyId equals c.Id into joinUC
                                from c in joinUC.DefaultIfEmpty()
                                join role in context.Roles on u.RoleId equals role.Id
                                join at in context.AuditTrails on u.AuditId equals at.Id
                                where at.DeletedBy == null && at.DeletedDate == null
                                select new
                                {
                                    Id = u.Id,
                                    CompanyId = u.CompanyId,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    EmailAddress = u.EmailAddress,
                                    Password = u.Password,
                                    MobileNumber = u.MobileNumber,
                                    RoleId = u.RoleId,
                                    IsActive = u.IsActive,
                                    Role = role.Name,
                                    Company = c.Name,
                                };

                    // Apply server-side filters where possible
                    if (mLister.SearchCriteria != null)
                    {
                        var criteria = mLister.SearchCriteria;

                        if (criteria.FirstName.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.FirstName) && x.FirstName.ToUpper().Contains(criteria.FirstName.ToUpper()));

                        if (criteria.LastName.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.LastName) && x.LastName.ToUpper().Contains(criteria.LastName.ToUpper()));

                        if (criteria.EmailAddress.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.EmailAddress) && x.EmailAddress.ToUpper().Contains(criteria.EmailAddress.ToUpper()));

                        if (criteria.MobileNumber.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.MobileNumber) && x.MobileNumber.ToUpper().Contains(criteria.MobileNumber.ToUpper()));

                        if (criteria.RoleId > 0)
                            query = query.Where(x => x.RoleId == criteria.RoleId);

                        if (criteria.CompanyId > 0)
                            query = query.Where(x => x.CompanyId == criteria.CompanyId);

                        if (criteria.Status.IsNotNullOrEmpty())
                        {
                            if (criteria.Status == "Active")
                                query = query.Where(x => x.IsActive);
                            else if (criteria.Status == "Disabled")
                                query = query.Where(x => !x.IsActive);
                        }
                    }

                    // Switch to in-memory for complex string filters
                    var efUsers = query.AsEnumerable();

                    var pagination = mLister.Pagination ?? new Domain.Pagination();

                    pagination.TotalRecord = Math.Max(efUsers.Count(), 1);

                    if (pagination.Take <= 0)
                    {
                        pagination.Take = pagination.TotalRecord;
                        pagination.Skip = 0;
                    }

                    pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;

                    pagination.TotalPage = pagination.Take > 0 ? (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take) : 1;

                    efUsers = efUsers.Skip(pagination.Skip).Take(pagination.Take);

                    mLister.Pagination = pagination;
                    mLister.List = JsonSerializer.Deserialize<List<Domain.User>>(JsonSerializer.Serialize(efUsers.ToList()));
                }
            }
            catch
            {
                throw;
            }

            return mLister;
        }

        public Domain.User Upsert(Domain.User mUser)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = context.Users.FirstOrDefault(x => x.Id == mUser.Id);
                    if (efUser == null)
                    {
                        var audit = new Persistence.AuditTrail
                        {
                            CreatedBy = mUser.CreatedBy,
                            CreatedDate = DateTime.Now
                        };

                        context.AuditTrails.Add(audit);
                        context.SaveChanges();

                        efUser = new Persistence.User
                        {
                            Password = mUser.Password,
                            AuditId = audit.Id
                        };

                        context.Users.Add(efUser);
                    }
                    else
                    {
                        var audit = context.AuditTrails.Find(efUser.AuditId);
                        if (audit != null)
                        {
                            audit.LastModifiedBy = mUser.LastModifiedBy;
                            audit.LastModifiedDate = DateTime.Now;
                        }
                    }

                    efUser.FirstName = mUser.FirstName;
                    efUser.LastName = mUser.LastName;
                    efUser.EmailAddress = mUser.EmailAddress;
                    efUser.MobileNumber = mUser.MobileNumber;
                    efUser.RoleId = mUser.RoleId;
                    efUser.CompanyId = mUser.CompanyId;
                    efUser.IsActive = mUser.IsActive;

                    context.SaveChanges();

                    mUser.Id = efUser.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mUser;
        }

        public Domain.User Get(int id)
        {
            var mUser = new Domain.User();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = (from user in context.Users
                                  where user.Id == id
                                  select new
                                  {
                                      Id = user.Id,
                                      FirstName = user.FirstName,
                                      LastName = user.LastName,
                                      EmailAddress = user.EmailAddress,
                                      MobileNumber = user.MobileNumber,
                                      RoleId = user.RoleId,
                                      IsActive = user.IsActive,
                                      CompanyId = user.CompanyId,
                                      AuditId = user.AuditId
                                  }).FirstOrDefault();

                    if (efUser != null)
                    {
                        mUser = JsonSerializer.Deserialize<Domain.User>(JsonSerializer.Serialize(efUser));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mUser;
        }

        public void Delete(int id, int loginUserId)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = context.Users.FirstOrDefault(x => x.Id == id);
                    if (efUser != null)
                    {
                        var audit = context.AuditTrails.Find(efUser.AuditId);
                        if (audit != null)
                        {
                            audit.DeletedBy = loginUserId;
                            audit.DeletedDate = DateTime.Now;
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Domain.User GetBy(string emailAddress)
        {
            var mUser = new Domain.User();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = (from user in context.Users
                                  where user.EmailAddress.Trim().ToUpper() == emailAddress.Trim().ToUpper()
                                  select new
                                  {
                                      Id = user.Id,
                                      FirstName = user.FirstName,
                                      LastName = user.LastName,
                                      EmailAddress = user.EmailAddress,
                                      Password = user.Password,
                                      MobileNumber = user.MobileNumber,
                                      RoleId = user.RoleId,
                                      IsActive = user.IsActive,
                                      CompanyId = user.CompanyId
                                  }).AsEnumerable();

                    mUser = JsonSerializer.Deserialize<Domain.User>(JsonSerializer.Serialize(efUser.FirstOrDefault()));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mUser;
        }

        public List<Domain.User> GetActiveUsers(int companyId)
        {
            var userList = new List<Domain.User>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUsers = from user in context.Users
                                  where user.CompanyId == companyId
                                  select new
                                  {
                                      Id = user.Id,
                                      FirstName = user.FirstName,
                                      LastName = user.LastName,
                                      RoleId = user.RoleId,
                                      EmailAddress = user.EmailAddress,
                                  };

                    var json = JsonSerializer.Serialize(efUsers.ToList());
                    userList = JsonSerializer.Deserialize<List<Domain.User>>(json);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return userList;
        }

        public string GetPasswordBy(int id)
        {
            string password = string.Empty;
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    password = context.Users.Where(x => x.Id == id).Select(s => s.Password).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return password;
        }

        public string SetPasswordBy(int id, string password)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = context.Users.Where(x => x.Id == id).FirstOrDefault();
                    if (efUser != null && efUser.Id > default(int))
                    {
                        efUser.Password = password;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return password;
        }

        //public List<string> GetNotificationEmails(int id, int companyId)
        //{
        //    try
        //    {
        //        using (var context = new Persistence.AttendEdgeDBEntities())
        //        {

        //            var requesterRole = context.Users
        //                                       .Where(u => u.Id == id && u.CompanyId == companyId)
        //                                       .Select(u => u.RoleId) // assuming you have a Role column
        //                                       .FirstOrDefault();

        //            if (requesterRole <= default(int))
        //                return new List<string>();

        //            // Step 2: Get recipients based on role
        //            if (requesterRole == (int)Enums.Role.Employee)
        //            {
        //                return context.Users.Where(u => (u.RoleId == (int)Enums.Role.Admin || u.RoleId == (int)Enums.Role.Super_Admin) && u.CompanyId == companyId)
        //                              .Select(u => u.EmailAddress)
        //                              .ToList();
        //            }
        //            else if (requesterRole == (int)Enums.Role.Admin)
        //            {
        //                return context.Users.Where(u => u.RoleId == (int)Enums.Role.Super_Admin && u.CompanyId == companyId)
        //                                    .Select(u => u.EmailAddress)
        //                                    .ToList();
        //            }
        //            else
        //            {
        //                return new List<string>();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public List<string> GetNotificationEmails(int loginUserId, int companyId)
        //{
        //    using (var context = new Persistence.AttendEdgeDBEntities())
        //    {
        //        // Get role of logged-in user
        //        var loginUserRole = context.Users
        //            .Where(u => u.Id == loginUserId && u.CompanyId == companyId)
        //            .Select(u => u.RoleId)
        //            .FirstOrDefault();

        //        if (loginUserRole <= 0)
        //            return new List<string>();

        //        // 1️⃣ Employee creates leave → notify Admin + SuperAdmin
        //        if (loginUserRole == (int)Enums.Role.Admin || loginUserRole == (int)Enums.Role.Super_Admin)
        //        {
        //            return context.Users
        //                .Where(u =>
        //                       (u.RoleId == (int)Enums.Role.Admin ||
        //                        u.RoleId == (int)Enums.Role.Super_Admin)
        //                        && u.CompanyId == companyId)
        //                .Select(u => u.EmailAddress)
        //                .ToList();
        //        }

        //        // 2️⃣ Admin or SuperAdmin approves leave → notify Employee
        //        if (loginUserRole == (int)Enums.Role.Employee)
        //        {
        //            // Get latest leave request created by employee
        //            var employeeUserId = context.LeaveRequests
        //                .OrderByDescending(l => l.Id)
        //                .Select(l => l.UserId)
        //                .FirstOrDefault();

        //            return context.Users
        //                .Where(u => u.Id == employeeUserId
        //                            && u.CompanyId == companyId
        //                            && u.RoleId == (int)Enums.Role.Employee)
        //                .Select(u => u.EmailAddress)
        //                .ToList();
        //        }
        //        return new List<string>();
        //    }
        //}

        public List<string> GetNotificationEmails(int loginUserId, int companyId)
        {
            using (var context = new Persistence.AttendEdgeDBEntities())
            {
                // Get login user role
                var loginUser = context.Users.Where(u => u.Id == loginUserId && u.CompanyId == companyId).Select(u => new { u.RoleId, u.EmailAddress })
                    .FirstOrDefault();

                if (loginUser == null)
                    return new List<string>();

                // ✅ 1. Employee → Admin + SuperAdmin
                if (loginUser.RoleId == (int)Enums.Role.Employee)
                {
                    return context.Users
                        .Where(u => (u.RoleId == (int)Enums.Role.Admin || u.RoleId == (int)Enums.Role.Super_Admin) && u.CompanyId == companyId).Select(u => u.EmailAddress).ToList();
                }

                // ✅ 2. Admin / SuperAdmin → Employee (LOGIN USER NE J MAIL)
                if (loginUser.RoleId == (int)Enums.Role.Admin || loginUser.RoleId == (int)Enums.Role.Super_Admin)
                {
                    return new List<string> { loginUser.EmailAddress };
                }

                return new List<string>();
            }
        }


        public bool CheckEmailAddressExist(Domain.User mUser)
        {
            bool isExist = false;
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    isExist = context.Users.Where(x => x.Id != mUser.Id && x.EmailAddress.Trim().ToUpper() == mUser.EmailAddress.Trim().ToUpper()).Count() > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return isExist;
        }

    }
}
