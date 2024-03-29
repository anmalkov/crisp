﻿using Crisp.Core.Models;
using Crisp.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using Category = Crisp.Core.Models.Category;
using Crisp.Core.Helpers;
using System.Text.RegularExpressions;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace Crisp.Core.Services;

public class ThreatModelsService : IThreatModelsService
{
    private const string GitHubAccountName = "anmalkov";
    private const string GitHubRepositoryName = "brief";
    private const string GitHubThreatModelFolderName = "Threat Model";
    private const string GitHubMarkdownThreatModelTemplateFilePath = GitHubThreatModelFolderName + "/threat-model-template.md";
    private const string GitHubWordThreatModelTemplateFilePath = GitHubThreatModelFolderName + "/threat-model-template.docx";
    private const string GitHubThreatMappingFileSuffix = ".map.csv";

    private const string CategoryCacheKey = "threatmodels.category";
    private const string MarkdownTemplateCacheKey = "threatmodels.template";
    private const string WordTemplateCacheKey = "threatmodels.word-template";
    private const string ProjectNamePlaceholder = "[tm-project-name]";
    private const string DataflowAttributesPlaceholder = "[tm-data-flow-attributes]";
    private const string ThreatPropertiesPlaceholder = "[tm-threat-properties]";
    private const string ImagesPlaceholderPrefix = "tm-image-";
    private readonly IGitHubRepository _gitHubRepository;
    private readonly IThreatModelsRepository _threatModelsRepository;
    private readonly IThreatModelCategoriesRepository _threatModelCategoriesRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IReportsRepository _reportsRepository;
    private readonly IRecommendationsService _recommendationsService;

    public ThreatModelsService(IGitHubRepository gitHubRepository, IThreatModelsRepository threatModelsRepository, 
        IThreatModelCategoriesRepository threatModelCategoriesRepository, IMemoryCache memoryCache, IReportsRepository reportsRepository,
        IRecommendationsService recommendationsService)
    {
        _gitHubRepository = gitHubRepository;
        _threatModelsRepository = threatModelsRepository;
        _threatModelCategoriesRepository = threatModelCategoriesRepository;
        _memoryCache = memoryCache;
        _reportsRepository = reportsRepository;
        _recommendationsService = recommendationsService;
    }

    
    public async Task<IEnumerable<ThreatModel>?> GetAllAsync()
    {
        return await _threatModelsRepository.GetAllAsync();
    }

    public async Task<ThreatModel?> GetAsync(string id)
    {
        return await _threatModelsRepository.GetAsync(id);
    }

    public async Task<Category?> GetCategoryAsync()
    {
        return await _memoryCache.GetOrCreateAsync(CategoryCacheKey, async entry =>
        {
            entry.SetPriority(CacheItemPriority.NeverRemove);
            var category = await _threatModelCategoriesRepository.GetAllAsync();
            if (category is null)
            {
                category = await GetRecommendationsFromGitHubAsync();
                await _threatModelCategoriesRepository.UpdateAllAsync(category);
            }
            return category;
        });
    }

    public async Task CreateAsync(ThreatModel threatModel)
    {
        await _threatModelsRepository.CreateAsync(threatModel);
        _reportsRepository.CreateReportDirectory(threatModel.Id, threatModel.ProjectName);
    }

    public async Task UpdateAsync(ThreatModel threatModel)
    {
        await _threatModelsRepository.UpdateAsync(threatModel);
        _reportsRepository.RenameAndCleanReportDirectory(threatModel.Id, threatModel.ProjectName, threatModel.Images?.Select(i => i.Value));
    }

    public async Task<string?> GetReportAsync(string threatModelId)
    {
        if (_reportsRepository.Exists(threatModelId, ReportType.Markdown))
        {
            var reportContent = await _reportsRepository.GetAsync(threatModelId, ReportType.Markdown);
            return Encoding.UTF8.GetString(reportContent!);
        }

        return await GenerateAndSaveReportsAsync(threatModelId);
    }

    public async Task<(byte[]? archiveContent, string fileName)> GetReportArchiveAsync(string threatModelId)
    {
        if (!_reportsRepository.Exists(threatModelId, ReportType.Markdown))
        {
            await GenerateAndSaveReportsAsync(threatModelId);
        }
        return await _reportsRepository.GetArchiveAsync(threatModelId);
    }

    public async Task StoreFileForReportAsync(string threatModelId, string fileName, byte[] content)
    {
        await _reportsRepository.StoreFileAsync(threatModelId, fileName, content);
    }

