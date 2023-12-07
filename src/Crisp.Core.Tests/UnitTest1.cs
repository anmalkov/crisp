using Crisp.Core.Helpers;
using Crisp.Core.Models;
using Crisp.Core.Repositories;
using Crisp.Core.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Text.RegularExpressions;

namespace Crisp.Core.Tests;

public class UnitTest1
{
    //[Fact]
    public async Task Test1()
    {
        var directoryName = "Security Domain";

        var httpClient = new HttpClient();
        var repository = new GitHubApiRepository(httpClient);

        var directory = await repository.GetContentAsync("anmalkov", "brief", directoryName);

        Assert.NotNull(directory);
        Assert.Equal(directoryName, directory.Name);
        Assert.NotNull(directory.Directories);
        Assert.True(directory.Directories.Count() > 0);
    }

    //[Fact]
    public async Task Test2()
    {
        var wordTemplate = File.ReadAllBytes("template.docx");

        var stream = new MemoryStream();
        stream.Write(wordTemplate, 0, wordTemplate.Length);

        using (var document = WordprocessingDocument.Open(stream, isEditable: true))
        {
            var body = document.MainDocumentPart.Document.Body;
            var tableElement = body.Descendants<Table>().First();
            for (int i = 0; i < 10; i++)
            {
                var row = new TableRow();
                row.Append(new TableCell(new Paragraph(new Run(new Text($"{i}-1")))));
                row.Append(new TableCell(new Paragraph(new Run(new Text($"{i}-2")))));
                row.Append(new TableCell(new Paragraph(new Run(new Text($"{i}-3")))));
                row.Append(new TableCell(new Paragraph(new Run(new Text($"{i}-4")))));
                row.Append(new TableCell(new Paragraph(new Run(new Text($"{i}-5")))));
                tableElement.Append(row);
            }
        }

        using (var document = WordprocessingDocument.Open(stream, isEditable: true))
        {
            string? documentContent = null;
            using (var reader = new StreamReader(document.MainDocumentPart.GetStream()))
            {
                documentContent = await reader.ReadToEndAsync();
            }

            var regex = new Regex(Regex.Escape("[tm-project-name]"));
            documentContent = regex.Replace(documentContent, "Test Project");

            using (var writer = new StreamWriter(document.MainDocumentPart.GetStream(FileMode.Create)))
            {
                await writer.WriteAsync(documentContent);
            }
        }

        using (var document = WordprocessingDocument.Open(stream, isEditable: true))
        {
            var body = document.MainDocumentPart.Document.Body;
            //var bookmark = body.Descendants<BookmarkStart>().First(b => b.Name == "tm_threat_properties");
            var header = body.Descendants<Paragraph>().Where(p => p.Descendants<Run>().Any(r => r.Descendants<Text>().Any(t => t.Text.ToLower() == "threats and mitigations"))).First();

            // hr
            var p1 = new Paragraph(new ParagraphProperties(new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Color = "auto", Space = 1, Size = 6 })));

            // threat #
            var p2 = new Paragraph();
            var r1 = new Run(new RunProperties(new Bold()));
            r1.Append(new Text("Threat #:"));
            var r2 = new Run(new Text(" 1"));
            r2.Append(new Break());
            p2.Append(r1);
            p2.Append(r2);

            header.InsertAfterSelf(p2);
            header.InsertAfterSelf(p1);
        }

        File.WriteAllBytes("result.docx", stream.ToArray());
    }

    //[Fact]
    public async Task Test3()
    {
        var recommendations = new List<Recommendation>
        {
            new Recommendation("4", "test 1", "**Principle:** Confidentiality and Integrity  \r\n**Affected Asset:** All services  \r\n**Threat:** Secrets leaking into unsecured locations are an easy way for adversaries to gain access to a system. These secrets can be used to either spoof the owners of these secrets or, in the case of encryption keys, use them to decrypt data.\r\n\r\n**Mitigation:**\r\n\r\nProper storage and management of secrets is critical in protecting systems from compromises, in most cases, with severe impact.\r\n\r\n1. Never store secrets in code, configuration files or databases. Instead, use a vault or any secure container (such as encrypted variables) to store secrets.\r\n2. Separate application secrets by environment.\r\n3. Rotate all secrets before turning over the application to the customer.\r\n\r\n- Store all secrets, encryption keys and certificates in Key Vault.\r\n- You can use multiple Key Vaults to separate secrets for different and critical services to minimize secrets leaking\r\n- Define and implement secrets rotation strategy. All items in the vault should have expiration dates."),
            new Recommendation("3", "test 1", "**Principle:** Confidentiality  \r\n**Affected Asset:** All services  \r\n**Threat:** Broken or non-existent authentication mechanisms may allow attackers to gain access to confidential information.\r\n\r\n**Mitigation:**\r\n\r\nAll services within the Azure Trust Boundary must authenticate all incoming requests, including requests coming from the same network. Proper authorizations should also be applied to prevent unnecessary privileges.\r\n\r\n1. Use Azure AD authentication for centralized identity management.\r\n2. Whenever available, use Azure Managed Identities to authenticate services. Service Principals may be used if Managed Identities are not supported.\r\n3. External users or services may use Username + Passwords, Tokens, or Certificates to authenticate, provided these are stored on Key Vault or any other vaulting solution.\r\n4. For authorization, use Azure RBAC to segregate duties and grant only the least amount of access to perform an action at a particular scope.\r\n5. Leverage AAD PIM for any administrative access.\r\n6. Avoid storing secrets in databases or configuration files."),
            new Recommendation("2", "test 1", "this is **bold and *italic* and** but this is \\*\\*not\\*\\* this is `new block` and this is \\`not a block\\`"),
            new Recommendation("1", "test 1", "this is [link test](http://www.google.com) and now **in bold [google](http://www.google.com?q=test&t=now) *italic* and bold**"),
        };

        var wordTemplate = File.ReadAllBytes("template.docx");

        var stream = new MemoryStream();
        stream.Write(wordTemplate, 0, wordTemplate.Length);

        OpenXmlHelper.AddThreats(stream, recommendations);

        File.WriteAllBytes("result2.docx", stream.ToArray());
    }

    [Fact]
    public async Task GetRecommendationsForResource()
    {
        var gitHubRepository = new GitHubGitRepository();
        var securityBenchmarksRepository = new SecurityBenchmarksV11Repository();
        var service = new RecommendationsService(gitHubRepository, securityBenchmarksRepository);

        var recommendations = await service.GetRecommendationsAsync(new[] { "Key Vault" });
    }
}