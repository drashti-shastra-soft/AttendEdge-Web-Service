using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.IService;
using AttendEdgeWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web.Hosting;

namespace AttendEdgeWebService.Service
{
    public class UserDocumentService : IUserDocumentService
    {
        private readonly IUserDocumentRepository _repo;

        public UserDocumentService(IUserDocumentRepository repo)
        {
            _repo = repo;
        }

        public Domain.UserDocument Create(Domain.UserDocument mDocument)
        {
            if (mDocument == null)
                throw new Infrastructure.CustomException.APIRequestFailedException("Object must have the value");

            if (mDocument.Base64String.IsNullOrEmpty())
                throw new Infrastructure.CustomException.APIRequestFailedException("Image is Required!");

            string relativeFolder = "~/Uploads/Documents/";
            string uniqueFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(mDocument.FileName)?.ToLower()}";
            mDocument.FilePath = StaticMethods.SaveBase64Image(mDocument.Base64String, relativeFolder, uniqueFileName);

            return _repo.Create(mDocument);
        }

        public List<Domain.UserDocument> Get(int userId)
        {
            var mDocuments = _repo.Get(userId);
            if (mDocuments != null && mDocuments.Count > 0)
            {
                foreach (var item in mDocuments)
                {
                    item.Base64String = StaticMethods.GetImageBase64(HostingEnvironment.MapPath("~/" + item.FilePath.TrimStart('/')));
                }
            }
            return mDocuments;
        }

        public void Delete(int id, int loginUserId)
        {
            _repo.Delete(id, loginUserId);
        }
    }
}
