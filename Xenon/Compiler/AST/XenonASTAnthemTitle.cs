using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Compiler.Suggestions;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTAnthemTitle : IXenonASTCommand, IXenonCommandSuggestionCallback
    {

        public string AnthemTitle { get; set; }
        public string Musician { get; set; }
        public string Accompanianst { get; set; }
        public string Credits { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTAnthemTitle title = new XenonASTAnthemTitle();
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "anthemtitle", "musician", "accompanianst", "credits");
            title.AnthemTitle = args["anthemtitle"];
            title.Musician = args["musician"];
            title.Accompanianst = args["accompanianst"];
            title.Credits = args["credits"];
            title.Parent = Parent;
            return title;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide titleslide = new Slide
            {
                Name = "UNNAMED_anthemtitle",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image
            };

            
            List<string> strings = new List<string>
            {
                AnthemTitle, Musician, Accompanianst, Credits
            };

            titleslide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = strings;
            titleslide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.AnthemTitle].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, titleslide, LanguageKeywordCommand.AnthemTitle);


            titleslide.AddPostset(_Parent, true, true);
            return titleslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTAnthemTitle>");
            Debug.WriteLine($"AnthemTitle='{AnthemTitle}'");
            Debug.WriteLine($"Musician='{Musician}'");
            Debug.WriteLine($"Accompanianst='{Accompanianst}'");
            Debug.WriteLine($"Credits='{Credits}'");
            Debug.WriteLine("</XenonASTAnthemTitle>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            ("#anthemtitle", false, "", new List<(string, string)> { ("#anthemtitle", "")}, null),
            ("\\(\"", false, "", new List<(string, string)> { ("(\"", "insert anthem name")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end anthem name")}, null ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert muscian name")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end musician name")}, null ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert accompanist")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end accompanist")}, null ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert credits")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end credits")}, null ),
            ("\\)", false, "", new List<(string, string)> {(")", "") }, null),
        };

    }
}
