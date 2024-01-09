using Crisp.Core.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace Crisp.Core.Helpers;

public static class OpenXmlHelper
{
    private enum ParagraphPartType
    {
        Text,
        Link,
        Style,
    }

    private record ParagraphPart(
        ParagraphPartType Type,
        string? Text,
        string? Style
    );

    public static async Task ReplaceAsync(Stream stream, string placeholder, string replacement)
    {
        using var document = WordprocessingDocument.Open(stream, isEditable: true);
        string? documentContent = null;
        using (var reader = new StreamReader(document.MainDocumentPart.GetStream()))
        {
            documentContent = await reader.ReadToEndAsync();
        }

        var regex = new Regex(Regex.Escape(placeholder));
        documentContent = regex.Replace(documentContent, replacement);

        using var writer = new StreamWriter(document.MainDocumentPart.GetStream(FileMode.Create));
        await writer.WriteAsync(documentContent);
    }

    public static void AddDataflowAttributes(Stream stream, IEnumerable<DataflowAttribute> attributes)
    {
        using var document = WordprocessingDocument.Open(stream, isEditable: true);

        var tableElement = document.MainDocumentPart.Document.Body.Descendants<Table>().First();

        foreach (var attribute in attributes)
        {
            tableElement.Append(
                new TableRow(new[] {
                    new TableCell(new Paragraph(new Run(new Text(attribute.Number)))),
                    new TableCell(new Paragraph(new Run(new Text(attribute.Transport)))),
                    new TableCell(new Paragraph(new Run(new Text(attribute.DataClassification)))),
                    new TableCell(new Paragraph(new Run(new Text(attribute.Authentication)))),
                    new TableCell(new Paragraph(new Run(new Text(attribute.Authorization)))),
                    new TableCell(new Paragraph(new Run(new Text(attribute.Notes))))
                })
            );
        }
    }

    public static void AddThreats(Stream stream, IEnumerable<Recommendation> threats)
    {
        using var document = WordprocessingDocument.Open(stream, isEditable: true);
        var body = document.MainDocumentPart.Document.Body;
        
        var header = FindParagraph(body, "Threats and Mitigations");
        if (header is null)
        {
            return;
        }

        var threatIndex = threats.Count();
        foreach (var threat in threats.ToArray().Reverse())
        {
            var paragraphs = new List<Paragraph>
            {
                GetHorizontalLine(),
                new Paragraph(
                    new Run(
                        new RunProperties(new Bold()),
                        new Text("Threat #:")
                    ),
                    new Run(new Text($" {threatIndex}") { Space = SpaceProcessingModeValues.Preserve })
                )
            };
            paragraphs.AddRange(GetParagraphsFromMarkdown(threat.Description));
            foreach (var paragraph in paragraphs.ToArray().Reverse())
            {
                var hyperlinks = paragraph.Descendants<Hyperlink>();
                foreach (var hyperlink in hyperlinks)
                {
                    var uri = new Uri(hyperlink.DocLocation);
                    var relationship = document.MainDocumentPart.AddHyperlinkRelationship(uri, true);
                    hyperlink.Id = relationship.Id;
                    hyperlink.DocLocation = "";
                }
                header.InsertAfterSelf(paragraph);
            }
            threatIndex--;
        }
    }

    public static void AddImage(Stream stream, string imageType, string fileName, byte[] fileContent)
    {
        var paragraphText = GetParagraphTextFor(imageType);
        if (string.IsNullOrEmpty(paragraphText))
        {
            return;
        }

        (var imageWidth, var imageHeight) = GetImageSize(fileContent);

        using var document = WordprocessingDocument.Open(stream, isEditable: true);
        var body = document.MainDocumentPart.Document.Body;

        var header = FindParagraph(body, paragraphText);
        if (header is null)
        {
            return;
        }

        var imagePartType = System.IO.Path.GetExtension(fileName)[1..].ToLower() switch
        {
            "jpg" or "jpeg" => ImagePartType.Jpeg,
            "gif" => ImagePartType.Gif,
            _ => ImagePartType.Png
        };
        var imagePart = document.MainDocumentPart.AddImagePart(imagePartType);
        using var imageStream = new MemoryStream(fileContent);
        imagePart.FeedData(imageStream);
        var imageElement = GetImageElement(document.MainDocumentPart.GetIdOfPart(imagePart), imageWidth, imageHeight);
        header.InsertAfterSelf(new Paragraph(new Run(imageElement)));
    }

