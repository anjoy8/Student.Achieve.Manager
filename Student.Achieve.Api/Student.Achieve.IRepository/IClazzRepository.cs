
using Student.Achieve.IRepository.Base;
using Student.Achieve.Model;
using Student.Achieve.Model.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Student.Achieve.IRepository
{
    public partial interface IClazzRepository : IBaseRepository<Clazz>
    {
        Task<PageModel<Clazz>> GetQueryPageOfMapperTb(Expression<Func<Clazz, bool>> whereExpression, int intPageIndex = 1, int intPageSize = 20, string strOrderByFileds = null);
    }
}