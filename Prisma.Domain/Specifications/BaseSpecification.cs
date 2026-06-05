using System.Linq.Expressions;
using Prisma.Domain.Entities;
using Prisma.Domain.Interfaces;

namespace Prisma.Domain.Specifications;

public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : BaseEntity
{
    public Expression<Func<TEntity, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public Expression<Func<TEntity, object>>? GroupBy { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    //specs to build queries
    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    protected void AddInclude(Expression<Func<TEntity, object>> include)
    {
        Includes.Add(include);
    }

    protected void AddInclude(string include)
    {
        IncludeStrings.Add(include);
    }

    protected void ApplyOrderBy(Expression<Func<TEntity, object>> order)
    {
        OrderBy = order;
    }

    protected void ApplyOrderByDescending(Expression<Func<TEntity, object>> order)
    {
        OrderByDescending = order;
    }

    protected void ApplyGroupBy(Expression<Func<TEntity, object>> order)
    {
        GroupBy = order;
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}