using System.ComponentModel.DataAnnotations;

namespace Municipality_Application.Models
{
    public class EventSearchFrequency
    {
        [Key]
        public string SearchTerm { get; set; } = string.Empty;
        public int Frequency { get; set; }
    }
}