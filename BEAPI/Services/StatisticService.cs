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

        public async Task<List<StatisticCountDto>> GetTotalRevenuesAsync(TimeScope timeScope, DateTime? chosenDate)
        {
            var date = chosenDate ?? DateTime.UtcNow;

            return timeScope switch
            {
                TimeScope.Year => await GetYearlyRevenue(date.Year),
                TimeScope.Month => await GetMonthlyRevenue(date.Year, date.Month),
                TimeScope.Week => await GetWeeklyRevenue(date),
                _ => await GetYearlyRevenue(date.Year)
            };
        }

        private async Task<List<StatisticCountDto>> GetYearlyRevenue(int year)
        {
            var stats = await _orderRepository.Get()
                .Where(o => o.CreationDate.HasValue && o.CreationDate.Value.Year == year && !o.IsDeleted)
                .GroupBy(o => o.CreationDate!.Value.Month)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = new DateTime(year, g.Key, 1).ToString(StatisticConstants.YearMonthFormat),
                    Count = g.Sum(x => (int)x.TotalPrice)
                })
                .ToListAsync();

            foreach (var month in Enumerable.Range(1, 12))
            {
                var key = new DateTime(year, month, 1).ToString(StatisticConstants.YearMonthFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
        }

        private async Task<List<StatisticCountDto>> GetMonthlyRevenue(int year, int month)
        {
            var stats = await _orderRepository.Get()
                .Where(o => o.CreationDate.HasValue &&
                            o.CreationDate.Value.Year == year &&
                            o.CreationDate.Value.Month == month &&
                            !o.IsDeleted)
                .GroupBy(o => o.CreationDate!.Value.Day)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = new DateTime(year, month, g.Key).ToString(StatisticConstants.FullDateFormat),
                    Count = g.Sum(x => (int)x.TotalPrice)
                })
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            foreach (var day in Enumerable.Range(1, daysInMonth))
            {
                var key = new DateTime(year, month, day).ToString(StatisticConstants.FullDateFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
        }

        private async Task<List<StatisticCountDto>> GetWeeklyRevenue(DateTime chosenDate)
        {
            var startOfWeek = chosenDate.Date.AddDays(-(int)chosenDate.DayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(7);

            var stats = await _orderRepository.Get()
                .Where(o => o.CreationDate.HasValue &&
                            o.CreationDate.Value.Date >= startOfWeek &&
                            o.CreationDate.Value.Date < endOfWeek &&
                            !o.IsDeleted)
                .GroupBy(o => o.CreationDate!.Value.Date)
                .Select(g => new StatisticCountDto
                {
                    TimePeriod = g.Key.ToString(StatisticConstants.FullDateFormat),
                    Count = g.Sum(x => (int)x.TotalPrice)
                })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var key = date.ToString(StatisticConstants.FullDateFormat);
                if (!stats.Any(s => s.TimePeriod == key))
                    stats.Add(new StatisticCountDto { TimePeriod = key, Count = 0 });
            }

            return stats.OrderBy(s => s.TimePeriod).ToList();
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

        public async Task<StatisticResponseDto> GetCurrentStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
            var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

            // Query database
            var totalRevenue = await _orderRepository.Get()
                .Where(o => o.CreationDate >= firstDayThisMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var totalRevenueLastMonth = await _orderRepository.Get()
                .Where(o => o.CreationDate >= firstDayLastMonth && o.CreationDate <= lastDayLastMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var totalOrders = await _orderRepository.Get()
                .CountAsync(o => o.CreationDate >= firstDayThisMonth);

            var totalOrdersLastMonth = await _orderRepository.Get()
                .CountAsync(o => o.CreationDate >= firstDayLastMonth && o.CreationDate <= lastDayLastMonth);

            var totalUsers = await _userRepository.Get()
                .CountAsync(u => u.CreationDate >= firstDayThisMonth);

            var totalUsersLastMonth = await _userRepository.Get()
                .CountAsync(u => u.CreationDate >= firstDayLastMonth && u.CreationDate <= lastDayLastMonth);

            return new StatisticResponseDto
            {
                RevenueStatisticResponse = new RevenueStatisticResponseDto
                {
                    TotalRevenue = totalRevenue,
                    TotalRevenueLastMonth = totalRevenueLastMonth,
                    PercentageCompareLastMonth = totalRevenueLastMonth == 0 ? 100 :
                        Math.Round(((double)(totalRevenue - totalRevenueLastMonth) / (double)totalRevenueLastMonth) * 100, 2)
                },
                OrderStatisticResponse = new OrderStatisticResponseDto
                {
                    TotalOrders = totalOrders,
                    TotalOrdersLastMonth = totalOrdersLastMonth,
                    PercentageCompareLastMonth = totalOrdersLastMonth == 0 ? 100 :
                        Math.Round(((double)(totalOrders - totalOrdersLastMonth) / totalOrdersLastMonth) * 100, 2)
                },
                UserStatisticResponse = new UserStatisticResponseDto
                {
                    TotalUsers = totalUsers,
                    TotalUsersLastMonth = totalUsersLastMonth,
                    PercentageCompareLastMonth = totalUsersLastMonth == 0 ? 100 :
                        Math.Round(((double)(totalUsers - totalUsersLastMonth) / totalUsersLastMonth) * 100, 2)
                }
            };
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
