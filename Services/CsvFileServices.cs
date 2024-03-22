using CsvHelper;
using CsvHelper.Configuration;
using System.Data.Common;
using System.Formats.Asn1;
using System.Globalization;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ImplementCSVHealper_Read_Csv_Files.Services
{
    public class CsvFileServices
    {
        private readonly ILogger<CsvFileServices> logger;
        public CsvFileServices(ILogger<CsvFileServices> logger)
        {
            this.logger = logger;   
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>To read Data from Csv File</returns>

        public async Task<IEnumerable<Dictionary<string, string>>> ReadCSVFile(string filePath)
        {
            logger.LogInformation($"CsvFileServices -- ReadCSVFile-- starts");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });
            //Records has Key-Value paired data.-->records[key] gives value for that key
            var records = csv.GetRecords<dynamic>().ToList();
            //Coloumns- has only all column names
            var Coloumns = csv.HeaderRecord;
            var data = new List<Dictionary<string, string>>();
            foreach (var record in records)
            {
                var row = new Dictionary<string, string>();
                foreach (var column in Coloumns) {

                        row[column] = ((IDictionary<string, object>)record)[column]?.ToString();

                }
                data.Add(row);
            }
            return data;
        }
        public async Task<string> WriteCSV(IEnumerable<Dictionary<string, string>> Data, string FilePath)
        {
            logger.LogInformation($"CsvFileServices -- WriteCSV -- Starts");
            var ReOrderedData = await ReorderData(Data, FilePath);
            var Records = ReOrderedData.Item1;
            var filemode = File.Exists(FilePath) ? FileMode.Append : FileMode.Create;
            using var writer = new StreamWriter(FilePath, append: true);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            var ColumnName = Data.SelectMany(d=>d.Keys).Distinct().ToList();

            if (filemode == FileMode.Create && Data.Any())
            {
                foreach (var key in Data.First().Keys)
                {
                    csv.WriteField(key);
                }
                csv.NextRecord();
            }
            if (Records != null)
            {
                foreach (var record in Records)
                {
                    var columns = ReOrderedData.Item2;

                    if (columns.Length == 0)
                    {

                        foreach (var column in ColumnName)
                        {
                            csv.WriteField(record[column]);
                        }
                        csv.NextRecord();
                    }
                }
            }
                return "Details Added Succsessfully";

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="FilePath"></param>
        /// <returns> Reorder the input data according to coloumn names in csv file.If Header(column names) doesnot exits in file it writes column names into file</returns>
        public async Task<(List<Dictionary<string, string>>?, string[]?)> ReorderData(IEnumerable<Dictionary<string, string>> Data, string FilePath)
        {
            try
            {
                logger.LogInformation($"CsvFileServices -- ReorderData Try block -- Starts");
                using var reader = new StreamReader(FilePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

                csv.Read();
                csv.ReadHeader();
                var columnNames = csv.HeaderRecord;
                List<Dictionary<string, string>> newrecord = new();

                foreach (var record in Data)
                {
                    Dictionary<string, string> rowdata = new();
                    if (columnNames != null)
                    {
                        foreach (var column in columnNames)
                        {
                            if (record.ContainsKey(column))
                            {
                                rowdata[column] = record[column];
                            }
                            else
                            {
                                rowdata[column] = " ";
                            }
                        }
                        newrecord.Add(rowdata);
                    }

                }
                return (newrecord, columnNames);

            }
            // while updating header is rewritten then data is rewritten
            catch (Exception ex)
            {
                logger.LogInformation($"CsvFileServices -- ReorderData -- catch block -- Starts");
                var filemode = File.Exists(FilePath) ? FileMode.Append : FileMode.Create;
                using var writer = new StreamWriter(FilePath, append: true);
                using var csvwriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
                List<Dictionary<string, string>> newrecord = new();
                var columns = Data.SelectMany(d => d.Keys).Distinct().ToList();
                foreach (var header in columns)
                {
                    csvwriter.WriteField(header.ToString());

                }
                csvwriter.NextRecord();
                foreach (var record in Data)
                {
                    Dictionary<string, string> rowdata = new();
                    foreach (var column in columns)
                    {
                        if (record.ContainsKey(column))
                        {
                            rowdata[column] = record[column];
                        }
                        else
                        {
                            rowdata[column] = " ";
                        }
                    }
                    newrecord.Add(rowdata);

                }
                return (newrecord, columns.ToArray());

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="FilePath"></param>
        /// <returns> To update existing data in file based on PRN.</returns>
        public async Task<string> UpdateCsvData(IEnumerable<Dictionary<string, string>> Data, string FilePath)
        {
            try
            {
                logger.LogInformation($"CsvFileServices -- UpdateCsvData Try block -- Starts");
                // Read existing CSV data
                IEnumerable<Dictionary<string, string>> AllPatientDetails = await ReadCSVFile(FilePath);

                // Iterate over the updated data and update existing records
                foreach (var record in Data)
                {
                    var patient = AllPatientDetails.FirstOrDefault(k => k.TryGetValue("PRN", out var prn) && prn == record["PRN"]);
                    if (patient != null)
                    {
                        foreach (var column in record.Keys)
                        {
                            patient[column] = record[column];
                        }
                    }
                }

                // Write the updated data back to the CSV file
                string response = await WriteCsvDataAsync(AllPatientDetails, FilePath);
                if (response == "Data written to CSV file")
                {
                    logger.LogInformation($"CsvFileServices -- UpdateCsvData Try block -- Data written to CSV file");

                    return "Updated Successfully";
                }
                logger.LogInformation($"UpdateCsvData Try block -- Updation Failed");
                return "Updation Failed";
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Exception --> CsvFileServices -->UpdateCsvData Catech block --{ex}");

                // Handle exceptions
                return "Error: " + ex.Message;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PRN"></param>
        /// <param name="FilePath"></param>
        /// <returns>To delete record from CSV file</returns>

        public async Task<string> DeleteCsvData(int PRN,string FilePath)
        {
            logger.LogInformation($"CsvFileServices --> DeleteCsvData Try block -- Starts");
            IEnumerable<Dictionary<string, string>> AllPatientDatails = await ReadCSVFile(FilePath);
            var patient=AllPatientDatails.FirstOrDefault(k => k["PRN"]==PRN.ToString());

            if (patient != null)
            {
                logger.LogInformation($" CsvFileServices -->DeleteCsvData--> Patient found to be deleted with details -- {patient}");
                AllPatientDatails =AllPatientDatails.Where(k=>k!=patient).ToList();
                await WriteCsvDataAsync(AllPatientDatails,FilePath);
                logger.LogInformation($"CsvFileServices --> DeleteCsvData Try block --> Patient Deleted Successfully -- Ends");
                return "Deleted Successfully";
            }
            return "Failed to Delete";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath"></param>
        /// <returns>Write the updated data back to the CSV file</returns>
        private async Task<string> WriteCsvDataAsync(IEnumerable<Dictionary<string, string>> data, string filePath)
        {
            logger.LogInformation($"CsvFileServices --> WriteCsvDataAsync -- Starts");
            var ReOrderedData = await ReorderData(data, filePath);
            var Records = ReOrderedData.Item1;
            var columns = ReOrderedData.Item2;
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            foreach (var header in columns)
            {
                
                csv.WriteField(header.ToString());

            }
            await csv.NextRecordAsync();
            logger.LogInformation($"CsvFileServices --> Printed Headers in CSV file");
            foreach (var record in Records)
            {
                foreach (var column in columns)
                {
                    csv.WriteField(record[column]);
                }
                await csv.NextRecordAsync();
            }
            logger.LogInformation($"CsvFileServices -->WriteCsvDataAsync--> Printed Data into CSV file ");
            logger.LogInformation($"CsvFileServices -->WriteCsvDataAsync-->Ends ");
            return "Data written to CSV file";
        }

    }
}



