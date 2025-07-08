using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using AvaloniaEdit.Document;
using System.Collections.Generic;
using System;

namespace qBittorrentCompanion.CustomControls
{
    public partial class BindableJsonEditor : BindableEditorBase
    {
        private readonly RegistryOptions? _registryOptions;
        private readonly TextMate.Installation? _textMateInstallation;
        private FoldingManager? _foldingManager;
        private readonly JsonFoldingStrategy? _foldingStrategy;

        public BindableJsonEditor()
        {
            _registryOptions = new RegistryOptions(ThemeName.DarkPlus);
            _textMateInstallation = this.InstallTextMate(_registryOptions);
            _textMateInstallation.SetGrammar("source.json");

            // Install folding manager
            _foldingManager = FoldingManager.Install(this.TextArea);
            _foldingStrategy = new JsonFoldingStrategy();

            // Apply folding
            this.TextChanged += OnTextChanged;
            UpdateFoldings();
        }

        private void OnTextChanged(object? sender, EventArgs e)
        {
            UpdateFoldings();
        }

        private void UpdateFoldings()
        {
            if (_foldingManager != null && _foldingStrategy != null && this.Document != null)
            {
                _foldingStrategy.UpdateFoldings(_foldingManager, this.Document);
            }
        }

        // Clean up resources when detached
        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (_foldingManager != null)
            {
                FoldingManager.Uninstall(_foldingManager);
                _foldingManager = null;
            }

            this.TextChanged -= OnTextChanged;
        }

        // Reinstall folding when reattached (to prevent it from being a one hit wonder)
        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // Reinstall folding manager if it was uninstalled
            if (_foldingManager == null && this.TextArea != null)
            {
                _foldingManager = FoldingManager.Install(this.TextArea);
                this.TextChanged += OnTextChanged;
                UpdateFoldings();
            }
        }
    }

    // Custom JSON folding strategy, heavily inspired by the XML folding pattern:
    //https://github.com/AvaloniaUI/AvaloniaEdit/blob/master/src/AvaloniaEdit/Folding/XmlFoldingStrategy.cs
    public class JsonFoldingStrategy
    {
        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document and updates the folding manager with them.
        /// </summary>
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var foldings = CreateNewFoldings(document, out var firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            var foldings = new List<NewFolding>();

            if (document == null || document.TextLength == 0)
                return foldings;

            try
            {
                var text = document.Text;
                var stack = new Stack<BraceFoldStart>();

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    if (c == '{' || c == '[')
                    {
                        var foldStart = new BraceFoldStart
                        {
                            StartOffset = i,
                            StartLine = document.GetLineByOffset(i).LineNumber,
                            BraceType = c
                        };
                        stack.Push(foldStart);
                    }
                    else if (c == '}' || c == ']')
                    {
                        if (stack.Count > 0)
                        {
                            var foldStart = stack.Pop();
                            var endLine = document.GetLineByOffset(i).LineNumber;

                            // Only create folding if it spans multiple lines
                            if (endLine > foldStart.StartLine)
                            {
                                foldStart.EndOffset = i + 1;

                                // Set folding name based on content
                                var startLine = document.GetLineByOffset(foldStart.StartOffset);
                                var lineText = document.GetText(startLine.Offset, Math.Min(50, startLine.Length));
                                foldStart.Name = lineText.Trim();
                                if (foldStart.Name.Length > 30)
                                    foldStart.Name = foldStart.Name.Substring(0, 30) + "...";

                                foldings.Add(foldStart);
                            }
                        }
                    }
                }

                // Sort foldings by start offset
                foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            }
            catch (Exception)
            {
                firstErrorOffset = 0;
            }

            return foldings;
        }
    }

    // Helper class to hold information about the start of a fold
    internal sealed class BraceFoldStart : NewFolding
    {
        internal int StartLine;
        internal char BraceType;
    }
}