using AutoMapper;
using AutoMapper.QueryableExtensions;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Payment;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IRepository<PaymentHistory> _repo;
        private readonly IMapper _mapper;

        public PaymentHistoryService(IRepository<PaymentHistory> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedResult<PaymentHistoryDto>> SearchAsync(PaymentHistorySearchDto request)
        {
            var query = _repo.Get()
                .Include(p => p.User)
                .AsQueryable();

            if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.CreationDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreationDate <= request.EndDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                if (Guid.TryParse(request.UserId, out var uid))
                {
                    query = query.Where(p => p.UserId == uid);
                }
            }

            query = query.OrderByDescending(p => p.CreationDate);

            var total = await query.CountAsync();
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PaymentHistoryDto
                {
                    Id = p.Id.ToString(),
                    Amount = p.Amount,
                    UserId = p.UserId.ToString(),
                    UserName = p.User != null ? p.User.FullName : null,
                    Avatar = p.User != null ? p.User.Avatar : null,
                    PaymentMenthod = p.PaymentMenthod,
                    paymentStatus = p.paymentStatus,
                    CreationDate = p.CreationDate,
                    OrderId = p.OrderId.HasValue ? p.OrderId.Value.ToString() : null
                })
                .ToListAsync();

            return new PagedResult<PaymentHistoryDto>
            {
                TotalItems = total,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = items
            };
        }
    }
}