    public static void RemoveParagraphForUnusedImages(Stream stream, IDictionary<string, string> images)
    {
        if (!images.Any() || images.Count == 3)
        {
            return;
        }

        using var document = WordprocessingDocument.Open(stream, isEditable: true);
        var body = document.MainDocumentPart.Document.Body;
        
        var imageTypes = new string[] { "arch", "map"};
        foreach (var imageType in imageTypes)
        {
            if (images.ContainsKey(imageType))
            {
                continue;
            }
            
            var paragraphText = GetParagraphTextFor(imageType);
            if (string.IsNullOrEmpty(paragraphText))
            {
                continue;
            }

            RemoveParagraph(body, paragraphText);
        }
    }

    public static void RemoveParagraph(Body body, string paragraphText)
    {
        var paragraph = FindParagraph(body, paragraphText);
        if (paragraph is null)
        {
            return;
        }

        paragraph.RemoveAllChildren();
        paragraph.Remove();
    }

    private static string GetParagraphTextFor(string imageType)
    {
        return imageType.ToLower() switch
        {
            "arch" => "Architecture Diagram",
            "flow" => "Data Flow Diagram",
            "map" => "Threat Map",
            _ => ""
        };
    }

    private static (long imageWidth, long imageHeight) GetImageSize(byte[] fileContent)
    {
        long width = 0;
        long height = 0;
        using (var image = SixLabors.ImageSharp.Image.Load(fileContent))
        {
            width = image.Width;
            height = image.Height;
        }

        width = (long)Math.Round((decimal)width * 9525);
        height = (long)Math.Round((decimal)height * 9525);

        double maxWidthCm = 17.4; // Our current margins gives us 17.4cm of space
        long maxWidthEmus = (long)(maxWidthCm * 360000);
        if (width > maxWidthEmus)
        {
            var ratio = (height * 1.0m) / width;
            width = maxWidthEmus;
            height = (long)(width * ratio);
        }
        
        return (width, height);
    }

