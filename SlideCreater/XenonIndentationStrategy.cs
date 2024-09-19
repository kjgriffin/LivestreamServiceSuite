using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Indentation.CSharp;

using System.Windows.Forms;

namespace SlideCreater
{
    internal class XenonIndentationStrategy : DefaultIndentationStrategy
    {
        Form dummmy = new Form();

        string m_indentationString = "    ";

        public string IndentationString
        {
            get => m_indentationString;
            set => m_indentationString = value;
        }

        public XenonIndentationStrategy()
        {
        }

        public XenonIndentationStrategy(TextEditorOptions options)
        {
            m_indentationString = options.IndentationString;
        }

        public void Indent(IDocumentAccessor document, bool keepEmptyLines)
        {
            // just strip it out and put it all back
            var raw = document.Text;

            // call into Xenon for fast formatting?
            // it should know how to re-format a whole chunk


            document.Text = raw + "testing123";
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLine(TextDocument, DocumentLine)">
        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            Indent(new TextDocumentAccessor(document, 0, document.LineCount - 1), true);
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLines(TextDocument, int, int)">
        public override void IndentLines(TextDocument document, int beginLine, int endLine)
        {
            Indent(new TextDocumentAccessor(document, beginLine, endLine), true);
        }

    }
}