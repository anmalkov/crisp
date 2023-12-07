using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Core.Repositories;

public interface IReportsRepository
{
    Task<byte[]?> GetAsync(string threatModelId, ReportType reportType);
    Task<(byte[]? archiveContent, string fileName)> GetArchiveAsync(string threatModelId);
    void CreateReportDirectory(string threatModelId, string projectName);
    void RenameAndCleanReportDirectory(string threatModelId, string projectName, IEnumerable<string>? keepImagesFileNames);
    Task CreateAsync(string threatModelId, string projectName, ReportType reportType, byte[] content);
    Task<bool> StoreFileAsync(string threatModelId, string fileName, byte[] content);
    Task<byte[]?> GetFileAsync(string threatModelId, string fileName);
    bool Exists(string threatModelId, ReportType reportType);
    void Delete(string threatModelId);
    Task<byte[]?> GetTemplateAsync(ReportType reportType);
    Task StoreTemplateAsync(ReportType reportType, byte[] content);
}
