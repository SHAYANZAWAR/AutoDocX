using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Processes;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace docx
{
    public static class _docx
    {
        static void CreateFile(string fileName)
        {
            using (WordprocessingDocument doc =
            WordprocessingDocument.Create(fileName, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                mainPart.Document.Save();
            }
        }
        public static void appendToDocX(bool fileFound, bool isMultipleFile, string wordFileName, string codeFilePath, string outputHeading, string imagePath)
        {
            // if the file does not exist then create it, 
            if (!fileFound)
            {
                CreateFile(wordFileName);
            }
            // if it exists then its gonna be opened.
            using (WordprocessingDocument doc = WordprocessingDocument.Open(wordFileName, true))
            {

                MainDocumentPart? mainPart = doc.MainDocumentPart;
                if (mainPart == null || mainPart.Document.Body == null)
                {
                    return;
                }

                // Extracting code out of the codeFileName
                string codeText = File.ReadAllText(codeFilePath);
                // Console.WriteLine("got here");
                string codeFileName = Path.GetFileName(codeFilePath);
                string? directoryPath = Path.GetDirectoryName(codeFilePath);
                string customIDValue = GetFileNameWithoutExtension(codeFileName);
                // Append the paragraph to the body of the document
                Body body = mainPart.Document.Body;
                // name of the file provided will be the ID
                if (!isMultipleFile)
                {
                    addParagraph(body, null, outputHeading, customIDValue + "_heading", "18", true, "000000");
                    addParagraph(body, null, codeText, customIDValue + "_code");

                }
                else
                {
                    // Console.WriteLine("got here too");
                    string[] fileNames = _Processes.getFileDependencies(codeFilePath);
                    foreach (string fileName in fileNames)
                    {
                        string fileNameWithoutExtension = GetFileNameWithoutExtension(fileName);
                        if (directoryPath == null || directoryPath == "")
                        {

                            codeText = File.ReadAllText(fileName);
                            addParagraph(body, null, fileName + ":", fileNameWithoutExtension + "_heading", "18", true, "000000");
                            addParagraph(body, null, codeText, fileNameWithoutExtension + "_code");

                        }
                        else
                        {
                            string currentPath = Path.Combine(directoryPath, fileName);
                            codeText = File.ReadAllText(currentPath);
                            addParagraph(body, null, fileName + ":", fileNameWithoutExtension + "_heading", "18", true, "000000");
                            addParagraph(body, null, codeText, fileNameWithoutExtension + "_code");
                        }

                    }

                }

                Paragraph paraWithImage = new Paragraph();

                AddImageToBody(mainPart, paraWithImage, imagePath, customIDValue + "_img");

                // Append the paragraph to the document
                mainPart.Document.Body.Append(paraWithImage);


                // Save changes to the document
                mainPart.Document.Save();

            }
        }


        // string currentPath = (directoryPath != null) ? Path.Combine(directoryPath, fileName) : fileName;
        // Console.WriteLine(currentPath);
        // codeText = File.ReadAllText(currentPath);


        // Console.WriteLine("got here");
        // addParagraph(body, fileName + ":", customIDValue + "_heading", "18", true, "000000");
        // addParagraph(body, codeText, customIDValue + "_code");


        public static void updateDocX(bool isMultipleFile, string wordFileName, string oldFileName, string newFilePath)
        {
            string oldFileNameWithoutExtension = GetFileNameWithoutExtension(oldFileName);
            string newFileName = Path.GetFileName(newFilePath);
            string? currentDirPath = Directory.GetCurrentDirectory();
            string? newFileDirPath = Path.GetDirectoryName(newFilePath);
            currentDirPath = Path.Combine(currentDirPath, newFileDirPath ?? "");
            string newFileNameWithoutExtension = GetFileNameWithoutExtension(newFileName);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(wordFileName, true))
            {

                MainDocumentPart? mainPart = doc.MainDocumentPart;
                bool imageFound = false;



                if (mainPart == null)
                {
                    return;
                }
                Document document = mainPart.Document;

                string[] oldDeps = _Processes.getFileDependencies(oldFileName);
                foreach (string filename in oldDeps)
                {
                    Console.WriteLine(filename);
                }
                // if the old file has multiple files that 
                // are in the wordFileName
                if (isMultipleFile)
                {
                    string[] newDeps = _Processes.getFileDependencies(newFilePath);
                    Console.WriteLine("file dependencies are; ");
                    foreach (string filename in newDeps)
                    {
                        Console.WriteLine(filename);
                    }
                    var oldDepLen = oldDeps.Length;
                    var newDepLen = newDeps.Length;
                    Console.WriteLine("old deps: " + oldDepLen);
                    Console.WriteLine("new deps: " + newDepLen);

                    if (oldDepLen == newDepLen)
                    {
                        Console.WriteLine("They have the same number of dependecies");
                        for (int i = 0; i < oldDepLen; i++)
                        {
                            searchAndUpdate(mainPart, oldDeps[i], newFileDirPath + newDeps[i]);
                        }

                    }
                    else if (oldDepLen < newDepLen)
                    {
                        Console.WriteLine("new file has more dependencies");
                        var oldDepsLength = oldDepLen;
                        var insertionIndex = 0;
                        for (int i = 0; i < newDepLen; i++)
                        {
                            if (oldDepsLength > 0)
                            {

                                insertionIndex = searchAndUpdate(mainPart, oldDeps[i], newFileDirPath + newDeps[i]);

                            }
                            else
                            {
                                string content = File.ReadAllText(newFileDirPath + newDeps[i]);
                                Body? body = mainPart.Document.Body;
                                if (body != null)
                                {
                                    addParagraph(body, insertionIndex, newDeps[i] + ":", newFileNameWithoutExtension + "_heading", "18", true);
                                    addParagraph(body, insertionIndex + 1, content, newFileNameWithoutExtension + "_code");
                                }
                            }

                            oldDepsLength--;
                        }
                    }

                    // else
                    // {
                    //     Console.WriteLine("old has more than the new one");
                    //     var newDepsLength = newDepLen;

                    //     foreach (string fileName in oldDeps)
                    //     {
                    //         if (newDepsLength > 0)
                    //         {

                    //             searchAndUpdate(mainPart, oldFileName, newFilePath);
                    //         }
                    //         else
                    //         {
                    //             string fileNameWithoutExtension = GetFileNameWithoutExtension(fileName);
                    //             RemovePara(mainPart, fileNameWithoutExtension + "_heading", false);
                    //             RemovePara(mainPart, fileNameWithoutExtension + "_code", false);
                    //         }

                    //         newDepsLength--;
                    //     }

                    // }


                }

            }

            // images are wrapped in paragraph element
            int imageIndex = SearchParagraph(document, oldFileNameWithoutExtension + "_img");
            if (imageIndex != -1 && (imageFound = true)) { }
            if (imageFound)
            {
                Console.WriteLine("image paragraph is also found");
                Paragraph? paragraph = GetPara(document, imageIndex);

                if (paragraph != null)
                {

                    paragraph.RemoveAllChildren();
                    AddImageToBody(mainPart, paragraph, newFileNameWithoutExtension + ".png", newFileNameWithoutExtension + "_img");

                }

            }

        }


    }

    private static int searchAndUpdate(MainDocumentPart mainPart, string oldFileName, string newFilePath)
    {
        Console.WriteLine("Path in the search and update function" + newFilePath);

        string oldFileNameWithoutExtension = GetFileNameWithoutExtension(oldFileName);
        string newFileName = Path.GetFileName(newFilePath);
        string newFileNameWitoutExtension = GetFileNameWithoutExtension(newFileName);
        Document document = mainPart.Document;

        bool headingFound = false, contentParaFound = false;
        // searching the heading with id (oldFileName + "_heading)
        int headingParaIndex = SearchParagraph(document, oldFileNameWithoutExtension + "_heading");
        int contentParaIndex = SearchParagraph(document, oldFileNameWithoutExtension + "_code");
        // images are wrapped in paragraph element



        if (headingParaIndex != -1 && (headingFound = true)
        && contentParaIndex != -1 && (contentParaFound = true)) { }


        if (headingFound)
        {
            Paragraph? headingPara = GetPara(document, headingParaIndex);
            if (headingPara != null)
            {
                updateParagraph(headingPara, newFileNameWitoutExtension + ":", newFileNameWitoutExtension + "_heading", "18", true, "000000");
            }
        }
        if (contentParaFound)
        {
            Paragraph? contentPara = GetPara(document, contentParaIndex);
            string codeText = File.ReadAllText(newFilePath);
            if (contentPara != null)
            {
                updateParagraph(contentPara, codeText, newFileNameWitoutExtension + "_code");
            }

        }

        return contentParaIndex;

    }
    public static void removeFromDocX(string wordFilePath, string codeFilePath)
    {


        // getting the filename without file type extension
        string mainFile = GetFileNameWithoutExtension(Path.GetFileName(codeFilePath));
        string[] fileDeps = _Processes.getFileDependencies(codeFilePath);
        using (WordprocessingDocument doc = WordprocessingDocument.Open(wordFilePath, true))
        {

            var mainPart = doc.MainDocumentPart;

            if (mainPart == null) return;

            foreach (string fileName in fileDeps)
            {
                RemovePara(mainPart, GetFileNameWithoutExtension(fileName) + "_heading", true);
                RemovePara(mainPart, GetFileNameWithoutExtension(fileName) + "_code", true);
            }

            RemovePara(mainPart, mainFile + "_img", true);


        }


    }




    static void AddImageToBody(MainDocumentPart mainPart, Paragraph paragraph, string imagePath, string customIdValue)
    {
        ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (FileStream stream = new FileStream(imagePath, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }

        AddImage(mainPart, paragraph, mainPart.GetIdOfPart(imagePart), customIdValue);

    }

    static void AddImage(MainDocumentPart mainPart, Paragraph paragraph, string relationshipId, string customId)
    {
        if (mainPart == null || mainPart.Document == null ||
        mainPart.Document.Body == null || string.IsNullOrEmpty(relationshipId)
        || string.IsNullOrEmpty(customId))
        {
            return;
        }

        setAttribute(paragraph, "customID", customId);
        // Create an inline drawing within the paragraph
        Drawing drawing = new Drawing(
            new DW.Inline(
                new DW.Extent() { Cx = 3990000L, Cy = 2700000L },
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
                                    Name = "New Bitmap Image.png"
                                },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip(
                                    new A.BlipExtensionList(
                                        new A.BlipExtension()
                                        {
                                            Uri =
                                            "{28A0092B-C50C-407E-A947-70E740481C1C}"
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
                            new A.Extents() { Cx = 990000L, Cy = 792000L }),
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
            });

        // Append the inline drawing (which includes the ImagePart) to the paragraph
        paragraph.Append(drawing);


    }

    static void addParagraph(Body mainBody, int? insertIndex, string value, string customID,
    string fontSize = "12", bool isBold = false, string fontColor = "000000")
    {
        // creating the paragraph
        Paragraph para = new Paragraph();
        setAttribute(para, "customID", customID);

        string[] lines = value.Split('\n');

        Run run = new Run();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                run.AppendChild(new Break()); // Insert a line break between lines
            }

            run.AppendChild(new Text(lines[i]));
        }

        // styling the run based on the given arguments
        RunProperties runProperties = new RunProperties();

        // setting font size
        FontSize fz = new FontSize() { Val = fontSize };
        runProperties.Append(fz);

        // setting boldness

        if (isBold)
        {
            Bold bold = new Bold();

            runProperties.Append(bold);
        }

        //setting the colour
        Color color = new Color() { Val = fontColor };
        runProperties.Append(color);

        // appending all the properties to the paragraph
        run.RunProperties = runProperties;
        para.Append(run);

        if (insertIndex == null) mainBody.Append(para);
        else
        {
            mainBody.InsertAt(para, (int)insertIndex);
        }

    }


    static void updateParagraph(Paragraph para, string newContent, string newCustomId, string fontSize = "12", bool isBold = false, string fontColor = "000000")
    {


        // preserving the original properties of paragraph (font, color, size)
        ParagraphProperties? oldProps = null;
        // if (para.ParagraphProperties != null)
        oldProps = para.ParagraphProperties;
        // clear the existing content
        para.RemoveAllChildren();

        // append the new content to the paragraph
        string[] lines = newContent.Split('\n');


        Run run = new Run();
        // styling the run based on the given arguments
        RunProperties runProperties = new RunProperties();

        // setting font size
        FontSize fz = new FontSize() { Val = fontSize };
        runProperties.Append(fz);

        // setting boldness

        if (isBold)
        {
            Bold bold = new Bold();

            runProperties.Append(bold);
        }

        //setting the colour
        Color color = new Color() { Val = fontColor };
        runProperties.Append(color);

        run.RunProperties = runProperties;

        if (lines.Length != 1)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    run.AppendChild(new Break()); // Insert a line break between lines
                }

                run.AppendChild(new Text(lines[i]));
            }
        }
        else
        {
            run.AppendChild(new Text(lines[0]));
        }

        para.AppendChild(run);


        // updating the customIDs of the paragraphs
        setAttribute(para, "customID", newCustomId);
    }


    public static void RemovePara(MainDocumentPart mainPart, string customID, bool recursive)
    {
        Console.WriteLine(customID);
        var document = mainPart.Document;
        var paragraphs = document.Descendants<Paragraph>();

        foreach (var paragraph in paragraphs)
        {

            var customIDValue = GetAttributeValue(paragraph, "customID");
            // Console.WriteLine("Custom id value: " + customIDValue);
            if (!string.IsNullOrEmpty(customIDValue) && customIDValue == customID)
            {
                if (recursive || paragraph == paragraphs.First())
                {

                    paragraph.Remove();
                }
            }

        }

    }
    static int SearchParagraph(Document document, string targetCustomID)
    {
        int index = 0;
        foreach (var paragraph in document.Descendants<Paragraph>())
        {
            string? customID = GetAttributeValue(paragraph, "customID");
            if (customID == targetCustomID)
            {
                return index;
            }
            index++;
        }
        return -1; // Custom ID not found
    }

    static Paragraph? GetPara(Document document, int targetIndex)
    {
        int currentIndex = 0;
        foreach (var paragraph in document.Descendants<Paragraph>())
        {
            if (currentIndex == targetIndex)
            {
                return paragraph;
            }
            currentIndex++;
        }
        return null; // Paragraph at the target index not found
    }
    static void setAttribute(OpenXmlElement elm, string attributeName, string attributeValue)
    {
        if (!String.IsNullOrEmpty(attributeName) && !String.IsNullOrEmpty(attributeValue))
        {
            elm.SetAttribute(new OpenXmlAttribute(attributeName, "", attributeValue));
        }
    }


    static string? GetAttributeValue(OpenXmlElement element, string attributeName)
    {
        var attribute = element.GetAttributes().FirstOrDefault(attr => attr.LocalName == attributeName);
        return attribute.Value;
    }

    private static string GetFileNameWithoutExtension(string filePath)
    {
        int extensionIndex = filePath.LastIndexOf('.');
        Console.WriteLine(filePath.Substring(extensionIndex));
        if (extensionIndex >= 0)
        {
            return filePath.Substring(0, extensionIndex);
        }

        // No extension found, return the original string
        return filePath;
    }


}
}