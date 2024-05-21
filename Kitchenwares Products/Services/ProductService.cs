using Kitchenwares_Products.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Kitchenwares_Products.Services;

public interface IProductService
{
    Task<List<Product>> FindAll();
    Task<Product?> FindOne(string id);
    Task Create(Product newProduct);
    Task Update(string id, Product updatedProduct);
    Task Delete(string id);
}

public class ProductService : IProductService
{
    private readonly IMongoCollection<Product> _products;
    
    public ProductService(IOptions<DatabaseSettings> databaseSettings) 
    {
        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        _products = mongoDatabase.GetCollection<Product>(
            databaseSettings.Value.CollectionName);
    }
    
    public async Task<List<Product>> FindAll() => await _products.Find(_ =>  true).ToListAsync();

    public async Task<Product?> FindOne(string id) => await _products.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task Create(Product newProduct) => await _products.InsertOneAsync(newProduct);

    public async Task Update(string id, Product updatedProduct) => await _products.ReplaceOneAsync(x => x.Id == id, updatedProduct);

    public async Task Delete(string id) => await _products.DeleteOneAsync(x => x.Id == id);
}