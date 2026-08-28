// Services/ContactInquiryService.cs
using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Extensions;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Requests;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Services;

public class ContactInquiryService(AppDbContext context) : BaseService<ContactInquiry>(context)
{
    public async Task SubmitInquiryAsync(ContactInquiryRequest request)
    {
        var inquiry = new ContactInquiry
        {
            DateTimeCreated = DateTime.UtcNow,
            DateTimeUpdated = DateTime.UtcNow,
            IsRead = false,
            FullName = request.FullName,
            Email = request.Email,
            Message = request.Message,
        };

        Context.ContactInquiries.Add(inquiry);
        await Context.SaveChangesAsync();
    }

    public async Task<PagedResponse<ContactInquiryResponse>> GetInquiriesAsync(int page, int pageSize, bool onlyUnread)
    {
        (page, pageSize) = PaginationExtensions.Normalize(page, pageSize);

        var query = Context.ContactInquiries.AsNoTracking();

        if (onlyUnread)
        {
            query = query.Where(x => !x.IsRead);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.DateTimeCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ContactInquiryResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.DateTimeCreated
            })
            .ToListAsync();

        return new PagedResponse<ContactInquiryResponse>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ToggleReadStatusAsync(long id)
    {
        var inquiry = await Context.ContactInquiries
            .Where(x => x.Id == id && !x.IsRead)
            .FirstOrDefaultAsync();

        if (inquiry == null)
        {
            throw new NotFoundException($"Inquiry with ID {id} does not exist.");
        }

        inquiry.IsRead = !inquiry.IsRead;
        inquiry.DateTimeUpdated = DateTime.UtcNow;

        await Context.SaveChangesAsync();
        return inquiry.IsRead;
    }
}