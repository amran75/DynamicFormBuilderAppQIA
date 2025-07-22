using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilderAppQIA.Models
{
    public class FormFieldModel
    {
        public int Id { get; set; }
        public int FormId { get; set; }

        [Required(ErrorMessage = "Field label is required")]
        [StringLength(100)]
        public string Label { get; set; }
        [Required]
        public string Options { get; set; }  // comma separated "Option1,Option2,Option3"
        public string SelectedOption { get; set; } 

        [DisplayName("Required Field")]
        public bool IsRequired { get; set; }
    }
}
