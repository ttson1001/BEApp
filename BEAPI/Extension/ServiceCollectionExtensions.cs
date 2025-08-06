using BEAPI.PaymentService.VnPay;
using BEAPI.Repositories;
using BEAPI.Services;
using BEAPI.Services.IServices;

namespace BEAPI.Extension
{ 
    public static class ServiceCollectionExtensions
    {
        public static void Register(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IValueService, ValueService>();
            services.AddScoped<IListOfValueService, ListOfValueService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddHttpClient<ILocationService, LocationService>();
            services.AddScoped<IinternalLocationService, InternalLocationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IStatisticService, StatisticService>();
            services.AddScoped<IProductPropertySerivce, ProductPropertyService>();
            services.AddScoped<IElderService, ElderService>();
            services.AddScoped<VNPayService>();
        }
    }
}
    