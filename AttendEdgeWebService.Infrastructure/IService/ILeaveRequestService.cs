using System.Collections.Generic;

namespace AttendEdgeWebService.Infrastructure.IService
{
    public interface ILeaveRequestService
    {
        Domain.LeaveRequestLister GetAll(Domain.LeaveRequestLister mLister);

        Domain.LeaveRequest Upsert(Domain.LeaveRequest mLeaveRequest);

        Domain.LeaveRequest Get(int id);

        void Delete(int id, int loginUserId);

        List<Domain.LeaveRequest> GetAll();
    }
}
