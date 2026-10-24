
namespace Nalogovaya
{
    public static class TaskDateValidator
    {
        public static bool IsValid(DateTime? dueDate)
        {
            return dueDate == null ||
                   dueDate.Value.Date >= DateTime.Today;
        }
    }
}
