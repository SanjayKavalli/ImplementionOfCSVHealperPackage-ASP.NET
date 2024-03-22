using ImplementCSVHealper_Read_Csv_Files.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ImplementCSVHealper_Read_Csv_Files.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CsvController : Controller
    {
        private readonly CsvFileServices csvFileServices;
        public CsvController(CsvFileServices csvFileServices) {
            this.csvFileServices = csvFileServices;
                }

        [HttpGet]
        [Route("GetCSV")]
        public async Task<List<Dictionary<string, string>>> GetPatientDetailsFromCSVFile()
        {
            string directory = Directory.GetCurrentDirectory();
             string FilePath = directory + "\\csvfile\\PatientRecords.csv";
            string path = FilePath.Replace('\\', '/');
            List<Dictionary<string, string>> PatientDetails = (List<Dictionary<string, string>>)await csvFileServices.ReadCSVFile(path);

            if (PatientDetails != null)
            {
                return PatientDetails;
            }
            else
            {
                List<Dictionary<string, string>> emptyResponse = [new Dictionary<string, string> { { "message", "File is empty" } }];
                return emptyResponse;
            }
        }

        [HttpPost]
        [Route("AddCsv")]
        public async Task<string> AddPatientToCSV(IEnumerable<Dictionary<string, string>> Data)
        {
            string directory = Directory.GetCurrentDirectory();
            string FilePath = directory + "\\csvfile\\PatientRecords.csv";
            string path = FilePath.Replace('\\', '/');
            string response= await csvFileServices.WriteCSV(Data, path);
            return response;

        }
        [HttpPost]
        [Route("UpdateCsv")]
        public async Task<string> UpdatePatientInCSV(IEnumerable<Dictionary<string, string>> Data)
        {
            string directory = Directory.GetCurrentDirectory();
            string FilePath = directory + "\\csvfile\\PatientRecords.csv";
            string path = FilePath.Replace('\\', '/');
            string response = await csvFileServices.UpdateCsvData(Data, path);
            return response;

        }
        [HttpPost]
        [Route("DeleteCsv/{PRN}")]
        public async Task<string> DeletePatientInCSV(int PRN)
        {
            string directory = Directory.GetCurrentDirectory();
            string FilePath = directory + "\\csvfile\\PatientRecords.csv";
            string path = FilePath.Replace('\\', '/');
            string response = await csvFileServices.DeleteCsvData(PRN, path);
            return response;

        }
    }
}