    private static Drawing GetImageElement(string relationshipId, long imageWidth, long imageHeight)
    {
        return new Drawing(
            new DW.Inline(
                new DW.Extent() { Cx = imageWidth, Cy = imageHeight },
                new DW.EffectExtent()
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 0L,
                    BottomEdge = 0L
                },
                new DW.DocProperties()
                {
                    Id = (UInt32Value)1U,
                    Name = "Picture 1"
                },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks() { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties()
                                {
                                    Id = (UInt32Value)0U,
                                    Name = "New Bitmap Image.jpg"
                                },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip(
                                    new A.BlipExtensionList(
                                        new A.BlipExtension()
                                        {
                                            Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                        })
                                )
                                {
                                    Embed = relationshipId,
                                    CompressionState =
                                    A.BlipCompressionValues.Print
                                },
                                new A.Stretch(
                                    new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset() { X = 0L, Y = 0L },
                                    new A.Extents() { Cx = imageWidth, Cy = imageHeight }),
                                new A.PresetGeometry(
                                    new A.AdjustValueList()
                                )
                                { Preset = A.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
            )
            {
                DistanceFromTop = (UInt32Value)0U,
                DistanceFromBottom = (UInt32Value)0U,
                DistanceFromLeft = (UInt32Value)0U,
                DistanceFromRight = (UInt32Value)0U,
                EditId = "50D07946"
            }
        );
    }

    private static IEnumerable<Paragraph> GetParagraphsFromMarkdown(string markdown)
    {
        var paragraphs = new List<Paragraph>();
        var lines = markdown.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            var text = line;
            var paragraph = new Paragraph();
            if (line.StartsWith('#')) {
                var level = line.TakeWhile(c => c == '#').Count();
                paragraph.Append(new ParagraphProperties(new ParagraphStyleId { Val = $"Heading{level}" }));
                text = line[level..];
            }

            var parts = new List<ParagraphPart>();
            var styles = new Stack<string>();
            var currentTextPart = "";
            for (int i = 0; i < text.Length; i++)
            {
                char? previousCharacter = i > 0 ? text[i - 1] : null;
                var currentCharacter = text[i];
                char? nextCharacter = i < text.Length - 1 ? text[i + 1] : null;
                
                if (currentCharacter == '[' && (previousCharacter is null || previousCharacter != '\\'))
                {
                    var regex = new Regex(@"\[(?<text>[^\]]+)\]\((?<url>[^\)]+)\)");
                    var match = regex.Match(text[i..]);
                    if (match.Success && match.Index == 0)
                    {
                        if (!string.IsNullOrEmpty(currentTextPart))
                        {
                            parts.Add(new ParagraphPart(ParagraphPartType.Text, currentTextPart, null));
                            currentTextPart = "";
                        }
                        parts.Add(new ParagraphPart(ParagraphPartType.Link, match.Groups["text"].Value, match.Groups["url"].Value));
                        i += match.Length;
                    }
                } 
                else if ((currentCharacter == '`' || currentCharacter == '*' || currentCharacter == '_') && (previousCharacter is null || previousCharacter != '\\'))
                {
                    if (!string.IsNullOrEmpty(currentTextPart))
                    {
                        parts.Add(new ParagraphPart(ParagraphPartType.Text, currentTextPart, null));
                        currentTextPart = "";
                    }
                    if (currentCharacter == '`')
                    {
                        var closeIndex = text.IndexOf('`', i + 1);
                        parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "<c>"));
                        if (closeIndex < 0)
                        {
                            closeIndex = text.Length;
                        }
                        parts.Add(new ParagraphPart(ParagraphPartType.Text, text[(i + 1)..(closeIndex - 1)], null));
                        parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "</c>"));
                        i = closeIndex + 1;
                    } 
                    else if (currentCharacter == '*' || currentCharacter == '_')
                    { 
                        if (nextCharacter == '*' || nextCharacter == '_')
                        {
                            if (styles.Contains("bold") && styles.Peek() == "bold")
                            {
                                styles.Pop();
                                parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "</b>"));
                            }
                            else
                            {
                                styles.Push("bold");
                                parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "<b>"));
                            }
                            i += 2;
                        }
                        else
                        {
                            if (styles.Contains("italic") && styles.Peek() == "italic")
                            {
                                styles.Pop();
                                parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "</i>"));
                            }
                            else
                            {
                                styles.Push("italic");
                                parts.Add(new ParagraphPart(ParagraphPartType.Style, null, "<i>"));
                            }
                            i += 1;
                        }
                    }
                }
                if (i < text.Length)
                {
                    currentTextPart += text[i];
                }
            }

            if (!string.IsNullOrEmpty(currentTextPart))
            {
                parts.Add(new ParagraphPart(ParagraphPartType.Text, currentTextPart, null));
            }


            var currentStyle = new List<string>();
            foreach (var part in parts)
            {
                if (part.Type == ParagraphPartType.Style)
                {
                    if (part.Style!.StartsWith("</"))
                    {
                        currentStyle.Remove(part.Style[2].ToString());
                    }
                    else
                    {
                        currentStyle.Add(part.Style[1].ToString());
                    }
                }
                else if (part.Type == ParagraphPartType.Text || part.Type == ParagraphPartType.Link)
                {
                    var run = new Run();
                    if (currentStyle.Any() || part.Type == ParagraphPartType.Link)
                    {
                        var runProperties = new RunProperties();
                        if (currentStyle.Contains("b"))
                        {
                            runProperties.Append(new Bold { Val = OnOffValue.FromBoolean(true) });
                        }
                        if (currentStyle.Contains("i"))
                        {
                            runProperties.Append(new Italic { Val = OnOffValue.FromBoolean(true) });
                        }
                        if (currentStyle.Contains("c"))
                        {
                            runProperties.Append(new RunFonts { Ascii = "Consolas" });
                            runProperties.Append(new Color() { Val = "#915100" });
                        }
                        if (part.Type == ParagraphPartType.Link)
                        {
                            runProperties.Append(new RunStyle { Val = "Hyperlink" });
                        }
                        run.Append(runProperties);
                    }
                    run.Append(new Text(part.Text) { Space = SpaceProcessingModeValues.Preserve });
                    if (part.Type == ParagraphPartType.Text)
                    {
                        paragraph.Append(run);
                    }
                    else
                    {
                        paragraph.Append(new Hyperlink(run) { DocLocation = part.Style });
                    }
                }
            }

            paragraphs.Add(paragraph);
        }
        return paragraphs;
    }
    
    private static Paragraph? FindParagraph(Body body, string text)
    {
        return body.Descendants<Paragraph>().Where(p => p.Descendants<Run>().Any(r => r.Descendants<Text>().Any(t => t.Text.ToLower() == text.ToLower()))).FirstOrDefault();
    }

    private static Paragraph GetHorizontalLine()
    {
        return new Paragraph(
            new ParagraphProperties(
                new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Color = "auto", Space = 1, Size = 6 }
                )
            )
        );
    }
}