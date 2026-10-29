using AttendEdgeWebService.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AttendEdgeWebService.Repository
{
    public class UserDocumentRepository : IUserDocumentRepository
    {
        public Domain.UserDocument Create(Domain.UserDocument mDocument)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var audit = new Persistence.AuditTrail
                    {
                        CreatedBy = mDocument.CreatedBy,
                        CreatedDate = DateTime.Now
                    };

                    context.AuditTrails.Add(audit);
                    context.SaveChanges();

                    var efDocument = new Persistence.UserDocument
                    {
                        UserId = mDocument.UserId,
                        FilePath = mDocument.FilePath,
                        MimeType = mDocument.MimeType,
                        AuditId = audit.Id
                    };

                    context.UserDocuments.Add(efDocument);
                    context.SaveChanges();
                    mDocument.Id = efDocument.Id;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mDocument;
        }



        public List<Domain.UserDocument> Get(int userId)
        {
            var mImages = new List<Domain.UserDocument>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efDocuments = (from ud in context.UserDocuments
                                       join at in context.AuditTrails on ud.AuditId equals at.Id
                                       where ud.UserId == userId && at.DeletedBy == null && at.DeletedDate == null
                                       select new
                                       {
                                           Id = ud.Id,
                                           UserId = ud.UserId,
                                           FilePath = ud.FilePath,
                                           MimeType = ud.MimeType
                                       }).AsEnumerable();

                    mImages = JsonSerializer.Deserialize<List<Domain.UserDocument>>(JsonSerializer.Serialize(efDocuments.ToList()));
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mImages;
        }

        public List<Domain.UserDocument> FetchUserDocument(int userId)
        {
            var mImages = new List<Domain.UserDocument>();
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efdocuments = (from ud in context.UserDocuments
                                       join at in context.AuditTrails on ud.AuditId equals at.Id
                                       where ud.UserId == userId && at.DeletedBy == null && at.DeletedDate == null
                                       select new
                                       {
                                           Id = ud.Id,
                                           UserId = ud.UserId,
                                           FilePath = ud.FilePath,
                                           MimeType = ud.MimeType
                                       }).AsEnumerable();


                    mImages = JsonSerializer.Deserialize<List<Domain.UserDocument>>(JsonSerializer.Serialize(efdocuments.ToList()));
                }
            }
            catch (Exception)
            {
                throw;
            }
            return mImages;
        }

        public void Delete(int id, int loginUserId)
        {
            try
            {
                using (var context = new Persistence.AttendEdgeDBEntities())
                {
                    var efImage = context.UserDocuments.FirstOrDefault(x => x.Id == id);
                    if (efImage != null && efImage.Id > default(int))
                    {
                        var audit = context.AuditTrails.Find(efImage.AuditId);
                        if (audit != null)
                        {
                            audit.DeletedBy = loginUserId;
                            audit.DeletedDate = DateTime.Now;
                            context.SaveChanges();
                        }
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
