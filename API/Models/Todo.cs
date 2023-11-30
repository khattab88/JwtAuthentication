using Microsoft.AspNetCore.Mvc.RazorPages;

namespace API.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
    }
}
