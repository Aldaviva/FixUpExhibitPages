using System;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FixUpExhibitPages;
using FluentAssertions;
using Xunit;

namespace Test {

    public class FixUpExhibitPagesTest {

        private static readonly HtmlParser HTML_PARSER = new HtmlParser();

        private static IDocument CreateDocument(string source) {
            return HTML_PARSER.ParseDocument(source);
        }

        [Fact]
        public void OpenGraphTitle() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                        <title>A B C</title>
                        <!-- InstanceBeginEditable name=""head"" -->
                        <!-- InstanceEndEditable -->
                    </head>
                    <body>
                        <main>
                            <article>
                                <header>
                                    <h1><!-- InstanceBeginEditable name=""TitleDerived"" -->Title goes here<!-- InstanceEndEditable --></h1>
                                </header>
                            <article>
                        <main>
                    </body>
                </html>");
            var pageFixer = new PageFixer(doc);

            pageFixer.FixTitle();

            doc.QuerySelector("head meta[property = 'og:title']").Attributes["content"].Value.Should().Be("A B C");
        }

        [Fact]
        public void HeaderTitle() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                        <title>A B C</title>
                        <!-- InstanceBeginEditable name=""head"" -->
                        <!-- InstanceEndEditable -->
                    </head>
                    <body>
                        <main>
                            <article>
                                <header>
                                    <h1><!-- InstanceBeginEditable name=""TitleDerived"" -->Title goes here<!-- InstanceEndEditable --></h1>
                                </header>
                            <article>
                        <main>
                    </body>
                </html>");
            var pageFixer = new PageFixer(doc);

            pageFixer.FixTitle();

            doc.QuerySelector("h1").TextContent.Should().Be("A B C");
        }

        [Fact]
        public void OpenGraphDescription() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                        <title>A B C</title>
                        <meta name=""Description"" content=""D E F"">
                        <!-- InstanceBeginEditable name=""head"" -->
                        <!-- InstanceEndEditable -->
                    </head>
                    <body>
                        <main>
                            <article>
                                <header>
                                    <h1><!-- InstanceBeginEditable name=""TitleDerived"" -->Title goes here<!-- InstanceEndEditable --></h1>
                                    <p><!-- InstanceBeginEditable name=""SubtitleDerived"" -->Subtitle goes here<!-- InstanceEndEditable --></p>
                                </header>
                            <article>
                        <main>
                    </body>
                </html>");
            var pageFixer = new PageFixer(doc);

            pageFixer.FixDescription();

            doc.QuerySelector("head meta[property = 'og:description']").Attributes["content"].Value.Should().Be("D E F");
            doc.QuerySelector("header p").TextContent.Should().Be("D E F");
        }

        [Fact]
        public void HeaderDescription() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                        <title>A B C</title>
                        <meta name=""Description"" content=""D E F"">
                        <!-- InstanceBeginEditable name=""head"" -->
                        <!-- InstanceEndEditable -->
                    </head>
                    <body>
                        <main>
                            <article>
                                <header>
                                    <h1><!-- InstanceBeginEditable name=""TitleDerived"" -->Title goes here<!-- InstanceEndEditable --></h1>
                                    <p><!-- InstanceBeginEditable name=""SubtitleDerived"" -->Subtitle goes here<!-- InstanceEndEditable --></p>
                                </header>
                            <article>
                        <main>
                    </body>
                </html>");
            var pageFixer = new PageFixer(doc);

            pageFixer.FixDescription();

            doc.QuerySelector("header p").TextContent.Should().Be("D E F");
        }

        [Fact]
        public void ArticleTime() {
            var fakeNow = new DateTime(2019, 7, 24, 2, 20, 56, DateTimeKind.Local);

            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                    </head>
                    <body>
                        <main>
                            <article>
                                <time></time>
                            <article>
                        <main>
                    </body>
                </html>");

            var pageFixer = new PageFixer(doc) {
                CurrentTimeProvider = () => fakeNow
            };

            pageFixer.FixTime();
            const string expectedIsoTimestamp = "2019-07-24T02:20:56.0000000-07:00";
            doc.QuerySelector("article > time").Attributes["datetime"].Value.Should().Be(expectedIsoTimestamp);
            doc.QuerySelector("article > time").Attributes["title"].Value.Should().Be(expectedIsoTimestamp);
            doc.QuerySelector("article > time").TextContent.Should().Be("Wednesday, July 24, 2019");
        }

        [Fact]
        public void ImageSources() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                    </head>
                    <body>
                        <main>
                            <article>
                                <section>
                                    <img src=""a/b.jpg"" id=""relativeImg"" />
                                    <img src=""https://a.b.com/d/e.jpg"" id=""absoluteImg"" />
                                </section>
                            <article>
                        <main>
                    </body>
                </html>");

            var pageFixer = new PageFixer(doc);

            pageFixer.FixImageSources();

            doc.QuerySelector("#relativeImg").GetAttribute("src").Should()
                .Be("https://west.aldaviva.com/exhibits/images/a/b.jpg");
            doc.QuerySelector("#absoluteImg").GetAttribute("src").Should()
                .Be("https://a.b.com/d/e.jpg");
        }

        [Fact]
        public void FigureImageAlternateText() {
            IDocument doc = CreateDocument(@"<!doctype html>
                <html>
                    <head>
                    </head>
                    <body>
                        <main>
                            <article>
                                <section>
                                    <figure>
                                        <img src=""a/b.jpg"" />
                                        <figcaption>X Y Z</figcaption>
                                    </figure>
                                </section>
                            <article>
                        <main>
                    </body>
                </html>");

            var pageFixer = new PageFixer(doc);

            pageFixer.FixImageAlternateText();

            doc.QuerySelector<IHtmlImageElement>("img").AlternativeText.Should().Be("X Y Z");
        }

    }

}