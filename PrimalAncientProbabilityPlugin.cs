﻿// https://github.com/User5981/Resu
// Primal Ancient Probability Plugin for TurboHUD Version 25/07/2019 18:25

using System;
using System.Globalization;
using Turbo.Plugins.Default;
using System.Linq;
using System.Collections.Generic;

namespace Turbo.Plugins.Resu
{
    public class PrimalAncientProbabilityPlugin : BasePlugin, IInGameTopPainter, ILootGeneratedHandler
    {
      
        public TopLabelDecorator ancientDecorator { get; set; }
        public TopLabelDecorator primalDecorator { get; set; }
        public string ancientText{ get; set; }
        public string primalText{ get; set; }
        public double ancientMarker{ get; set; }
        public double primalMarker{ get; set; }
        public double legendaryCount{ get; set; }
        public double prevInventoryLegendaryCount { get; set; }
        public double prevInventoryAncientCount { get; set; }
        public double prevInventoryPrimalCount { get; set; }
        public bool RunOnlyOnce { get; set; }
        public HashSet<string> legendaries = new HashSet<string>() {"0"};
        
        public PrimalAncientProbabilityPlugin()
        {
            Enabled = true;
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);
            ancientText = String.Empty;
            primalText = String.Empty;
            ancientMarker = 0;
            primalMarker = 0;
            legendaryCount = 0;
            prevInventoryLegendaryCount = 0;
            prevInventoryAncientCount = 0;
            prevInventoryPrimalCount = 0;
            RunOnlyOnce = true;
            
        }
        
         public void OnLootGenerated(IItem item, bool gambled)
        {
          if (item != null && item.SnoItem != null && item.SnoItem.MainGroupCode != "gems_unique" && item.SnoItem.MainGroupCode != "potion") 
           {
            string itemID = item.SnoItem.Sno.ToString() + item.CreatedAtInGameTick.ToString();
            if (item.IsLegendary && !legendaries.Contains(itemID)) legendaryCount++;
            if (item.AncientRank == 1 && !legendaries.Contains(itemID)) ancientMarker = legendaryCount;
            if (item.AncientRank == 2 && !legendaries.Contains(itemID)) primalMarker = legendaryCount;
            if (item.IsLegendary && !legendaries.Contains(itemID)) legendaries.Add(itemID);
           }
        } 
        
        public void OnNewArea(bool newGame, ISnoArea area)
        {
            if (newGame)
            {
                legendaries.Clear();
                legendaries.Add("0");
            }
        }
        
