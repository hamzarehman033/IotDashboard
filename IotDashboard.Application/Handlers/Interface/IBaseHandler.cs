using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IBaseHandler<TVM> where TVM : class
    {
        Task<Response<TVM>> CreateAsync(TVM model);
        Task<Response<TVM>> UpdateAsync(long id, TVM model);
        Task<Response<TVM>> DeleteAsync(long Id);
        Task<Response<TVM>> GetByIdAsync(long Id);
        Task<Response<PagerModel<TVM>>> GetAllAsync(int pagesize = 10, int currentPage = 1, IEnumerable<FilterVM> filters = null);
    }
}
