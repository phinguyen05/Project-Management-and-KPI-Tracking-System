using System.Threading.Tasks;

namespace MIS_Project_API.Interfaces
{
    public interface IBackgroundJobService
    {
        Task CheckAndAlertOverdueTasksAsync();
        Task AutoApprovePendingLogTimesAsync();
    }
}
