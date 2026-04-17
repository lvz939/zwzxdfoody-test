using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FoodyTest1.Models
{
    internal class FoodDTO
    {
        [JsonPropertyName("name")]

        public string? Name { get; set; }

        [JsonPropertyName("description")]

        public string? Description { get; set; }

        [JsonPropertyName("url")]

        public string? Url { get; set; }
    }
}
