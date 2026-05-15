using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=db;Database=productsdb;Username=admin;Password=admin123";

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpMetrics();
app.MapMetrics();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public class Product
{
    public int id { get; set; }

    [Required]
    public string name { get; set; } = "";

    [Range(0.01, 1000000)]
    public decimal price { get; set; }

    [Range(0, 10000)]
    public int quantity { get; set; }

    public DateTime created_at { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Product> products { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_db.products.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var product = _db.products.Find(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        product.created_at = DateTime.UtcNow;
        _db.products.Add(product);
        _db.SaveChanges();
        
        return CreatedAtAction(nameof(Get), new { id = product.id }, product);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Product product)
    {
        if (id != product.id) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        _db.Entry(product).State = EntityState.Modified;
        _db.SaveChanges();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _db.products.Find(id);
        if (product == null) return NotFound();
        
        _db.products.Remove(product);
        _db.SaveChanges();
        
        return NoContent();
    }
}
