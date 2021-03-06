﻿using DevExpress.CodeParser;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.RichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace KryGamesBotControls.Helpers
{
    public class MySyntaxHighlightService : ISyntaxHighlightService
    {
        readonly RichEditControl syntaxEditor;
        SyntaxColors syntaxColors;
        SyntaxHighlightProperties commentProperties;
        SyntaxHighlightProperties keywordProperties;
        SyntaxHighlightProperties stringProperties;
        SyntaxHighlightProperties xmlCommentProperties;
        SyntaxHighlightProperties textProperties;

        public MySyntaxHighlightService(RichEditControl syntaxEditor)
        {
            this.syntaxEditor = syntaxEditor;
            //syntaxEditor.Document.the
            bool dark = false;
            int BrushValue =  (int)syntaxEditor.AutoBackground.Color.R + (int)syntaxEditor.AutoBackground.Color.G + (int)syntaxEditor.AutoBackground.Color.B;
            dark = (double)BrushValue / 3.0 < 125;
            syntaxColors = new SyntaxColors(dark);

        }

        void HighlightSyntax(TokenCollection tokens)
        {
            commentProperties = new SyntaxHighlightProperties();
            commentProperties.ForeColor = syntaxColors.CommentColor;

            keywordProperties = new SyntaxHighlightProperties();
            keywordProperties.ForeColor = syntaxColors.KeywordColor;

            stringProperties = new SyntaxHighlightProperties();
            stringProperties.ForeColor = syntaxColors.StringColor;

            xmlCommentProperties = new SyntaxHighlightProperties();
            xmlCommentProperties.ForeColor = syntaxColors.XmlCommentColor;

            textProperties = new SyntaxHighlightProperties();
            textProperties.ForeColor = syntaxColors.TextColor;

            if (tokens == null || tokens.Count == 0)
                return;

            Document document = syntaxEditor.Document;
            CharacterProperties cp = document.BeginUpdateCharacters(0, 1);
            List<SyntaxHighlightToken> syntaxTokens = new List<SyntaxHighlightToken>(tokens.Count);
            foreach (Token token in tokens)
            {
                HighlightCategorizedToken((CategorizedToken)token, syntaxTokens);
            }
            document.ApplySyntaxHighlight(syntaxTokens);
            document.EndUpdateCharacters(cp);
        }
        void HighlightCategorizedToken(CategorizedToken token, List<SyntaxHighlightToken> syntaxTokens)
        {
            if (syntaxEditor.ActiveView == null)
                return;
            Color backColor = syntaxEditor.ActiveView.BackColor;
            TokenCategory category = token.Category;
            if (category == TokenCategory.Comment)
                syntaxTokens.Add(SetTokenColor(token, commentProperties, backColor));
            else if (category == TokenCategory.Keyword)
                syntaxTokens.Add(SetTokenColor(token, keywordProperties, backColor));
            else if (category == TokenCategory.String)
                syntaxTokens.Add(SetTokenColor(token, stringProperties, backColor));
            else if (category == TokenCategory.XmlComment)
                syntaxTokens.Add(SetTokenColor(token, xmlCommentProperties, backColor));
            else
                syntaxTokens.Add(SetTokenColor(token, textProperties, backColor));
        }
        SyntaxHighlightToken SetTokenColor(Token token, SyntaxHighlightProperties foreColor, Color backColor)
        {
            if (syntaxEditor.Document.Paragraphs.Count < token.Range.Start.Line)
                return null;
            int paragraphStart = DocumentHelper.GetParagraphStart(syntaxEditor.Document.Paragraphs[token.Range.Start.Line - 1]);
            int tokenStart = paragraphStart + token.Range.Start.Offset - 1;
            if (token.Range.End.Line != token.Range.Start.Line)
                paragraphStart = DocumentHelper.GetParagraphStart(syntaxEditor.Document.Paragraphs[token.Range.End.Line - 1]);

            int tokenEnd = paragraphStart + token.Range.End.Offset - 1;
            Debug.Assert(tokenEnd > tokenStart);
            return new SyntaxHighlightToken(tokenStart, tokenEnd - tokenStart, foreColor);
        }

        #region #ISyntaxHighlightServiceMembers
        public void Execute()
        {
            string newText = syntaxEditor.Text;
            // Determine language by file extension.
            string ext = Path.GetExtension(syntaxEditor.Options.DocumentSaveOptions.CurrentFileName);
            
            ParserLanguageID lang_ID = ParserLanguage.FromFileExtension(ext);
            
            if (ext.ToLower() == ".lua")
                lang_ID = ParserLanguageID.Basic;
            else
            // Do not parse HTML or XML.
            if (lang_ID == ParserLanguageID.Html ||
                lang_ID == ParserLanguageID.Xml ||
                lang_ID == ParserLanguageID.None) return;
            // Use DevExpress.CodeParser to parse text into tokens.
            ITokenCategoryHelper tokenHelper = TokenCategoryHelperFactory.CreateHelper(lang_ID);
            TokenCollection highlightTokens;
            highlightTokens = tokenHelper.GetTokens(newText);
            HighlightSyntax(highlightTokens);
        }

        public void ForceExecute()
        {
            Execute();
        }
        #endregion #ISyntaxHighlightServiceMembers
    }
    /// <summary>
    ///  This class provides colors to highlight the tokens.
    /// </summary>
    public class SyntaxColors
    {
        bool Dark = false;
        static Color DefaultCommentColor { get { return Color.Green; } }
        static Color DefaultKeywordColor { get { return Color.Blue; } }
        static Color DefaultStringColor { get { return Color.Brown; } }
        static Color DefaultXmlCommentColor { get { return Color.Gray; } }
        static Color DefaultTextColor { get { return Color.Black; } }
        

        public Color CommentColor { get { return GetCommonColorByName(nameof(CommentColor), DefaultCommentColor); } }
        public Color KeywordColor { get { return GetCommonColorByName(nameof(KeywordColor), DefaultKeywordColor); ; } }
        public Color TextColor { get { return GetCommonColorByName(nameof(TextColor), DefaultTextColor); ; } }
        public Color XmlCommentColor { get { return GetCommonColorByName(nameof(XmlCommentColor), DefaultXmlCommentColor); ; } }
        public Color StringColor { get { return GetCommonColorByName(nameof(StringColor), DefaultStringColor); ; } }

        public SyntaxColors(bool Dark)
        {
            this.Dark = Dark;


        }

        Color GetCommonColorByName(string colorName, Color defaultColor)
        {
            if (!Dark)
                return defaultColor;
            switch (colorName)
            {
                case nameof(CommentColor): return Color.LimeGreen; break;
                case nameof(KeywordColor):return Color.DodgerBlue; break;
                case nameof(StringColor):return Color.Peru; break;
                case nameof(XmlCommentColor): return Color.LightGray; break;
                case nameof(TextColor): return Color.GhostWhite; break;
            }
            /*Skin skin = CommonSkins.GetSkin(lookAndFeel);
            if (skin == null)*/
                return defaultColor;
            //return skin.Colors[colorName];
        }
    }
}
