# Dynamic Form Builder Application with API Integration
 
## Project Architecture:
**Controllers**: Contains the API and MVC controllers for handling requests.

**DTOs**: Contains Data Transfer Objects for form submission and response.

**Repositories**: Contains the repository interfaces and implementations for data access.

**Views**: Contains the views for the MVC application.

**Data**: Contains the DbContext and database connection logic.
 
<img width="1611" height="802" alt="image" src="https://github.com/user-attachments/assets/1117022b-a7f2-4b1c-95b5-d3c1509310c3" />


## Features:
- Create dynamic forms with multiple fields. 
- Preview forms after submission.

### Form API Controller 

```
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

```

### Form Controller 

```
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

```


### DTOS

```
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


namespace DynamicFormBuilderAppQIA.DTOs
{
    public class FormTitleDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}


```

### Repositories 

**FormRepository.cs**
```
using DynamicFormBuilderAppQIA.Data;
using DynamicFormBuilderAppQIA.Models;
using Microsoft.Data.SqlClient; 
using System.Data; 

namespace DynamicFormBuilderAppQIA.Repositories
{
    public class FormRepository : IFormRepository
    {
        private readonly DbContext _dbContext;

        public FormRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateFormAsync(FormModel form)
        {
            string sql = @"
                INSERT INTO Forms (Title, CreatedAt) 
                OUTPUT INSERTED.Id
                VALUES (@Title, @CreatedAt)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Title", form.Title),
                new SqlParameter("@CreatedAt", DateTime.Now)
            };

            return await _dbContext.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task CreateFormFieldAsync(FormFieldModel field, int formId)
        {
            string sql = @"
                INSERT INTO FormFields (FormId, Label, Options, SelectedOption, IsRequired)
                VALUES (@FormId, @Label, @Options, @SelectedOption, @IsRequired)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@FormId", formId),
                new SqlParameter("@Label", field.Label),
                new SqlParameter("@Options", field.Options),
                new SqlParameter("@SelectedOption", field.SelectedOption),
                new SqlParameter("@IsRequired", field.IsRequired)
            };

            await _dbContext.ExecuteCommandAsync(sql, parameters);
        }

        public async Task<FormModel> GetFormByIdAsync(int id)
        {
            string formSql = "SELECT * FROM Forms WHERE Id = @Id";
            var formParams = new SqlParameter[] { new SqlParameter("@Id", id) };

            DataTable formTable = await _dbContext.QueryAsync(formSql, formParams);
            if (formTable.Rows.Count == 0) return null;

            DataRow formRow = formTable.Rows[0];
            var form = new FormModel
            {
                Id = Convert.ToInt32(formRow["Id"]),
                Title = formRow["Title"].ToString(),
                CreatedAt = Convert.ToDateTime(formRow["CreatedAt"])
            };

            // Get the fields for this form
            string fieldsSql = "SELECT * FROM FormFields WHERE FormId = @FormId";
            var fieldsParams = new SqlParameter[] { new SqlParameter("@FormId", id) };
            DataTable fieldsTable = await _dbContext.QueryAsync(fieldsSql, fieldsParams);

            form.Fields = new List<FormFieldModel>();
            foreach (DataRow fieldRow in fieldsTable.Rows)
            {
                form.Fields.Add(new FormFieldModel
                {
                    Id = Convert.ToInt32(fieldRow["Id"]),
                    Label = fieldRow["Label"].ToString(),
                    Options = fieldRow["Options"].ToString(),
                    SelectedOption = fieldRow["SelectedOption"].ToString(),
                    IsRequired = Convert.ToBoolean(fieldRow["IsRequired"])
                });
            }

            return form;
        }

        public async Task<IEnumerable<FormModel>> GetAllFormsAsync()
        {
            string sql = "SELECT * FROM Forms ORDER BY CreatedAt DESC";
            DataTable formsTable = await _dbContext.QueryAsync(sql);

            var forms = new List<FormModel>();
            foreach (DataRow row in formsTable.Rows)
            {
                forms.Add(new FormModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = row["Title"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                });
            }

            return forms;
        }
    }
}
```
**IFormRepository.cs**
```
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

```
**IUnitOfWork.cs**
```
namespace DynamicFormBuilderAppQIA.Repositories
{
    public interface IUnitOfWork
    {
        public interface IUnitOfWork : IDisposable
        {
            IFormRepository Forms { get; }
            Task<bool> SaveChangesAsync();
            Task BeginTransactionAsync();
            Task CommitTransactionAsync();
            Task RollbackTransactionAsync();
        }
    }
}

```
**UnitOfWork.cs**

