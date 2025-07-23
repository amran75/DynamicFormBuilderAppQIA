namespace DynamicFormBuilderAppQIA.DTOs
{
    public class FormFieldResponseDTO
    {
        public string Label { get; set; }
        public bool IsRequired { get; set; }
        public string Options { get; set; }
        public string SelectedOption { get; set; }
    }

    public class FormResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<FormFieldResponseDTO> Fields { get; set; }
    }

    public class FormSubmitDTO
    {
        public string Title { get; set; }
        public List<FormSubmitFieldDTO> Fields { get; set; }
    }

    public class FormSubmitFieldDTO
    {
        public string Label { get; set; }
        public  string  Options { get; set; } 
        public string SelectedOption { get; set; }
        public bool IsRequired { get; set; }
    }
}
