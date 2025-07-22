using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilderAppQIA.Models
{
    public class FormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Form title is required")]
        [StringLength(255)]
        [Display(Name = "Form Title")]
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<FormFieldModel> Fields { get; set; } = new List<FormFieldModel>();

    }
}
