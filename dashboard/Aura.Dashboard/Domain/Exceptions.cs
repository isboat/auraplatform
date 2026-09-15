namespace Aura.Dashboard.Domain;
public sealed class DashboardRuleException(string message) : Exception(message);
public sealed class ConcurrencyException(string message) : Exception(message);
