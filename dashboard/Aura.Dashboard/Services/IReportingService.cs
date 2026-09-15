using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;

namespace Aura.Dashboard.Services;

public interface IReportingService { Task<DashboardMetrics> GetAsync(); }
