﻿using System;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

#nullable enable

namespace FixUpExhibitPages {

    internal class PageFixer {

        private const string FIXUP_HINT = "Run FixUpExhibitPages.exe to fill in";
        private static readonly Uri WEST_IMAGES_BASE_URI = new Uri("https://west.aldaviva.com/exhibits/images/");

        private readonly IDocument document;

        internal Func<DateTime> currentTimeProvider { private get; set; } = () => DateTime.Now;

        public PageFixer(IDocument document) {
            this.document = document;
        }

        public void fixTitle() {
            string title = document.Head.QuerySelector("title").TextContent;
            if (!string.IsNullOrWhiteSpace(title)) {
                // copy title to og title
                PageManipulator.upsertHeadElement(document, "meta", "property", "og:title", title);
                // copy title to h1
                PageManipulator.replaceTemplateText(document, document.QuerySelector("main article header h1"), "TitleDerived", title);
            }
        }

        public void fixDescription() {
            string description = document.Head.QuerySelector(@"meta[name = 'Description']")?.GetAttribute(PageManipulator.CONTENT) ??
                                 "";
            // copy description to og description
            if (!string.IsNullOrWhiteSpace(description)) {
                PageManipulator.upsertHeadElement(document, "meta", "property", "og:description", description);
                // copy description to header p
                PageManipulator.replaceTemplateText(document, document.QuerySelector("main article header p"), "SubtitleDerived",
                    description);
            }
        }

        public void fixTime() {
            // fill in article time and its datetime
            IElement timeEl = document.QuerySelector("main article > time");
            DateTime now = currentTimeProvider();
            if (string.IsNullOrWhiteSpace(timeEl.GetAttribute("datetime"))) {
                timeEl.SetAttribute("datetime", now.ToString("O"));
            }

            if (string.IsNullOrWhiteSpace(timeEl.TextContent) || timeEl.TextContent == FIXUP_HINT) {
                timeEl.TextContent = now.ToLongDateString();
            }

            // copy time datetime to title
            timeEl.SetAttribute("title", timeEl.GetAttribute("datetime"));
        }

        public void fixImageSources() {
            // rewrite relative img src to be resolved against https://west.aldaviva.com/exhibits/images/
            foreach (IHtmlImageElement imageEl in document.QuerySelectorAll<IHtmlImageElement>("img")) {
                var imageSourceUri = new Uri(imageEl.GetAttribute("src"), UriKind.RelativeOrAbsolute);
                if (!imageSourceUri.IsAbsoluteUri) {
                    imageEl.Source = new Uri(WEST_IMAGES_BASE_URI, imageSourceUri).ToString();
                }
            }
        }

        public void fixImageAlternateText() {
            // copy figcaptions to figure img alt
            foreach (IElement figureEl in document.QuerySelectorAll("figure")) {
                IElement imageEl = figureEl.QuerySelector("img");
                if (imageEl != null && string.IsNullOrWhiteSpace(imageEl.GetAttribute("alt"))) {
                    IElement captionEl = figureEl.QuerySelector("figcaption");
                    imageEl.SetAttribute("alt", captionEl.TextContent);
                }
            }
        }

    }

}