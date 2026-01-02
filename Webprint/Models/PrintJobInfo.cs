using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
namespace Webprint.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Hosting;
    using static System.Net.Mime.MediaTypeNames;

    public class PrintJobInfo
    {
        public string PrinterName { get; set; }
        public string Document { get; set; }
        public string Status { get; set; }
        public string PagesPrinted { get; set; }
        public string TotalPages { get; set; }
        public string Size { get; set; }
        public string currentSize { get; set; }
        public DateTime? TimeSubmitted { get; set; }
    }
    
    public class PrintJobService
    {
        public static string GetProjectDirectoryWithHostingEnvironment()
        {
            //取得Web.config裡面設定的檔案路徑
            return HostingEnvironment.ApplicationPhysicalPath;
        }
        public string Calculator(string path, string jobname)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fiArr = directoryInfo.GetFiles("*.SPL");
            int MB = 1024 * 1024;
            // Display the names and sizes of the files.
            Console.WriteLine("The directory {0} contains the following files:", directoryInfo.Name);
            Dictionary<string, long> openWith = new Dictionary<string, long>();
            string textnum = "";
            jobname = jobname.Split(' ')[1];
            foreach (FileInfo f in fiArr)
            {
                openWith.Add(f.Name, f.Length);
            }
            foreach (KeyValuePair<string, long> kvp in openWith)
            {
                int.TryParse(kvp.Key.Split('.')[0], out int jobnumber);
                if (jobname==jobnumber.ToString())
                {
                    textnum = Math.Round((float)kvp.Value / MB).ToString();
                }
            }
            return textnum;
        }
        public List<PrintJobInfo> GetActivePrintJobs()
        {
            try
            {
                //定義取得目前剩餘列印大小數
                string currentProcessingSize = "0";
                string districtJsonPath = ConfigurationManager.AppSettings["DistrictJsonPath"];
                string spoolFilePath = ConfigurationManager.AppSettings["spoolFilePath"];
                var getSpoolSize = new PrintJobService();
                string jsonContent = File.ReadAllText(districtJsonPath);
                JObject jsonobject = JObject.Parse(jsonContent);
                var list = new List<PrintJobInfo>();
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PrintJob where StatusMask != 128 ");
                
                
                foreach (ManagementObject job in searcher.Get())
                {
                    var jobName = job["Name"]?.ToString(); // 格式為 PrinterName, JobId
                    var document = job["Document"]?.ToString();
                    var status = job["JobStatus"]?.ToString();
                    var pagesPrinted = job["PagesPrinted"]?.ToString();
                    var totalpages = job["TotalPages"]?.ToString();
                    var TimeSubmitted = job["TimeSubmitted"]?.ToString();
                    var Size = job["Size"];
                    DateTime? timeSubmitted = ManagementDateTimeConverter.ToDateTime(TimeSubmitted);
                    var printName = jobName.Split(',');
                    string wmiPrinterName = printName[0];

                    // 取得 WMI 的 Job ID
                    int wmiJobId = 0;
                    if (printName.Length > 1)
                    {
                        int.TryParse(printName[1].Trim(), out wmiJobId);
                    }

                    // 對應真實印表機名稱 (從你的 JSON)
                    

                    // 呼叫 P/Invoke 取得該印表機佇列詳細資料
                    var spoolerJobs = GetPrintJobs(wmiPrinterName);

                    // *** 關鍵修正：比對 Job ID ***
                    // 在 spoolerJobs 裡找到 ID 跟我現在 WMI 迴圈這筆一樣的 Job
                    var matchedJob = spoolerJobs.FirstOrDefault(j => j.JobId == wmiJobId);
                    // 判斷是否找到 (因為是 struct，若沒找到所有欄位會是預設值 0)
                    if (matchedJob.JobId != 0)
                    {
                        // 這裡取得「目前已列印大小」
                        currentProcessingSize = FormatBytes(matchedJob.Size);
                    }
                    else
                    {
                        currentProcessingSize = "等待中/無法取得";
                    }
                    // 重新組合文件名稱
                    string documentRegex = document.Split('_')[1]+"_"+document.Split('_')[3];
                    string size = getSpoolSize.Calculator(spoolFilePath, printName[1].ToString());
                    list.Add(new PrintJobInfo
                    {
                        PrinterName = (string)jsonobject[printName[0]],
                        Document = documentRegex,
                        Status = status,
                        TotalPages = totalpages,
                        Size = size,
                        currentSize = currentProcessingSize,
                        TimeSubmitted = timeSubmitted
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                //如果有發生問題,回傳錯誤訊息
                var list = new List<PrintJobInfo>();
                list.Add(new PrintJobInfo
                {
                    PrinterName = "Fail",
                    Document = "Fail",
                    Status = "Fail",
                    TotalPages = "Fail",
                    currentSize = "Fail",
                    //錯誤訊息由Size欄位帶回
                    Size = ex.Message
                });
                return list;
            }
        }
        // 取得工作列表的核心邏輯
        static List<SpoolerApi.JOB_INFO_2> GetPrintJobs(string printerName)
        {
            List<SpoolerApi.JOB_INFO_2> jobList = new List<SpoolerApi.JOB_INFO_2>();
            IntPtr hPrinter = IntPtr.Zero;

            try
            {
                if (!SpoolerApi.OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                    return jobList;

                uint bytesNeeded = 0;
                uint jobsReturned = 0;

                // 第一次呼叫取得所需記憶體大小
                SpoolerApi.EnumJobs(hPrinter, 0, 127, 2, IntPtr.Zero, 0, out bytesNeeded, out jobsReturned);

                if (bytesNeeded > 0)
                {
                    IntPtr pJobBuffer = Marshal.AllocHGlobal((int)bytesNeeded);
                    try
                    {
                        // 第二次呼叫取得實際資料
                        if (SpoolerApi.EnumJobs(hPrinter, 0, 127, 2, pJobBuffer, bytesNeeded, out bytesNeeded, out jobsReturned))
                        {
                            int structSize = Marshal.SizeOf(typeof(SpoolerApi.JOB_INFO_2));
                            IntPtr currentJobPtr = pJobBuffer;

                            for (int i = 0; i < jobsReturned; i++)
                            {
                                var job = (SpoolerApi.JOB_INFO_2)Marshal.PtrToStructure(currentJobPtr, typeof(SpoolerApi.JOB_INFO_2));
                                jobList.Add(job);
                                currentJobPtr = (IntPtr)((long)currentJobPtr + structSize);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pJobBuffer);
                    }
                }
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    SpoolerApi.ClosePrinter(hPrinter);
            }
            return jobList;
        }

        // 單位轉換函式 (1024 base)
        static string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            // 格式化範例: 1.32 GB
            return String.Format("{0:0.00} {1}", dblSByte, suffix[i]);
        }
    }

    // --- Windows API 定義區 ---
    public static class SpoolerApi
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumJobs(
            IntPtr hPrinter,
            uint FirstJob,
            uint NoJobs,
            uint Level,
            IntPtr pJob,
            uint cbBuf,
            out uint pcbNeeded,
            out uint pcReturned
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct JOB_INFO_2
        {
            public uint JobId;
            [MarshalAs(UnmanagedType.LPTStr)] public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pMachineName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pUserName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDocument;
            [MarshalAs(UnmanagedType.LPTStr)] public string pNotifyName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)] public string pPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)] public string pParameters;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDriverName;
            public IntPtr pDevMode;
            [MarshalAs(UnmanagedType.LPTStr)] public string pStatus;
            public IntPtr pSecurityDescriptor;
            public uint Status;
            public uint Priority;
            public uint Position;
            public uint StartTime;
            public uint UntilTime;
            public uint TotalPages;
            public uint Size;          // 檔案目前剩餘大小()
            public uint SizePrinted;  
            public uint Time;
            public uint PagesPrinted;
        }
    }
}