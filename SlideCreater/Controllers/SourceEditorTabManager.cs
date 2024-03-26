using CCU.Config;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Search;

using Microsoft.Win32;

using SlideCreater.ViewControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using UIControls;

using Xenon.SlideAssembly;

namespace SlideCreater.Controllers
{
    interface IFullAccess<T>
    {
        T Value { get; }
    }

    internal class SourceEditorTabManager
    {

        public event EventHandler OnTextEditDiry;

        public enum SourceFileType
        {
            XENON,
            CCUCONFIG,
            BMDCONFIG,
        }

        TabControl _Host;
        Window _Parent;
        IFullAccess<Project> _proj;

        HashSet<string> _openFiles = new HashSet<string>();

        public SourceEditorTabManager(TabControl host, Window parent, IFullAccess<Project> proj)
        {
            _Host = host;
            _Parent = parent;
            _proj = proj;
        }

        public void OpenEditorWindowForFile(string filename, SourceFileType type)
        {
            if (_openFiles.Contains(filename))
            {
                // just switch to that tab
                // may need to update??
                foreach (var item in _Host.Items)
                {
                    if (item is TabItem tab && tab.Header.ToString() == filename)
                    {
                        var tabindex = _Host.Items.IndexOf(item);
                        _Host.SelectedIndex = tabindex;
                        break;
                    }
                }

                return;
            }

            switch (type)
            {
                case SourceFileType.XENON:
                    OpenXenonFile(filename);
                    break;
                case SourceFileType.CCUCONFIG:
                    OpenCCUFile(filename);
                    break;
                case SourceFileType.BMDCONFIG:
                    OpenBMDFile(filename);
                    break;
                default:
                    break;
            }
            // mark opened
            _openFiles.Add(filename);
        }


        public void RefreshAllTextViews()
        {
            // NEEDS TO HANDLE THE CASE OF NEW PROJECT, where files may not yet be present...
            // correct action is to just close those tabs

            List<string> closed = new List<string>();
            foreach (var open in _openFiles)
            {
                if (defaults.TryGetValue(open, out var type))
                {
                    // no valid project should ommit these...
                    // reopen with prejudice
                    if (TryFindTabForFile(open, out var page, out _))
                    {
                        switch (type)
                        {
                            case SourceFileType.XENON:
                                ((TextEditor)page).Text = _proj?.Value?.SourceCode ?? string.Empty;
                                break;
                            case SourceFileType.BMDCONFIG:
                                ((TextEditor)page).Text = _proj?.Value?.SourceConfig ?? string.Empty;
                                break;
                            case SourceFileType.CCUCONFIG:
                                ((CCUEditorCtrl)page).UpdateFromProj();
                                break;
                        }
                    }
                }
                else
                {
                    if (TryFindTabForFile(open, out var page, out var tindex))
                    {
                        // update the editor directly
                        if (_proj?.Value?.ExtraSourceFiles?.TryGetValue(open, out var text) == true)
                        {
                            ((TextEditor)page).Text = text;
                        }
                        else
                        {
                            closed.Add(open);
                            _Host.Items.RemoveAt(tindex);
                        }
                    }
                }
            }
            foreach (var close in closed)
            {
                _openFiles.Remove(close);
            }

        }

        private bool TryFindTabForFile(string filename, out object tabpage, out int index)
        {
            index = -1;
            tabpage = null;

            foreach (var item in _Host.Items)
            {
                if (item is TabItem tab && tab.Header.ToString() == filename)
                {
                    index = _Host.Items.IndexOf(item);
                    tabpage = tab.Content;
                    return true;
                }
            }

            return false;
        }


        static Dictionary<string, SourceFileType> defaults = new Dictionary<string, SourceFileType>
        {
            ["main.xenon"] = SourceFileType.XENON,
            ["BMDSwitcherConfig.json"] = SourceFileType.BMDCONFIG,
            ["CCUConfig.json"] = SourceFileType.CCUCONFIG,
        };

        public void UpdateProjectSourceFromEditors()
        {
            foreach (var open in _openFiles)
            {
                UpdateProjectFromOpenTextEditor(open);
            }
        }

        public void RequestInsertTextIntoActiveTab(string text)
        {
            if (_Host.SelectedIndex != -1)
            {
                var tab = _Host.Items[_Host.SelectedIndex] as TabItem;
                if (tab.Content is TextEditor editor)
                {
                    editor.Document.Insert(editor.CaretOffset, System.Environment.NewLine);
                    foreach (var line in text.Split(Environment.NewLine))
                    {
                        editor.Document.Insert(editor.CaretOffset, line);
                        editor.Document.Insert(editor.CaretOffset, System.Environment.NewLine);
                    }
                }
            }
        }


