using System.ComponentModel.DataAnnotations;

namespace FlowCare.Models
{
    public class Config
    {
        [Key]
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}