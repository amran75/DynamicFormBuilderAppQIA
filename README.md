# Dynamic Form Builder Application
 
## Project Architecture:

**Controllers**: Contains the API and MVC controllers for handling requests.

**Repositories**: Contains the repository interfaces and implementations for data access.

**Views**: Contains the Razor views for the MVC application.

**Data**: Contains the DbContext and database connection logic.

**Models**: Contains the data models for forms and form fields.


## Features:
- Create dynamic forms with multiple fields. 
- Preview forms after submission.


## Project Architecture:
<img width="1916" height="1040" alt="image" src="https://github.com/user-attachments/assets/a7cb457d-590f-44b6-a3ef-255d3d6f1531" />

### Form Controller 

```
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
```
### Models

```
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Form}/{action=Index}/{id?}");

app.Run();

```
**Index View**
```
@model IEnumerable<DynamicFormBuilderAppQIA.Models.FormModel>

@{
    ViewData["Title"] = "Form History";
}

<div class="container mt-5">

    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Dynamic Forms History</h2>
        <a asp-action="Create" class="btn btn-success">
            <i class="fas fa-plus me-1"></i> Create New Form
        </a>
    </div>

    @if (!Model.Any())
    {
        <div class="alert alert-info shadow-sm">
            <strong>No forms available.</strong> You can
            <a asp-action="Create" class="alert-link">create your first form here</a>.
        </div>
    }
    else
    {
        <div class="table-responsive shadow-sm rounded">
            <table class="table table-hover table-bordered align-middle">
                <thead class="table-info">
                    <tr>
                        <th scope="col">Form Title</th>
                        <th scope="col" class="text-center" style="width: 150px;">Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var form in Model)
                    {
                        <tr>
                            <td>@form.Title</td>
                            <td class="text-center">
                                <a asp-action="Preview" asp-route-id="@form.Id" class="btn btn-sm btn-info">
                                    <i class="fas fa-eye me-1"></i> Preview
                                </a>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

</div>

```
**Create View**
```
@model DynamicFormBuilderAppQIA.Models.FormModel

@{
    ViewData["Title"] = "Create Dynamic Form";
}

@section Styles {
    <link href="~/css/custom.css" rel="stylesheet" />
}

<div class="container mt-5">

    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Create Dynamic Form</h2>
        <a asp-action="Index" class="btn btn-success">
            <i class="fas fa-list me-1"></i> View All Forms
        </a>
    </div>

    <form asp-action="Create" id="form-builder">
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>

        <!-- Form Title -->
        <div class="form-group mb-4">
            <label asp-for="Title" class="form-label fw-bold">Form Title</label>
            <input asp-for="Title" class="form-control" placeholder="e.g., Customer Survey" />
            <span asp-validation-for="Title" class="text-danger"></span>
        </div>

        <!-- Dynamic Fields Container -->
        <div id="fields-container"></div>

        <!-- Action Buttons -->
        <div class="form-group d-flex justify-content-between mt-4">
            <button type="button" id="add-field" class="btn btn-outline-primary">
                <i class="fas fa-plus me-1"></i> Add More
            </button>
            <div>
                <button type="reset" class="btn btn-outline-secondary me-2">
                    <i class="fas fa-times"></i> Clear
                </button>
                <button type="submit" class="btn btn-success">
                    <i class="fas fa-save"></i> Save Form
                </button>
            </div>
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
});

```
**Preview View**
```
@model DynamicFormBuilderAppQIA.Models.FormModel

@{
    ViewData["Title"] = "Preview: " + Model.Title;
}

<div class="container py-5">
    <div class="card shadow-lg border-0">
        <div class="card-header bg-success text-white">
            <h4 class="mb-0">@Model.Title</h4>
        </div>

        <div class="card-body">
            <form>
                @foreach (var field in Model.Fields)
                {
                    <div class="mb-4">
                        <label class="form-label fw-semibold">
                            @field.Label
                            @if (field.IsRequired)
                            {
                                <span class="text-danger">*</span>
                            }
                        </label>

                        <select class="form-select">
                            @foreach (var option in field.Options.Split(','))
                            {
                                <option value="@option" selected="@(option == field.SelectedOption)">
                                    @option
                                </option>
                            }
                        </select>
                    </div>
                }
            </form>
        </div>

        <div class="card-footer text-end bg-light">
            <a asp-action="Index" class="btn btn-outline-secondary">
                <i class="fas fa-arrow-left me-1"></i> Back to List
            </a>
        </div>
    </div>
</div>

```



**Run The Application**

<img width="1912" height="1036" alt="image" src="https://github.com/user-attachments/assets/2603f728-249c-42f5-b394-7828278fec26" />



**Step 1: Provide a From Title** 
 <img width="1688" height="560" alt="image" src="https://github.com/user-attachments/assets/53faea02-7855-426c-99e5-d1be19327ac4" />
 <img width="1474" height="449" alt="image" src="https://github.com/user-attachments/assets/99284b58-7c35-4a7f-8963-258d43bd4cf1" />


**Step 2: Click Add more Dropdown Item and Save.**
 <img width="1496" height="986" alt="image" src="https://github.com/user-attachments/assets/2a6f694c-2c3d-40aa-80cf-894848030700" />

**Step 3: Saved Form Appeared in the list with Preview Button.**
<img width="1660" height="1039" alt="image" src="https://github.com/user-attachments/assets/c3b71d83-921d-473d-9d6c-125a5be1fb4b" />

**Step 4: Preview Page:**
<img width="1662" height="1030" alt="image" src="https://github.com/user-attachments/assets/31d5138e-4412-459e-948f-9cd7b1d7ef7e" />

**More forms in the List**
<img width="1107" height="611" alt="image" src="https://github.com/user-attachments/assets/fa2a68f0-eb77-46c4-b955-48996208537a" />
<img width="1119" height="360" alt="image" src="https://github.com/user-attachments/assets/9a812f8b-97e3-4341-8c71-2c80085dbfe2" />

