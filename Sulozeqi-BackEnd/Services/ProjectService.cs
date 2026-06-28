using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Models;

namespace Sulozeqi_BackEnd.Services;

public class ProjectService(AppDbContext context) : BaseService<Project>(context)
{
    public async Task<IEnumerable<Project>> GetPortfolioCatalogAsync()
    {
        return await Context.Projects.Include(p => p.Photos)
                                     .AsNoTracking()
                                     .ToListAsync();
    }
    
    public async Task<Project> GetProjectDetailsAsync(long id)
    {
        var project = await Context.Projects.Include(p => p.Photos.OrderBy(photo => photo.DisplayOrder))
                                            .FirstOrDefaultAsync(p => p.Id == id);

        return project ?? throw new NotFoundException($"Project with ID {id} does not exist.");
    }
}