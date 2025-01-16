using System.Threading.Tasks;
using Xunit;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using OrderProcessingSystem.Models;
using System;

namespace OrderProcessingSystem.Tests
{
    public class OrderServiceTests
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public OrderServiceTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .UseInternalServiceProvider(serviceProvider);

            _context = new ApplicationDbContext(builder.Options);
            _orderService = new OrderService(_context);

            SeedTestData();
        }

        [Fact]
        public async Task CreateOrder_Success()
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "John Doe");
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == "Product 1");
            var productIds = new List<int> { product.ProductId };

            var order = await _orderService.CreateOrderAsync(customer.CustomerId, productIds);

            Assert.NotNull(order);
            Assert.Equal(product.Price, order.TotalPrice);
            Assert.Single(order.OrderProducts);
            Assert.Equal(customer.CustomerId, order.CustomerId);

            var foundOrder = await _orderService.GetOrderByIdAsync(order.OrderId);
            Assert.NotNull(foundOrder);
            Assert.Equal(order.OrderId, foundOrder.OrderId);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnOrder()
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "Jane Smith");
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == "Product 2");
            var productIds = new List<int> { product.ProductId };
            var order = await _orderService.CreateOrderAsync(customer.CustomerId, productIds);

            var fetchedOrder = await _orderService.GetOrderByIdAsync(order.OrderId);

            Assert.NotNull(fetchedOrder);
            Assert.Equal(order.OrderId, fetchedOrder.OrderId);
        }

        [Fact]
        public async Task CreateOrder_CustomerNotFound_ThrowsException()
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == "Product 1");
            var productIds = new List<int> { product.ProductId };

            var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(999, productIds));
            Assert.Equal("Customer not found", exception.Message);
        }

        [Fact]
        public async Task GetOrderById_OrderNotFound_ReturnsNull()
        {
            var fetchedOrder = await _orderService.GetOrderByIdAsync(9999);
            Assert.Null(fetchedOrder);
        }

        private void SeedTestData()
        {
            var customers = new[]
            {
                new Customer { CustomerId = 1, Name = "John Doe" },
                new Customer { CustomerId = 2, Name = "Jane Smith" },
            };

            var products = new[]
            {
                new Product { ProductId = 1, Name = "Product 1", Price = 100 },
                new Product { ProductId = 2, Name = "Product 2", Price = 200 },
            };

            _context.Customers.AddRange(customers);
            _context.Products.AddRange(products);
            _context.SaveChanges();
        }
    }
}
