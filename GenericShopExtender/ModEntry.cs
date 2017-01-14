using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StardewValley.Menus;
using System;

namespace GenericShopExtender
{
    public class ModEntry : Mod
    {

        private ModConfig config;

        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            MenuEvents.MenuChanged += Events_MenuChanged;

            config = this.Helper.ReadConfig<ModConfig>();
            this.Monitor.Log("Printing the configuration", LogLevel.Warn);
            this.Monitor.Log(config.ToString(), LogLevel.Warn);
            foreach (string s in config.shopkeepers.Keys)
            {
                this.Monitor.Log(s, LogLevel.Warn);
            }
        }

        void Events_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            //Is this even a shop?
            if (e.NewMenu is ShopMenu)
            {
                //Yup, sure is.
                ShopMenu currentShopMenu = (ShopMenu)e.NewMenu;
                //But who runs it?
                foreach (string shopkeep in config.shopkeepers.Keys)
                {
                    string formattedShopkeep = shopkeep;
                    int yearDefined = 0;
                    this.Monitor.Log("Checking " + shopkeep + " if it contains " + "_Year", LogLevel.Warn);
                    if (shopkeep.Contains("_Year"))
                    {
                        this.Monitor.Log("Found a year modifier", LogLevel.Warn);
                        formattedShopkeep = shopkeep.Substring(0, shopkeep.IndexOf("_Year"));
                        yearDefined = Int32.Parse(shopkeep.Substring(shopkeep.IndexOf("_Year") + 5));
                        this.Monitor.Log(formattedShopkeep, LogLevel.Warn);
                        this.Monitor.Log("With year " + yearDefined, LogLevel.Warn);
                    }
                    if (currentShopMenu.portraitPerson.Equals(StardewValley.Game1.getCharacterFromName(formattedShopkeep, false)) && Game1.year >= yearDefined)
                    {
                        //The current shopkeep does! Now we need to get the list of what's being sold
                        IPrivateField<Dictionary<Item, int[]>> inventoryInformation = this.Helper.Reflection.GetPrivateField<Dictionary<Item, int[]>>(currentShopMenu, "itemPriceAndStock");
                        Dictionary<Item, int[]> itemPriceAndStock = inventoryInformation.GetValue();
                        IPrivateField<List<Item>> forSaleInformation = this.Helper.Reflection.GetPrivateField<List<Item>>(currentShopMenu, "forSale");
                        List<Item> forSale = forSaleInformation.GetValue();

                        //Now, lets add a few things...
                        int[,] itemsAndPrices = config.shopkeepers[shopkeep];
                        for (int index = 0; index < itemsAndPrices.GetLength(0); index++)
                        {
                            int itemId = itemsAndPrices[index, 0];
                            int price = itemsAndPrices[index, 1];
                            Item objectToAdd = (Item)new StardewValley.Object(Vector2.Zero, itemId, int.MaxValue);
                            itemPriceAndStock.Add(objectToAdd, new int[2] { price, int.MaxValue });
                            forSale.Add(objectToAdd);
                        }

                        //Now, lets update that shop inventory with the new items
                        inventoryInformation.SetValue(itemPriceAndStock);
                        forSaleInformation.SetValue(forSale);
                    }
                }
            }
        }
    }
}