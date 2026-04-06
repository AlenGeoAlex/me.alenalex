using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Bloggi.Backend.Api.Web.Extensions;

public static class QueryableExtension
{
    extension<T>(IQueryable<T> queryable) where T : class
    {
        public IQueryable<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? queryable.Where(predicate) : queryable;
        }

        public IQueryable<T> WhereIf(Func<bool> condition,
            Expression<Func<T, bool>> expression)
        {
            return condition() ? queryable.Where(expression) : queryable;
        }

        public IQueryable<T> IncludeIf<TProperty>(
            Func<bool> condition,
            Expression<Func<T, TProperty>> navigationPropertyPath)
        {
            return condition() ? queryable.Include(navigationPropertyPath) : queryable;
        }

        public IQueryable<T> IncludeIf<TProperty>(
            bool condition,
            Expression<Func<T, TProperty>> navigationPropertyPath,
            Expression<Func<TProperty, TProperty>>? includeExpression = null)
        {
            return condition ?
                includeExpression is null ?
                    queryable.Include(navigationPropertyPath) :
                    queryable.Include(navigationPropertyPath)
                        .ThenInclude(includeExpression)
                : queryable;
        }
    }
    
}