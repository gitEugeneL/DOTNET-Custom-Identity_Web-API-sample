using System.Collections;
using Api.Domain.Enums;

namespace Api.IntegrationTests.TestData;

public sealed record TestUser(
    string Email,
    string Password,
    Role Role = Role.Customer
);

public class UserData : IEnumerable<object[]>
{
    private static readonly List<TestUser> Users =
    [
        new("dev@dev.com", "devDev123!", Role.Admin),
        new("mailt@mail.test", "strongPwd!1"),
        new("mail1@mail.test", "myPassword12@"),
        new("user@example.com", "SecurePass123!"),
        new("test@domain.org", "TestUser456#")
    ];

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return Users.Select(company => (object[])[company]).GetEnumerator();
    }
}