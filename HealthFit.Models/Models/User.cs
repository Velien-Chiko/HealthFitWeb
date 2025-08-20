using System;
using System.Collections.Generic;
using HealthFit.Models.Models;

namespace HealthFit.Models;
using Microsoft.AspNetCore.Identity;

public partial class User : IdentityUser<int>
{

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Gender { get; set; }
    public string? FullName { get; set; }
    public string? District { get; set; }


    public virtual ICollection<Aiinteraction> Aiinteractions { get; set; } = new List<Aiinteraction>();

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<CustomerProfile> CustomerProfiles { get; set; } = new List<CustomerProfile>();

    public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();

    public virtual ICollection<Order> OrderSellers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderUsers { get; set; } = new List<Order>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}