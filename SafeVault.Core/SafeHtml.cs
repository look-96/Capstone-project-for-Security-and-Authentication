using System.Text.Encodings.Web;

namespace SafeVault.Core;

public static class SafeHtml
{
    /// <summary>Encodes untrusted text for insertion into an HTML text context.</summary>
    public static string EncodeText(string untrustedText) => HtmlEncoder.Default.Encode(untrustedText);
}
