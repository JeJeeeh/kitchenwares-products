using System.Security.Claims;
using System.Text.Json;
using Kitchenwares_Products.Controllers;
using Kitchenwares_Products.Models;
using Kitchenwares_Products.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace Kitchenwares_Products_Test;

public class ProductsControllerTest
{
    [Fact]
    public async Task GetProducts_Returns_Products()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            CollectionName = "kitchenwares",
            DatabaseName = "products"
        });

        var productServiceMock = new Mock<IProductService>();
        var mockProducts = new List<Product>
        {
            new Product {Id = "id1", Name = "Item 1"},
            new Product {Id = "id2", Name = "Item 2"}
        };

        productServiceMock.Setup(x => x.FindAll()).ReturnsAsync(mockProducts);

        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object);

        // act
        var result = await controller.FindAll();

        // assert
        Assert.IsType<ActionResult<List<Product>>>(result);
    }

    [Fact]
    public async Task FindOne_Returns_Product()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            CollectionName = "kitchenwares",
            DatabaseName = "products"
        });
        
        var productServiceMock = new Mock<IProductService>();
        const string existingProductId = "1";
        var existingProduct = new Product { Id = existingProductId, Name = "Item 1" };

        productServiceMock.Setup(x => x.FindOne(existingProductId)).ReturnsAsync(existingProduct);

        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object);

        // act
        var result = await controller.FindOne(existingProductId);

        // assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task FindOne_Returns_NotFound()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            CollectionName = "kitchenwares",
            DatabaseName = "products"
        });
        
        var productServiceMock = new Mock<IProductService>();
        const string nonExistingProductId = "100";

        productServiceMock.Setup(x => x.FindOne(nonExistingProductId)).ReturnsAsync((Product)null!);

        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object);
        
        // act
        var result = await controller.FindOne(nonExistingProductId);

        // assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateProduct_Returns_Created()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDb",
            CollectionName = "testCollection"
        });

        var productServiceMock = new Mock<IProductService>();

        var httpContextMock = new Mock<HttpContext>();
        var userClaims = new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "mocked_username"),
        };
        var identity = new ClaimsIdentity(userClaims);
        var principal = new ClaimsPrincipal(identity);
        httpContextMock.Setup(h => h.User).Returns(principal);
        
        var newProduct = new ProductRequest { Name = "New Product", Price = 10, Stock = 5};
        var expectedResult = new Product
        {
            Name = "New Product", Price = 10, Stock = 5, StoreName = "mocked_username"
        };
        
        productServiceMock.Setup(x => x.Create(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };

        // act
        var result = await controller.Create(newProduct);
        
        // assert
        var createdAtRouteResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(JsonSerializer.Serialize(expectedResult), JsonSerializer.Serialize(createdAtRouteResult.Value));
    }

    [Fact]
    public async Task UpdateProduct_Returns_NoContent()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDb",
            CollectionName = "testCollection"
        });

        var productServiceMock = new Mock<IProductService>();

        var httpContextMock = new Mock<HttpContext>();
        var userClaims = new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "mocked_username"),
        };
        var identity = new ClaimsIdentity(userClaims);
        var principal = new ClaimsPrincipal(identity);
        httpContextMock.Setup(h => h.User).Returns(principal);
        
        const string existingProductId = "1";
        var productRequest = new ProductRequest { Name = "Product 1", Price = 10, Stock = 10};
        var existingProduct = new Product { Id = existingProductId, Name = "Product 1", Price = 10, Stock = 10, StoreName = "mocked_username"};

        productServiceMock.Setup(x => x.FindOne(existingProductId)).ReturnsAsync(existingProduct);
        
        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
        
        // act
        var result = await controller.Update(existingProductId, productRequest);
        
        // assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_Returns_NoContent()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDb",
            CollectionName = "testCollection"
        });

        var productServiceMock = new Mock<IProductService>();

        var httpContextMock = new Mock<HttpContext>();
        var userClaims = new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "mocked_username"),
        };
        var identity = new ClaimsIdentity(userClaims);
        var principal = new ClaimsPrincipal(identity);
        httpContextMock.Setup(h => h.User).Returns(principal);
        
        const string existingProductId = "1";
        var existingProduct = new Product { Id = existingProductId, Name = "Product 1", Price = 10, StoreName = "mocked_username"};

        productServiceMock.Setup(x => x.FindOne(existingProductId)).ReturnsAsync(existingProduct);

        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
        
        // act
        var result = await controller.Delete(existingProductId);
        
        // assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_Returns_NotFound()
    {
        // arrange
        var databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "testDb",
            CollectionName = "testCollection"
        });

        var productServiceMock = new Mock<IProductService>();

        const string nonExistingProductId = "100"; 

        productServiceMock.Setup(x => x.FindOne(nonExistingProductId)).ReturnsAsync((Product)null!);
        
        var rabbitMqServiceMock = new Mock<IRabbitMqService>();
        
        var controller = new ProductsController(productServiceMock.Object, rabbitMqServiceMock.Object);
        
        // act
        var result = await controller.Delete(nonExistingProductId);
        
        // assert
        Assert.IsType<NotFoundResult>(result);
    }
}