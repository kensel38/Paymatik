using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DAL
{
    public interface IRepository<T>
    {
        T GetByID(int id);
        List<T> GetAll_ByParam(Expression<Func<T, bool>> _where);
        T Get_ByParam(Expression<Func<T, bool>> _where);

        List<TType> GetAll_Where_Select<TType>(Expression<Func<T, bool>> _where, Expression<Func<T, TType>> _select) where TType : class;
        List<T> GetAll();

        T Add(T entity);

        int AddRange(List<T> entities);

        T Update(T entity);
        int Delete(int id);
        int DeleteByParam(Expression<Func<T, bool>> _where);

    }
}