```
using DynamicFormBuilderAppQIA.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DynamicFormBuilderAppQIA.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private bool _disposed = false;
        private SqlTransaction _transaction;

        public IFormRepository Forms { get; }

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            Forms = new FormRepository(dbContext);
        }

        public async Task BeginTransactionAsync()
        {
            if (_dbContext.Connection.State != System.Data.ConnectionState.Open)
            {
                await _dbContext.Connection.OpenAsync();
            }
            _transaction = _dbContext.Connection.BeginTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await Task.Run(() => _transaction.Commit());
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await Task.Run(() => _transaction.Rollback());
                _transaction = null;
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            // For ADO.NET, we don't have a change tracker
            // This method exists for interface consistency
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _dbContext.Connection?.Close();
                    _dbContext.Connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
```
**Program.cs**
```
using DynamicFormBuilderAppQIA.Data;
using DynamicFormBuilderAppQIA.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<DbContext>(provider =>
    new DbContext(connectionString));

// Register Unit of Work and Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();
app.UseCors();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); 
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

 

app.UseAuthorization();
app.MapControllers();  

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Form}/{action=Index}/{id?}");

app.Run();


```


### Data Layer

**DbContext.cs**
```
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DynamicFormBuilderAppQIA.Data
{
    public class DbContext : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private bool _disposed = false;

        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
                return _connection;
            }
        }

        public DbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task OpenConnectionAsync()
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }
        }

        public async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }

        public async Task<int> ExecuteCommandAsync(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await OpenConnectionAsync();

            using var command = new SqlCommand(sql, Connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            var result = await command.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
```


### Views

**Index View**
```
@{
    ViewData["Title"] = "Forms";
}


<div class="d-flex justify-content-between align-items-center mb-4">
    <h2 class="mb-0">Dynamic Forms History Using API</h2>
    <button class="btn btn-success create-btn">
        <i class="fas fa-plus me-1"></i> Create New Form
    </button>
</div>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Title</th>
            <th class="text-center">Actions</th>
        </tr>
    </thead>
    <tbody id="form-table-body">
        <!-- Loaded by JavaScript -->
    </tbody>
</table>

@section Scripts {
    <script>
        $(document).ready(function () {
            // Fetch all forms using the API
            $.get("/api/FormApi/titles", function (data) {
                if (!data || data.length === 0) {
                    $('#form-table-body').html('<tr><td colspan="2"><div class="alert alert-info">No forms available. <a href="/Form/Create">Create one</a>.</div></td></tr>');
                } else {
                    let rows = '';
                    data.forEach(form => {
                        rows += `
                            <tr>
                                <td>${form.title}</td>
                                <td class="text-center">
                                    <button class="btn btn-sm btn-info preview-btn" data-id="${form.id}">
                                        <i class="fas fa-eye me-1"></i> Preview
                                    </button>
                                </td>
                            </tr>`;
                    });
                    $('#form-table-body').html(rows);
                }
            });

             
            $(document).on('click', '.preview-btn', function () {
                const id = $(this).data("id");
                window.location.href = `/Form/Preview?id=${id}`;
            });

            $(document).on('click', '.create-btn', function () {
                window.location.href = `/Form/Create`;
            });
        });
    </script>
}


```
**Create View**
```
@{
    ViewData["Title"] = "Create Dynamic Form";
}

<div class="container mt-5">
 
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Create Dynamic Form Using API</h2>
        

        <button class="btn btn-success view-btn">
            <i class="fas fa-list me-1"></i> View All Forms
        </button>
    </div>

    <form id="form-builder">
        <!-- Title -->
        <div class="form-group mb-3">
            <label for="form-title" class="form-label">Form Title</label>
            <input type="text" id="Title" class="form-control" placeholder="e.g., Customer Feedback" required />
        </div>

        <!-- Fields container -->
        <div id="fields-container"></div>

        <!-- Add Field Button -->
        <div class="form-group mt-3">
            <button type="button" id="add-field" class="btn btn-outline-primary">
                <i class="fas fa-plus"></i> Add Field
            </button>
        </div>

        <!-- Submit Button -->
        <div class="form-group mt-4">
            <button type="submit" class="btn btn-success">
                <i class="fas fa-save"></i> Submit Form
            </button>
        </div>
    </form>
</div>
 
@section Scripts {
    <script src="~/js/form/create.js"></script>
} 
```
**create.js**
```
$(document).ready(function () {
    let fieldIndex = 0;  

    $("#add-field").click(function () {
        if (!$("#Title").valid()) {
            $("#title-validation-placeholder").removeClass("d-none");
            $("#Title").addClass('is-invalid');
            $(".form-title-section").css('animation', 'shake 0.5s');
            setTimeout(() => {
                $(".form-title-section").css('animation', '');
            }, 500);
            return;
        }

        $("#title-validation-placeholder").addClass("d-none");

        const newFieldHtml = `
            <div class="card mb-4 field-group p-3">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <div class="field-header">Label ${fieldIndex + 1}</div>
                        <button type="button" class="btn btn-sm btn-outline-danger remove-field">
                            <i class="fas fa-trash-alt"></i> Remove
                        </button>
                    </div>

                    <div class="form-group mb-3">
                        <label>Label Name</label>
                        <input name="Fields[${fieldIndex}].Label" class="form-control" placeholder="e.g., Select Country" />
                    </div>

                    <div class="form-group mb-3">
                        <label>Options</label>
                        <select name="Fields[${fieldIndex}].SelectedOption" class="form-control">
                            <option value="">Select Item</option>
                            <option value="Option1">Option 1</option>
                            <option value="Option2">Option 2</option>
                            <option value="Option3">Option 3</option>
                        </select>
                        <input type="hidden" name="Fields[${fieldIndex}].Options" value="Option1,Option2,Option3" />
                    </div>

                    <div class="form-check">
                        <input type="checkbox" name="Fields[${fieldIndex}].IsRequired" class="form-check-input" value="true" />
                        <input type="hidden" name="Fields[${fieldIndex}].IsRequired" value="false" />
                        <label class="form-check-label">
                            Required Field <span class="text-danger">*</span>
                        </label>
                    </div>
                </div>
            </div>`;

        $("#fields-container").append(newFieldHtml);
        fieldIndex++;
    });

    $(document).on("click", ".remove-field", function () {
        $(this).closest(".field-group").remove();
    });

    $("#form-builder").submit(function (e) {
        e.preventDefault();

        const formData = {
            Title: $("#Title").val(),
            Fields: []
        };

        $(".field-group").each(function () {
            const idx = $(this).index();
            formData.Fields.push({
                Label: $(this).find(`input[name="Fields[${idx}].Label"]`).val(),
                Options: $(this).find(`input[name="Fields[${idx}].Options"]`).val(),
                SelectedOption: $(this).find(`select[name="Fields[${idx}].SelectedOption"]`).val(),
                IsRequired: $(this).find(`input[name="Fields[${idx}].IsRequired"]:checked`).length > 0
            });
        });

        $.ajax({
            url: "/api/FormApi/Create",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function () {
                alert("Form created successfully");
                window.location.href = "/Form/Index";
            },
            error: function () {
                alert("Error creating form");
            }
        });
    });


    $(document).on('click', '.view-btn', function () {
        window.location.href = `/Form`;
    });


});


```
**Preview View**
```
@{
    ViewData["Title"] = "Form Preview";
}

<div class="container py-5">
    <div class="card shadow-lg border-0" id="formCard" style="display: none;">
        <div class="card-header bg-success text-white">
            <h4 class="mb-0" id="formTitle"></h4>
        </div>

        <div class="card-body">
            <form id="dynamicForm"></form>
        </div>

        <div class="card-footer text-end bg-light"> 
            <button class="btn btn-outline-secondary view-btn">
                <i class="fas fa-list me-1"></i> View All Forms
            </button>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function () {
            // Get formId from query string or URL
            const urlParams = new URLSearchParams(window.location.search);
            const formId = urlParams.get('id'); // ?id=123

            if (!formId) {
                alert("No form ID provided.");
                return;
            }

            $.get(`/api/FormApi/${formId}`, function (data) {
                if (!data) {
                    alert("Form not found");
                    return;
                } 

                const $form = $('#dynamicForm');
                $form.empty();

                data.fields.forEach(field => {
                    const isRequired = field.isRequired ? '<span class="text-danger">*</span>' : '';
                    const $wrapper = $('<div class="mb-4"></div>');
                    const $label = $(`<label class="form-label fw-semibold">${field.label} ${isRequired}</label>`);
                    const $select = $('<select class="form-select"></select>');

                    field.options.split(',').forEach(option => {
                        const selected = option === field.selectedOption ? 'selected' : '';
                        $select.append(`<option value="${option}" ${selected}>${option}</option>`);
                    });

                    $wrapper.append($label).append($select);
                    $form.append($wrapper);
                });

                 $('#formTitle').text(data.title);

                $('#formCard').show();
            });

             $(document).on('click', '.view-btn', function () {
                window.location.href = `/Form`;
            });
            
        });

       
    </script>
}


```



