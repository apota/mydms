using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Configure pipeline
app.UseCors("AllowAll");
app.MapControllers();

app.Run("http://localhost:7001");

// Simple customer model
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string CustomerType { get; set; } = "Sales";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string Country { get; set; } = "USA";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// Simple in-memory store
public static class CustomerStore
{
    private static List<Customer> _customers = new()
    {
        new Customer { Id = 1, Name = "John Smith", Phone = "(555) 123-4567", Email = "john.smith@example.com", CustomerType = "Sales" },
        new Customer { Id = 2, Name = "Sarah Johnson", Phone = "(555) 987-6543", Email = "sarah.johnson@example.com", CustomerType = "Service" },
        new Customer { Id = 3, Name = "Michael Brown", Phone = "(555) 555-1234", Email = "michael.brown@example.com", CustomerType = "Parts" },
        new Customer { Id = 4, Name = "Emily Wilson", Phone = "(555) 777-8888", Email = "emily.wilson@example.com", CustomerType = "Lead" }
    };
    
    private static int _nextId = 5;
    
    public static List<Customer> GetAll() => _customers;
    
    public static Customer? GetById(int id) => _customers.FirstOrDefault(c => c.Id == id);
    
    public static Customer Add(Customer customer)
    {
        customer.Id = _nextId++;
        customer.CreatedAt = DateTime.UtcNow;
        _customers.Add(customer);
        return customer;
    }
    
    public static Customer? Update(int id, Customer updatedCustomer)
    {
        var existing = GetById(id);
        if (existing == null) return null;
        
        existing.Name = updatedCustomer.Name;
        existing.Email = updatedCustomer.Email;
        existing.Phone = updatedCustomer.Phone;
        existing.CustomerType = updatedCustomer.CustomerType;
        existing.Address = updatedCustomer.Address;
        existing.City = updatedCustomer.City;
        existing.State = updatedCustomer.State;
        existing.ZipCode = updatedCustomer.ZipCode;
        existing.Country = updatedCustomer.Country;
        existing.UpdatedAt = DateTime.UtcNow;
        
        return existing;
    }
    
    public static bool Delete(int id)
    {
        var customer = GetById(id);
        if (customer == null) return false;
        
        _customers.Remove(customer);
        return true;
    }
}

// API Controller
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Customer>> GetCustomers()
    {
        return Ok(CustomerStore.GetAll());
    }
    
    [HttpGet("{id}")]
    public ActionResult<Customer> GetCustomer(int id)
    {
        var customer = CustomerStore.GetById(id);
        if (customer == null)
            return NotFound();
        
        return Ok(customer);
    }
    
    [HttpPost]
    public ActionResult<Customer> CreateCustomer([FromBody] Customer customer)
    {
        if (string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
            return BadRequest("Name and Email are required");
        
        var created = CustomerStore.Add(customer);
        return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    public ActionResult<Customer> UpdateCustomer(int id, [FromBody] Customer customer)
    {
        var updated = CustomerStore.Update(id, customer);
        if (updated == null)
            return NotFound();
        
        return Ok(updated);
    }
    
    [HttpDelete("{id}")]
    public ActionResult DeleteCustomer(int id)
    {
        var deleted = CustomerStore.Delete(id);
        if (!deleted)
            return NotFound();
        
        return NoContent();
    }
}
