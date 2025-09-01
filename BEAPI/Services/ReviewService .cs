using BEAPI.Dtos.Review;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;

namespace BEAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<Review> _repository;

        public ReviewService(IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task AddReviewAsync(Guid userId, CreateReviewDto dto)
        {
            var order = await _repository.Get().FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null)
                throw new Exception("Order not found or not completed");

            var existed = await _repository.Get().AnyAsync(r => r.OrderId == dto.OrderId && r.UserId == userId);
            if (existed)
                throw new Exception("You already reviewed this product in this order");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                OrderId = dto.OrderId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await _repository.AddAsync(review);
            await _repository.SaveChangesAsync();
        }

        public async Task<ReviewDto?> GetOrdertReviewsAsync(Guid orderId)
        {
            return await _repository.Get()
          .Where(r => r.OrderId == orderId)
          .OrderByDescending(r => r.CreationDate)
          .Select(r => new ReviewDto
          {
              Id = r.Id,
              UserId = r.UserId,
              Rating = r.Rating,
              Comment = r.Comment,
              CreatedAt = r.CreationDate,
          }).FirstOrDefaultAsync();
        }
    }
}
