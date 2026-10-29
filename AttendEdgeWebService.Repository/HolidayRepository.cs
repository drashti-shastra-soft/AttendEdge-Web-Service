using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class HolidayRepository : IHolidayRepository
    {
        public Domain.HolidayLister GetAll(Domain.HolidayLister mLister)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var query = from h in context.Holidays
                                join c in context.Companies on h.CompanyId equals c.Id into joinUC
                                from c in joinUC.DefaultIfEmpty()
                                where h.DeletedBy == null && h.DeletedDate == null 
                                orderby h.Id descending
                                select new
                                {
                                    Id = h.Id,
                                    Name = h.Name,
                                    Date = h.Date,
                                    CompanyId = h.CompanyId,
                                    Company = c.Name,
                                    IsActive = h.IsActive,
                                    IsHalfDay = h.IsHalfDay,
                                };

                    // Apply server-side filters where possible
                    if (mLister.SearchCriteria != null)
                    {
                        var criteria = mLister.SearchCriteria;

                        if (criteria.Status.IsNotNullOrEmpty())
                        {
                            if (criteria.Status == "Active")
                                query = query.Where(x => x.IsActive);
                            else if (criteria.Status == "Disabled")
                                query = query.Where(x => !x.IsActive);
                        }


                        if (criteria.Name.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper().Contains(criteria.Name.ToUpper()));
                    }


                    // Switch to in-memory for complex string filters
                    var efHolidays = query.AsEnumerable();

                    var pagination = mLister.Pagination ?? new Domain.Pagination();

                    pagination.TotalRecord = efHolidays.Count();

                    if (pagination.Take <= 0)
                    {
                        pagination.Take = pagination.TotalRecord;
                        pagination.Skip = 0;
                    }

                    pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;

                    pagination.TotalPage = pagination.Take > 0 ? (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take) : 1;

                    efHolidays = efHolidays.Skip(pagination.Skip).Take(pagination.Take);

                    mLister.Pagination = pagination;
                    mLister.List = JsonSerializer.Deserialize<List<Domain.Holiday>>(JsonSerializer.Serialize(efHolidays.ToList()));
                }
            }
            catch
            {
                throw;
            }

            return mLister;
        }

        public Domain.Holiday Upsert(Domain.Holiday mHoliday)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efHoliday = context.Holidays.FirstOrDefault(x => x.Id == mHoliday.Id);

                    if (efHoliday == null || efHoliday.Id <= default(int))
                    {
                        efHoliday = new Persistence.Holiday();
                        efHoliday.CreatedBy = mHoliday.CreatedBy;
                        efHoliday.CreatedDate = DateTime.Now;
                        context.Holidays.Add(efHoliday);
                    }
                    else
                    {
                        efHoliday.LastModifiedBy = mHoliday.LastModifiedBy;
                        efHoliday.LastModifiedDate = DateTime.Now;
                    }

                    efHoliday.CompanyId = mHoliday.CompanyId;
                    efHoliday.Date = mHoliday.Date;
                    efHoliday.Name = mHoliday.Name;
                    efHoliday.Description = mHoliday.Description;
                    efHoliday.IsHalfDay = mHoliday.IsHalfDay;
                    efHoliday.IsActive = mHoliday.IsActive;
                    context.SaveChanges();

                    mHoliday.Id = efHoliday.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mHoliday;
        }

        public Domain.Holiday Get(int id)
        {
            var mHoliday = new Domain.Holiday();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efHoliday = (from hol in context.Holidays
                                     where hol.Id == id
                                     select new
                                     {
                                         Id = hol.Id,
                                         Name = hol.Name,
                                         Date = hol.Date,
                                         Description = hol.Description,
                                         IsActive = hol.IsActive,
                                     }).AsEnumerable();

                    mHoliday = JsonSerializer.Deserialize<Domain.Holiday>(JsonSerializer.Serialize(efHoliday.FirstOrDefault()));
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mHoliday;
        }

        public void Delete(int id, int loginUserId)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efHoliday = context.Holidays.Where(x => x.Id == id).FirstOrDefault();
                    if (efHoliday != null && efHoliday.Id > default(int))
                    {
                        efHoliday.DeletedBy = loginUserId;
                        efHoliday.DeletedDate = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Domain.Holiday> GetAll()
        {
            var mHoliday = new List<Domain.Holiday>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efHoliday = from h in context.Holidays
                                    where h.DeletedBy == null && h.DeletedDate == null
                                    orderby h.Date.Month, h.Date.Day
                                    select new
                                    {
                                         Id = h.Id,
                                         Date = h.Date,
                                         Name = h.Name,
                                         Description = h.Description,
                                         IsHalfDay = h.IsHalfDay,
                                         IsActive = h.IsActive,
                                    };

                    mHoliday = JsonSerializer.Deserialize<List<Domain.Holiday>>(JsonSerializer.Serialize(efHoliday.ToList()));
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mHoliday;
        }


    }
}
