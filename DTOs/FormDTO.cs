using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilderAppQIA.DTOs
{
    public class FormDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Form title is required")]
        [StringLength(255)]
        [Display(Name = "Form Title")]
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } 

        public List<FormFieldDTO> Fields { get; set; } = new List<FormFieldDTO>();

    }
}
