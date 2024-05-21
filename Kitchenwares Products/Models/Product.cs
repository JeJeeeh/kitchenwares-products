using MongoDB.Bson.Serialization.Attributes;

namespace Kitchenwares_Products.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public int? Price { get; set; } = 0;
    public int? Stock { get; set; } = 0;
    public string StoreName { get; set; } = null!;
}