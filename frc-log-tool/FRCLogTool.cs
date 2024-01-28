using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;
using Renci.SshNet;
using System.ComponentModel;
using System.Configuration;
using System.Net.Sockets;
using System.Transactions;

namespace FRC_Log_Tool
{
    public partial class FRCLogTool : Form
    {
        public delegate void UpdateLogDelegate(string message);

        private BackgroundWorker sqlImportWorker;
        private string csvFilePath = ConfigurationManager.AppSettings["CSVFilePath"];
        private string connectionString = ConfigurationManager.ConnectionStrings["ReportingDatabase"].ConnectionString;
        private string host = ConfigurationManager.AppSettings["RoboRioHost"];
        private string username = ConfigurationManager.AppSettings["RoboRioUsername"];
        private string password = ConfigurationManager.AppSettings["RoboRioPassword"];
        private string remoteDirectory = ConfigurationManager.AppSettings["RoboRioRemoteDirectory"];

        private string tempLogDirectory;

        public FRCLogTool()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            InitializeProgressBar();

            SetupTransferDirectory();
            CheckRioConnectionAsync();
        }

        #region UI Functions

        private void InitializeBackgroundWorker()
        {
            sqlImportWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            sqlImportWorker.DoWork += ImportCSV;
            sqlImportWorker.ProgressChanged += SqlImportWorker_ProgressChanged;
        }

        private void InitializeProgressBar()
        {
            progressBarImport.Minimum = 0;
            progressBarImport.Maximum = 100;
            progressBarImport.Value = 0;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            sqlImportWorker.RunWorkerAsync();
        }

        private void ImportCSV(object sender, DoWorkEventArgs e)
        {
            ProcessCSV();
        }

        private void SqlImportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgressBarValue(e.ProgressPercentage);
        }

        private void UpdateProgressBarValue(int value)
        {
            if (progressBarImport.InvokeRequired)
            {
                progressBarImport.Invoke(new Action(() => progressBarImport.Value = value));
            }
            else
            {
                progressBarImport.Value = value;
            }
        }

        private void UpdateLog(string message)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => rtbLog.AppendText(message + Environment.NewLine)));
            }
            else
            {
                rtbLog.AppendText(message + Environment.NewLine);
            }
        }

        #endregion UI Functions

        #region ProcessCSV

        public void ProcessCSV()
        {
            int totalCount = CountRecordsInCSV();
            UpdateLog($"Found '{totalCount}' records to import");
            sqlImportWorker.ReportProgress(0);

            try
            {
                using (TextFieldParser csvParser = new TextFieldParser(csvFilePath))
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    InitializeParser(csvParser);
                    connection.Open();
                    UpdateLog($"Connected to database");

                    using (TransactionScope scope = new TransactionScope())
                    {
                        ProcessRecords(csvParser, connection, totalCount);
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateLog("Error processing CSV: " + ex.Message);
            }
        }

        private void InitializeParser(TextFieldParser parser)
        {
            parser.CommentTokens = new string[] { "#" };
            parser.SetDelimiters(new string[] { "," });
            parser.HasFieldsEnclosedInQuotes = true;
            parser.ReadLine(); // Skip header line
        }

        private void ProcessRecords(TextFieldParser parser, SqlConnection connection, int totalCount)
        {
            int processedCount = 0;

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                InsertDataIntoSQL(connection, fields);
                UpdateProgress(ref processedCount, totalCount);
            }
        }

        private void InsertDataIntoSQL(SqlConnection connection, string[] fields)
        {
            string groupId = fields[0];
            string key = fields[1];
            string value = fields[2];
            string timeStamp = fields[3];

            string tableName = groupId;
            string commandText = $"INSERT INTO {tableName} (key_id, value, insert_at) VALUES (@key, @value, @timeStamp)";

            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.Parameters.AddWithValue("@timeStamp", Convert.ToDateTime(timeStamp));

                command.ExecuteNonQuery();
            }
        }

        private int CountRecordsInCSV()
        {
            int recordCount = 0;

            try
            {
                using (TextFieldParser csvParser = new TextFieldParser(csvFilePath))
                {
                    InitializeParser(csvParser);

                    while (!csvParser.EndOfData)
                    {
                        csvParser.ReadFields();
                        recordCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateLog("Error counting records in CSV: " + ex.Message);
            }

            return recordCount;
        }

        private void UpdateProgress(ref int processedCount, int totalCount)
        {
            processedCount++;
            int progressPercentage = (int)((double)processedCount / totalCount * 100);
            sqlImportWorker.ReportProgress(progressPercentage);
        }


        #endregion ProcessCSV

        #region Rio/Transfer

        private async void CheckRioConnectionAsync()
        {
            while (true)
            {
                bool isConnected = await IsRioConnectedAsync();
                UpdateRobotStatusLabel(isConnected);
                await Task.Delay(5000); // Wait for 5 seconds before checking again
            }
        }

        private async Task<bool> IsRioConnectedAsync()
        {
            string host = ConfigurationManager.AppSettings["RoboRioHost"];
            using (var client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(host, 22); // SSH port, assuming the RoboRio is using SSH
                    return client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void UpdateRobotStatusLabel(bool isConnected)
        {
            Action updateAction = () =>
            {
                if (lblRobotStatus.InvokeRequired)
                {
                    lblRobotStatus.Invoke(new Action(() => UpdateRobotStatusLabel(isConnected)));
                }
                else
                {
                    lblRobotStatus.Text = isConnected ? "Connected" : "Waiting for the robot to be connected...";
                    lblRobotStatus.ForeColor = isConnected ? Color.Green : Color.Black;

                    btnImport.Enabled = isConnected;
                }
            };

            this.Invoke(updateAction);
        }

        private void SetupTransferDirectory()
        {
            // Determine the local directory based on the executable's location
            string exePath = Application.StartupPath;
            tempLogDirectory = Path.Combine(exePath, "logs");

            // Create the local directory if it does not exist
            if (!Directory.Exists(tempLogDirectory))
            {
                Directory.CreateDirectory(tempLogDirectory);
            }

        }

        private void TransferAndDeleteFilesFromRio()
        {
            using (var sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (var file in files)
                    {
                        if (!file.Name.EndsWith(".csv")) continue;

                        string remoteFileName = file.FullName;
                        string localFileName = Path.Combine(tempLogDirectory, file.Name);

                        using (Stream fileStream = File.Create(localFileName))
                        {
                            sftp.DownloadFile(remoteFileName, fileStream);
                        }

                        sftp.DeleteFile(remoteFileName);
                        UpdateLog($"Transferred and deleted {file.Name}");
                    }
                    sftp.Disconnect();
                }
                catch (Exception ex)
                {
                    UpdateLog($"Error: {ex.Message}");
                }
            }
        }

        #endregion Rio/Transfer
    }
}
