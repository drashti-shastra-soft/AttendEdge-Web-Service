using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        public Domain.LeaveRequestLister GetAll(Domain.LeaveRequestLister mLister)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var query = from l in context.LeaveRequests
                                join user in context.Users on l.UserId equals user.Id
                                where l.DeletedBy == null && l.DeletedDate == null 
                                orderby l.Id descending
                                select new
                                {
                                    Id = l.Id,
                                    CompanyId = l.CompanyId,
                                    UserId = l.UserId,
                                    StartDate = l.StartDate,
                                    EndDate = l.EndDate,
                                    LeaveType = l.LeaveType,
                                    Reason = l.Reason,
                                    Status = l.Status,
                                    AdminNote = l.AdminNote,
                                    ProcessedByAdminId = l.ProcessedByAdminId,
                                    IsHalfDay = l.IsHalfDay,
                                    HalfType = l.HalfType,
                                    Username = user.FirstName + " " + user.LastName
                                };

                    // Server-side filtering
                    if (mLister.SearchCriteria != null)
                    {
                        var criteria = mLister.SearchCriteria;

                        if (criteria.Username.IsNotNullOrEmpty())
                            query = query.Where(x => x.Username != null && x.Username.ToUpper() == criteria.Username.ToUpper());

                        if (criteria.CompanyId > 0)
                            query = query.Where(x => x.CompanyId == criteria.CompanyId);

                        if (criteria.Status.IsNotNullOrEmpty())
                            query = query.Where(x => x.Status != null && x.Status.ToUpper() == criteria.Status.ToUpper());

                        if (criteria.LeaveType.IsNotNullOrEmpty())
                            query = query.Where(x => x.LeaveType != null && x.LeaveType.ToUpper() == criteria.LeaveType.ToUpper());

                        if (criteria.LeaveStatus.IsNotNullOrEmpty())
                        {
                            if (criteria.LeaveStatus == "Pending")
                                query = query.Where(x => x.Status == "Pending");
                            else if (criteria.LeaveStatus == "Approved")
                                query = query.Where(x => x.Status == "Approved");
                            else if (criteria.LeaveStatus == "Rejected")
                                query = query.Where(x => x.Status == "Rejected");
                        }

                    }

                    // Switch to in-memory for complex string search
                    var efLeaveRequests = query.AsEnumerable();

                    if (mLister.SearchCriteria != null)
                    {
                        var criteria = mLister.SearchCriteria;

                        if (criteria.Reason.IsNotNullOrEmpty())
                        {
                            efLeaveRequests = efLeaveRequests.Where(x => x.Reason != null && x.Reason.ToUpper().Contains(criteria.Reason.ToUpper()));
                        }

                        if (criteria.AdminNote.IsNotNullOrEmpty())
                        {
                            efLeaveRequests = efLeaveRequests.Where(x => x.AdminNote != null && x.AdminNote.ToUpper().Contains(criteria.AdminNote.ToUpper()));
                        }
                    }

                    // Pagination
                    var pagination = mLister.Pagination ?? new Domain.Pagination();

                    pagination.TotalRecord = efLeaveRequests.Count();

                    if (pagination.Take <= 0)
                    {
                        pagination.Take = pagination.TotalRecord;
                        pagination.Skip = 0;
                    }

                    pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;

                    pagination.TotalPage = pagination.Take > 0
                        ? (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take)
                        : 1;

                    efLeaveRequests = efLeaveRequests.Skip(pagination.Skip).Take(pagination.Take);

                    mLister.Pagination = pagination;

                    // Convert to Domain.LeaveRequest list
                    mLister.List = JsonSerializer.Deserialize<List<Domain.LeaveRequest>>(JsonSerializer.Serialize(efLeaveRequests.ToList()));
                }
            }
            catch
            {
                throw;
            }

            return mLister;
        }

        public Domain.LeaveRequest Upsert(Domain.LeaveRequest mLeave)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efLeave = context.LeaveRequests.FirstOrDefault(x => x.Id == mLeave.Id);
                    if (efLeave == null || efLeave.Id <= default(int))
                    {
                        efLeave = new Persistence.LeaveRequest();
                        efLeave.CreatedBy = mLeave.CreatedBy;
                        efLeave.CreatedDate = DateTime.Now;
                        context.LeaveRequests.Add(efLeave);
                        efLeave.Status = EnumHelpers.GetEnumString(Enums.Status.Pending);
                    }
                    else
                    {
                        efLeave.Status = mLeave.Status;
                        efLeave.LastModifiedBy = mLeave.LastModifiedBy;
                        efLeave.LastModifiedDate = DateTime.Now;
                    }

                    efLeave.CompanyId = mLeave.CompanyId;
                    efLeave.UserId = mLeave.UserId;
                    efLeave.StartDate = mLeave.StartDate;
                    efLeave.EndDate = mLeave.EndDate;
                    efLeave.LeaveType = mLeave.LeaveType;
                    efLeave.Reason = mLeave.Reason;
                    efLeave.IsHalfDay = mLeave.IsHalfDay;
                    efLeave.HalfType = mLeave.HalfType;
                    efLeave.AdminNote = mLeave.AdminNote;
                    efLeave.ProcessedByAdminId = mLeave.ProcessedByAdminId;
                    context.SaveChanges();

                    mLeave.Id = efLeave.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mLeave;
        }

        public Domain.LeaveRequest Get(int id)
        {
            var mLeave = new Domain.LeaveRequest();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efLeave = (from leave in context.LeaveRequests
                                   where leave.Id == id
                                   select new
                                   {
                                       Id = leave.Id,
                                       CompanyId = leave.CompanyId,
                                       UserId = leave.UserId,
                                       StartDate = leave.StartDate,
                                       EndDate = leave.EndDate,
                                       LeaveType = leave.LeaveType,
                                       Reason = leave.Reason,
                                       Status = leave.Status,
                                       AdminNote = leave.AdminNote,
                                       ProcessedByAdminId = leave.ProcessedByAdminId,
                                       IsHalfDay = leave.IsHalfDay,
                                       HalfType = leave.HalfType
                                   }).AsEnumerable();

                    mLeave = JsonSerializer.Deserialize<Domain.LeaveRequest>(JsonSerializer.Serialize(efLeave.FirstOrDefault()));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return mLeave;
        }

        public void Delete(int id, int loginUserId)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efLeaveRequest = context.LeaveRequests.Where(x => x.Id == id).FirstOrDefault();
                    if (efLeaveRequest != null && efLeaveRequest.Id > default(int))
                    {
                        efLeaveRequest.DeletedBy = loginUserId;
                        efLeaveRequest.DeletedDate = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Domain.LeaveRequest> GetAll()
        {
            var mLeaveRequest = new List<Domain.LeaveRequest>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efLeaveRequest = from l in context.LeaveRequests
                                         join user in context.Users on l.UserId equals user.Id
                                         where l.DeletedBy == null && l.DeletedDate == null
                                         select new
                                         {
                                             Id = l.Id,
                                             CompanyId = l.CompanyId,
                                             UserId = l.UserId,
                                             StartDate = l.StartDate,
                                             EndDate = l.EndDate,
                                             LeaveType = l.LeaveType,
                                             Reason = l.Reason,
                                             Status = l.Status,
                                             AdminNote = l.AdminNote,
                                             ProcessedByAdminId = l.ProcessedByAdminId,
                                             IsHalfDay = l.IsHalfDay,
                                             HalfType = l.HalfType,
                                             Username = user.FirstName + " " + user.LastName
                                         };

                    mLeaveRequest = JsonSerializer.Deserialize<List<Domain.LeaveRequest>>(JsonSerializer.Serialize(efLeaveRequest.ToList()));
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mLeaveRequest;
        }

        public Domain.User GetActiveUsers(int companyId)
        {
            var mUser = new Domain.User();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efUser = (from user in context.Users
                                  where user.CompanyId == companyId
                                  select new
                                  {
                                      Id = user.Id,
                                      FirstName = user.FirstName,
                                      LastName = user.LastName,
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


    }
}
