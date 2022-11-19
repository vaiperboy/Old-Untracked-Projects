using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NLog;
using System.Linq;

namespace COD_Discord_Bot.Classes {
    class GSheet {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string AuthFile { get; set; }
        private const string AppName = "Application"; //hardcode : )
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private SheetsService Service;
        private UserCredential Credentials;

        public GSheet() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./assets/credentials.json")) {

        }


        public GSheet(string authFile) {
            this.AuthFile = authFile;
            using (var stream =
                new FileStream(this.AuthFile, FileMode.Open, FileAccess.Read)) {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "./assets/token.json";
                Credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            logger.Debug("Finished IO for google sheets...");

            // Create Google Sheets API service.
            this.Service = new SheetsService(new BaseClientService.Initializer() {
                HttpClientInitializer = Credentials,
                ApplicationName = AppName,
            });
            logger.Debug("Google Sheets service started");
            
        }

        public async Task<ShoppySampleProduct> GetProduct(string hName) {
            var range = Configuration.WorksheetName; 
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    this.Service.Spreadsheets.Values.Get(Configuration.SpreadshetID, range);
            ValueRange response = await request.ExecuteAsync();
            IList<IList<Object>> rows = response.Values;
            rows = rows.Where(x => x.Count >= 4 && x[0].ToString().Equals(hName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (rows.Count != 1) //allow one result only
                return default;
            var row = rows.First();
            return new ShoppySampleProduct() {
                Title = row[1].ToString().Trim(),
                Price = float.Parse(row[2].ToString().Trim()),
                Items = row[3].ToString().Split(',').Select(x => x.Trim()).ToList()
            };
        }
    }
}
