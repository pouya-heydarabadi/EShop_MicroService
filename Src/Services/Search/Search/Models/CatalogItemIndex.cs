﻿using BuildingBlocks.Messaging.Events;

namespace Search.Models;

public record CatalogItemIndex
{
    // Index Name
    public const string IndexName = "catalog-item-index";
    
    
    // Datas
    public string Name { get; set; } = default!;
    public List<string> Categories { get; set; } = new();
    public string Description { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public decimal Price { get; set; } = decimal.Zero;
    public Guid Id { get; set; }
    public DateTime OccurredOn { get; set; }
    public string EventType { get; set; }
};