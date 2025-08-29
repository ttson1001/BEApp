namespace BEAPI.Dtos.Statistic
{
    public class UserStatisticResponseDto
    {
        public int TotalUsers { get; set; }
        public int TotalUsersLastMonth { get; set; }
        public double PercentageCompareLastMonth { get; set; }
    }
}
