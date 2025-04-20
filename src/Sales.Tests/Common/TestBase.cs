using AutoMapper;
using Bogus;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sales.Application.Mappings;
using Sales.Domain.Interfaces;

namespace Sales.Tests.Common;

public abstract class TestBase
{
    protected readonly IMapper Mapper;
    protected readonly ILogger<TestBase> LoggerMock;
    protected readonly Faker Faker;

    protected TestBase()
    {
        var saleRepositoryMock = Substitute.For<ISaleRepository>();
        var productRepositoryMock = Substitute.For<IProductRepository>();
        LoggerMock = Substitute.For<ILogger<TestBase>>();
        Faker = new Faker();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        Mapper = mapperConfig.CreateMapper();

        
        saleRepositoryMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        productRepositoryMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        
        productRepositoryMock.ProductExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
    }
}