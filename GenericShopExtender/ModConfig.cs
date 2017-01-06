using StardewModdingAPI;
using System.Collections.Generic;

namespace GenericShopExtender
{
    public class ModConfig : Config
    {
        public Dictionary<string, int[,]> shopkeepers { get; set; }

    }
}