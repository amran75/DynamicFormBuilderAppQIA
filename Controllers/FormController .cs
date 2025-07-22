using DynamicFormBuilderAppQIA.Models;
using DynamicFormBuilderAppQIA.Repositories;
using Microsoft.AspNetCore.Mvc; 

namespace DynamicFormBuilderAppQIA.Controllers
{
    public class FormController : Controller
    {
        private readonly IFormRepository _formRepository;

        public FormController(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }
         
        public IActionResult Create()
        {
            return View(new FormModel());
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormModel form)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Insert the form
                    int formId = await _formRepository.CreateFormAsync(form);

                    // Insert each field
                    foreach (var field in form.Fields)
                    {
                        await _formRepository.CreateFormFieldAsync(field, formId);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log error
                    ModelState.AddModelError("", $"Error saving form: {ex.Message}");
                }
            }

            return View(form);
        }

         
        public async Task<IActionResult> Index()
        {
            try
            {
                var forms = await _formRepository.GetAllFormsAsync();
                return View(forms);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading forms: {ex.Message}";
                return View(new List<FormModel>());
            }
        }

        
        public async Task<IActionResult> Preview(int id)
        {
            try
            {
                var form = await _formRepository.GetFormByIdAsync(id);
                if (form == null)
                {
                    return NotFound();
                }

                return View(form);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading preview: {ex.Message}";
                return View(new FormModel());
            }
        }
    }
}