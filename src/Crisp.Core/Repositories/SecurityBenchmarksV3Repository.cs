using Crisp.Core.Models;
using ExcelDataReader;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Crisp.Core.Repositories;

public class SecurityBenchmarksV3Repository : ISecurityBenchmarksRepository
{
    private const string SecurityBaselineVersion = "3";
    private const string SecurityBaselineFileSuffix = $"-azure-security-benchmark-v{SecurityBaselineVersion}-latest-security-baseline.xlsx";


    public Task<IEnumerable<string>> GetAllResourceNamesAsync(string rootDirectoryPath)
    {
        return Task.Run<IEnumerable<string>>(() =>
        {
            var benchmarksDirectory = GetBenchmarksDirectory(rootDirectoryPath);
            if (!Directory.Exists(benchmarksDirectory))
            {
                return Enumerable.Empty<string>();
            }

            var resourceNames = Directory.GetFiles(benchmarksDirectory).Select(f => GetResourceNameFromSecurityBaselineFileName(f)).ToArray();
            return resourceNames;
        });
    }

    public async Task<IEnumerable<SecurityBenchmark>> GetSecurityBenchmarksForResourceAsync(string resourceName, string rootDirectoryPath)
    {
        var fileFullName = GetFileFullNameForResource(rootDirectoryPath, resourceName);
        if (!File.Exists(fileFullName))
        {
            return Enumerable.Empty<SecurityBenchmark>();
        }

        return await GetAllSecurityBenchmarksAsync(fileFullName);
    }

    public async Task<IEnumerable<SecurityBenchmarkControl>> GetSecurityBenchmarkControlsAsync(string rootDirectoryPath)
    {
        var benchmarkControlsFileName = Path.Combine(rootDirectoryPath, "Microsoft Cloud Security Benchmark", "Microsoft_cloud_security_benchmark_v1.xlsx");
        if (!File.Exists(benchmarkControlsFileName))
        {
            return Enumerable.Empty<SecurityBenchmarkControl>();
        }

        var benchmarkControls = await GetAllSecurityBenchmarkControlsAsync(benchmarkControlsFileName);
        return benchmarkControls;
    }


