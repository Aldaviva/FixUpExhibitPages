using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Console = Colorful.Console;

#nullable enable

namespace FixUpExhibitPages {

    public static class FixUpExhibitPages {

        public static void Main(string[] args) {
            if (args.Length >= 1) {
                Environment.CurrentDirectory = args[0];
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            IList<string> htmlFilenames = Directory.EnumerateFiles(".", "*.html", SearchOption.TopDirectoryOnly).ToList();
            Console.WriteLine($"Found {htmlFilenames.Count} HTML files in {Directory.GetCurrentDirectory()}", Color.HotPink);

            IConfiguration angleSharpConfig = Configuration.Default;
            IBrowsingContext browsingContext = BrowsingContext.New(angleSharpConfig);

            Task.WaitAll((from htmlFilename in htmlFilenames
                          let fileStream = File.OpenRead(htmlFilename)
                          select browsingContext.OpenAsync(req => {
                                  req.Address("https://aldaviva.com/exhibits/fake.html");
                                  req.Content(fileStream);
                              })
                              .ContinueWith(task => {
                                  fileStream.Dispose();
                                  using IDocument document = task.Result;

                                  try {
                                      var pageFixer = new PageFixer(document);

                                      pageFixer.fixTitle();
                                      pageFixer.fixDescription();
                                      pageFixer.fixTime();
                                      pageFixer.fixImageSources();
                                      pageFixer.fixImageAlternateText();
                                  } catch (Exception e) {
                                      Console.WriteLine($"Error while editing {htmlFilename}");
                                      Console.WriteLine(e.Message);
                                      Console.WriteLine(e.StackTrace);
                                      throw;
                                  }

                                  using (TextWriter writer = new StreamWriter(htmlFilename, false, Encoding.UTF8)) {
                                      document.ToHtml(writer);
                                  }

                                  Console.WriteLine($"Saved {Path.GetFileName(htmlFilename)}", Color.DeepSkyBlue);
                              })).ToArray());

            stopwatch.Stop();
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds} ms.", Color.LawnGreen);
            Console.ReadKey();
        }

    }

}