### Snapshot

 <img width="1573" height="1030" alt="image" src="https://github.com/user-attachments/assets/5a0d4273-173f-42b7-a5b3-48a0916a1e2e" />


**Step 1: Provide a From Title** 
 <img width="1576" height="557" alt="image" src="https://github.com/user-attachments/assets/b4200eb4-f913-41b2-afa1-a87d9be3879d" />
<img width="1347" height="956" alt="image" src="https://github.com/user-attachments/assets/92f22c16-8755-45c2-ae20-e630f027ddd1" />

**Step 2: Click Add more Dropdown Item and Save.**
<img width="1347" height="956" alt="image" src="https://github.com/user-attachments/assets/92f22c16-8755-45c2-ae20-e630f027ddd1" />

 <img width="1596" height="835" alt="image" src="https://github.com/user-attachments/assets/ef51fed3-aaef-4fc8-a2b8-448286502c90" />

**Step 3: Saved Form Appeared in the list with Preview Button.**
 <img width="1673" height="1044" alt="image" src="https://github.com/user-attachments/assets/083b3a75-5851-40cb-8128-c5a1ce3932bb" />

**Step 4: Preview Page:**
 <img width="1665" height="1034" alt="image" src="https://github.com/user-attachments/assets/db928f24-7950-4f94-b953-f8143c61fd7e" />

**More forms in the List**
 
<img width="1593" height="554" alt="image" src="https://github.com/user-attachments/assets/195e6ee4-be2a-40c1-b7ca-2c91ea10ec5c" />

 
