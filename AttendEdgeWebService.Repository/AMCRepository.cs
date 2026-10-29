using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class AMCRepository : IAMCRepository
    {
        public Domain.AMCLister GetAll(Domain.AMCLister mLister)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var query = from amc in context.AMCs
                                join c in context.Companies on amc.CompanyId equals c.Id into joinUC
                                from c in joinUC.DefaultIfEmpty()
                                join at in context.AuditTrails on amc.AuditId equals at.Id
                                where at.DeletedBy == null && at.DeletedDate == null
                                select new
                                {
                                    Id = amc.Id,
                                    Name = amc.Name,
                                    CompanyId = amc.CompanyId,
                                    ContactPerson = amc.ContactPerson,
                                    MobileNumber = amc.MobileNumber,
                                    EmailAddress = amc.EmailAddress,
                                    ContractStartDate = amc.ContractStartDate,
                                    ContractEndDate = amc.ContractEndDate,
                                    ServicesCovered = amc.ServicesCovered,
                                    ContractDocumentPath = amc.ContractDocumentPath,
                                    Notes = amc.Notes,
                                    Company = c.Name,
                                    IsActive = amc.IsActive,
                                    AuditId = amc.AuditId
                                };

                    // Server-side filtering
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
                            query = query.Where(x => x.Name != null && x.Name.ToUpper().Contains(criteria.Name.ToUpper()));

                        if (criteria.EmailAddress.IsNotNullOrEmpty())
                            query = query.Where(x => x.EmailAddress != null && x.EmailAddress.ToUpper().Contains(criteria.EmailAddress.ToUpper()));


                        if (criteria.ContactPerson.IsNotNullOrEmpty())
                            query = query.Where(x => x.ContactPerson != null && x.ContactPerson.ToUpper().Contains(criteria.ContactPerson.ToUpper()));

                        if (criteria.MobileNumber.IsNotNullOrEmpty())
                            query = query.Where(x => !string.IsNullOrEmpty(x.MobileNumber) && x.MobileNumber.ToUpper().Contains(criteria.MobileNumber.ToUpper()));

                        if (criteria.ServicesCovered.IsNotNullOrEmpty())
                            query = query.Where(x => x.ServicesCovered != null && x.ServicesCovered.ToUpper().Contains(criteria.ServicesCovered.ToUpper()));
                    }

                    var efAMCs = query.AsEnumerable();

                    // Pagination
                    var pagination = mLister.Pagination ?? new Domain.Pagination();

                    pagination.TotalRecord = efAMCs.Count();

                    if (pagination.Take <= 0)
                    {
                        pagination.Take = pagination.TotalRecord;
                        pagination.Skip = 0;
                    }

                    pagination.CurrentPage = pagination.Take > 0 ? (pagination.Skip / pagination.Take) + 1 : 1;

                    pagination.TotalPage = pagination.Take > 0
                        ? (int)Math.Ceiling((double)pagination.TotalRecord / pagination.Take)
                        : 1;

                    efAMCs = efAMCs.Skip(pagination.Skip).Take(pagination.Take);

                    mLister.Pagination = pagination;

                    mLister.List = JsonSerializer.Deserialize<List<Domain.AMC>>(JsonSerializer.Serialize(efAMCs.ToList()));
                }
            }
            catch
            {
                throw;
            }

            return mLister;
        }

        public Domain.AMC Upsert(Domain.AMC mAMC)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efAMC = context.AMCs.FirstOrDefault(x => x.Id == mAMC.Id);
                    if (efAMC == null)
                    {
                        var audit = new Persistence.AuditTrail
                        {
                            CreatedBy = mAMC.CreatedBy,
                            CreatedDate = DateTime.Now
                        };

                        context.AuditTrails.Add(audit);
                        context.SaveChanges();

                        efAMC = new Persistence.AMC
                        {
                            AuditId = audit.Id
                        };
                        context.AMCs.Add(efAMC);
                    }
                    else
                    {
                        var audit = context.AuditTrails.Find(efAMC.AuditId);
                        if (audit != null)
                        {
                            audit.LastModifiedBy = mAMC.LastModifiedBy;
                            audit.LastModifiedDate = DateTime.Now;
                        }
                    }

                    efAMC.Name = mAMC.Name;
                    efAMC.CompanyId = mAMC.CompanyId;
                    efAMC.ContactPerson = mAMC.ContactPerson;
                    efAMC.MobileNumber = mAMC.MobileNumber;
                    efAMC.EmailAddress = mAMC.EmailAddress;
                    efAMC.ContractStartDate = mAMC.ContractStartDate;
                    efAMC.ContractEndDate = mAMC.ContractEndDate;
                    efAMC.ServicesCovered = mAMC.ServicesCovered;
                    efAMC.ContractDocumentPath = mAMC.ContractDocumentPath;
                    efAMC.Notes = mAMC.Notes;
                    efAMC.IsActive = mAMC.IsActive;
                    efAMC.AuditId = mAMC.AuditId;

                    context.SaveChanges();

                    mAMC.Id = efAMC.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mAMC;
        }

        public Domain.AMC Get(int id)
        {
            var mAMC = new Domain.AMC();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efAMC = (from amc in context.AMCs
                                 where amc.Id == id
                                 select new
                                 {
                                     Id = amc.Id,
                                     CompanyId = amc.CompanyId,
                                     Name = amc.Name,
                                     ContactPerson = amc.ContactPerson,
                                     MobileNumber = amc.MobileNumber,
                                     EmailAddress = amc.EmailAddress,
                                     ContractStartDate = amc.ContractStartDate,
                                     ContractEndDate = amc.ContractEndDate,
                                     ServicesCovered = amc.ServicesCovered,
                                     ContractDocumentPath = amc.ContractDocumentPath,
                                     Notes = amc.Notes,
                                     AuditId = amc.AuditId,
                                     IsActive = amc.IsActive,
                                 }).AsEnumerable();

                    mAMC = JsonSerializer.Deserialize<Domain.AMC>(JsonSerializer.Serialize(efAMC.FirstOrDefault()));
                }
            }
            catch
            {
                throw;
            }

            return mAMC;
        }

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

    }
}
