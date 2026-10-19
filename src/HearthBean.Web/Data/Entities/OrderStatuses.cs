namespace HearthBean.Web.Data.Entities;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Preparing = "Preparing";
    public const string Ready = "Ready";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Pending, Confirmed, Preparing, Ready, Completed, Cancelled];

    public static bool CanTransition(string current, string next) => current switch
    {
        Pending => next is Confirmed or Cancelled,
        Confirmed => next is Preparing or Cancelled,
        Preparing => next == Ready,
        Ready => next == Completed,
        _ => false
    };
}