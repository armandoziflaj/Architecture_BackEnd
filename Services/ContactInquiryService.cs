// Services/ContactInquiryService.cs
using Microsoft.EntityFrameworkCore;
using Sulozeqi_BackEnd.ExceptionMiddleware;
using Sulozeqi_BackEnd.Models;
using Sulozeqi_BackEnd.Responses;

namespace Sulozeqi_BackEnd.Services;

public class ContactInquiryService(AppDbContext context) : BaseService<ContactInquiry>(context)
{
    public async Task SubmitInquiryAsync(ContactInquiry inquiry)
    {
        inquiry.DateTimeCreated = DateTime.UtcNow;
        inquiry.DateTimeUpdated = DateTime.UtcNow;
        inquiry.IsRead = false;
        
        Context.ContactInquiries.Add(inquiry);
        await Context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContactInquiryResponse>> GetAllInquiriesAsync()
    {
        return await Context.ContactInquiries
            .AsNoTracking()
            .Where(x => !x.IsRead)
            .OrderByDescending(x => x.DateTimeCreated)
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
    }

    public async Task<bool> ToggleReadStatusAsync(long id)
    {
        var inquiry = await Context.ContactInquiries
            .Where(x => x.Id == id && !x.IsRead)
            .FirstOrDefaultAsync();

        if (inquiry == null) 
            throw new NotFoundException($"Inquiry with ID {id} does not exist.");

        inquiry.IsRead = !inquiry.IsRead;
        inquiry.DateTimeUpdated = DateTime.UtcNow;

        await Context.SaveChangesAsync();
        return inquiry.IsRead;
    }
}