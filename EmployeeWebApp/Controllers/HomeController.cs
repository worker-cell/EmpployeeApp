using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Diagnostics;
using EmployeeWebApp.Models;
using System.Data.SqlClient;

namespace EmployeeWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index() => View();

        [HttpPost()]
        public async Task<IActionResult> Index(IFormFile formFile)
        {
            if (!CheckInput(formFile))
                return View("Index");

            await ProcessData(formFile);

            return Ok();
        }

        private bool CheckInput(IFormFile formFile)
        {
            if (formFile == null || formFile.Length <= 0)
            {
                ModelState.AddModelError("", "File not selected");
                return false;
            }

            var permittedExtension = ".csv";
            var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || permittedExtension != extension)
            {
                ModelState.AddModelError("", "Invalid file type.");
                return false;
            }

            // Optional: Validate MIME type as well
            var mimeType = formFile.ContentType;
            var permittedMimeType = "text/csv";
            if (permittedMimeType != mimeType)
            {
                ModelState.AddModelError("", "Invalid MIME type.");
                return false;
            }

            //Validating the File Size
            if (formFile.Length > 10_000_000) // Limit to 10 MB
            {
                ModelState.AddModelError("", "The file is too large.");
                return false;
            }

            return true;
        }

        private async Task ProcessData(IFormFile formFile)
        {
            string filePath = await HandleFile(formFile);

            DataTable csvData = ReadCsvFile(filePath);

            AddToDB(csvData);

            System.IO.File.Delete(filePath);// after process of data remove the uploaded temporary csv file
        }
        private async Task<string> HandleFile(IFormFile formFile)
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot\\uploads",
                formFile.FileName
            );

            var folderPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //Using Buffering
            using (var stream = System.IO.File.Create(filePath))
                await formFile.CopyToAsync(stream);

            //Using Streaming
            //using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            //    await SingleFile.CopyToAsync(stream);

            return filePath;
        }
        private DataTable ReadCsvFile(string filePath)
        {
            try
            {
                var csvData = new DataTable("Employees");
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    //parser.HasFieldsEnclosedInQuotes = true;

                    string[] colFields = parser.ReadFields();
                    foreach (string column in colFields)
                    {
                        var datecolumn = new DataColumn(column.Replace("Personnel_Records.", ""))
                        {
                            AllowDBNull = true
                        };
                        csvData.Columns.Add(datecolumn);
                    }

                    while (!parser.EndOfData)
                        csvData.Rows.Add(parser.ReadFields());
                    #region parse to model
                    //parser.ReadFields(); // skip headline
                    //while (!parser.EndOfData)
                    //{
                    //    // Process row
                    //    string[] fields = parser.ReadFields();
                    //    if (fields.Length != 11) return;
                    //    var payrollNumber = fields[0];
                    //    var forenames = fields[1];
                    //    var surname = fields[2];
                    //    var dateOfBirth = fields[3];
                    //    var telephone = fields[4];
                    //    var mobile = fields[5];
                    //    var address = fields[6];
                    //    var address2 = fields[7];
                    //    var postcode = fields[8];
                    //    var emailHome = fields[9];
                    //    var startDate = fields[10];
                    //}
                    #endregion
                }
                //var jsonString = JsonConvert.SerializeObject(csvData);
                return csvData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void AddToDB(DataTable csvData)
        {
            var dtColumns = csvData.Columns.Cast<DataColumn>();
            string cols1 = string.Join(", ", dtColumns.Select(x => x.ColumnName));
            string cols2 = string.Join(", ", dtColumns.Select(x => $"@{x.ColumnName}"));

            //string sql = "INSERT INTO T (A, B, C) VALUES (@A, @B, @C)";
            string sql = $"INSERT INTO Employees ({cols1}) VALUES ({cols2})";

            var sqlConnStrBuilder = new SqlConnectionStringBuilder
            {
                DataSource = "localhost",
                InitialCatalog = "EmployeeWebAppDB",
                IntegratedSecurity = true
            };
            var sqlConn = new SqlConnection(sqlConnStrBuilder.ConnectionString);
            using (var conn = sqlConn)
            {
                conn.Open();
                foreach (DataRow row in csvData.Rows)
                {
                    SqlCommand cmd = conn.CreateCommand();
                    foreach (DataColumn col in csvData.Columns)
                        cmd.Parameters.AddWithValue($"@{col.ColumnName}", row[col.ColumnName]);
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IActionResult Privacy()
		{
			return View();
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
    }
}