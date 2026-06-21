using MIS_Project_API.Models;

namespace MIS_Project_API.Interfaces
{
    public interface IKpiService
    {
        Task<double> CalculateKpiTimelinessAsync(int userId, int month, int year);
        Task<double> CalculateKpiEfficiencyAsync(int userId, int month, int year);
        Task<double> CalculateKpiCapacityAsync(int userId, int month, int year);
        Task<(bool Success, string Message, KpiReview? Data)> FinalizeKpiReviewAsync(int userId, int month, int year, double managerScore, string note);
    }
}
