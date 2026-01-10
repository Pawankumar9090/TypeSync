namespace TypeSync.Tests;

#region Basic Mapping Models

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime MappedAt { get; set; }
}

#endregion

#region Flattening Models

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class Order
{
    public int Id { get; set; }
    public Customer Customer { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddressCity { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

#endregion

#region Collection Mapping Models

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new();
}

public class Warehouse
{
    public string Name { get; set; } = string.Empty;
    public Product[] Inventory { get; set; } = Array.Empty<Product>();
}

public class WarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public ProductDto[] Inventory { get; set; } = Array.Empty<ProductDto>();
}

#endregion

#region Conditional Mapping Models

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public bool IsEmailVisible { get; set; }
    public string? Nickname { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Nickname { get; set; } = string.Empty;
}

#endregion

#region Custom Constructor Models

public class Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EntityDto
{
    public Guid Id { get; }
    public string Name { get; set; } = string.Empty;

    public EntityDto()
    {
        Id = Guid.NewGuid();
    }

    public EntityDto(Guid id)
    {
        Id = id;
    }
}

#endregion

#region Nested Complex Models

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Employee Manager { get; set; } = new();
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EmployeeDto Manager { get; set; } = new();
}

#endregion

#region Value Resolver Models

public class Invoice
{
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
}

public class InvoiceDto
{
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TotalWithTax { get; set; }
}

#endregion

#region Type Conversion Models

public class SourceWithNumbers
{
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public string StringNumber { get; set; } = string.Empty;
}

public class DestWithStrings
{
    public string IntValue { get; set; } = string.Empty;
    public string DoubleValue { get; set; } = string.Empty;
    public int StringNumber { get; set; }
}

public enum Status
{
    Active,
    Inactive,
    Pending
}

public class SourceWithEnum
{
    public string Status { get; set; } = "Active";
}

public class DestWithEnum
{
    public Status Status { get; set; }
}

#endregion
