using DynamicFormBuilderAppQIA.Data;
using DynamicFormBuilderAppQIA.DTOs;
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

        public async Task<int> CreateFormAsync(FormDTO form)
        {
            string sql = @"
                INSERT INTO Forms (Title) 
                OUTPUT INSERTED.Id
                VALUES (@Title)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Title", form.Title) 
            };

            return await _dbContext.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task CreateFormFieldAsync(FormFieldDTO field, int formId)
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

        public async Task<FormDTO> GetFormByIdAsync(int id)
        {
            string formSql = "SELECT * FROM Forms WHERE Id = @Id";
            var formParams = new SqlParameter[] { new SqlParameter("@Id", id) };

            DataTable formTable = await _dbContext.QueryAsync(formSql, formParams);
            if (formTable.Rows.Count == 0) return null;

            DataRow formRow = formTable.Rows[0];
            var form = new FormDTO
            {
                Id = Convert.ToInt32(formRow["Id"]),
                Title = formRow["Title"].ToString(),
                CreatedAt = Convert.ToDateTime(formRow["CreatedAt"])
            };

            // Get the fields for this form
            string fieldsSql = "SELECT * FROM FormFields WHERE FormId = @FormId";
            var fieldsParams = new SqlParameter[] { new SqlParameter("@FormId", id) };
            DataTable fieldsTable = await _dbContext.QueryAsync(fieldsSql, fieldsParams);

            form.Fields = new List<FormFieldDTO>();
            foreach (DataRow fieldRow in fieldsTable.Rows)
            {
                form.Fields.Add(new FormFieldDTO
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

        public async Task<IEnumerable<FormDTO>> GetAllFormsAsync()
        {
            string sql = "SELECT * FROM Forms ORDER BY CreatedAt DESC";
            DataTable formsTable = await _dbContext.QueryAsync(sql);

            var forms = new List<FormDTO>();
            foreach (DataRow row in formsTable.Rows)
            {
                forms.Add(new FormDTO
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