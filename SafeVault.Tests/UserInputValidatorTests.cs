using SafeVault.Core;

namespace SafeVault.Tests;

[TestFixture]
public sealed class UserInputValidatorTests
{
    [Test]
    public void Validate_AcceptsOrdinaryUserDetails()
    {
        var user = UserInputValidator.Validate("  sam.w  ", " sam@example.com ");

        Assert.That(user.Username, Is.EqualTo("sam.w"));
        Assert.That(user.Email, Is.EqualTo("sam@example.com"));
    }

    [TestCase("' OR 1=1 --")]
    [TestCase("<script>alert(1)</script>")]
    [TestCase("ab")]
    public void Validate_RejectsInvalidOrHostileUsernames(string username)
    {
        Assert.Throws<ArgumentException>(() => UserInputValidator.Validate(username, "sam@example.com"));
    }

    [TestCase("<script>alert(1)</script>@example.com")]
    [TestCase("victim@example.com' OR 1=1 --")]
    public void Validate_RejectsMalformedOrHostileEmailValues(string email)
    {
        Assert.Throws<ArgumentException>(() => UserInputValidator.Validate("sam", email));
    }

    [Test]
    [TestCase("' OR 1=1 --")]
    [TestCase("'; DROP TABLE Users;--")]
    public void FindByEmail_KeepsInjectionPayloadInParameterRatherThanSql(string payload)
    {
        var statement = UserSql.FindByEmail(payload);

        Assert.That(statement.CommandText, Does.Not.Contain(payload));
        Assert.That(statement.CommandText, Does.Contain("Email = @Email"));
        Assert.That(statement.Parameters["@Email"], Is.EqualTo(payload));
    }

    [Test]
    public void Insert_UsesParametersForEveryUserSuppliedValue()
    {
        var user = new UserInput("sam", "sam@example.com");

        var statement = UserSql.Insert(user);

        Assert.That(statement.CommandText, Does.Not.Contain(user.Username));
        Assert.That(statement.CommandText, Does.Not.Contain(user.Email));
        Assert.That(statement.Parameters["@Username"], Is.EqualTo(user.Username));
        Assert.That(statement.Parameters["@Email"], Is.EqualTo(user.Email));
    }
}
