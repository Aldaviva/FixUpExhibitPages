using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;

namespace FixUpExhibitPages {

    public static class FixUpExhibitPages {

        private static readonly Uri WEST_IMAGES_BASE_URI = new Uri("https://west.aldaviva.com/exhibits/images/");

        public static async Task Main(string[] args) {
            if (args.Length >= 1) {
                Environment.CurrentDirectory = args[0];
            }

            IList<string> htmlFilenames = Directory.EnumerateFiles(".", "*.html", SearchOption.TopDirectoryOnly).ToList();
            Console.WriteLine($"Found {htmlFilenames.Count} HTML files in {Directory.GetCurrentDirectory()}");

            IConfiguration angleSharpConfig = Configuration.Default;
            IBrowsingContext browsingContext = BrowsingContext.New(angleSharpConfig);

            IList<Task> tasks = (from htmlFilename in htmlFilenames
                let fileStream = File.OpenRead(htmlFilename)
                select browsingContext.OpenAsync(req => req.Content(fileStream))
                    .ContinueWith(task => {
                        fileStream.Dispose();
                        IDocument document = task.Result;

                        try {
                            FixUpDocument(document);
                        } catch (Exception e) {
                            Console.WriteLine($"Error while editing {htmlFilename}");
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            throw;
                        }

                        File.WriteAllText(htmlFilename, document.ToHtml(), Encoding.UTF8);
                        Console.WriteLine($"Saved {Path.GetFileName(htmlFilename)}");
                    })).ToList();

            foreach (Task taskToAwait in tasks) {
                await taskToAwait;
            }
        }

        private static void FixUpDocument(IDocument document) {
            string title = document.Head.QuerySelector("title").TextContent;
            if (!string.IsNullOrWhiteSpace(title)) {
                // copy title to og title
                PageManipulator.UpsertHeadElement(document, "meta", "property", "og:title", title);
                // copy title to h1
                PageManipulator.ReplaceTemplateText(document, document.QuerySelector("main article header h1"), "TitleDerived", title);
            }

            string description = document.Head.QuerySelector(@"meta[name = 'Description']")?.GetAttribute(PageManipulator.CONTENT) ??
                                 "";
            // copy description to og description
            if (!string.IsNullOrWhiteSpace(description)) {
                PageManipulator.UpsertHeadElement(document, "meta", "property", "og:description", description);
                // copy description to header p
                PageManipulator.ReplaceTemplateText(document, document.QuerySelector("main article header p"), "SubtitleDerived",
                    description);
            }

            // fill in article time and its datetime
            IElement timeEl = document.QuerySelector("main article > time");
            DateTime now = DateTime.Now;
            if (string.IsNullOrWhiteSpace(timeEl.GetAttribute("datetime"))) {
                timeEl.SetAttribute("datetime", now.ToString("O"));
            }

            if (string.IsNullOrWhiteSpace(timeEl.TextContent)) {
                timeEl.TextContent = now.ToLongDateString();
            }

            // copy time datetime to title
            timeEl.SetAttribute("title", timeEl.GetAttribute("datetime"));

            // rewrite relative img src to be resolved against https://west.aldaviva.com/exhibits/images/
            foreach (IElement element in document.QuerySelectorAll("img")) {
                var imageEl = (IHtmlImageElement) element;
                var imageSourceUri = new Uri(imageEl.Source, UriKind.RelativeOrAbsolute);
                if (!imageSourceUri.IsAbsoluteUri) {
                    var absoluteWestUri = new Uri(WEST_IMAGES_BASE_URI, imageSourceUri);
                    imageEl.Source = absoluteWestUri.ToString();
                }
            }

            // copy figcaptions to figure img alt
            foreach (IElement figureEl in document.QuerySelectorAll("figure")) {
                IElement imageEl = figureEl.QuerySelector("img");
                IElement captionEl = figureEl.QuerySelector("figcaption");
                if (imageEl != null && string.IsNullOrWhiteSpace(imageEl.GetAttribute("alt"))) {
                    imageEl.SetAttribute("alt", captionEl.TextContent);
                }
            }
        }

    }

}