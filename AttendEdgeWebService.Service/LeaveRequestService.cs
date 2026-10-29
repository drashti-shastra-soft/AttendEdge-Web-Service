using System.Linq;
using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.IService;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using AttendEdgeWebService.Infrastructure.Utils;

namespace AttendEdgeWebService.Service
{
    public class LeaveRequestService : ILeaveRequestService
    {
        #region Declaration
        private readonly ILeaveRequestRepository _repo;
        private readonly IUserRepository _userRepository;
        private readonly IFileReaderService _fileReaderService;
        private readonly IEmailSenderService _emailSenderService;

        public LeaveRequestService(ILeaveRequestRepository repo, IUserRepository userRepository, IFileReaderService fileReaderService, IEmailSenderService emailSenderService)
        {
            _repo = repo;
            _userRepository = userRepository;
            _fileReaderService = fileReaderService;
            _emailSenderService = emailSenderService;
        }

        #endregion

        #region Public Methods
        public Domain.LeaveRequestLister GetAll(Domain.LeaveRequestLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        //public Domain.LeaveRequest Upsert(Domain.LeaveRequest mLeaveRequest)
        //{
        //    var isNew = mLeaveRequest.Id == 0;

        //    var mCreatedLeaveRequest = _repo.Upsert(mLeaveRequest);

        //    if (mCreatedLeaveRequest.Id > 0)
        //    {
        //        var mUser = _userRepository.Get(mCreatedLeaveRequest.UserId);

        //        // ✅ 1. Employee creates leave
        //        if (isNew && mCreatedLeaveRequest.Status == Convert.ToString(Enums.Status.Pending))
        //        {
        //            SendEmailWhileLeaveRequestPosted(mCreatedLeaveRequest, mUser);
        //        }
        //        // ✅ 2. Admin/SuperAdmin updates status
        //        else if (!isNew && mCreatedLeaveRequest.Status != Convert.ToString(Enums.Status.Pending))
        //        {
        //            SendEmailWhileLeaveRequestStatusUpdate(mCreatedLeaveRequest, mUser);
        //        }
        //    }

        //    return mCreatedLeaveRequest;
        //}

        public Domain.LeaveRequest Upsert(Domain.LeaveRequest mLeaveRequest)
        {
            var isNew = mLeaveRequest.Id == 0;
            var mCreatedLeaveRequest = _repo.Upsert(mLeaveRequest);

            if (mCreatedLeaveRequest.Id > 0 && mCreatedLeaveRequest.Status == Convert.ToString(Enums.Status.Pending))
            {
                var mUser = _userRepository.Get(mCreatedLeaveRequest.UserId);
                SendEmailWhileLeaveRequestPosted(mCreatedLeaveRequest, mUser);
            }
            else if (mCreatedLeaveRequest.Id != 0 && mCreatedLeaveRequest.Status == mLeaveRequest.Status)
            {
                var mUser = _userRepository.Get(mCreatedLeaveRequest.UserId);
                SendEmailWhileLeaveRequestStatusUpdate(mCreatedLeaveRequest, mUser);
            }

            return mCreatedLeaveRequest;
        }

        public Domain.LeaveRequest Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id,int loginUserId)
        {
            _repo.Delete(id, loginUserId);
        }

        public List<Domain.LeaveRequest> GetAll()
        {
            return _repo.GetAll();
        }


        #endregion

        #region Private Methods email sending
        private string PopulateEmailTemplate(string template, Domain.LeaveRequest mLeaveRequest, Domain.User mUser, Domain.User manager)
        {
            var replacements = new Dictionary<string, string>
            {
                { "{{EmployeeName}}", mUser.FirstName + " " + mUser.LastName},
                { "{{LeaveType}}", mLeaveRequest.Status },
                { "{{ManagerName}}", manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager" },
                { "{{StartDate}}", Convert.ToString(mLeaveRequest.StartDate) },
                { "{{EndDate}}", Convert.ToString(mLeaveRequest.EndDate)},
                { "{{Reason}}", mLeaveRequest.Reason },
            };

            foreach (var pair in replacements)
            {
                template = template.Replace(pair.Key, pair.Value);
            }

            return template;
        }

        private void SendEmailWhileLeaveRequestPosted(Domain.LeaveRequest mLeaveRequest, Domain.User mUser)
        {
            var managers = _userRepository.GetActiveUsers(mUser.CompanyId);
            var manager = managers.FirstOrDefault(u => u.RoleId == (int)Enums.Role.Admin);
            string managerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager";

            var emailAddresses = _userRepository.GetNotificationEmails(mLeaveRequest.UserId, mLeaveRequest.CompanyId);
            if (emailAddresses != null && emailAddresses.Count > default(int))
            {
                string content = _fileReaderService.ReadFileContent("~/EmailTemplates/SendLeaveInfoOnLeaveCreate.html");
                string finalContent = PopulateEmailTemplate(content, mLeaveRequest, mUser, manager);
                _emailSenderService.SendEmail(finalContent, emailAddresses, $"New Leave Request Submitted by {mUser.FirstName} {mUser.LastName}");
            }
        }

        #endregion

        #region Private Methods email sending on status update 
        private string PopulateEmailTemplateStatus(string template, Domain.LeaveRequest mLeaveRequest, Domain.User mUser, Domain.User manager)
        {
            var replacements = new Dictionary<string, string>
            {
                { "{{EmployeeName}}", mUser.FirstName + " " + mUser.LastName},
                { "{{LeaveType}}", mLeaveRequest.Status },
                { "{{ManagerName}}", manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager" },
                { "{{StartDate}}", Convert.ToString(mLeaveRequest.StartDate) },
                { "{{EndDate}}", Convert.ToString(mLeaveRequest.EndDate)},
                { "{{AdminNote}}", mLeaveRequest.AdminNote },
            };

            foreach (var pair in replacements)
            {
                template = template.Replace(pair.Key, pair.Value);
            }

            return template;
        }

        private void SendEmailWhileLeaveRequestStatusUpdate(Domain.LeaveRequest mLeaveRequest, Domain.User mUser)
        {
            var managers = _userRepository.GetActiveUsers(mUser.CompanyId);
            var manager = managers.FirstOrDefault(u => u.RoleId == (int)Enums.Role.Admin);
            string managerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Manager";

            var emailAddresses = _userRepository.GetNotificationEmails(mLeaveRequest.UserId, mLeaveRequest.CompanyId);
            if (emailAddresses != null && emailAddresses.Count > default(int))
            {
                string content = _fileReaderService.ReadFileContent("~/EmailTemplates/SendLeaveInfoOnLeaveStatus.html");
                string finalContent = PopulateEmailTemplateStatus(content, mLeaveRequest, mUser, manager);
                _emailSenderService.SendEmail(finalContent, emailAddresses, $"Leave Request from {mUser.FirstName} {mUser.LastName}");
            }
        }
        #endregion
    }
}
