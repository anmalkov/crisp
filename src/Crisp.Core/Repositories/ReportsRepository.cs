using Crisp.Core.Models;
using DocumentFormat.OpenXml.Vml.Office;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Crisp.Core.Repositories;

public enum ReportType
{
    Markdown = 1,
    Word
};

public class ReportsRepository : IReportsRepository
{
    private const string RepositoriesDirectoryName = "data";
    private const string ReportsDirectoryName = "reports";
    private const string MarkdownReportFileName = "security-plan.md";
    private const string WordReportFileName = "security-plan.docx";
    private const string MarkdownTemplateFileName = "template.md";
    private const string WordTemplateFileName = "template.docx";

    private readonly string _reportsFullPath;


    public ReportsRepository()
    {
        _reportsFullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", RepositoriesDirectoryName, ReportsDirectoryName);
    }


    public void CreateReportDirectory(string threatModelId, string projectName)
    {
        var reportDirectory = GetReportDirectoryFullName(threatModelId, projectName);
        if (!Directory.Exists(reportDirectory))
        {
            Directory.CreateDirectory(reportDirectory);
        }
    }

    public void RenameAndCleanReportDirectory(string threatModelId, string projectName, IEnumerable<string>? keepImagesFileNames)
    {
        var reportDirectory = GetReportDirectoryFullName(threatModelId, projectName);
        if (!Directory.Exists(reportDirectory))
        {
            var oldDirectory = Directory.GetDirectories(_reportsFullPath, $"*{threatModelId}").FirstOrDefault();
            if (oldDirectory is null)
            {
                Directory.CreateDirectory(reportDirectory);
                return;
            }
            if (oldDirectory != reportDirectory)
            {
                Directory. Move(oldDirectory, reportDirectory);
            }
        }

        var fileNames = Directory.GetFiles(reportDirectory);
        foreach (var fileName in fileNames)
        {
            if (keepImagesFileNames is null || !keepImagesFileNames.Contains(Path.GetFileName(fileName)))
            {
                File.Delete(fileName);
            }
        }
    }

    public async Task CreateAsync(string threatModelId, string projectName, ReportType reportType, byte[] content)
    {
        var reportDirectory = GetReportDirectoryFullName(threatModelId, projectName);
        if (!Directory.Exists(reportDirectory))
        {
            Directory.CreateDirectory(reportDirectory);
        }

        var fileName = GetReportFileName(reportType);
        var fileFullName = Path.Combine(reportDirectory, fileName);
        await File.WriteAllBytesAsync(fileFullName, content);
    }

    public async Task<bool> StoreFileAsync(string threatModelId, string fileName, byte[] content)
    {
        var directory = GetReportDirectory(threatModelId);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        var fileFullName = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(fileFullName, content);
        return true;
    }

    public async Task<byte[]?> GetFileAsync(string threatModelId, string fileName)
    {
        var directory = GetReportDirectory(threatModelId);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }
        
        var fileFullName = Path.Combine(directory, fileName);
        return await File.ReadAllBytesAsync(fileFullName);
    }
    
    public async Task<byte[]?> GetAsync(string threatModelId, ReportType reportType)
    {
        var fileFullName = GetReportFullFileName(threatModelId, reportType);
        if (string.IsNullOrWhiteSpace(fileFullName))
        {
            return null;
        }
        
        return await File.ReadAllBytesAsync(fileFullName);
    }

    public async Task<(byte[]? archiveContent, string fileName)> GetArchiveAsync(string threatModelId)
    {
        var directory = GetReportDirectory(threatModelId);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return (null, "");
        }

        var directoryName = Path.GetFileName(directory);
        var archiveFileName = directoryName[..(directoryName.Length - threatModelId.Length - 1)] + ".zip";
        var archiveFileFullName = Path.Combine(_reportsFullPath, archiveFileName);
        if (File.Exists(archiveFileFullName))
        {
            File.Delete(archiveFileFullName);
        }

        ZipFile.CreateFromDirectory(directory, archiveFileFullName);

        return (await File.ReadAllBytesAsync(archiveFileFullName), archiveFileName);
    }

    public bool Exists(string threatModelId, ReportType reportType)
    {
        var fileFullName = GetReportFullFileName(threatModelId, reportType);
        if (string.IsNullOrWhiteSpace(fileFullName))
        {
            return false;
        }

        return File.Exists(fileFullName);
    }

    public void Delete(string threatModelId)
    {
        if (!Directory.Exists(_reportsFullPath))
        {
            return;
        }
        
        var directory = Directory.GetDirectories(_reportsFullPath, $"*{threatModelId}").FirstOrDefault();
        if (directory is null)
        {
            return;
        }

        Directory.Delete(directory, true);
    }

    public async Task<byte[]?> GetTemplateAsync(ReportType reportType)
    {
        if (!Directory.Exists(_reportsFullPath))
        {
            return null;
        }

        var fileName = GetTemplateFileName(reportType);
        var templateFileFullName = Path.Combine(_reportsFullPath, fileName);
        return File.Exists(templateFileFullName) ? await File.ReadAllBytesAsync(templateFileFullName) : null;
    }

    public async Task StoreTemplateAsync(ReportType reportType, byte[] content)
    {
        if (!Directory.Exists(_reportsFullPath))
        {
            Directory.CreateDirectory(_reportsFullPath);
        }

        var fileName = GetTemplateFileName(reportType);
        var templateFileFullName = Path.Combine(_reportsFullPath, fileName);
        await File.WriteAllBytesAsync(templateFileFullName, content);
    }

    public string? GetReportFullFileName(string threatModelId, ReportType reportType)
    {
        var directory = GetReportDirectory(threatModelId);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return null;
        }

        var fileName = GetReportFileName(reportType);
        var fileFullName = Path.Combine(directory, fileName);
        return fileFullName;
    }
    

    private string? GetReportDirectory(string threatModelId)
    {
        if (!Directory.Exists(_reportsFullPath))
        {
            return null;
        }
        
        var directories = Directory.GetDirectories(_reportsFullPath, $"*{threatModelId}");
        return directories.Any() ? directories.First() : null;
    }

    private static string GetTemplateFileName(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.Word => WordTemplateFileName,
            _ => MarkdownTemplateFileName
        };
    }

    private static string GetReportFileName(ReportType reportType)
    {
        return reportType switch
        {
            ReportType.Word => WordReportFileName,
            _ => MarkdownReportFileName
        };
    }

    private string GetReportDirectoryFullName(string threatMpdelId, string projectName)
    {
        var reportDirectoryName = $"{projectName.Replace(" ", "-").ToLower()}-{threatMpdelId}";
        return Path.Combine(_reportsFullPath, reportDirectoryName);
    }
}
