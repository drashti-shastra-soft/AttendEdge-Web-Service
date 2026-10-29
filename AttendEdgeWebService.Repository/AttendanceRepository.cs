using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class AttendanceRepository : IAttendanceRepository
    {
        public Domain.AttendanceLister GetAll(Domain.AttendanceLister mLister)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var query = from attend in context.Attendances
                                join user in context.Users on attend.UserId equals user.Id
                                join c in context.Companies on attend.CompanyId equals c.Id into joinUC
                                from c in joinUC.DefaultIfEmpty()
                                join at in context.AuditTrails on attend.AuditId equals at.Id
                                where at.DeletedBy == null && at.DeletedDate == null
                                orderby attend.Id descending
                                select new
                                {
                                    Id = attend.Id,
                                    CompanyId = attend.CompanyId,
                                    UserId = attend.UserId,
                                    CheckInTime = attend.CheckInTime,
                                    CheckOutTime = attend.CheckOutTime,
                                    BreakOutTime = attend.BreakOutTime,
                                    BreakInTime = attend.BreakInTime,
                                    LatitudeCheckIn = attend.LatitudeCheckIn,
                                    LatitudeCheckOut = attend.LatitudeCheckOut,
                                    LongitudeCheckIn = attend.LongitudeCheckIn,
                                    LongitudeCheckOut = attend.LongitudeCheckOut,
                                    LatitudeBreakIn = attend.LatitudeBreakIn,
                                    LatitudeBreakOut = attend.LatitudeBreakOut,
                                    LongitudeBreakIn = attend.LongitudeBreakIn,
                                    LongitudeBreakOut = attend.LongitudeBreakOut,
                                    CheckInImagePath = attend.CheckInImagePath,
                                    CheckOutImagePath = attend.CheckOutImagePath,
                                    BreakInImagePath = attend.BreakInImagePath,
                                    BreakOutImagePath = attend.BreakOutImagePath,
                                    LocationDescription = attend.LocationDescription,
                                    DeviceInfo = attend.DeviceInfo,
                                    IPAddress = attend.IPAddress,
                                    Company = c.Name,
                                    Username = user.FirstName + " " + user.LastName
                                };

                    // Optional filtering
                    if (mLister.SearchCriteria != null)
                    {
                        var criteria = mLister.SearchCriteria;

                        if (criteria.Company.IsNotNullOrEmpty())
                            query = query.Where(x => x.Company != null && x.Company.ToUpper() == criteria.Company.ToUpper());

                        if (criteria.Username.IsNotNullOrEmpty())
                            query = query.Where(x => x.Username != null && x.Username.ToUpper() == criteria.Username.ToUpper());

                        if (criteria.CompanyId > 0)
                            query = query.Where(x => x.CompanyId == criteria.CompanyId);

                        if (criteria.DeviceInfo.IsNotNullOrEmpty())
                            query = query.Where(x => x.DeviceInfo != null &&
                                x.DeviceInfo.ToUpper() == criteria.DeviceInfo.ToUpper());

                        if (criteria.IPAddress.IsNotNullOrEmpty())
                            query = query.Where(x => x.IPAddress != null &&
                                x.IPAddress.ToUpper() == criteria.IPAddress.ToUpper());
                    }


                    var efAttendance = query.AsEnumerable();

                    // Pagination
                    var pagination = mLister.Pagination ?? new Domain.Pagination();
                    pagination.TotalRecord = efAttendance.Count();

                    if (pagination.Take <= 0)
                    {
                        pagination.Take = pagination.TotalRecord;
                        pagination.Skip = 0;
                    }

                    pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;
                    pagination.TotalPage = pagination.Take > 0
                        ? (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take)
                        : 1;

                    efAttendance = efAttendance.Skip(pagination.Skip).Take(pagination.Take);

                    mLister.Pagination = pagination;
                    mLister.List = JsonSerializer.Deserialize<List<Domain.Attendance>>(JsonSerializer.Serialize(efAttendance.ToList()));
                }
            }
            catch(Exception)
            {
                throw;
            }

            return mLister;
        }

        public Domain.Attendance Upsert(Domain.Attendance mAttendance)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efAttendance = context.Attendances
                        .FirstOrDefault(x => x.Id == mAttendance.Id);

                    if (efAttendance == null)
                    {
                        var audit = new Persistence.AuditTrail
                        {
                            CreatedBy = mAttendance.CreatedBy,
                            CreatedDate = DateTime.Now
                        };

                        context.AuditTrails.Add(audit);
                        context.SaveChanges();

                        efAttendance = new Persistence.Attendance
                        {
                            AuditId = audit.Id
                        };

                        context.Attendances.Add(efAttendance);
                    }
                    else
                    {
                        var audit = context.AuditTrails.Find(efAttendance.AuditId);

                        if (audit != null)
                        {
                            audit.LastModifiedBy = mAttendance.LastModifiedBy;
                            audit.LastModifiedDate = DateTime.Now;
                        }
                    }

                    efAttendance.CompanyId = mAttendance.CompanyId;
                    efAttendance.UserId = mAttendance.UserId;
                    efAttendance.CheckInTime = mAttendance.CheckInTime;
                    efAttendance.CheckOutTime = mAttendance.CheckOutTime;
                    efAttendance.BreakOutTime = mAttendance.BreakOutTime;
                    efAttendance.BreakInTime = mAttendance.BreakInTime;
                    efAttendance.LatitudeCheckIn = mAttendance.LatitudeCheckIn;
                    efAttendance.LatitudeCheckOut = mAttendance.LatitudeCheckOut;
                    efAttendance.LongitudeCheckIn = mAttendance.LongitudeCheckIn;
                    efAttendance.LongitudeCheckOut = mAttendance.LongitudeCheckOut;
                    efAttendance.LatitudeBreakIn = mAttendance.LatitudeBreakIn;
                    efAttendance.LatitudeBreakOut = mAttendance.LatitudeBreakOut;
                    efAttendance.LongitudeBreakIn = mAttendance.LongitudeBreakIn;
                    efAttendance.LongitudeBreakOut = mAttendance.LongitudeBreakOut;
                    if (mAttendance.CheckInImagePath.IsNotNullOrEmpty()) { efAttendance.CheckInImagePath = mAttendance.CheckInImagePath; }
                    if (mAttendance.CheckOutImagePath.IsNotNullOrEmpty()) { efAttendance.CheckOutImagePath = mAttendance.CheckOutImagePath; }
                    if (mAttendance.BreakInImagePath.IsNotNullOrEmpty()) { efAttendance.BreakInImagePath = mAttendance.BreakInImagePath; }
                    if (mAttendance.BreakOutImagePath.IsNotNullOrEmpty()) { efAttendance.BreakOutImagePath = mAttendance.BreakOutImagePath; }
                    efAttendance.LocationDescription = mAttendance.LocationDescription.IsNullOrEmpty() ? string.Empty : mAttendance.LocationDescription;
                    efAttendance.DeviceInfo = mAttendance.DeviceInfo.IsNullOrEmpty() ? string.Empty : mAttendance.DeviceInfo;
                    efAttendance.IPAddress = mAttendance.IPAddress.IsNullOrEmpty() ? string.Empty : mAttendance.IPAddress;
                    context.SaveChanges();

                    mAttendance.Id = efAttendance.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mAttendance;
        }

        //public Domain.Attendance Upsert(Domain.Attendance mAttendance)
        //{
        //    try
        //    {
        //        using (var context = new Persistence.AttendEdgeDBEntities())
        //        {
        //            var efAttendance = context.Attendances.FirstOrDefault(x => x.Id == mAttendance.Id);
        //            if (efAttendance == null)
        //            {
        //                var audit = new Persistence.AuditTrail
        //                {
        //                    CreatedBy = mAttendance.CreatedBy,
        //                    CreatedDate = DateTime.Now
        //                };

        //                context.AuditTrails.Add(audit);
        //                context.SaveChanges();

        //                efAttendance = new Persistence.Attendance
        //                {
        //                    AuditId = audit.Id
        //                };
        //                context.Attendances.Add(efAttendance);
        //            }
        //            else
        //            {
        //                var audit = context.AuditTrails.Find(efAttendance.AuditId);
        //                if (audit != null)
        //                {
        //                    audit.LastModifiedBy = mAttendance.LastModifiedBy;
        //                    audit.LastModifiedDate = DateTime.Now;
        //                }
        //            }

        //            efAttendance.CompanyId = mAttendance.CompanyId;
        //            efAttendance.UserId = mAttendance.UserId;
        //            efAttendance.CheckInTime = mAttendance.CheckInTime;
        //            efAttendance.CheckOutTime = mAttendance.CheckOutTime;
        //            efAttendance.BreakOutTime = mAttendance.BreakOutTime;
        //            efAttendance.BreakInTime = mAttendance.BreakInTime;
        //            efAttendance.LatitudeCheckIn = mAttendance.LatitudeCheckIn;
        //            efAttendance.LatitudeCheckOut = mAttendance.LatitudeCheckOut;
        //            efAttendance.LongitudeCheckIn = mAttendance.LongitudeCheckIn;
        //            efAttendance.LongitudeCheckOut = mAttendance.LongitudeCheckOut;
        //            efAttendance.LatitudeBreakIn = mAttendance.LatitudeBreakIn;
        //            efAttendance.LatitudeBreakOut = mAttendance.LatitudeBreakOut;
        //            efAttendance.LongitudeBreakIn = mAttendance.LongitudeBreakIn;
        //            efAttendance.LongitudeBreakOut = mAttendance.LongitudeBreakOut;
        //            efAttendance.CheckInImagePath = mAttendance.CheckInImagePath.IsNullOrEmpty() ? string.Empty : mAttendance.CheckInImagePath;
        //            efAttendance.CheckOutImagePath = mAttendance.CheckOutImagePath;
        //            efAttendance.BreakInImagePath = mAttendance.BreakInImagePath;
        //            efAttendance.BreakOutImagePath = mAttendance.BreakOutImagePath;
        //            efAttendance.LocationDescription = mAttendance.LocationDescription.IsNullOrEmpty() ? string.Empty : mAttendance.LocationDescription;
        //            efAttendance.DeviceInfo = mAttendance.DeviceInfo.IsNullOrEmpty() ? string.Empty : mAttendance.DeviceInfo;
        //            efAttendance.IPAddress = mAttendance.IPAddress.IsNullOrEmpty() ? string.Empty : mAttendance.IPAddress;

        //            context.SaveChanges();

        //            mAttendance.Id = efAttendance.Id;
        //        }
        //    }
        //    catch(Exception)
        //    {
        //        throw;
        //    }
        //    return mAttendance;
        //}


        public void Delete(int id, int loginUserId)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efAMC = context.AMCs.FirstOrDefault(x => x.Id == id);
                    if (efAMC != null)
                    {
                        var audit = context.AuditTrails.Find(efAMC.AuditId);
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

        public List<Domain.Attendance> GetActiveUsers(int companyId)
        {
            var userList = new List<Domain.Attendance>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUsers = from attend in context.Users
                                  where attend.CompanyId == companyId
                                  select new
                                  {
                                      Id = attend.Id,
                                      FirstName = attend.FirstName,
                                      LastName = attend.LastName,
                                  };

                    var json = JsonSerializer.Serialize(efUsers.ToList()); 
                    userList = JsonSerializer.Deserialize<List<Domain.Attendance>>(json);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return userList;
        }
    }
}
