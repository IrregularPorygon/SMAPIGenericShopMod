using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace GenericShopExtender
{
    public class ModEntry : Mod
    {

        private ModConfig _config;

        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.MenuChanged += Events_MenuChanged;
            //MenuEvents.MenuChanged += Events_MenuChanged;

            _config = Helper.ReadConfig<ModConfig>();
            //this.Monitor.Log("Printing the configuration", LogLevel.Info);
            //this.Monitor.Log(config.ToString(), LogLevel.Info);
            //foreach (string s in config.shopkeepers.Keys)
            //{
            //    this.Monitor.Log(s, LogLevel.Info);
            //}
        }

        void Events_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            //Is this even a shop?
            var menu = e.NewMenu as ShopMenu;
            if (menu != null)
            {
                //Yup, sure is.
                ShopMenu currentShopMenu = menu;
                //But who runs it?
                foreach (string shopkeep in _config.Shopkeepers.Keys)
                {
                    string formattedShopkeep = shopkeep;
                    int yearDefined = 0;
                    List<string> seasonsDefined = new List<string>();
                    //this.Monitor.Log("Checking " + shopkeep + " if it contains " + "_Year", LogLevel.Info);
                    if (formattedShopkeep.Contains("_Year"))
                    {
                        Monitor.Log("Found a year modifier", LogLevel.Info);
                        yearDefined = Int32.Parse(formattedShopkeep.Substring(formattedShopkeep.IndexOf("_Year", StringComparison.Ordinal) + 5,1));
                        formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_Year", StringComparison.Ordinal), 6);
                        Monitor.Log(formattedShopkeep, LogLevel.Info);
                        Monitor.Log("With year " + yearDefined, LogLevel.Info);
                    }

                    if(formattedShopkeep.Contains("_Season"))
                    {
                        Monitor.Log("Found a season modifier", LogLevel.Info);
                        if (formattedShopkeep.IndexOf("_spring", StringComparison.Ordinal) != -1)
                        {
                            seasonsDefined.Add("spring");
                            formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_spring", StringComparison.Ordinal), 7);
                        }
                        if (formattedShopkeep.IndexOf("_summer", StringComparison.Ordinal) != -1)
                        {
                            seasonsDefined.Add("summer");
                            formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_summer", StringComparison.Ordinal), 7);
                        }
                        if (formattedShopkeep.IndexOf("_winter", StringComparison.Ordinal) != -1)
                        {
                            seasonsDefined.Add("winter");
                            formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_winter", StringComparison.Ordinal), 7);
                        }
                        if (formattedShopkeep.IndexOf("_fall", StringComparison.Ordinal) != -1)
                        {
                            seasonsDefined.Add("fall");
                            formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_fall", StringComparison.Ordinal), 5);
                        }
                        formattedShopkeep = formattedShopkeep.Remove(formattedShopkeep.IndexOf("_Season", StringComparison.Ordinal), 7);
                    }
                    else
                    {
                        seasonsDefined.Add("spring");
                        seasonsDefined.Add("summer");
                        seasonsDefined.Add("winter");
                        seasonsDefined.Add("fall");
                    }
                    Monitor.Log(formattedShopkeep, LogLevel.Info);

                    if (currentShopMenu.portraitPerson.Equals(Game1.getCharacterFromName(formattedShopkeep)) && Game1.year >= yearDefined && seasonsDefined.Contains(Game1.currentSeason))
                    {
                        //The current shopkeep does! Now we need to get the list of what's being sold
                        IReflectedField<Dictionary<Item, int[]>> inventoryInformation = Helper.Reflection.GetField<Dictionary<Item, int[]>>(currentShopMenu, "itemPriceAndStock");
                        Dictionary<Item, int[]> itemPriceAndStock = inventoryInformation.GetValue();
                        IReflectedField<List<Item>> forSaleInformation = Helper.Reflection.GetField<List<Item>>(currentShopMenu, "forSale");
                        List<Item> forSale = forSaleInformation.GetValue();

                        //Now, lets add a few things...
                        int[,] itemsAndPrices = _config.Shopkeepers[shopkeep];
                        for (int index = 0; index < itemsAndPrices.GetLength(0); index++)
                        {
                            int itemId = itemsAndPrices[index, 0];
                            int price = itemsAndPrices[index, 1];
                            Item objectToAdd = new Object(Vector2.Zero, itemId, int.MaxValue);
                            itemPriceAndStock.Add(objectToAdd, new[] { price, int.MaxValue });
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