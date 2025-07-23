using DynamicFormBuilderAppQIA.DTOs;

namespace DynamicFormBuilderAppQIA.Repositories
{
    public interface IFormRepository
    {
        Task<int> CreateFormAsync(FormDTO form);
        Task<FormDTO> GetFormByIdAsync(int id);
        Task<IEnumerable<FormDTO>> GetAllFormsAsync();
        Task CreateFormFieldAsync(FormFieldDTO field, int formId);
    }
}
