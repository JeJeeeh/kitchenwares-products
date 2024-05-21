namespace Kitchenwares_Products.Models;

public class ProductRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public int? Price { get; set; } = 0;
    public int? Stock { get; set; } = 0;
}