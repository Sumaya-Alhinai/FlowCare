namespace FlowCare.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Results { get; set; } = new();
        public int Total { get; set; }
    }
}