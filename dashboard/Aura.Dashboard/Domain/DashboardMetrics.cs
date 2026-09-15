namespace Aura.Dashboard.Domain;

public sealed record DashboardMetrics(long TotalMedia, long Videos, long Images, long Audio, long Pending, long Approved, long Rejected, long TotalUsers, long VerifiedUsers, long BlockedUsers, long Reviewers, long Administrators);
