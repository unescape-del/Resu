﻿// https://github.com/User5981/Resu
// Deluxe Shrine labels plugin for TurboHUD version 07/09/2019 05:57
// Psycho's Shrine labels plugin with new features 

using Turbo.Plugins.Default;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Turbo.Plugins.Resu
{
    public class DeluxeShrineLabelsPlugin : BasePlugin, ICustomizer, IInGameWorldPainter
    {
        public Dictionary<ShrineType, WorldDecoratorCollection> ShrineDecorators { get; set; }
        public Dictionary<ShrineType, WorldDecoratorCollection> ShrineShortDecorators { get; set; }
        public Dictionary<ShrineType, string> ShrineCustomNames { get; set; }
        public Dictionary<ShrineType, string> ShrineCustomNamesShort { get; set;}
        public WorldDecoratorCollection PossibleRiftPylonDecorators { get; set; }
        public string PossibleRiftPylonName { get; set; }
        public bool ShowHealingWells { get; set;}
        public bool ShowPoolOfReflection { get; set;}
        public bool ShowAllWhenHealthIsUnder40 { get; set;}
        public WorldDecoratorCollection LeaveMessageDecorator { get; set; }

        public DeluxeShrineLabelsPlugin()
        {
            Enabled = true;
            ShowAllWhenHealthIsUnder40 = true;
        }

        public WorldDecoratorCollection CreateMapDecorators(float size = 6f, int a = 192, int r = 255, int g = 255, int b = 55, float radiusOffset = 5f)
        {
            return new WorldDecoratorCollection(
                new MapLabelDecorator(Hud)
                {
                    LabelFont = Hud.Render.CreateFont("tahoma", size, a, r, g, b, false, false, 128, 0, 0, 0, true),
                    RadiusOffset = radiusOffset,
                });
        }

        public WorldDecoratorCollection CreateGroundLabelDecorators(float size = 6f, int a = 192, int r = 255, int g = 255, int b = 55,int bga = 255, int bgr = 0, int bgg = 0, int bgb = 0)
        {
            var grounLabelBackgroundBrush = Hud.Render.CreateBrush(bga, bgr, bgg, bgb, 0);
            return new WorldDecoratorCollection(
                new GroundLabelDecorator(Hud)
                {
                    BackgroundBrush = grounLabelBackgroundBrush,
                    BorderBrush = Hud.Render.CreateBrush(a, r, g, b, 1),
                    TextFont = Hud.Render.CreateFont("tahoma", size, a, r, g, b, false, false, 128, 0, 0, 0, true),
                });
        }
        
        public override void Load(IController hud)
        {
            base.Load(hud);

            ShrineDecorators = new Dictionary<ShrineType, WorldDecoratorCollection>();
            ShrineShortDecorators = new Dictionary<ShrineType, WorldDecoratorCollection>();
            ShrineCustomNames = new Dictionary<ShrineType, string>();
            ShrineCustomNamesShort = new Dictionary<ShrineType, string>();
            
            
            foreach (ShrineType shrine in Enum.GetValues(typeof(ShrineType)))
            {
                ShrineDecorators[shrine] = CreateMapDecorators();
                ShrineShortDecorators[shrine] = CreateGroundLabelDecorators();
                ShrineCustomNames[shrine] = string.Empty;
                ShrineCustomNamesShort[shrine] = string.Empty;
            }
            PossibleRiftPylonDecorators = CreateMapDecorators();
            PossibleRiftPylonName = string.Empty;
            
          LeaveMessageDecorator = new WorldDecoratorCollection(
          new GroundLabelDecorator(Hud)
          {
           BackgroundBrush = Hud.Render.CreateBrush(0, 0, 0, 0, 0),
           TextFont = Hud.Render.CreateFont("tahoma", 16, 255, 255, 255, 255, true, true, true),
          });
        }

        public void Customize()
        {
            Hud.TogglePlugin<DeluxeShrineLabelsPlugin>(false);
        }

        public void PaintWorld(WorldLayer layer)
        {
            string NemesisMessage = string.Empty;
            string MyNemesisMessage = string.Empty;
            int MyNemesisCount = 0;
            foreach (var player in Hud.Game.Players.OrderBy(p => p.PortraitIndex))
            {
             if (player == null) continue;
             var Nemo = player.Powers.GetBuff(318820);
             if (Nemo == null || !Nemo.Active) {} 
             else
                {
                 if (player.IsMe) MyNemesisCount++;
                 else
                  {
                   if (NemesisMessage == string.Empty) NemesisMessage += Environment.NewLine + player.BattleTagAbovePortrait;
                   else NemesisMessage += Environment.NewLine + " or " + player.BattleTagAbovePortrait;
                  }
                }
            }
            
            
            if (NemesisMessage != string.Empty) NemesisMessage = "Leave for" + NemesisMessage;
            if (MyNemesisCount == 1) NemesisMessage = "HIT ME!";

            var GriftBar = Hud.Render.GreaterRiftBarUiElement;
            var RiftPercentage = Hud.Game.RiftPercentage;

            if (GriftBar.Visible && RiftPercentage > 95 && NemesisMessage == "HIT ME!")
             {
               NemesisMessage = "Keep for boss!";
             }

             var shrines = Hud.Game.Shrines.Where(x => !x.IsDisabled && !x.IsOperated);
            foreach (var shrine in shrines)
            {
                if (ShowAllWhenHealthIsUnder40 && Hud.Game.Me.Defense.HealthPct < (float)40) {ShowHealingWells = true; ShowPoolOfReflection = true;}
                else if (ShowAllWhenHealthIsUnder40 && Hud.Game.Me.Defense.HealthPct >= (float)40) {ShowHealingWells = false; ShowPoolOfReflection = false;}

                if (shrine.Type == ShrineType.HealingWell && ShowHealingWells == false) continue;
                if (shrine.Type == ShrineType.PoolOfReflection && ShowPoolOfReflection == false) continue;
                if (shrine.Type == ShrineType.HealingWell && ShowHealingWells == true || shrine.Type == ShrineType.PoolOfReflection && ShowPoolOfReflection == true) NemesisMessage =string.Empty;
                
                var shrineName = (ShrineCustomNames[shrine.Type] != string.Empty) ? ShrineCustomNames[shrine.Type] : shrine.SnoActor.NameLocalized;
                ShrineDecorators[shrine.Type].Paint(layer, shrine, shrine.FloorCoordinate, shrineName);

                var ShrineNameShort = (ShrineCustomNamesShort[shrine.Type] != string.Empty) ? ShrineCustomNamesShort[shrine.Type] : shrine.SnoActor.NameLocalized;
                
                switch (shrine.Type)
                {
                 case ShrineType.BlessedShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x000026E8) + " -25% damage received " + Char.ConvertFromUtf32(0x000026E8);
                      break;
                 case ShrineType.EnlightenedShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + "+25% EXP gain";
                      break;
                 case ShrineType.FortuneShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x00002728) + " +25% magic & gold find " + Char.ConvertFromUtf32(0x00002728);
                      break;
                 case ShrineType.FrenziedShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x00002694) + " +25% attack speed " + Char.ConvertFromUtf32(0x00002694);
                      break;
                 case ShrineType.EmpoweredShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + "+100% resource gain" + Environment.NewLine + Char.ConvertFromUtf32(0x0000231A) + " -50% cooldown time " + Char.ConvertFromUtf32(0x0000231A);
                      break;
                 case ShrineType.FleetingShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x0000D83C) + Char.ConvertFromUtf32(0x0000DFC3) + " +25% movement speed " + Char.ConvertFromUtf32(0x0000D83C) + Char.ConvertFromUtf32(0x0000DFC3) + Environment.NewLine + "+20yd pickup radius";
                      break;
                 case ShrineType.PowerPylon:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine +  Char.ConvertFromUtf32(0x0000D83D) +  Char.ConvertFromUtf32(0x0000DCAA) + " +400% damage dealt " +   Char.ConvertFromUtf32(0x0000D83D) +  Char.ConvertFromUtf32(0x0000DCAA);
                      break;
                 case ShrineType.ConduitPylon:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x000026A1) + " HIGH VOLTAGE " + Char.ConvertFromUtf32(0x000026A1);
                      break;
                 case ShrineType.ChannelingPylon:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x0000231A) + " -75% cooldown time " + Char.ConvertFromUtf32(0x0000231A)  + Environment.NewLine + "No resource cost";
                      break;
                 case ShrineType.ShieldPylon:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + "60s of invulnerability";
                      break;
                 case ShrineType.SpeedPylon:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x00002694) + " +30% attack speed " + Char.ConvertFromUtf32(0x00002694) + Environment.NewLine + Char.ConvertFromUtf32(0x0000D83C) + Char.ConvertFromUtf32(0x0000DFC3) + " +80% movement speed " + Char.ConvertFromUtf32(0x0000D83C) + Char.ConvertFromUtf32(0x0000DFC3);
                      break;
                 case ShrineType.PoolOfReflection:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + "+25% EXP gain"; NemesisMessage = string.Empty;
                      break;
                 case ShrineType.BanditShrine:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x0000D83D) + Char.ConvertFromUtf32(0x0000DCB0) + " GOBLINS! " + Char.ConvertFromUtf32(0x0000D83D) + Char.ConvertFromUtf32(0x0000DCB0);
                      break;
                 case ShrineType.HealingWell:
                      ShrineNameShort = ShrineNameShort + Environment.NewLine + Char.ConvertFromUtf32(0x00002764) + " restores life " + Char.ConvertFromUtf32(0x00002764); NemesisMessage = string.Empty;
                      break;
                }
                
                ShrineShortDecorators[shrine.Type].Paint(layer, shrine, shrine.FloorCoordinate, ShrineNameShort);
                if(shrine.FloorCoordinate.Offset(0, 0, 10).IsOnScreen()) LeaveMessageDecorator.Paint(layer, null, shrine.FloorCoordinate.Offset(0, 0, 10), NemesisMessage);
            }

            var riftPylonSpawnPoints = Hud.Game.Actors.Where(x =>  x.SnoActor.Sno == ActorSnoEnum._markerlocation_tieredriftpylon); //  428690
            foreach (var actor in riftPylonSpawnPoints)
            {
                PossibleRiftPylonDecorators.Paint(layer, actor, actor.FloorCoordinate, (PossibleRiftPylonName != string.Empty) ? PossibleRiftPylonName : "Pylon?");
            }
        }
    }
}