        public void PaintTopInGame(ClipState clipState)
        {
                
            if (Hud.Game.Me.CurrentLevelNormal != 70 && Hud.Game.Me.CurrentLevelNormal > 0)  {ancientMarker = legendaryCount; return;}
            if (Hud.Game.Me.HighestSoloRiftLevel < 70 && Hud.Game.Me.HighestSoloRiftLevel > 0) primalMarker = legendaryCount;
            
           if(RunOnlyOnce)
            {
             if (Hud.Tracker.CurrentAccountTotal.DropLegendary != 0 && Hud.Tracker.CurrentAccountTotal.DropPrimalAncient > 0)
             {
               legendaryCount = Hud.Tracker.CurrentAccountTotal.DropLegendary;
               if (Hud.Tracker.CurrentAccountTotal.DropAncient <= 1 && (legendaryCount - Hud.Tracker.CurrentAccountTotal.DropAncient) > 100) ancientMarker = legendaryCount;
               else
                   {
                    long DropAncientTotal = Hud.Tracker.CurrentAccountTotal.DropAncient;
                    if (DropAncientTotal == 0) DropAncientTotal = 1;
                        var StatDrop = legendaryCount / (100 - 90.2246666);
                        var StatDropRate = StatDrop / legendaryCount * 100;
                        var ActualDropRate = DropAncientTotal / legendaryCount * 100;
                        var FixedDropRate = StatDropRate - ActualDropRate;
                        if (FixedDropRate <= 0) ancientMarker = legendaryCount;
                        else ancientMarker = (int)(legendaryCount + (legendaryCount*(FixedDropRate/100)));
                    }
               if (Hud.Tracker.CurrentAccountTotal.DropPrimalAncient <= 1) primalMarker = legendaryCount;
               else 
                   {
                    long DropPrimalAncientTotal = Hud.Tracker.CurrentAccountTotal.DropPrimalAncient;
                    if (DropPrimalAncientTotal == 0) DropPrimalAncientTotal = 1;
                        var StatDrop = legendaryCount / (100 - 99.7753334);
                        var StatDropRate = StatDrop / legendaryCount * 100;
                        var ActualDropRate = DropPrimalAncientTotal / legendaryCount * 100;
                        var FixedDropRate = StatDropRate - ActualDropRate;
                        if (FixedDropRate <= 0)
                            primalMarker = legendaryCount;
                        else
                            primalMarker = (int)(legendaryCount + (legendaryCount * (FixedDropRate / 100)));
                    }
             } 
             RunOnlyOnce = false;
            } 

            long PrimalAncientTotal = Hud.Tracker.CurrentAccountTotal.DropPrimalAncient;
            long AncientTotal = Hud.Tracker.CurrentAccountTotal.DropAncient;
            long LegendariesTotal = Hud.Tracker.CurrentAccountTotal.DropLegendary;
            string TotalPercPrimal = ((float)PrimalAncientTotal / (float)LegendariesTotal).ToString("0.00%");
            string TotalPercAncient = ((float)AncientTotal / (float)LegendariesTotal).ToString("0.00%");
                    
             ancientDecorator = new TopLabelDecorator(Hud)
            {
                 TextFont = Hud.Render.CreateFont("arial", 6, 220, 227, 153, 25, true, false, 255, 0, 0, 0, true),
                 TextFunc = () => ancientText,
                 HintFunc = () => "Chance for the next Legendary drop to be Ancient." + Environment.NewLine + "Total Ancient drops : " + AncientTotal + " (" + TotalPercAncient + ") of Legendary drops",
              
            };
            
             primalDecorator = new TopLabelDecorator(Hud)
            {
                 TextFont = Hud.Render.CreateFont("arial", 6, 180, 255, 64, 64, true, false, 255, 0, 0, 0, true),
                 TextFunc = () => primalText,
                 HintFunc = () => "Chance for the next Legendary drop to be Primal Ancient." + Environment.NewLine + "Total Primal Ancient drops : " + PrimalAncientTotal + " (" + TotalPercPrimal + ") of Legendary drops",
              
            };
            
           
            double probaAncient = 0;
            double probaPrimal = 0;
            double powAncient = legendaryCount-ancientMarker;
            double powPrimal = legendaryCount-primalMarker;
            double ancientMaths = 90.2246666/100; 
            double primalMaths = 99.7753334/100; 
            
            if (powAncient <= 0) powAncient = 1;
            if (powPrimal <= 0) powPrimal = 1;
            
            probaAncient = (1 - Math.Pow(ancientMaths, powAncient))*100;
            probaPrimal = (1 - Math.Pow(primalMaths, powPrimal))*100;
            
            probaAncient = Math.Round(probaAncient, 2);
            probaPrimal = Math.Round(probaPrimal, 2);
            
            
            ancientText = "A: " + probaAncient  + "%";
            primalText =  "P: " + probaPrimal  + "%";

            var uiRect = Hud.Render.GetUiElement("Root.NormalLayer.game_dialog_backgroundScreenPC.game_progressBar_healthBall").Rectangle;
            
            ancientDecorator.Paint(uiRect.Right - (uiRect.Width/1.6f), uiRect.Top + uiRect.Height * 0.88f, 50f, 50f, HorizontalAlign.Left);

            if (Hud.Game.Me.HighestSoloRiftLevel >= 70)
            {
            primalDecorator.Paint(uiRect.Right - (uiRect.Width / 4.1f), uiRect.Top + uiRect.Height * 0.88f, 50f, 50f, HorizontalAlign.Left);
            }
            
            bool KanaiRecipe = Hud.Render.GetUiElement("Root.NormalLayer.Kanais_Recipes_main").Visible;
            double InventoryLegendaryCount = 0;
            double InventoryAncientCount = 0;
            double InventoryPrimalCount = 0;
            
            foreach (var item in Hud.Inventory.ItemsInInventory)
                    {
                     if (item != null) 
                      {
                       if (item.SnoItem == null || item.SnoItem.MainGroupCode == "gems_unique" || item.SnoItem.MainGroupCode == "potion") continue;
                                              
                       string itemID = item.SnoItem.Sno.ToString() + item.CreatedAtInGameTick.ToString();
                       if (item.IsLegendary && !legendaries.Contains(itemID)) InventoryLegendaryCount++;
                       if (item.AncientRank == 1 && !legendaries.Contains(itemID)) InventoryAncientCount++;
                       if (item.AncientRank == 2 && !legendaries.Contains(itemID)) InventoryPrimalCount++;
                       if (item.IsLegendary && !legendaries.Contains(itemID)) legendaries.Add(itemID);
                      }
                    }
            
            if (KanaiRecipe)
               {
                if (InventoryLegendaryCount > prevInventoryLegendaryCount) legendaryCount++; prevInventoryLegendaryCount = InventoryLegendaryCount;
                if (InventoryAncientCount > prevInventoryAncientCount) ancientMarker = legendaryCount; prevInventoryAncientCount = InventoryAncientCount;
                if (InventoryPrimalCount > prevInventoryPrimalCount) primalMarker = legendaryCount; prevInventoryPrimalCount = InventoryPrimalCount;
               }
            else prevInventoryLegendaryCount = InventoryLegendaryCount; prevInventoryAncientCount = InventoryAncientCount; prevInventoryPrimalCount = InventoryPrimalCount;
           

        }
    }
}