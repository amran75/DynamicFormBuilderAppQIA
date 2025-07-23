using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilderAppQIA.DTOs;
using DynamicFormBuilderAppQIA.Repositories;
using Microsoft.SqlServer.Server;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DynamicFormBuilderAppQIA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormApiController : ControllerBase
    {
        private readonly IFormRepository _formRepository;

        public FormApiController(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        [HttpGet("titles")]
        public async Task<IActionResult> GetFormTitles()
        {
            var forms = await _formRepository.GetAllFormsAsync();
            var formTitles = forms.Select(f => new FormTitleDTO
            {
                Id = f.Id,
                Title = f.Title
            }).ToList();

            return Ok(formTitles);
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            var forms = _formRepository.GetAllFormsAsync();
            return Ok(forms);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] FormSubmitDTO formDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formDto.Title) || formDto.Fields == null || !formDto.Fields.Any())
                {
                    return BadRequest(new
                    {
                        error = "Form title and at least one field are required."
                    });
                }

                FormDTO Obj = new FormDTO
                {
                    Title = formDto.Title,
                    CreatedAt = DateTime.UtcNow,
                    Fields = formDto.Fields.Select(f => new FormFieldDTO
                    {
                        Label = f.Label,
                        Options = f.Options,
                        SelectedOption = f.SelectedOption,
                        IsRequired = f.IsRequired
                    }).ToList()
                };

                var formId = await _formRepository.CreateFormAsync(Obj);
                // Insert each field
                foreach (var field in Obj.Fields)
                {
                    await _formRepository.CreateFormFieldAsync(field, formId);
                }

                return Ok(new
                {
                    message = "Form created successfully.",
                    formId = formId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while creating the form.", 
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var form = await _formRepository.GetFormByIdAsync(id);
            if (form == null)
            {
                return NotFound();
            }

            //  Map FormDTO to FormResponseDTO
            var response = new FormResponseDTO
            {
                Id = form.Id,
                Title = form.Title,
                Fields = form.Fields.Select(f => new FormFieldResponseDTO
                {
                    Label = f.Label,
                    IsRequired = f.IsRequired,
                    Options = f.Options,
                    SelectedOption = f.SelectedOption
                }).ToList()
            };

                return Ok(response);
        }

    }
}
