﻿using RestaurantAPI.Enums;

namespace RestaurantAPI.Models.DTOs
{
    public class OrderPostModel
    {
        public StatusEnum Status { get; set; }
        public int Price { get; set; }
        public int TableModelId { get; set; }
        public int DishModelId { get; set; }
        public string IdentityUserId { get; set; }
    }
}
