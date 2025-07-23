using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilderAppQIA.Repositories;

namespace DynamicFormBuilderAppQIA.Controllers
{
    public class FormController : Controller
    {
        private readonly IFormRepository _formRepository;

        public FormController(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        public IActionResult Index()
        {
            return View(); 
        }

        public IActionResult Create()
        {
            return View(); 
        }

        public IActionResult Preview()
        {
            return View(); 
        }
    }
}