        public void JumpToSource(string filename, int line)
        {
            if (!_openFiles.Contains(filename))
            {
                OpenXenonFile(filename);
            }
            if (TryFindTabForFile(filename, out var page, out int index))
            {
                var editor = page as TextEditor;

                _Host.SelectedIndex = index;
                if (line >= 0 && line <= editor.LineCount)
                {
                    editor.ScrollToLine(line);
                    editor.TextArea.Caret.Line = line + 1;
                    editor.TextArea.Caret.Column = 0;
                    editor.TextArea.Focus();
                    editor.Select(editor.CaretOffset, editor.Document.Lines[line].Length);
                }
                else
                {
                    editor.Focus();
                    editor.TextArea.ClearSelection();
                }
            }
        }


        private void UpdateProjectFromOpenTextEditor(string filename)
        {
            if (defaults.TryGetValue(filename, out var type))
            {
                // no valid project should ommit these...
                // reopen with prejudice
                if (TryFindTabForFile(filename, out var page, out _))
                {
                    switch (type)
                    {
                        case SourceFileType.XENON:
                            _proj.Value.SourceCode = ((TextEditor)page).Text;
                            break;
                        case SourceFileType.BMDCONFIG:
                            _proj.Value.SourceConfig = ((TextEditor)page).Text;
                            break;
                        case SourceFileType.CCUCONFIG:
                            // pretty sure this get's auto updated on it's own
                            break;
                    }
                }
            }
            else
            {
                if (TryFindTabForFile(filename, out var page, out var tindex))
                {
                    // update the editor directly
                    if (_proj?.Value?.ExtraSourceFiles?.ContainsKey(filename) == true)
                    {
                        _proj.Value.ExtraSourceFiles[filename] = ((TextEditor)page).Text;
                    }
                }
            }
        }

        private void OpenXenonFile(string filename)
        {
            if (filename != "main.xenon" && _proj?.Value?.ExtraSourceFiles?.ContainsKey(filename) != true)
            {
                return;
            }

            TabItem tab = new TabItem();
            tab.Header = filename;
            tab.Background = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
            tab.Foreground = new SolidColorBrush(Colors.White);
            //tab.FontWeight = FontWeights.Regular;
            tab.Style = (Style)_Parent.FindResource("DarkTabClosable");
            tab.ApplyTemplate();

            TextEditor editor = new TextEditor();

            _Host.Items.Add(tab);
            tab.Content = editor;

            _Host.SelectedIndex = _Host.Items.Count - 1;

            var closeButton = (Button)tab.Template.FindName("tabClose", tab);
            closeButton.Click += CloseButton_Click;

            // events??
            //editor.TextArea.TextEntering += TextArea_TextEntering;
            editor.TextArea.PreviewTextInput += TextArea_PreviewTextInput;
            editor.TextArea.TextInput += TextArea_TextInput;
            editor.TextChanged += Editor_TextChanged;

            editor.LoadLanguage_XENON();
            editor.Options.IndentationSize = 4;
            editor.Options.ConvertTabsToSpaces = true;
            editor.TextArea.TextView.LinkTextForegroundBrush = System.Windows.Media.Brushes.LawnGreen;
            editor.Background = new SolidColorBrush(Color.FromRgb(0x27, 0x27, 0x27));
            editor.Foreground = new SolidColorBrush(Color.FromRgb(0xea, 0xea, 0xea));
            editor.FontFamily = new FontFamily("cascadia Code");
            editor.FontSize = 14;
            editor.ShowLineNumbers = true;
            editor.Padding = new Thickness(3);

            SearchPanel.Install(editor.TextArea);

            // probably need to setup the content...
            if (filename == "main.xenon")
            {
                editor.Text = _proj?.Value?.SourceCode ?? string.Empty;
            }
            else
            {
                if (_proj?.Value?.ExtraSourceFiles?.TryGetValue(filename, out var text) == true)
                {
                    editor.Text = text;
                }
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            OnTextEditDiry?.Invoke(this, e);
        }

        CompletionWindow completionWindow;

        private void TextArea_TextInput(object sender, TextCompositionEventArgs e)
        {
            // run suggestions after input too?
            _ = ShowSuggestionsForEditor(sender as TextArea);
        }

        private void TextArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == " " && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
            }
            _ = ShowSuggestionsForEditor(sender as TextArea);
        }

