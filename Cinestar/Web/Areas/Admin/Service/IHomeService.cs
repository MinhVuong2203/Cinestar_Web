namespace Web.Areas.Admin.Service
{
    public interface IHomeService
    {
        int GetTotalMovies();
        int GetNowShowingMovies();
        int GetComingSoon();
        double GetAverageDuration();

        Task<decimal> GetMonthlyRevenue(DateTime from, DateTime to);
        Task<double> GetRevenueGrowthPercentage(DateTime currentFrom, DateTime currentTo);
    }
}