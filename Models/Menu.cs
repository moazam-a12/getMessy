using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;



public class Menu
{
    public int Id { get; set; }

//    [Required]
public required string Name { get; set; }

//    [Required]
    public decimal Price { get; set; }

//    [Required]
    public DateTime Date { get; set; }

    public bool IsFood { get; set; } = true; 
}

