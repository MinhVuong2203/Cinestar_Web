namespace Web.Areas.Admin.Service
{
    public interface IHomeService
    {
        int GetTotalMovies();
        int GetNowShowingMovies();
        int GetComingSoon();
        double GetAverageDuration();
    }
}