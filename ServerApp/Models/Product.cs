namespace ServerApp.Models;

public sealed record Product(int Id, string Name, decimal Price, int Stock, Category Category);
