using Nanoray.Kiwi;
using NUnit.Framework;

namespace Kiwi.Tests;

public sealed class StrengthTests
{
    [Test]
    public void AccessingPredefinedStrengths()
    {
        Assert.Less(Strength.Weak, Strength.Medium);
        Assert.Less(Strength.Medium, Strength.Strong);
        Assert.Less(Strength.Strong, Strength.Required);
    }

    [Test]
    public void CreatingStrengths()
    {
        Assert.Less(Strength.Create(0, 0, 1), Strength.Create(0, 1, 0));
        Assert.Less(Strength.Create(0, 1, 0), Strength.Create(1, 0, 0));
        Assert.Less(Strength.Create(1, 0, 0, 1), Strength.Create(1, 0, 0, 4));
    }
}
