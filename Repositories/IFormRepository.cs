using DynamicFormBuilderAppQIA.Models;

namespace DynamicFormBuilderAppQIA.Repositories
{
    public interface IFormRepository
    {
        Task<int> CreateFormAsync(FormModel form);
        Task<FormModel> GetFormByIdAsync(int id);
        Task<IEnumerable<FormModel>> GetAllFormsAsync();
        Task CreateFormFieldAsync(FormFieldModel field, int formId);
    }
}
