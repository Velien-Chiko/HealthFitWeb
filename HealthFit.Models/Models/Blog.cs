using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HealthFit.Models;

public partial class Blog
{
    [Key]
    public int BlogId { get; set; }

    public int? UserId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be less than 200 characters.")]
    [DisplayName("Title")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Content is required.")]
    [StringLength(5000, ErrorMessage = "Content must be less than 5000 characters.")]
    [DisplayName("Content")]
    public string Content { get; set; } = null!;

    [DisplayName("BMI Range")]
    [Range(0, 100, ErrorMessage = "BMI phải từ 0 đến 100.")]
    public double? Bmirange { get; set; }

    [DisplayName("Created Date")]
    [DataType(DataType.DateTime)]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    [DisplayName("Is Approved")]
    public bool? IsApproved { get; set; } = false;

    [DisplayName("Last Updated")]
    [DataType(DataType.DateTime)]
    public DateTime? LastUpdated { get; set; }

    [DisplayName("Image URL")]
    public string? ImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    public virtual User? User { get; set; }

}
