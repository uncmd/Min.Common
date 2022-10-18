using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Min.DependencyInjection;

public class DependencyInjectionTests
{
    private DefaultTypeProvider _typeProvider = default!;
    private IEnumerable<Type> _allTypes = default!;

    public DependencyInjectionTests()
    {
        _typeProvider = new DefaultTypeProvider();
        _allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
    }

    [Fact]
    public void TestSkip()
    {
        _typeProvider.IsSkip(typeof(BaseService)).ShouldBeFalse();
        _typeProvider.IsSkip(typeof(GoodsBaseService)).ShouldBeTrue();
    }

    [Fact]
    public void TestGetServiceTypesReturnCountIs2()
    {
        var serviceTypes = _typeProvider.GetServiceTypes(_allTypes.ToList(), typeof(ISingletonDependency));

        serviceTypes.Count.ShouldBe(2);
    }

    [Fact]
    public void TestGetImplementationTypesReturnCountIs1()
    {
        var implementationTypes = _typeProvider.GetImplementationTypes(_allTypes.ToList(), typeof(BaseService));

        implementationTypes.Count.ShouldBe(1);
    }

    [Fact]
    public void TestAssignableFrom()
    {
        _typeProvider.IsAssignableFrom(typeof(BaseService), typeof(GoodsBaseService)).ShouldBeTrue();
        _typeProvider.IsAssignableFrom(typeof(GoodsBaseService), typeof(BaseService)).ShouldBeFalse();
    }

    [Fact]
    public void TestAssignableTo()
    {
        _typeProvider.IsAssignableTo(typeof(BaseService), typeof(GoodsBaseService)).ShouldBeFalse();
        _typeProvider.IsAssignableTo(typeof(GoodsBaseService), typeof(BaseService)).ShouldBeTrue();
    }

    [Fact]
    public void TestAddAutoInject()
    {
        var services = new ServiceCollection();
        services.AddAutoInject();
        var serviceProvider = services.BuildServiceProvider();

        BaseService.Count.ShouldBe(0);

        var serviceBase = serviceProvider.GetService<BaseService>();
        serviceBase.ShouldNotBeNull();

        BaseService.Count.ShouldBe(1);

        var goodsBaseService = serviceProvider.GetService<GoodsBaseService>();
        goodsBaseService.ShouldBeNull();
    }

    [Fact]
    public void TestAddAutoInjectAndEmptyAssemblyReturnServiceIsNull()
    {
        var services = new ServiceCollection();
        services.AddAutoInject(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetService<BaseService>().ShouldBeNull();
        serviceProvider.GetService<GoodsBaseService>().ShouldBeNull();
    }

    [Fact]
    public void TestAny()
    {
        var services = new ServiceCollection();
        services.Any<BaseService>().ShouldBeFalse();
        services.Any<BaseService>(ServiceLifetime.Singleton).ShouldBeFalse();
        services.AddScoped<BaseService>();
        services.Any<BaseService>().ShouldBeTrue();
        services.Any<BaseService>(ServiceLifetime.Singleton).ShouldBeFalse();
        services.Any<BaseService>(ServiceLifetime.Scoped).ShouldBeTrue();
        services.Any<BaseService>(ServiceLifetime.Transient).ShouldBeFalse();
    }

    [Fact]
    public void TestDependencyReturnProviderServiceIs1()
    {
        var services = new ServiceCollection();
        services.AddAutoInject();
        var serviceProvider = services.BuildServiceProvider();
        var factories = serviceProvider.GetServices<IClientFactory>().ToList();

        factories.Count.ShouldBe(1);
        factories[0].GetClientName().ShouldBe(nameof(CustomizeClientFactory));
    }

    private class BaseService : ISingletonDependency
    {
        public static int Count { get; set; } = 0;

        public BaseService()
        {
            Count++;
        }
    }

    [IgnoreInjection]
    private class GoodsBaseService : BaseService
    {
        public GoodsBaseService()
        {
        }
    }

    public interface IClientFactory : ISingletonDependency
    {
        string GetClientName();
    }

    [Dependency(ReplaceServices = true)]
    public class CustomizeClientFactory : IClientFactory
    {
        public string GetClientName() => nameof(CustomizeClientFactory);
    }

    public class EmptyClientFactory : IClientFactory
    {
        public string GetClientName() => nameof(EmptyClientFactory);
    }
}
