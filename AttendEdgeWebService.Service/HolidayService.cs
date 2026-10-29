using AttendEdgeWebService.Infrastructure.IRepository;
using AttendEdgeWebService.Infrastructure.IService;
using AttendEdgeWebService.Infrastructure.Utils;
using System.Collections.Generic;
using System.Web.Hosting;

namespace AttendEdgeWebService.Service
{
    public class HolidayService : IHolidayService
    {
        #region Declaration
        private readonly IHolidayRepository _repo;

        public HolidayService(IHolidayRepository repo)
        {
            _repo = repo;
        }

        #endregion

        #region Public Methods
        public Domain.HolidayLister GetAll(Domain.HolidayLister mLister)
        {
            return _repo.GetAll(mLister);
        }

        public Domain.Holiday Upsert(Domain.Holiday mHoliday)
        {
            return _repo.Upsert(mHoliday);
        }

        public Domain.Holiday Get(int id)
        {
            return _repo.Get(id);
        }

        public void Delete(int id, int loginUserId)
        {
            _repo.Delete(id, loginUserId);
        }

        public List<Domain.Holiday> GetAll()
        {
            return _repo.GetAll();
        }

        #endregion
    }
}
