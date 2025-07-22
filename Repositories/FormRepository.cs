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