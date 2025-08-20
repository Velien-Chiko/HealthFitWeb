using HealthFit.Models;
using HealthFit.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace HealthFit.DataAccess.Data;

public partial class HealthyShopContext : IdentityDbContext<User, IdentityRole<int>, int>
{

    public HealthyShopContext()
    {
    }

    public HealthyShopContext(DbContextOptions<HealthyShopContext> options)
      : base(options)
    {
    }


    public virtual DbSet<Aiinteraction> Aiinteractions { get; set; }
    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }

    public virtual DbSet<EmailLog> EmailLogs { get; set; }

    public virtual DbSet<MealPlanDetail> MealPlanDetails { get; set; }

    public virtual DbSet<MealPlanProductDetail> MealPlanProductDetails { get; set; }

    public virtual DbSet<Order> Orders { get; set; } // Đổi từ Order sang Orders

    public virtual DbSet<OrderProductDetail> OrderProductDetails { get; set; }

    public virtual DbSet<OrderMealPlanDetail> OrderMealPlanDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<ProductCategoryMapping> ProductCategoryMappings { get; set; }




    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Aiinteraction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK__AIIntera__922C0376A0A6DA09");

            entity.ToTable("AIInteractions");

            entity.Property(e => e.InteractionId).HasColumnName("InteractionID");
            entity.Property(e => e.InteractionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QueryText).HasColumnType("text");
            entity.Property(e => e.ResponseText).HasColumnType("text");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Aiinteractions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AIInterac__UserI__7B5B524B");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blogs__54379E50D5F7D8ED");

            entity.Property(e => e.BlogId).HasColumnName("BlogID");

            entity.Property(e => e.Bmirange)
                .HasMaxLength(20)
                .IsUnicode(true)
                .HasColumnName("BMIRange");

            entity.Property(e => e.Content)
                .HasColumnType("nvarchar(max)")
                .IsUnicode(true);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.IsApproved).HasDefaultValue(false);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(true);

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Blogs__UserID__4D94879B");
        });


        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__BlogComm__C3B4DFAA828574F9");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.BlogId).HasColumnName("BlogID");
            entity.Property(e => e.CommentText).HasColumnType("text");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsReply).HasDefaultValue(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("FK__BlogComme__BlogI__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BlogComme__UserI__5535A963");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2A598519C6");

            entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__CartItems__Produ__5AEE82B9");

            entity.HasOne(d => d.User).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CartItems__UserI__59FA5E80");
        });

        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__Customer__290C8884424A92FC");

            entity.Property(e => e.ProfileId).HasColumnName("ProfileID");
            entity.Property(e => e.Bmi).HasColumnName("BMI");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Height).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.CustomerProfiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CustomerP__UserI__3F466844");
        });

        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.EmailLogId).HasName("PK__EmailLog__E8CB41EC8AF66D0D");

            entity.Property(e => e.EmailLogId).HasColumnName("EmailLogID");
            entity.Property(e => e.EmailType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.EmailLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__EmailLogs__UserI__7F2BE32F");
        });

        modelBuilder.Entity<MealPlanDetail>(entity =>
        {
            entity.HasKey(e => e.MealPlanDetailId).HasName("PK__MealPlan__37DC012B0B828298");

            entity.Property(e => e.MealPlanDetailId).HasColumnName("MealPlanDetailID");
            entity.Property(e => e.Bmirange)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BMIRange");
            entity.Property(e => e.IsApproved)
                 .HasMaxLength(20)
                 .IsUnicode(false)
                 .HasDefaultValue("Approved")
                 .HasColumnName("IsApproved");

            entity.Property(e => e.PlanDescription)
                .IsUnicode(true)
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.Price)
               .HasColumnType("decimal(10,2)")
               .HasDefaultValue(0m);
            entity.Property(e => e.ImageUrl)
               .HasMaxLength(255)
               .IsUnicode(false)
               .HasColumnName("ImageURL");
        });

        modelBuilder.Entity<MealPlanProductDetail>(entity =>
        {
            entity.HasKey(e => e.MealPlanProductDetailId).HasName("PK__MealPlan__575B79B920E58552");

            entity.HasIndex(e => new { e.MealPlanDetailId, e.ProductId }, "UQ__MealPlan__FC9CCD445C779BE0").IsUnique();

            entity.Property(e => e.MealPlanProductDetailId).HasColumnName("MealPlanProductDetailID");
            entity.Property(e => e.MealPlanDetailId).HasColumnName("MealPlanDetailID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.MealPlanDetail).WithMany(p => p.MealPlanProductDetails)
                .HasForeignKey(d => d.MealPlanDetailId)
                .HasConstraintName("FK__MealPlanP__MealP__2645B050");

            entity.HasOne(d => d.Product).WithMany(p => p.MealPlanProductDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__MealPlanP__Produ__2739D489");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAFBC92CCCA");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SellerId).HasColumnName("SellerID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID").IsRequired(false);

            // Payment properties
            entity.Property(e => e.PaymentStatus).HasColumnType("nvarchar(max)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime2");
            entity.Property(e => e.PaymentDueDate).HasColumnType("date");
            entity.Property(e => e.SessionId).HasColumnType("nvarchar(max)");
            entity.Property(e => e.PaymentIntentId).HasColumnType("nvarchar(max)");

            // Shipping properties
            entity.Property(e => e.PhoneNumber).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.Address).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.City).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.Country).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.FullName).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.Email).IsRequired().HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderSellers)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__Orders__SellerID__628FA481");

            //entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
            //    .HasForeignKey(d => d.UserId)
            //    .HasConstraintName("FK__Orders__UserID__5FB337D6");
        });

        modelBuilder.Entity<OrderProductDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C9DA0C818");
            entity.ToTable("OrderProductDetails");
            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasColumnType("int");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderProductDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__Order__66603565");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderProductDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderDeta__Produ__6754599E");
        });

        modelBuilder.Entity<OrderMealPlanDetail>(e =>
        {
            e.HasKey(x => x.OrderMealPlanDetailId);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(10,2)");

            // 1 Order có nhiều OrderMealPlanDetail
            e.HasOne(x => x.Order)
             .WithMany(o => o.OrderMealPlanDetails)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // 1 Combo (MealPlanDetail) có nhiều OrderMealPlanDetail
            e.HasOne(x => x.MealPlanDetail)
             .WithMany(mp => mp.OrderMealPlanDetails)
             .HasForeignKey(x => x.MealPlanDetailId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED450455F3");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ImageURL");
            entity.Property(e => e.Ingredients).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsActive)
                  .HasMaxLength(20)
                  .IsUnicode(false)
                  .HasDefaultValue("Active");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(true);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantity)
                .HasColumnType("int")
                .HasDefaultValue(0);

            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Products__Create__4316F928");

            // Quan hệ 1-N với ProductCategoryMapping
            entity.HasMany(p => p.ProductCategoryMappings)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId)
                .HasConstraintName("FK_ProductCategoryMapping_Product");
        });


        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ProductC__19093A2BCDE758FB");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnType("nvarchar(50)"); ;
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(true)
                .HasColumnType("nvarchar(50)"); ;
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__ProductR__74BC79AE6A40E940");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Response).HasColumnType("text");
            entity.Property(e => e.SellerReply).HasColumnType("text");
            entity.Property(e => e.ReplyAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.ToTable("ProductReview");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductRe__Produ__6B24EA82");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ProductRe__UserI__6A30C649");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FullName)
                .IsUnicode(true)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A5801B726E3");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(e => e.Order)
                .WithOne(o => o.Payment) // Mỗi Order có 1 Payment
                .HasForeignKey<Payment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Payments__OrderID");
        });

        modelBuilder.Entity<ProductCategoryMapping>()
    .HasKey(x => new { x.ProductId, x.CategoryId });

        modelBuilder.Entity<ProductCategoryMapping>()
            .HasOne(x => x.Product)
            .WithMany(p => p.ProductCategoryMappings)
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<ProductCategoryMapping>()
            .HasOne(x => x.Category)
            .WithMany(c => c.ProductCategoryMappings)
            .HasForeignKey(x => x.CategoryId);


        modelBuilder.Entity<ProductCategory>().HasData(
    new ProductCategory { CategoryId = 1, CategoryName = "Món mặn", Description = "Các món chính giàu đạm, chất xơ và ít dầu mỡ." },
    new ProductCategory { CategoryId = 2, CategoryName = "Tráng miệng", Description = "Món ngọt nhẹ, ít đường, tốt cho tiêu hóa." },
    new ProductCategory { CategoryId = 3, CategoryName = "Đồ uống", Description = "Nước ép, detox, trà và sinh tố tốt cho sức khỏe." }
);

        modelBuilder.Entity<ProductCategoryMapping>().HasKey(x => new { x.ProductId, x.CategoryId });
        modelBuilder.Entity<ProductCategoryMapping>().HasData(
            new { ProductId = 1, CategoryId = 1 },
            new { ProductId = 2, CategoryId = 1 },
            new { ProductId = 3, CategoryId = 1 },
            new { ProductId = 4, CategoryId = 1 },
            new { ProductId = 5, CategoryId = 1 },

            new { ProductId = 6, CategoryId = 2 },
            new { ProductId = 7, CategoryId = 2 },
            new { ProductId = 8, CategoryId = 2 },
            new { ProductId = 9, CategoryId = 2 },

            new { ProductId = 10, CategoryId = 3 },
            new { ProductId = 11, CategoryId = 3 },
            new { ProductId = 12, CategoryId = 3 },
            new { ProductId = 13, CategoryId = 3 }
        );

        modelBuilder.Entity<Product>().HasData(
    new Product { ProductId = 1, Name = "Gà áp chảo sốt chanh dây", Description = "Ức gà áp chảo, sốt chanh dây ít đường", Price = 45000, Calo = 300, Quantity = 100, ImageUrl = "/images/product/GaApChao.jpg", IsActive = "Pending" },
    new Product { ProductId = 2, Name = "Cơm gạo lứt cá hồi nướng", Description = "Cơm gạo lứt, cá hồi áp chảo, rau củ hấp", Price = 65000, Calo = 500, Quantity = 100, ImageUrl = "/images/product/ComGaoNutCaHoi.jpeg", IsActive = "Pending" },
    new Product { ProductId = 3, Name = "Salad bò nướng mè rang", Description = "Bò nạc nướng, rau xà lách, mè, giấm táo", Price = 50000, Calo = 400, Quantity = 100, ImageUrl = "/images/product/SaladBoNuong.jpg", IsActive = "Pending" },
    new Product { ProductId = 4, Name = "Đậu hũ non sốt nấm", Description = "Đậu hũ hấp, nấm sốt", Price = 40000, Calo = 350, Quantity = 100, ImageUrl = "/images/product/DauSotNam.png", IsActive = "Approved" },
    new Product { ProductId = 5, Name = "Miến gà thảo mộc", Description = "Miến dong, gà xé, nấm, hành, gừng", Price = 35000, Calo = 280, Quantity = 100, ImageUrl = "/images/product/MienGa.jpg", IsActive = "Approved" },
    new Product { ProductId = 6, Name = "Sữa chua hạt chia", Description = "Tự làm, ít đường, topping dâu/việt quất", Price = 20000, Calo = 120, Quantity = 100, ImageUrl = "/images/product/SuaChuaHatChia.jpg", IsActive = "Approved" },
    new Product { ProductId = 7, Name = "Chè yến mạch", Description = "Yến mạch nấu chín", Price = 28000, Calo = 200, Quantity = 100, ImageUrl = "/images/product/CheYenMach.jpg" },
    new Product { ProductId = 8, Name = "Thạch dừa rau câu lá dứa", Description = "Không đường tinh luyện, thơm nhẹ", Price = 15000, Calo = 100, Quantity = 100, ImageUrl = "/images/product/ThacDuaRauCau.jpg", IsActive = "Approved" },
    new Product { ProductId = 9, Name = "Bánh chuối nướng yến mạch", Description = "Không bơ, không bột mì", Price = 25000, Calo = 180, Quantity = 100, ImageUrl = "/images/product/BanhChuoiNuong.jpg", IsActive = "Approved" },
    new Product { ProductId = 10, Name = "Nước detox chnanh dưa leo", Description = "Nước lọc + lát chanh + dưa leo tươi", Price = 15000, Calo = 30, Quantity = 100, ImageUrl = "/images/product/NuocDetxChanhDuaLeo.jpg", IsActive = "Approved" },
    new Product { ProductId = 11, Name = "Sinh tố bơ không đường", Description = "Bơ sáp xay cùng sữa hạt", Price = 35000, Calo = 220, Quantity = 100, ImageUrl = "/images/product/SinhToBo.jpg", IsActive = "Approved" },
    new Product { ProductId = 12, Name = "Trà hoa cúc mật ong", Description = "Ít ngọt, nhẹ bụng, giải nhiệt", Price = 18000, Calo = 50, Quantity = 100, ImageUrl = "/images/product/TraHoaCucMatOng.jpg", IsActive = "Approved" },
    new Product { ProductId = 13, Name = "Nước táo ép cần tây", Description = "Không đường, giàu vitamin", Price = 30000, Calo = 90, Quantity = 100, ImageUrl = "/images/product/NươcEpTaoCanTay.jpg", IsActive = "Approved" }
);

        modelBuilder.Entity<MealPlanDetail>().HasData(
           new MealPlanDetail
           {
               MealPlanDetailId = 1,
               PlanDescription = "Combo cho người gầy",
               Bmirange = "<18.5",
               Price = 115000,
               ImageUrl = "/images/mealplan/nguoiGay.jpg"
           },
           new MealPlanDetail { MealPlanDetailId = 2, PlanDescription = "Combo cho BMI bình thường", Bmirange = "18.5-24.9", Price = 85000, ImageUrl = "/images/mealplan/nguoiBinhThuong.jpg", IsApproved = "Approved" },
           new MealPlanDetail { MealPlanDetailId = 3, PlanDescription = "Combo cho người thừa cân", Bmirange = ">25", Price = 75000, ImageUrl = "/images/mealplan/nguoiBeo.jpg", IsApproved = "Approved" },
           new MealPlanDetail { MealPlanDetailId = 4, PlanDescription = "Combo Tăng cơ - Giữ dáng", Bmirange = "21.0-24.9", Price = 110000, ImageUrl = "/images/mealplan/tangCo.jpg", IsApproved = "Approved" },
           new MealPlanDetail { MealPlanDetailId = 5, PlanDescription = "Combo Văn phòng vui vẻ", Bmirange = "23.0-27.0", Price = 80000, ImageUrl = "/images/mealplan/vanPhong.jpg", IsApproved = "Approved" },
           new MealPlanDetail { MealPlanDetailId = 6, PlanDescription = "Combo Thanh lọc - Nhẹ bụng", Bmirange = ">24.0", Price = 70000, ImageUrl = "/images/mealplan/thanhLoc.jpg", IsApproved = "Approved" }

        );

        modelBuilder.Entity<MealPlanProductDetail>().HasData(
            new MealPlanProductDetail { MealPlanProductDetailId = 1, MealPlanDetailId = 1, ProductId = 2, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 2, MealPlanDetailId = 1, ProductId = 11, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 3, MealPlanDetailId = 1, ProductId = 9, Quantity = 1 },

            new MealPlanProductDetail { MealPlanProductDetailId = 4, MealPlanDetailId = 2, ProductId = 3, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 5, MealPlanDetailId = 2, ProductId = 6, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 6, MealPlanDetailId = 2, ProductId = 12, Quantity = 1 },

            new MealPlanProductDetail { MealPlanProductDetailId = 7, MealPlanDetailId = 3, ProductId = 1, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 8, MealPlanDetailId = 3, ProductId = 8, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 9, MealPlanDetailId = 3, ProductId = 10, Quantity = 1 },

            new MealPlanProductDetail { MealPlanProductDetailId = 10, MealPlanDetailId = 4, ProductId = 1, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 11, MealPlanDetailId = 4, ProductId = 3, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 12, MealPlanDetailId = 4, ProductId = 11, Quantity = 1 },

            new MealPlanProductDetail { MealPlanProductDetailId = 13, MealPlanDetailId = 5, ProductId = 4, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 14, MealPlanDetailId = 5, ProductId = 6, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 15, MealPlanDetailId = 5, ProductId = 12, Quantity = 1 },

            new MealPlanProductDetail { MealPlanProductDetailId = 16, MealPlanDetailId = 6, ProductId = 5, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 17, MealPlanDetailId = 6, ProductId = 8, Quantity = 1 },
            new MealPlanProductDetail { MealPlanProductDetailId = 18, MealPlanDetailId = 6, ProductId = 13, Quantity = 1 }
        );



        modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );

        // 2) Chuẩn bị PasswordHasher để tạo PasswordHash
        var hasher = new PasswordHasher<User>();

        // -- Admin Account --
        var adminAccount = new User
        {
            Id = 1,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            FullName = "Administrator",
            Address = "Số 1 Đường ABC, Quận XYZ",   // ví dụ
            City = "Hà Nội",
            PhoneNumber = "0123456789",
            Country = "Việt Nam",
            Gender = "Male",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        // Gán PasswordHash cho admin
        adminAccount.PasswordHash = hasher.HashPassword(adminAccount, "Admin@123");

        // -- Simple User Account --
        var simpleUser = new User
        {
            Id = 2,
            UserName = "user",
            NormalizedUserName = "USER",
            Email = "huybat2004@gmail.com",
            NormalizedEmail = "USER@EXAMPLE.COM",
            FullName = "Simple User",
            Address = "Số 2 Đường DEF, Quận UVW",     // ví dụ
            City = "Hồ Chí Minh",
            Country = "Việt Nam",
            Gender = "Female",
            PhoneNumber = "0123456789",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        // Gán PasswordHash cho user bình thường
        simpleUser.PasswordHash = hasher.HashPassword(simpleUser, "User@123");

        // Đưa hai user này vào seed data
        modelBuilder.Entity<User>().HasData(adminAccount, simpleUser);

        // 3) Seed mapping giữa User và Role (AspNetUserRoles)
        modelBuilder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int>
            {
                UserId = 1,
                RoleId = 1   // admin → Admin
            },
            new IdentityUserRole<int>
            {
                UserId = 2,
                RoleId = 2   // user  → User
            }
        );

        modelBuilder.Entity<Order>().HasData(
    new Order
    {
        OrderId = 1,
        UserId = 2,
        SellerId = 1,
        OrderDate = DateTime.Now,
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 100000m,
        FullName = "Minh Cute",
        PhoneNumber = "0987654321",
        Address = "123 Đường Coder, Quận Dev",
        City = "Hà Nội",
        Country = "Việt Nam",
        Email = "minhcute@example.com",

        // Các trường liên quan đến thanh toán:
        PaymentStatus = "Paid",
        PaymentDate = DateTime.Now,
        SessionId = "sess_123abcxyz",
        PaymentIntentId = "pi_123abcxyz"
    }
);

        modelBuilder.Entity<OrderProductDetail>().HasData(
    new OrderProductDetail
    {
        OrderDetailId = 1,
        OrderId = 1,
        ProductId = 2, // ví dụ Cơm gạo lứt cá hồi nướng
        Quantity = 2,
        UnitPrice = 45000m,
        Subtotal = 90000m
    },
    new OrderProductDetail
    {
        OrderDetailId = 2,
        OrderId = 1,
        ProductId = 6, // ví dụ Sữa chua hạt chia
        Quantity = 1,
        UnitPrice = 10000m,
        Subtotal = 10000m
    }
);
        modelBuilder.Entity<Payment>().HasData(
    new Payment
    {
        PaymentId = 1,
        OrderId = 1,
        Method = "Cash",
        Status = "Paid",
        Amount = 100000m,
        PaymentDate = DateTime.Now
    }
);

        modelBuilder.Entity<Blog>().HasData(
        new Blog
        {
            BlogId = 1,
            Title = "Lợi ích của ăn sáng đúng cách",
            Content = "Bữa sáng là bữa ăn quan trọng nhất trong ngày vì nó cung cấp năng lượng cho cơ thể sau một đêm dài không ăn. Ăn sáng đầy đủ giúp tăng khả năng tập trung, cải thiện tâm trạng và giảm nguy cơ mắc các bệnh mãn tính như tiểu đường và béo phì. Bạn nên chọn các thực phẩm giàu protein và chất xơ như trứng, yến mạch, trái cây tươi để bắt đầu ngày mới một cách khỏe mạnh.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 2,
            Title = "5 thực phẩm giúp giảm cân hiệu quả",
            Content = "Để giảm cân một cách bền vững, việc chọn thực phẩm đúng là vô cùng quan trọng. Một số thực phẩm được các chuyên gia dinh dưỡng khuyên dùng bao gồm: yến mạch, vì giàu chất xơ giúp no lâu; trứng luộc, cung cấp protein mà không nhiều calo; cá hồi, giàu omega-3 giúp đốt mỡ; bông cải xanh, ít calo mà lại nhiều chất chống oxy hóa; và sữa chua Hy Lạp, hỗ trợ hệ tiêu hóa và cung cấp protein.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 3,
            Title = "Cách uống nước đúng để đẹp da",
            Content = "Nước chiếm đến 70% cơ thể người và đóng vai trò quan trọng trong việc giữ cho làn da luôn mềm mại, tươi trẻ. Uống đủ nước giúp thanh lọc cơ thể, giảm mụn và làm chậm quá trình lão hóa da. Hãy bắt đầu ngày mới bằng một ly nước ấm, tránh uống quá nhiều nước một lúc và nên bổ sung từ 1.5 đến 2 lít nước mỗi ngày, tùy theo trọng lượng và hoạt động của cơ thể.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 4,
            Title = "Tác hại của thức ăn nhanh",
            Content = "Thức ăn nhanh thường chứa nhiều chất béo bão hòa, muối và đường, có thể gây ra các bệnh như tim mạch, cao huyết áp, tiểu đường và béo phì nếu sử dụng thường xuyên. Ngoài ra, chúng còn ít chất xơ và vitamin, làm rối loạn tiêu hóa và tăng nguy cơ ung thư đại tràng. Thay vì ăn fast food, hãy chọn các món ăn nấu tại nhà để kiểm soát nguyên liệu và hàm lượng dinh dưỡng.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 5,
            Title = "Thực đơn eat clean trong 7 ngày",
            Content = "Chế độ ăn 'eat clean' giúp thanh lọc cơ thể, hỗ trợ tiêu hóa và giảm cân hiệu quả. Dưới đây là gợi ý cho 7 ngày: Ngày 1: Ức gà luộc + gạo lứt + rau xanh; Ngày 2: Cá hồi hấp + bông cải xanh + khoai lang; Ngày 3: Trứng luộc + yến mạch + trái cây tươi. Các ngày tiếp theo nên thay đổi linh hoạt giữa các nhóm thực phẩm giàu protein, chất xơ và hạn chế đường, dầu mỡ.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 6,
            Title = "Tại sao nên ăn nhiều rau xanh?",
            Content = "Rau xanh là nguồn cung cấp dồi dào vitamin A, C, K cùng chất xơ giúp điều hòa tiêu hóa, kiểm soát cân nặng và ngăn ngừa nhiều bệnh tật. Các loại rau như rau bina, cải xoăn, bông cải xanh có chứa chất chống oxy hóa mạnh, làm chậm lão hóa tế bào và giảm nguy cơ ung thư. Nên ăn ít nhất 400g rau mỗi ngày, kết hợp đa dạng để đảm bảo đủ dưỡng chất.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 7,
            Title = "Chế độ ăn cho người tập gym",
            Content = "Người tập gym cần chế độ ăn giàu protein để xây dựng cơ bắp, đồng thời vẫn cung cấp đủ năng lượng từ carb và chất béo tốt. Mỗi bữa ăn nên có 1 phần protein như thịt nạc, trứng, cá, 1 phần carb chậm như yến mạch, khoai lang, và rau xanh. Đừng quên bổ sung nước và hạn chế các món chiên, nước ngọt có gas vì chúng làm giảm hiệu quả tập luyện.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 8,
            Title = "Uống trà detox có tốt không?",
            Content = "Trà detox thường được pha từ các loại thảo mộc, hoa quả như chanh, dưa leo, gừng giúp tăng cường trao đổi chất, loại bỏ độc tố và giảm cảm giác đầy hơi. Tuy nhiên, trà detox không nên được xem là biện pháp giảm cân duy nhất. Hãy kết hợp với lối sống lành mạnh, chế độ ăn uống cân bằng và tập luyện thể dục thường xuyên để đạt hiệu quả tối ưu.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 9,
            Title = "Làm sao để kiểm soát lượng đường?",
            Content = "Để giảm nguy cơ mắc tiểu đường và các bệnh chuyển hóa, bạn cần hạn chế thực phẩm chứa đường tinh luyện như bánh kẹo, nước ngọt, và thay thế bằng trái cây tươi, ngũ cốc nguyên hạt. Đọc kỹ nhãn mác sản phẩm để phát hiện đường ẩn. Sử dụng các loại gia vị tự nhiên như quế, vani để tăng vị ngọt mà không cần đường.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 10,
            Title = "Ngũ cốc nguyên hạt có lợi gì?",
            Content = "Ngũ cốc nguyên hạt như yến mạch, gạo lứt, lúa mạch chứa đầy đủ phần cám, mầm và nội nhũ nên giàu chất xơ, vitamin B và khoáng chất. Chúng giúp ổn định đường huyết, giảm cholesterol xấu và hỗ trợ tiêu hóa. Ăn ngũ cốc nguyên hạt thường xuyên còn giúp giảm cân và duy trì vóc dáng thon gọn.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 11,
            Title = "Ăn chay đúng cách để không thiếu chất",
            Content = "Ăn chay đúng cách không chỉ tốt cho sức khỏe mà còn bảo vệ môi trường. Tuy nhiên, bạn cần đảm bảo đủ protein từ đậu, nấm, hạt; bổ sung sắt, kẽm và đặc biệt là vitamin B12 từ thực phẩm chức năng. Kết hợp đa dạng các loại thực phẩm thực vật để tránh thiếu hụt dưỡng chất quan trọng.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 12,
            Title = "Thói quen ăn uống lành mạnh",
            Content = "Thói quen ăn uống lành mạnh bao gồm việc ăn đúng giờ, không bỏ bữa, hạn chế đồ chiên rán và đồ ngọt, đồng thời tăng cường rau xanh, trái cây và thực phẩm tươi sống. Ngoài ra, việc nhai kỹ và ăn chậm cũng góp phần cải thiện tiêu hóa và kiểm soát cân nặng hiệu quả hơn.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 13,
            Title = "Lợi ích của dầu ô liu trong bữa ăn",
            Content = "Dầu ô liu nguyên chất chứa nhiều chất chống oxy hóa và axit béo không bão hòa, có tác dụng giảm viêm, tốt cho tim mạch và làn da. Bạn có thể sử dụng dầu ô liu để trộn salad, nấu ăn ở nhiệt độ thấp hoặc uống một thìa mỗi sáng để hỗ trợ tiêu hóa.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 14,
            Title = "Cách bảo quản thực phẩm tươi lâu",
            Content = "Để thực phẩm tươi lâu, bạn cần phân loại trước khi bảo quản: rau củ nên để khô ráo và bọc bằng giấy hoặc túi lưới; thịt cá nên được cấp đông nhanh sau khi mua. Không nên để thực phẩm quá lâu trong tủ lạnh vì sẽ mất chất dinh dưỡng. Kiểm tra định kỳ để tránh thực phẩm bị hỏng, lên men.",
            UserId = 2
        },
        new Blog
        {
            BlogId = 15,
            Title = "Ăn gì để tăng sức đề kháng?",
            Content = "Hệ miễn dịch khỏe mạnh cần được nuôi dưỡng từ thực phẩm giàu vitamin C như cam, ổi, chanh; vitamin D từ ánh nắng và cá béo; kẽm từ hải sản và các loại hạt. Ngoài ra, sữa chua giúp cải thiện hệ vi sinh đường ruột cũng rất cần thiết. Hãy ăn đủ chất, nghỉ ngơi hợp lý và vận động thường xuyên để tăng cường sức đề kháng.",
            UserId = 2
        }
    );

        modelBuilder.Entity<Order>().HasData(
    new Order
    {
        OrderId = 2,
        UserId = 2,
        SellerId = 1,
        OrderDate = new DateTime(2024, 1, 10),
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 120000,
        FullName = "Nguyễn Văn A",
        PhoneNumber = "0901234567",
        Address = "123 Lê Lợi",
        City = "HCM",
        Country = "Việt Nam",
        Email = "a@example.com",
        PaymentStatus = "Paid",
        PaymentDate = new DateTime(2024, 1, 10),
        SessionId = "sess_2",
        PaymentIntentId = "pi_2"
    },
    new Order
    {
        OrderId = 3,
        UserId = 2,
        SellerId = 1,
        OrderDate = new DateTime(2024, 3, 25),
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 200000,
        FullName = "Lê Thị B",
        PhoneNumber = "0909876543",
        Address = "456 Phan Bội Châu",
        City = "Đà Nẵng",
        Country = "Việt Nam",
        Email = "b@example.com",
        PaymentStatus = "Paid",
        PaymentDate = new DateTime(2024, 3, 25),
        SessionId = "sess_3",
        PaymentIntentId = "pi_3"
    },
    new Order
    {
        OrderId = 4,
        UserId = 2,
        SellerId = 1,
        OrderDate = new DateTime(2024, 5, 12),
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 180000,
        FullName = "Trần Văn C",
        PhoneNumber = "0912345678",
        Address = "789 Nguyễn Huệ",
        City = "Huế",
        Country = "Việt Nam",
        Email = "c@example.com",
        PaymentStatus = "Paid",
        PaymentDate = new DateTime(2024, 5, 12),
        SessionId = "sess_4",
        PaymentIntentId = "pi_4"
    },
    new Order
    {
        OrderId = 5,
        UserId = 2,
        SellerId = 1,
        OrderDate = new DateTime(2024, 7, 20),
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 160000,
        FullName = "Phạm Thị D",
        PhoneNumber = "0987766554",
        Address = "12 Hai Bà Trưng",
        City = "Cần Thơ",
        Country = "Việt Nam",
        Email = "d@example.com",
        PaymentStatus = "Paid",
        PaymentDate = new DateTime(2024, 7, 20),
        SessionId = "sess_5",
        PaymentIntentId = "pi_5"
    },
    new Order
    {
        OrderId = 6,
        UserId = 2,
        SellerId = 1,
        OrderDate = new DateTime(2024, 10, 8),
        LastUpdated = DateTime.Now,
        Status = "Completed",
        TotalAmount = 210000,
        FullName = "Đinh Văn E",
        PhoneNumber = "0977001122",
        Address = "88 Pasteur",
        City = "Nha Trang",
        Country = "Việt Nam",
        Email = "e@example.com",
        PaymentStatus = "Paid",
        PaymentDate = new DateTime(2024, 10, 8),
        SessionId = "sess_6",
        PaymentIntentId = "pi_6"
    }
);
        // 1. Tạo thêm 1 user mới đóng vai trò Customer
        var customerUser = new User
        {
            Id = 3,
            UserName = "customer1",
            NormalizedUserName = "CUSTOMER1",
            Email = "he1808817nguyenthaihuy@gmail.com",
            NormalizedEmail = "CUSTOMER1@EXAMPLE.COM",
            FullName = "Nguyễn Khách Hàng",
            Address = "123 Đường Khỏe Mạnh",
            City = "Biên Hòa",
            PhoneNumber = "0909999999",
            Country = "Việt Nam",
            Gender = "Male",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        customerUser.PasswordHash = hasher.HashPassword(customerUser, "Customer@123");

        // 2. Gán role USER (Id = 2)
        modelBuilder.Entity<User>().HasData(customerUser);
        modelBuilder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int>
            {
                UserId = 3,
                RoleId = 2
            }
        );

        // 3. Gán thêm CustomerProfile cho user này
        modelBuilder.Entity<CustomerProfile>().HasData(
            new CustomerProfile
            {
                ProfileId = 1,
                UserId = 3,
                Gender = "Male",
                Height = 170.0m,
                Weight = 65.0m,
                Bmi = 21
            }
        );



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
