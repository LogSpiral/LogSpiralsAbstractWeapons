using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace LogSpiralsAbstractWeapons.Contents.BoulderGod;

public static class BoulderGodHelper
{
    public static void BoulderMessage(string textOrKey,bool isKey = true) 
    {
        if (isKey)
            textOrKey = Language.GetTextValue($"Mods.LogSpiralsAbstractWeapons.NPCs.BoulderGod.{textOrKey}");
        Main.NewText($"[c/999999:{textOrKey}]");
    }
}
