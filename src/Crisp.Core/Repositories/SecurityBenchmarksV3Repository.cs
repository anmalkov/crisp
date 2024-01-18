using Crisp.Core.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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


    private static Task<IEnumerable<SecurityBenchmark>> GetAllSecurityBenchmarksAsync(string fileFullName)
    {
        return Task.Run<IEnumerable<SecurityBenchmark>>(() =>
        {
            var benchmarks = new List<SecurityBenchmark>();
            // Register the code pages to support ExcelDataReader on non-Windows systems
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stream = File.Open(fileFullName, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            // Skip the first sheet and move to the second one
            reader.NextResult();
            // Skip the header row
            reader.Read();
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
