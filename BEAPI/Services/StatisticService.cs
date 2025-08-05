using BEAPI.Constants;
using BEAPI.Dtos.Statistic;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BEAPI.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<User> _userRepository;

        public StatisticService(
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            IRepository<User> userRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<List<StatisticCountDto>> GetTotalOrdersAsync(TimeScope timeScope, DateTime? chosenDate)
        {
            var date = chosenDate ?? DateTime.UtcNow;
            return await GetStatisticsAsync(_orderRepository, timeScope, date, x => x.CreationDate);
        }

        public async Task<List<StatisticCountDto>> GetTotalProductsAsync(TimeScope timeScope, DateTime? chosenDate)
        {
            var date = chosenDate ?? DateTime.UtcNow;
            return await GetStatisticsAsync(_productRepository, timeScope, date, x => x.CreationDate);
        }

        public async Task<List<StatisticCountDto>> GetTotalCustomersAsync(TimeScope timeScope, DateTime? chosenDate)
        {
            var date = chosenDate ?? DateTime.UtcNow;
            return await GetStatisticsAsync(_userRepository, timeScope, date, x => x.CreationDate);
        }

        public Task<List<StatisticCountDto>> GetTotalRevenuesAsync(TimeScope timeScope, DateTime? chosenDate)
        {
            var date = chosenDate ?? DateTime.UtcNow;
            var dummyResult = new List<StatisticCountDto>();

            if (timeScope == TimeScope.Year)
            {
                for (int month = 1; month <= 12; month++)
                {
                    dummyResult.Add(new StatisticCountDto
                    {
                        TimePeriod = new DateTime(date.Year, month, 1).ToString(StatisticConstants.YearMonthFormat),
                        Count = month * 10 // fake value
                    });
                }
            }
            else if (timeScope == TimeScope.Month)
            {
                int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    dummyResult.Add(new StatisticCountDto
                    {
                        TimePeriod = new DateTime(date.Year, date.Month, day).ToString(StatisticConstants.FullDateFormat),
                        Count = day % 5 * 3 // fake value
                    });
                }
            }
            else // Week
            {
                var startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
                for (int i = 0; i < 7; i++)
                {
                    var currentDate = startOfWeek.AddDays(i);
                    dummyResult.Add(new StatisticCountDto
                    {
                        TimePeriod = currentDate.ToString(StatisticConstants.FullDateFormat),
                        Count = i * 7 // fake value
                    });
                }
            }

            return Task.FromResult(dummyResult);
        }

        public async Task<TopNProductStatisticDto> GetTopNProductsAsync(int topN)
        {
            var productStats = await _orderRepository.Get()
                .Include(x => x.OrderDetails).ThenInclude(x => x.ProductVariant).ThenInclude(x => x.Product)
                .Where(o => o.IsDeleted == false && o.OrderDetails != null)
                .SelectMany(o => o.OrderDetails)
                .GroupBy(d => new { d.ProductVariant.Product.Name })
                .Select(g => new ProductStatisticItemDto
                {
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(topN)
                .ToListAsync();

            return new TopNProductStatisticDto
            {
                ProductItems = productStats
            };
        }

        public async Task<TopNCustomerStatisticDto> GetTopNCustomersAsync(int topN)
        {
            var customerStats = await _orderRepository.Get().Include(x => x.Customer).Include(x => x.OrderDetails)
                .Where(o => o.IsDeleted == false && o.Customer != null)
                .GroupBy(o => o.Customer!.FullName)
                .Select(g => new CustomerStatisticItemDto
                {
                    CustomerName = g.Key,
                    TotalOrders = g.Count(),
                    TotalSpending = g.Sum(x => x.OrderDetails!.Sum(d => d.Quantity * d.Price))
                })
                .OrderByDescending(x => x.TotalSpending)
                .Take(topN)
                .ToListAsync();

            return new TopNCustomerStatisticDto
            {
                CustomerItems = customerStats
            };
        }

        //TODO Remove Dummy
        public Task<object> GetCurrentStatisticsAsync()
        {
            var dummy = new
            {

                RevenueStatisticResponse = new
                {
                    TotalRevenue = 1850000000,
                    TotalRevenueLastMonth = 1580000000,
                    PercentageCompareLastMonth = 17.1
                },
                OrderStatisticResponse = new
                {
                    TotalOrders = 34,
                    TotalOrdersLastMonth = 28,
                    PercentageCompareLastMonth = 19.7
                },
                UserStatisticResponse = new
                {
                    TotalUsers = 78,
                    TotalUsersLastMonth = 73,
                    PercentageCompareLastMonth = 6.5
                }
            };
            return Task.FromResult((object)dummy);
        }

        private async Task<List<StatisticCountDto>> GetStatisticsAsync<TEntity>(
            IRepository<TEntity> repo,
            TimeScope timeScope,
            DateTime date,
            Expression<Func<TEntity, DateTimeOffset?>> creationSelector)
            where TEntity : class
        {
            return timeScope switch
            {
                TimeScope.Year => await GetYearlyStatistics(repo, date.Year, creationSelector),
                TimeScope.Month => await GetMonthlyStatistics(repo, date.Year, date.Month, creationSelector),
                TimeScope.Week => await GetWeeklyStatistics(repo, date, creationSelector),
                _ => await GetYearlyStatistics(repo, date.Year, creationSelector)
            };
        }

        private async Task<List<StatisticCountDto>> GetYearlyStatistics<TEntity>(
            IRepository<TEntity> repo,
            int year,
            Expression<Func<TEntity, DateTimeOffset?>> creationSelector)
            where TEntity : class
        {
            var stats = await repo.Get()
                .Where(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).HasValue &&
                            EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Year == year)
                .GroupBy(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Month)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = new DateTime(year, g.Key, 1).ToString(StatisticConstants.YearMonthFormat),
                    Count = g.Count()
                })
                .ToListAsync();

            foreach (var month in StatisticConstants.AllMonths)
            {
                var key = new DateTime(year, month, 1).ToString(StatisticConstants.YearMonthFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                {
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
                }
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
        }

        private async Task<List<StatisticCountDto>> GetMonthlyStatistics<TEntity>(
            IRepository<TEntity> repo,
            int year,
            int month,
            Expression<Func<TEntity, DateTimeOffset?>> creationSelector)
            where TEntity : class
        {
            var stats = await repo.Get()
                .Where(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).HasValue &&
                            EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Year == year &&
                            EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Month == month)
                .GroupBy(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Day)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = new DateTime(year, month, g.Key).ToString(StatisticConstants.FullDateFormat),
                    Count = g.Count()
                })
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            foreach (var day in Enumerable.Range(1, daysInMonth))
            {
                var key = new DateTime(year, month, day).ToString(StatisticConstants.FullDateFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                {
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
                }
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
        }

        private async Task<List<StatisticCountDto>> GetWeeklyStatistics<TEntity>(
            IRepository<TEntity> repo,
            DateTime chosenDate,
            Expression<Func<TEntity, DateTimeOffset?>> creationSelector)
            where TEntity : class
        {
            var startOfWeek = chosenDate.Date.AddDays(-(int)chosenDate.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(7);

            var stats = await repo.Get()
                .Where(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).HasValue &&
                            EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Date >= startOfWeek &&
                            EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Date < endOfWeek)
                .GroupBy(e => EF.Property<DateTimeOffset?>(e, nameof(BaseEntity.CreationDate)).Value.Date)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = g.Key.ToString(StatisticConstants.FullDateFormat),
                    Count = g.Count()
                })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var key = date.ToString(StatisticConstants.FullDateFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                {
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
                }
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
        }
    }
}
