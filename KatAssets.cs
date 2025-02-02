﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;

namespace Katarina
{
    class KatAssets
    {
        public static AssetBundle MainAssetBundle = null;

        public static void PopulateAssets()
        {
            if (MainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(MainPlugin.MODNAME + "." + "katarinaassets"))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }
            
            using (var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(MainPlugin.MODNAME + "." + "BattleQueenSounds.bnk"))
            {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
            
            /*using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(MainPlugin.MODNAME + "." + "Bomber.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }*/
        }
    }
}
