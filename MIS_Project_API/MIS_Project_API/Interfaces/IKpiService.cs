namespace MIS_Project_API.Interfaces
{
    public interface IKpiService
    {
        Task<double> CalculateKpiTimelinessAsync(int userId, int month, int year);
    }
}
