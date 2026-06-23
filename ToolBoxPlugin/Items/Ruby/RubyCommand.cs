using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Input.Ime;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;
using ToolBox.Settings;
using ToolBox.Items.Ruby.Settings;
using IFELanguage = YukkuriMovieMaker.KanjiToYomi.Core.IME.IFELanguage;

namespace ToolBox.Items.Ruby
{
    internal static partial class RubyCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any(item =>
            {
                if (item is VoiceItem voiceItem)
                {
                    return !string.IsNullOrEmpty(voiceItem.Serif);
                }
                if (item is TextItem textItem)
                {
                    return !string.IsNullOrEmpty(textItem.Text);
                }
                return false;
            });
        }

        private static RubySettings GetSettings()
            => ToolBoxSettings.Default.Items
                .OfType<RubySettings>()
                .FirstOrDefault() ?? new RubySettings();

        private static string RemoveRuby(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return RubyRegex().Replace(text, "$1");
        }

        public static async Task AddRuby(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null || undoRedoManager is null) return;

            var itemsToRuby = selectedItems
                .Where(item => (item is VoiceItem v && !string.IsNullOrEmpty(v.Serif)) ||
                               (item is TextItem t && !string.IsNullOrEmpty(t.Text)))
                .ToList();

            if (itemsToRuby.Count == 0) return;

            var settings = GetSettings();

            foreach (var item in itemsToRuby)
            {
                if (item is VoiceItem voiceItem)
                {
                    string currentSerif = voiceItem.Serif;
                    bool alreadyHasRuby = currentSerif.Contains("<rb", StringComparison.OrdinalIgnoreCase);
                    string plainText = RemoveRuby(currentSerif);
                    string rubyText = await GetRubyTextAsync(plainText, settings.RubyType);
                    
                    if (voiceItem.Serif != rubyText)
                    {
                        voiceItem.Serif = rubyText;
                        if (!alreadyHasRuby && settings.IsAutoAdjustLineHeight)
                        {
                            await AdjustLineHeightAsync(voiceItem.LineHeight2, settings.LineHeightMultiplier);
                        }
                    }
                }
                else if (item is TextItem textItem)
                {
                    string currentText = textItem.Text;
                    bool alreadyHasRuby = currentText.Contains("<rb", StringComparison.OrdinalIgnoreCase);
                    string plainText = RemoveRuby(currentText);
                    string rubyText = await GetRubyTextAsync(plainText, settings.RubyType);
                    
                    if (textItem.Text != rubyText)
                    {
                        textItem.Text = rubyText;
                        if (!alreadyHasRuby && settings.IsAutoAdjustLineHeight)
                        {
                            await AdjustLineHeightAsync(textItem.LineHeight2, settings.LineHeightMultiplier);
                        }
                    }
                }
            }

            undoRedoManager.Record();
        }

        private static async Task AdjustLineHeightAsync(dynamic? lineHeight, double multiplier)
        {
            if (lineHeight == null) return;

            lineHeight.BeginEdit();
            if (lineHeight.Values != null)
            {
                foreach (var val in lineHeight.Values)
                {
                    val.Value *= multiplier;
                }
            }
            await lineHeight.EndEditAsync();
        }

        private static Guid GetMSIMEJapanCLSID()
        {
            if (PInvoke.CLSIDFromString("MSIME.Japan\0", out var pclsid).Value == (int)HRESULT.S_OK)
            {
                return pclsid;
            }
            return Guid.Empty;
        }

        private static IFELanguage? CreateIFELanguage()
        {
            Guid rclsid = GetMSIMEJapanCLSID();
            if (rclsid == Guid.Empty) return null;

            Guid riid = new("019F7152-E6DB-11D0-83C3-00C04FDDB82E");
            if (PInvoke.CoCreateInstance(in rclsid, null, CLSCTX.CLSCTX_INPROC_SERVER | CLSCTX.CLSCTX_LOCAL_SERVER, in riid, out var ppv).Value == (int)HRESULT.S_OK)
            {
                return (IFELanguage)ppv;
            }
            return null;
        }

        private static string ConvertKana(string str, RubyType rubyType)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            string? hiragana = Microsoft.VisualBasic.Strings.StrConv(str, Microsoft.VisualBasic.VbStrConv.Hiragana);
            if (hiragana == null) return str;

            if (rubyType == RubyType.Katakana)
            {
                return Microsoft.VisualBasic.Strings.StrConv(hiragana, Microsoft.VisualBasic.VbStrConv.Katakana) ?? hiragana;
            }
            return hiragana;
        }

        public static async Task<string> GetRubyTextAsync(string str, RubyType rubyType)
        {
            return await STATask.Run(delegate
            {
                unsafe
                {
                    IFELanguage? iFELanguage = null;
                    bool flag = false;

                    if (string.IsNullOrEmpty(str)) return "";

                    HRESULT hRESULT = PInvoke.CoInitialize();
                    try
                    {
                        iFELanguage = CreateIFELanguage();
                        if (iFELanguage == null) return str;

                        iFELanguage.Open();
                        flag = true;

                        var lines = str.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                        StringBuilder finalResult = new();

                        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                        {
                            string line = lines[lineIdx];
                            if (string.IsNullOrEmpty(line))
                            {
                                if (lineIdx < lines.Length - 1) finalResult.Append("\r\n");
                                continue;
                            }

                            MORRSLT* ptr = null;
                            fixed (char* value = line)
                            {
                                PCWSTR pwchInput = new(value);
                                iFELanguage.GetJMorphResult(196608u, 16u, line.Length, pwchInput, null, &ptr);
                            }

                            if (ptr == null)
                            {
                                finalResult.Append(line);
                                if (lineIdx < lines.Length - 1) finalResult.Append("\r\n");
                                continue;
                            }

                            string yomiFull = Marshal.PtrToStringUni((nint)ptr->pwchOutput.Value, ptr->cchOutput) ?? "";
                            WDD* pWDD = ptr->pWDD;
                            int cWDD = ptr->cWDD;

                            for (int i = 0; i < cWDD; i++)
                            {
                                WDD wdd = pWDD[i];

                                if (wdd.Anonymous1.wReadPos > line.Length ||
                                    wdd.Anonymous1.wReadPos + wdd.Anonymous2.cchRead > line.Length ||
                                    wdd.wDispPos > yomiFull.Length ||
                                    wdd.wDispPos + wdd.cchDisp > yomiFull.Length)
                                {
                                    continue;
                                }

                                string surface = line.Substring(wdd.Anonymous1.wReadPos, wdd.Anonymous2.cchRead);
                                string yomi = yomiFull.Substring(wdd.wDispPos, wdd.cchDisp);

                                yomi = ConvertKana(yomi, rubyType);

                                if (!ContainsKanji(surface))
                                {
                                    finalResult.Append(surface);
                                    continue;
                                }

                                int prefixMatch = 0;
                                while (prefixMatch < surface.Length && prefixMatch < yomi.Length &&
                                       surface[prefixMatch] == yomi[prefixMatch])
                                {
                                    prefixMatch++;
                                }

                                int suffixMatch = 0;
                                while (suffixMatch < surface.Length - prefixMatch &&
                                       suffixMatch < yomi.Length - prefixMatch &&
                                       surface[surface.Length - 1 - suffixMatch] == yomi[yomi.Length - 1 - suffixMatch])
                                {
                                    suffixMatch++;
                                }

                                string prefix = surface[..prefixMatch];
                                string kanjiPart = surface[prefixMatch..^suffixMatch];
                                string yomiPart = yomi[prefixMatch..^suffixMatch];
                                string suffix = surface[^suffixMatch..];

                                finalResult.Append($"{prefix}<rb {kanjiPart},{yomiPart}>{suffix}");
                            }

                            PInvoke.CoTaskMemFree(ptr);
                            if (lineIdx < lines.Length - 1) finalResult.Append("\r\n");
                        }

                        return finalResult.ToString();
                    }
                    finally
                    {
                        if (iFELanguage != null)
                        {
                            if (flag) iFELanguage.Close();
                            Marshal.ReleaseComObject(iFELanguage);
                        }
                        if (hRESULT == HRESULT.S_OK) PInvoke.CoUninitialize();
                    }
                }
            });
        }

        private static bool ContainsKanji(string text)
        {
            return text.Any(c => (c >= 0x4E00 && c <= 0x9FFF) || (c >= 0x3400 && c <= 0x4DBF) || (c >= 0xF900 && c <= 0xFAFF));
        }

        [GeneratedRegex(@"<rb ([^,]+),[^>]*>", RegexOptions.IgnoreCase)]
        private static partial Regex RubyRegex();
    }
}
