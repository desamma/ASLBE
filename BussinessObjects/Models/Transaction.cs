using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BussinessObjects.Models;

public class Transaction
{
    public Guid Id { get; set; }
    
    [Required]
    public long OrderCode { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Currency must be a positive number.")]
    public decimal CurrencyAmount { get; set; }
    
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? TransactionDate { get; set; } = DateTime.Now;
    
    public Guid UserId { get; set; }
    [ValidateNever]
    public virtual User User { get; set; }
}