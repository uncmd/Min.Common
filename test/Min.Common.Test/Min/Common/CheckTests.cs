namespace Min.Common;

public class CheckTests
{
    [Fact]
    public void NotNull_Test()
    {
        Check.NotNull("test").ShouldBe("test");
        Check.NotNull(string.Empty).ShouldBe(string.Empty);

        var ex = Assert.Throws<ArgumentNullException>(() => Check.NotNull<object>(null));
    }

    [Fact]
    public void NotNullOrWhiteSpace_Test()
    {
        Check.NotNullOrWhiteSpace("test").ShouldBe("test");

        Assert.Throws<ArgumentException>(() => Check.NotNullOrWhiteSpace(null));
        Assert.Throws<ArgumentException>(() => Check.NotNullOrWhiteSpace(string.Empty));
    }

    [Fact]
    public void NotNullOrEmpty_Test()
    {
        Check.NotNullOrEmpty("test").ShouldBe("test");
        Check.NotNullOrEmpty(new List<string> { "test" });

        Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty(null));
        Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty(string.Empty));
        Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty(new List<string>()));
    }

    [Fact]
    public void AssignableTo_Test()
    {
        Check.AssignableTo<object>(typeof(string)).ShouldBe(typeof(string));
        Check.AssignableTo<Parent>(typeof(Child)).ShouldBe(typeof(Child));
        Check.AssignableTo<Child>(typeof(Child2)).ShouldBe(typeof(Child2));
        Check.AssignableTo<Parent>(typeof(Child2)).ShouldBe(typeof(Child2));

        Assert.Throws<ArgumentException>(() => Check.AssignableTo<Child>(typeof(Parent)));
        Assert.Throws<ArgumentException>(() => Check.AssignableTo<Child2>(typeof(Child)));
        Assert.Throws<ArgumentException>(() => Check.AssignableTo<Child2>(typeof(Parent)));
    }

    [Fact]
    public void Length_Test()
    {
        Check.Length("test", maxLength: 4).ShouldBe("test");
        Check.Length("test", maxLength: 5).ShouldBe("test");
        Check.Length("test", maxLength: 4, minLength: 0).ShouldBe("test");
        Check.Length("test", maxLength: 4, minLength: 4).ShouldBe("test");

        Assert.Throws<ArgumentException>(() => Check.Length("test", maxLength: 0));
        Assert.Throws<ArgumentException>(() => Check.Length("test", maxLength: 3));
        Assert.Throws<ArgumentException>(() => Check.Length("test", maxLength: 4, minLength: 5));
    }

    class Parent
    {

    }

    class Child : Parent
    {

    }

    class Child2 : Child
    {

    }
}