    public async Task<byte[]?> GetReportFileAsync(string threatModelId, string fileName)
    {
       return await _reportsRepository.GetFileAsync(threatModelId, fileName);
    }

    public async Task DeleteAsync(string id)
    {
        await _threatModelsRepository.DeleteAsync(id);
        _reportsRepository.Delete(id);
    }


    private async Task<string?> GenerateAndSaveReportsAsync(string threatModelId)
    {
        var threatModel = await _threatModelsRepository.GetAsync(threatModelId);
        if (threatModel is null)
        {
            return null;
        }

        return await GenerateAndSaveReportsAsync(threatModel);
    }

    private async Task<string?> GenerateAndSaveReportsAsync(ThreatModel threatModel)
    {
        var mdReport = await GenerateMarkdownReportAsync(threatModel);
        if (mdReport is not null)
        {
            await _reportsRepository.CreateAsync(threatModel.Id, threatModel.ProjectName, ReportType.Markdown, Encoding.UTF8.GetBytes(mdReport));
        }

        var wordReport = await GenerateWordReportAsync(threatModel);
        if (wordReport is not null)
        {
            await _reportsRepository.CreateAsync(threatModel.Id, threatModel.ProjectName, ReportType.Word, wordReport);
        }

        return mdReport;
    }

    private async Task<string?> GenerateMarkdownReportAsync(ThreatModel threatModel)
    {
        var mdReport = await _memoryCache.GetOrCreateAsync(MarkdownTemplateCacheKey, async entry =>
        {
            var template = "";
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(24));
            var templateContent = await _reportsRepository.GetTemplateAsync(ReportType.Markdown);
            if (templateContent is not null)
            {
                template = Encoding.UTF8.GetString(templateContent);
            }
            else
            {
                var file = await _gitHubRepository.GetFileAsync(GitHubAccountName, GitHubRepositoryName, GitHubMarkdownThreatModelTemplateFilePath);
                template = file.Content;
                await _reportsRepository.StoreTemplateAsync(ReportType.Markdown, Encoding.UTF8.GetBytes(template));
            }
            return template;
        });
        if (string.IsNullOrWhiteSpace(mdReport))
        {
            return null;
        }
        
        mdReport = mdReport.Replace(ProjectNamePlaceholder, threatModel.ProjectName);

        var dataflowAttributeSection = MarkdownReportHelper.GenerateDataflowAttributeSection(threatModel);
        mdReport = mdReport.Replace(DataflowAttributesPlaceholder, dataflowAttributeSection);

        IDictionary<string, IEnumerable<SecurityBenchmark>>? benchmarks = threatModel.AddResourcesRecommendations && threatModel.Resources is not null
            ? (await Task.WhenAll(
                threatModel.Resources.Select(async r => new KeyValuePair<string, IEnumerable<SecurityBenchmark>>(r, await _recommendationsService.GetBenchmarksAsync(r)))
              )).ToDictionary(pair => pair.Key, pair => pair.Value)
            : null;
        var threatModelPropertiesSection = MarkdownReportHelper.GenerateThreatModelPropertiesSection(threatModel, benchmarks);
        mdReport = mdReport.Replace(ThreatPropertiesPlaceholder, threatModelPropertiesSection);

