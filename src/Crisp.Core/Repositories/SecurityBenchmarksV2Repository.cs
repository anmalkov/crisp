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

public class SecurityBenchmarksV2Repository : ISecurityBenchmarksRepository
{
    private const string SecurityBaselineVersion = "2.0";
    private const string SecurityBaselineFileSuffix = $"-security-baseline-v{SecurityBaselineVersion}.xlsx";


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
            // Skip the header row
            reader.Read();
            while (reader.Read())
            {
                var title = reader.GetValue(5)?.ToString();
                if (string.IsNullOrEmpty(title))
                {
                    break;
                }
                benchmarks.Add(new SecurityBenchmark(
                    reader.GetValue(1)?.ToString() ?? "",
                    reader.GetValue(2)?.ToString() ?? "",
                    title,
                    reader.GetValue(6)?.ToString() ?? "",
                    reader.GetValue(7)?.ToString()
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
        return Path.Combine(rootDirectoryPath, "Azure Offer Security Baselines", SecurityBaselineVersion);
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
        if (filePrefix.Contains(" iot ",StringComparison.InvariantCultureIgnoreCase))
        {
            resourceName = Regex.Replace(resourceName, @"\biot\b", "IoT", RegexOptions.IgnoreCase);
        }

        return resourceName;
    }
}