        private async Task ShowSuggestionsForEditor(TextArea editor)
        {
            var text = editor.Document.Text;
            int offset = editor.Caret.Offset;
            List<(string item, string description, int cindex)> suggestions = new List<(string, string, int)>();
            await Task.Run(() =>
            {
                suggestions = _proj?.Value?.XenonSuggestionService.GetSuggestions(text, offset);
            });
            editor.Dispatcher.Invoke(() =>
            {
                if (suggestions.Any())
                {
                    completionWindow = new CompletionWindow(editor);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    foreach (var suggestion in suggestions)
                    {
                        data.Add(new CommonCompletion(suggestion.item, suggestion.description, suggestion.cindex));
                    }
                    completionWindow.Show();
                    completionWindow.SizeToContent = SizeToContent.Width;
                    completionWindow.CompletionList.SelectedItem = data.First(d => d.Text == suggestions.First().item);
                    completionWindow.Closed += delegate
                    {
                        completionWindow.CompletionList.InsertionRequested -= CompletionList_InsertionRequested;
                        completionWindow = null;
                    };
                    completionWindow.CompletionList.InsertionRequested += CompletionList_InsertionRequested;
                }
                else
                {
                    completionWindow?.Close();
                }
            });
        }

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            _ = ShowSuggestionsForEditor(((sender as CompletionList)?.Parent as CompletionWindow)?.TextArea);
        }

        private void OpenBMDFile(string filename)
        {
            TabItem tab = new TabItem();
            tab.Header = filename;
            tab.Background = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
            tab.Foreground = new SolidColorBrush(Colors.White);
            //tab.FontWeight = FontWeights.Regular;
            tab.Style = (Style)_Parent.FindResource("DarkTabClosable");
            tab.ApplyTemplate();

            TextEditor editor = new TextEditor();

            _Host.Items.Add(tab);
            tab.Content = editor;

            _Host.SelectedIndex = _Host.Items.Count - 1;

            var closeButton = (Button)tab.Template.FindName("tabClose", tab);
            closeButton.Click += CloseButton_Click;

            // events??
            editor.TextChanged += Editor_TextChanged;

            editor.LoadLanguage_JSON();
            editor.Options.IndentationSize = 4;
            editor.Options.ConvertTabsToSpaces = true;
            editor.TextArea.TextView.LinkTextForegroundBrush = System.Windows.Media.Brushes.LawnGreen;
            editor.Background = new SolidColorBrush(Color.FromRgb(0x27, 0x27, 0x27));
            editor.Foreground = new SolidColorBrush(Color.FromRgb(0xea, 0xea, 0xea));
            editor.FontFamily = new FontFamily("cascadia code");
            editor.FontSize = 14;
            editor.ShowLineNumbers = true;
            editor.Padding = new Thickness(3);

            SearchPanel.Install(editor.TextArea);

            // probably need to setup the content...
            editor.Text = _proj?.Value?.SourceConfig ?? string.Empty;
        }
        private void OpenCCUFile(string filename)
        {
            TabItem tab = new TabItem();
            tab.Header = filename;
            tab.Background = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
            tab.Foreground = new SolidColorBrush(Colors.White);
            //tab.FontWeight = FontWeights.Regular;
            tab.Style = (Style)_Parent.FindResource("DarkTabClosable");
            tab.ApplyTemplate();

            CCUEditorCtrl editor = new CCUEditorCtrl(_proj);

            _Host.Items.Add(tab);
            tab.Content = editor;

            _Host.SelectedIndex = _Host.Items.Count - 1;

            var closeButton = (Button)tab.Template.FindName("tabClose", tab);
            closeButton.Click += CloseButton_Click;

            // events??
            editor.OnTextEditDirty += Editor_OnTextEditDirty;
        }

        private void Editor_OnTextEditDirty(object sender, EventArgs e)
        {
            OnTextEditDiry?.Invoke(this, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_Host.SelectedIndex != -1)
            {
                var fname = (((FrameworkElement)sender).TemplatedParent as TabItem).Header.ToString();

                //var tab = (TabItem)_Host.Items[_Host.SelectedIndex];
                //var fname = tab.Header.ToString();

                // TODO: auto save into project before close
                UpdateProjectFromOpenTextEditor(fname);

                TryCloseTab_NoSave(fname);
            }
        }

        public void TryCloseTab_NoSave(string fname)
        {
            if (TryFindTabForFile(fname, out var tab, out int index))
            {

                if (tab is TextEditor editor)
                {
                    //editor.TextArea.TextEntering -= TextArea_TextEntering;
                    editor.TextArea.PreviewTextInput -= TextArea_PreviewTextInput;
                    editor.TextArea.TextInput -= TextArea_TextInput;
                    editor.TextChanged -= Editor_TextChanged;
                }
                else if (tab is CCUEditorCtrl ctrl)
                {
                    ctrl.OnTextEditDirty -= Editor_OnTextEditDirty;
                }

                // close the active tab
                _openFiles.Remove(fname);
                _Host.Items.RemoveAt(index);
            }

        }

    }
}
