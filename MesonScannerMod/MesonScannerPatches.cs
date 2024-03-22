using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Networks;
using UnityEngine;

namespace MesonScannerMod
{
    [HarmonyPatch(typeof(SPUMesonScanner), "Render")]
    public class SPUMesonScannerRender
    {
        static void AddPipesToBatch()
        {
            foreach (PipeNetwork allPipeNetwork in PipeNetwork.AllPipeNetworks)
            {
                foreach (INetworkedStructure structure in allPipeNetwork.StructureList)
                {
                    if (structure is Pipe pipe && !pipe.IsOccluded && !(pipe is HydroponicTray))
                    {
                        if (Utils.IsFiltered(pipe))
                        {
                            SPUMesonScannerAddToBatch.AddToBatch(pipe);
                        }
                    }
                }
            }
        }

        static void AddCablesToBatch()
        {
            foreach (CableNetwork allCableNetwork in CableNetwork.AllCableNetworks)
            {
                foreach (Cable cable in allCableNetwork.CableList)
                {
                    if (!cable.IsOccluded)
                    {
                        if (Utils.IsFiltered(cable))
                        {
                            SPUMesonScannerAddToBatch.AddToBatch(cable);
                        }
                    }
                }
            }
        }

        static void AddChutesToBatch()
        {
            foreach (ChuteNetwork allChuteNetwork in ChuteNetwork.AllChuteNetworks)
            {
                foreach (Chute chute in allChuteNetwork.StructureList)
                {
                    if (!chute.IsOccluded)
                    {
                        if (Utils.IsFiltered(chute))
                        {
                            SPUMesonScannerAddToBatch.AddToBatch(chute);
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        public static bool Prefix(SPUMesonScanner __instance)
        {
            if (InventoryManager.Instance.ActiveHand.Slot.Get() is SprayCan sprayCan)
            {
                Utils.colorSwitch = sprayCan.GetPaintMaterial();
            }
            else if (InventoryManager.Instance.InactiveHand.Slot.Get() is SprayCan sprayCan2)
            {
                Utils.colorSwitch = sprayCan2.GetPaintMaterial();
            }
            else
            {
                Utils.colorSwitch = null;
            }

            switch (Utils.CurrentMode)
            {
                case Utils.Mode.All:
                    AddPipesToBatch();
                    AddCablesToBatch();
                    AddChutesToBatch();
                    break;
                case Utils.Mode.Pipes:
                    AddPipesToBatch();
                    break;
                case Utils.Mode.Cables:
                    AddCablesToBatch();
                    break;
                case Utils.Mode.Chutes:
                    AddChutesToBatch();
                    break;
                default:
                    break;
            }
            SPUMesonScannerRenderMeshes.RenderMeshes(__instance);
            SPUMesonScannerCleanUp.CleanUp(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "AddToBatch")]
    public class SPUMesonScannerAddToBatch
    {
        [HarmonyReversePatch]
        public static void AddToBatch(Structure structure)
        {
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "RenderMeshes")]
    public class SPUMesonScannerRenderMeshes
    {
        [HarmonyReversePatch]
        public static void RenderMeshes(SPUMesonScanner instance)
        {
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "CleanUp")]
    public class SPUMesonScannerCleanUp
    {
        [HarmonyReversePatch]
        public static void CleanUp(SPUMesonScanner instance)
        {
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "NormalMode")]
    public class KeyToggles
    {
        [HarmonyPrefix]
        public static void Prefix(InventoryManager __instance)
        {
            if (KeyManager.GetButtonDown(KeyCode.Mouse2))
            {
                if (InventoryManager.Parent is Human human)
                {
                    if (human.GlassesSlot.Get() is SensorLenses lenses)
                    {
                        if (lenses.Sensor is SPUMesonScanner)
                        {
                            Utils.NextMode();
                        }
                    }
                }
            }
        }
    }

    public class Utils
    {
        public static Material colorSwitch;
        public static Mode CurrentMode = Mode.All;
        public enum Mode { All, Pipes, Cables, Chutes }

        public static bool IsFiltered(Thing thing)
        {
            if (colorSwitch == null)
            {
                return true;
            }

            if (thing.CustomColor.Normal == null && colorSwitch == thing.PaintableMaterial)
            {
                return true;
            }

            if (colorSwitch == thing.CustomColor.Normal)
            {
                return true;
            }

            return false;
        }

        public static void NextMode()
        {
            switch (CurrentMode)
            {
                case Mode.All:
                    CurrentMode = Mode.Pipes;
                    ConsoleWindow.Print("Set Mode Pipes");
                    break;
                case Mode.Pipes:
                    CurrentMode = Mode.Cables;
                    ConsoleWindow.Print("Set Mode Cables");
                    break;
                case Mode.Cables:
                    CurrentMode = Mode.Chutes;
                    ConsoleWindow.Print("Set Mode Chutes");
                    break;
                case Mode.Chutes:
                    CurrentMode = Mode.All;
                    ConsoleWindow.Print("Set Mode <All>");
                    break;
                default:
                    break;
            }
        }
    }
}