    private static Task<IEnumerable<SecurityBenchmark>> GetAllSecurityBenchmarksAsync(string fileFullName)
    {
        return Task.Run<IEnumerable<SecurityBenchmark>>(() =>
        {
            var benchmarks = new List<SecurityBenchmark>();
            // Register the code pages to support ExcelDataReader on non-Windows systems
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stream = File.Open(fileFullName, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            reader.NextResult();  // Skip the first sheet and move to the second one
            reader.Read();  // Skip the header row
            while (reader.Read())
            {
                var title = reader.GetValue(2)?.ToString();
                if (string.IsNullOrEmpty(title))
                {
                    break;
                }

                var responsibility = reader.GetValue(4)?.ToString();
                if (string.Equals(responsibility, "Not Applicable", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(responsibility, "Microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var description = reader.GetValue(3)?.ToString() ?? "";
                if (description.StartsWith("This feature is not applicable"))
                {
                    continue;
                }

                benchmarks.Add(new SecurityBenchmark(
                    reader.GetValue(1)?.ToString() ?? "",
                    reader.GetValue(0)?.ToString() ?? "",
                    title,
                    description,
                    reader.GetValue(2)?.ToString(),
                    reader.GetValue(5)?.ToString(),
                    reader.GetValue(6)?.ToString(),
                    reader.GetValue(10)?.ToString(),
                    reader.GetValue(9)?.ToString(),
                    responsibility
                ));
            }
            return benchmarks;
        });
    }

    private static Task<IEnumerable<SecurityBenchmarkControl>> GetAllSecurityBenchmarkControlsAsync(string fileFullName)
    {
        return Task.Run<IEnumerable<SecurityBenchmarkControl>>(() => {
            var controls = new List<SecurityBenchmarkControl>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  // Register the code pages to support ExcelDataReader on non-Windows systems
            using var stream = File.Open(fileFullName, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            reader.NextResult();  // Skip the first sheet and move to the second one
            do {
                reader.Read();  // Skip the header row
                while (reader.Read())
                {
                    var fieldCount = reader.FieldCount;
                    var azureGuidance = reader.GetValue(8)?.ToString();
                    var azureImplementation = reader.GetValue(9)?.ToString();
                    var awsGuidance = (string)null;// reader.GetValue(10)?.ToString();
                    var awsImplementation = (string)null;// reader.GetValue(11)?.ToString();

                    var azure = azureGuidance is null && azureImplementation is null
                        ? null
                        : $"{azureGuidance}{((azureGuidance is not null && azureImplementation is not null) ? Environment.NewLine + Environment.NewLine : "")}{azureImplementation}";

                    var aws = awsGuidance is null && awsImplementation is null
                        ? null
                        : $"{awsGuidance}{((awsGuidance is not null && awsImplementation is not null) ? Environment.NewLine + Environment.NewLine : "")}{awsImplementation}";

                    controls.Add(new SecurityBenchmarkControl(
                        reader.GetValue(0)?.ToString() ?? "",
                        reader.GetValue(1)?.ToString() ?? "",
                        reader.GetValue(6)?.ToString() ?? "",
                        reader.GetValue(7)?.ToString(),
                        azure,
                        aws,
                        null
                    ));
                }
            }
            while (reader.NextResult());
            return controls;
        });
    }


    private static string GetFileFullNameForResource(string rootDirectoryPath, string resourceName)
    {
        return Path.Combine(GetBenchmarksDirectory(rootDirectoryPath), GetSecurityBaselineFileName(resourceName));
    }

    private static string GetBenchmarksDirectory(string rootDirectoryPath)
    {
        return Path.Combine(rootDirectoryPath, "Azure Offer Security Baselines", SecurityBaselineVersion + ".0");
    }

    private static string GetSecurityBaselineFileName(string resourceName)
    {
        var filePrefix = resourceName.Trim().ToLower().Replace(' ', '-');
        return $"{filePrefix}{SecurityBaselineFileSuffix}";
    }

    private static string GetResourceNameFromSecurityBaselineFileName(string fileName)
    {
        if (fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            fileName = Path.GetFileName(fileName);
        }

        if (!fileName.Contains(SecurityBaselineFileSuffix))
        {
            return "";
        }

        var filePrefix = fileName[..^SecurityBaselineFileSuffix.Length].Replace('-', ' ').Trim();
        var resourceName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(filePrefix);
        if (filePrefix.Contains("api ", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"api\b", "API", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" wan", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bwan", "WAN", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" iaas", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\biaas", "IaaS", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" pubsub", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bpubsub", "PubSub", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" signalr", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bsignalr", "SignalR", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains("(aro)", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\(aro\)", "(ARO)", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" openshift", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bopenshift", "OpenShift", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" openai", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bopenai", "OpenAI", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" netapp", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bnetapp", "NetApp", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" hci", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bhci", "HCI", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" aks", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\baks", "AKS", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" hpc", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bhpc", "HPC", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" devtest", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bdevtest", "DevTest", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" hsm", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bhsm", "HSM", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" ddos", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bddos", "DDoS", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" mysql", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bmysql", "MySQL", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" postgresql", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bpostgresql", "PostgreSQL", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" mariadb", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bmariadb", "MariaDB", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" sap", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bsap", "SAP", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" db", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bdb", "DB", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" iot", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\biot", "IoT", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains("iot ", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"iot\b", "IoT", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" ip", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bip", "IP", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" sql", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bsql", "SQL", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains("sql ", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"sql\b", "SQL", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" dns", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bdns", "DNS", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains(" nat", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\bnat", "NAT", RegexOptions.IgnoreCase);
        }
        if (filePrefix.Contains("vpn", StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"vpn\b", "VPN", RegexOptions.IgnoreCase);
        }

        return resourceName;
    }
}