        if (threatModel.Images is not null)
        {
            mdReport = RemoveHeadersForUnusedImages(mdReport, threatModel.Images);
            foreach (var image in threatModel.Images)
            {
                mdReport = mdReport.Replace($"[{ImagesPlaceholderPrefix}{image.Key}]", $"![{image.Key}](./{image.Value})");
            }
        }
        return mdReport;
    }

    private static string RemoveHeadersForUnusedImages(string report, IDictionary<string, string> images)
    {
        if (!images.Any() || images.Count == 3)
        {
            return report;
        }
        
        var imageTypes = new string[] { "arch", "map" };
        foreach (var imageType in imageTypes)
        {
            if (images.ContainsKey(imageType))
            {
                continue;
            }

            var headerText = imageType.ToLower() switch
            {
                "arch" => "Architecture Diagram",
                "flow" => "Data Flow Diagram",
                "map" => "Threat Map",
                _ => ""
            };
            if (string.IsNullOrEmpty(headerText))
            {
                continue;
            }

            var regex = new Regex($"(\n|\r|\r\n)[#]+[ ]+{headerText}");
            var match = regex.Match(report);
            if (!match.Success)
            {
                continue;
            }
            var startIndex = match.Index;

            var placeholder = $"\\[{ImagesPlaceholderPrefix}{imageType}\\]";
            regex = new Regex($"{placeholder}(\n|\r|\r\n)");
            match = regex.Match(report);
            if (!match.Success)
            {
                continue;
            }
            var endIndex = match.Index + match.Length;
            
            report = report.Remove(startIndex, endIndex - startIndex + 1);
        }

        return report;
    }

    private async Task<byte[]?> GenerateWordReportAsync(ThreatModel threatModel)
    {
        var wordTemplate = await _memoryCache.GetOrCreateAsync(WordTemplateCacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(24));
            var templateContent = await _reportsRepository.GetTemplateAsync(ReportType.Word);
            if (templateContent is null)
            { 
                var file = await _gitHubRepository.GetFileAsync(GitHubAccountName, GitHubRepositoryName, GitHubWordThreatModelTemplateFilePath);
                templateContent = file.BinaryContent;
                await _reportsRepository.StoreTemplateAsync(ReportType.Word, templateContent);
            }
            return templateContent;
        });
        if (wordTemplate is null)
        {
            return null;
        }

        var stream = new MemoryStream();
        stream.Write(wordTemplate, 0, wordTemplate.Length);

        await OpenXmlHelper.ReplaceAsync(stream, ProjectNamePlaceholder, threatModel.ProjectName);
        OpenXmlHelper.AddDataflowAttributes(stream, threatModel.DataflowAttributes);

        IDictionary<string, IEnumerable<SecurityBenchmark>>? benchmarks = threatModel.AddResourcesRecommendations && threatModel.Resources is not null
            ? (await Task.WhenAll(
                threatModel.Resources.Select(async r => new KeyValuePair<string, IEnumerable<SecurityBenchmark>>(r, await _recommendationsService.GetBenchmarksAsync(r)))
              )).ToDictionary(pair => pair.Key, pair => pair.Value)
            : null;
        OpenXmlHelper.AddThreats(stream, threatModel.Threats, benchmarks);

        if (threatModel.Images is not null)
        {
            OpenXmlHelper.RemoveParagraphForUnusedImages(stream, threatModel.Images);
            foreach (var image in threatModel.Images)
            {
                var imageContent = await _reportsRepository.GetFileAsync(threatModel.Id, image.Value);
                if (imageContent is null)
                {
                    continue;
                }
                OpenXmlHelper.AddImage(stream, image.Key, image.Value, imageContent);
            }
        }
        
        return stream.ToArray();
    }

    private async Task<Category> GetRecommendationsFromGitHubAsync()
    {
        var directory = await _gitHubRepository.GetContentAsync(GitHubAccountName, GitHubRepositoryName, GitHubThreatModelFolderName);

        return MapDirectoryToCategory(directory);
    }

    private Category MapDirectoryToCategory(GitHubDirectory directory)
    {
        var markdownTemplateFilename = Path.GetFileName(GitHubMarkdownThreatModelTemplateFilePath);
        var wordTemplateFilename = Path.GetFileName(GitHubWordThreatModelTemplateFilePath);
        return new Category(
            GenerateIdFor(directory.Url),
            directory.Name,
            "",
            directory.Directories?.Select(d => MapDirectoryToCategory(d)).ToList(),
            directory.Files?.Where(f => f.Name != markdownTemplateFilename && f.Name != wordTemplateFilename && f.Name.EndsWith(".md")).Select(f => MapFileToRecommendation(f)).ToList()
        );
    }

    private static string GenerateIdFor(string text)
    {
        return string.Join("", (SHA1.HashData(Encoding.UTF8.GetBytes(text))).Select(b => b.ToString("x2")));
    }

    private Recommendation MapFileToRecommendation(GitHubFile file)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
        var threatMmappingFilePath = Path.Combine(Path.GetDirectoryName(file.Url)!, fileNameWithoutExtension + GitHubThreatMappingFileSuffix);
        IEnumerable<string> benchmarkIds = null;
        if (File.Exists(threatMmappingFilePath))
        {
            var mappingFileContent = File.ReadAllText(threatMmappingFilePath);
            benchmarkIds = mappingFileContent.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
        }

        return new Recommendation(
            GenerateIdFor(file.Url),
            fileNameWithoutExtension,
            file.Content ?? "",
            benchmarkIds
        );
    }
}
