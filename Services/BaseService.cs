using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Models;

namespace Sulozeqi_BackEnd.Services;

public abstract class BaseService<T>(AppDbContext context) where T : CommonData
{
    protected readonly AppDbContext Context = context;

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Context.Set<T>().ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        return await Context.Set<T>().FindAsync(id) ?? throw new NotFoundException("The entity you are trying to find does not exist.");
    }

    public virtual async Task<bool> DeleteAsync(long id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            throw new NotFoundException("The entity you are trying to delete does not exist.");
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }
}