using SafeVault.Core;

namespace SafeVault.Tests;

[TestFixture]
public sealed class XssProtectionTests
{
    [Test]
    [TestCase("<script>alert('xss')</script>")]
    [TestCase("<img src=x onerror=alert(1)>")]
    [TestCase("\"><svg onload=alert(1)>")]
    public void EncodeText_EncodesUntrustedMarkupAsText(string payload)
    {
        var encoded = SafeHtml.EncodeText(payload);

        Assert.That(encoded, Does.Not.Contain("<"));
        Assert.That(encoded, Does.Contain("&lt;"));
    }
}
