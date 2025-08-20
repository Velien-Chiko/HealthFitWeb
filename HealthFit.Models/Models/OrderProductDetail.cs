﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthFit.Models;

[Table("OrderProductDetails")]
public partial class OrderProductDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int  ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
