using Kitchenwares_Products.Models;
using Kitchenwares_Products.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kitchenwares_Products.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProductsController(IProductService productService, IRabbitMqService rabbitMqService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> FindAll()
    {
        var products = await productService.FindAll();
        return Ok(products);
    }
    
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult> FindOne(string id)
    {
        var product = await productService.FindOne(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> Create(ProductRequest productRequest)
    {
        var storeName = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")!.Value;

        var product = new Product
        {
            Name = productRequest.Name,
            Description = productRequest.Description,
            Price = productRequest.Price,
            Stock = productRequest.Stock,
            StoreName = storeName
        };
        await productService.Create(product);

        return CreatedAtAction(nameof(FindOne), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, ProductRequest updatedProductRequest)
    {
        var product = await productService.FindOne(id);

        if (product is null)
        {
            return NotFound();
        }

        var storeName = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")!.Value;
        if (product.StoreName != storeName)
        {
            return Unauthorized();
        }

        var newProduct = new Product
        {
            Id = id,
            Name = updatedProductRequest.Name,
            Price = updatedProductRequest.Price,
            Stock = updatedProductRequest.Stock,
            StoreName = storeName
        };
        await productService.Update(id, newProduct);

        return NoContent();
    }

    [Authorize(Roles = "Seller")]
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await productService.FindOne(id);
        
        if (product is null)
        {
            return NotFound();
        }

        var storeName = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")!.Value;
        if (product.StoreName != storeName)
        {
            return Unauthorized();
        }
        
        rabbitMqService.SendMessage(id);
        await productService.Delete(id);

        return NoContent();
    }
}