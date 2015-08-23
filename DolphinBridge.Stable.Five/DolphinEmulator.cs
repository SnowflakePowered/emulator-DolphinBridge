using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Snowflake.Controller;
using Snowflake.Emulator;
using Snowflake.Emulator.Configuration;
using Snowflake.Emulator.Input;
using Snowflake.Emulator.Input.InputManager;
using Snowflake.Game;
using Snowflake.InputManager;
using Snowflake.Platform;
using Snowflake.Service;

namespace DolphinBridge.Stable.Five
{
  
    public class DolphinEmulator : EmulatorBridge
    {
        [ImportingConstructor]
        public DolphinEmulator([Import("coreInstance")] ICoreService coreInstance) : 
            base(Assembly.GetExecutingAssembly(), coreInstance)
        {

        }

        private static readonly string dolphinCoreTemplate = "DolphinCore";
        private static readonly string dolphinGFXTemplate = "DolphinGFX";
        private static readonly string dolphinInputGCPadTemplate = "GCPadNew";
        private static readonly string dolphinInputWiimoteTemplate = "WiimoteNew";

        private Process dolphinInstance;

        public override void StartRom(IGameInfo game)
        {
            var platform = this.CoreInstance.LoadedPlatforms[game.PlatformID];
            string emulatorPath =
                Path.Combine(this.CoreInstance.EmulatorManager.GetAssemblyDirectory(this.EmulatorAssembly), this.EmulatorAssembly.MainAssembly);
            
            var gfxConfigProfile = this.ConfigurationTemplates[DolphinEmulator.dolphinGFXTemplate].ConfigurationStore.GetConfigurationProfile(game);
            var coreConfigProfile = this.ConfigurationTemplates[DolphinEmulator.dolphinCoreTemplate].ConfigurationStore.GetConfigurationProfile(game);

            coreConfigProfile.ConfigurationValues["SlotA"] = 8; //Force GCI Folder
            coreConfigProfile.ConfigurationValues["WiimoteContinuousScanning"] = true; //Force Continuous Scanning

            /* Plug in all gamecube controllers */
            coreConfigProfile.ConfigurationValues["SIDevice0"] = 6; 
            coreConfigProfile.ConfigurationValues["SIDevice1"] = 6; 
            coreConfigProfile.ConfigurationValues["SIDevice2"] = 6; 
            coreConfigProfile.ConfigurationValues["SIDevice3"] = 6; 


            this.ProcessFlags(game, ref coreConfigProfile, ref gfxConfigProfile);

            //compile configuration
            string coreCfg = this.CompileConfiguration(this.ConfigurationTemplates[DolphinEmulator.dolphinCoreTemplate], coreConfigProfile, game);

            var _core_backend = this.ConfigurationFlagStore.GetValue(game, "video_backend", ConfigurationFlagTypes.SELECT_FLAG);
            string gfxBackend = this.ConfigurationFlags["video_backend"].SelectValues[_core_backend].Value;

            string gfxCfg = this.CompileConfiguration(this.ConfigurationTemplates[DolphinEmulator.dolphinGFXTemplate], gfxConfigProfile, game);

            

            if(platform.PlatformID == StonePlatforms.NINTENDO_WII)
            {
                for (int i = 1; i <= 4; i++)
                { 
                    //build the first 4 as wiimotes
                    string controller = this.CompileController(i, platform, this.InputTemplates[DolphinEmulator.dolphinInputWiimoteTemplate], game);
                    File.AppendAllText(Path.Combine(this.PluginDataPath, "WiimoteNew.ini.tmp"), Environment.NewLine + controller);
                }
                for (int i = 5; i <= 8; i++)
                {
                    string controller = this.CompileController(i, platform, this.InputTemplates[DolphinEmulator.dolphinInputGCPadTemplate], game);
                    File.AppendAllText(Path.Combine(this.PluginDataPath, "GCPadNew.ini.tmp"), Environment.NewLine + controller);
                }

            }

            if (platform.PlatformID == StonePlatforms.NINTENDO_GCN)
            {
                for (int i = 1; i <= platform.MaximumInputs; i++)
                {
                    string controller = this.CompileController(i, platform, this.InputTemplates[DolphinEmulator.dolphinInputGCPadTemplate], game);
                    File.AppendAllText(Path.Combine(this.PluginDataPath, "GCPadNew.ini"), Environment.NewLine + controller);
                }
            }            
        }
        


        void ProcessFlags(IGameInfo game, ref IConfigurationProfile dolphinCore, ref IConfigurationProfile dolphinGfx)
        {

            var core_speakerdata = this.ConfigurationFlagStore.GetValue(game, "speakerdata", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinCore.ConfigurationValues["WiimoteEnableSpeaker"] = core_speakerdata;


            var core_dualcore = this.ConfigurationFlagStore.GetValue(game, "dualcore", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinCore.ConfigurationValues["CPUThread"] = core_dualcore;

            var core_fullscreen = this.ConfigurationFlagStore.GetValue(game, "fullscreen_mode", ConfigurationFlagTypes.SELECT_FLAG);
            dolphinCore.ConfigurationValues["Fullscreen"] = this.ConfigurationFlags["fullscreen_mode"].SelectValues[core_fullscreen].Value;

            var core_backend = this.ConfigurationFlagStore.GetValue(game, "video_backend", ConfigurationFlagTypes.SELECT_FLAG);
            dolphinGfx.ConfigurationValues["GFXBackend"] = this.ConfigurationFlags["video_backend"].SelectValues[core_backend].Value;

            var core_hle_audio = this.ConfigurationFlagStore.GetValue(game, "hle_audio", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinCore.ConfigurationValues["DSPHLE"] = core_hle_audio;

            var core_cpu_oc = this.ConfigurationFlagStore.GetValue(game, "cpu_oc", ConfigurationFlagTypes.INTEGER_FLAG);
            dolphinCore.ConfigurationValues["OverclockEnable"] = (core_cpu_oc == 100);
            dolphinCore.ConfigurationValues["Overclock"] = core_cpu_oc / 100d;

            var gfx_internal_res = this.ConfigurationFlagStore.GetValue(game, "internal_res", ConfigurationFlagTypes.SELECT_FLAG);
            dolphinGfx.ConfigurationValues["EFBScale"] = this.ConfigurationFlags["internal_res"].SelectValues[gfx_internal_res].Value;

            var gfx_per_pixel_lighting = this.ConfigurationFlagStore.GetValue(game, "per_pixel_lighting", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinGfx.ConfigurationValues["EnablePixelLighting"] = gfx_per_pixel_lighting;

            var gfx_vsync = this.ConfigurationFlagStore.GetValue(game, "vsync", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinGfx.ConfigurationValues["VSync"] = gfx_vsync;

            var gfx_fps = this.ConfigurationFlagStore.GetValue(game, "fps", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinGfx.ConfigurationValues["ShowFPS"] = gfx_fps;

            var gfx_widescreen_hack = this.ConfigurationFlagStore.GetValue(game, "widescreen_hack", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinGfx.ConfigurationValues["widescreenHack"] = gfx_widescreen_hack;

            var gfx_aspect_ratio = this.ConfigurationFlagStore.GetValue(game, "aspect_ratio", ConfigurationFlagTypes.SELECT_FLAG);
            dolphinGfx.ConfigurationValues["AspectRatio"] = this.ConfigurationFlags["aspect_ratio"].SelectValues[gfx_aspect_ratio].Value;

            var gfx_anisotropic_filtering = this.ConfigurationFlagStore.GetValue(game, "anisotropic_filtering", ConfigurationFlagTypes.SELECT_FLAG);
            dolphinGfx.ConfigurationValues["MaxAnisotropy"] = this.ConfigurationFlags["anisotropic_filtering"].SelectValues[gfx_anisotropic_filtering].Value;

            var gfx_crop = this.ConfigurationFlagStore.GetValue(game, "crop", ConfigurationFlagTypes.BOOLEAN_FLAG);
            dolphinGfx.ConfigurationValues["Crop"] = gfx_crop;

            var gfx_antialiasing_mode = this.ConfigurationFlagStore.GetValue(game, "antialiasing_mode", ConfigurationFlagTypes.SELECT_FLAG);
          
            var oglMSAALevels = new string[5] {
                "0", /* None */
                "1", /* 2x MSAA */
                "2", /* 4x MSAA */
                "3", /* 8x MSAA */
                "4" /* 2xSSAA */
            };

            var dxMSAALevels = new string[5] {
                "0", /* None */
                "1", /*Level 2 */
                "4", /* Level 4 */
                "21", /* Level 8 */
                "53" /* Level 8 (32 Samples) */
            };
            switch (this.ConfigurationFlags["video_backend"].SelectValues[core_backend].Value as string)
            {
                case "D3D":
                    dolphinGfx.ConfigurationValues["MSAA"] = dxMSAALevels[gfx_antialiasing_mode];
                    break;
                case "OGL":
                    dolphinGfx.ConfigurationValues["MSAA"] = oglMSAALevels[gfx_antialiasing_mode];
                    break;
                default:
                    //If for some reason the backend value is broken, use opengl on the safe side
                    dolphinGfx.ConfigurationValues["MSAA"] = oglMSAALevels[gfx_antialiasing_mode]; 
                    break;
            }
        }


        public override string CompileController(int playerIndex, IPlatformInfo platformInfo, IControllerDefinition controllerDefinition, IControllerTemplate controllerTemplate, IGamepadAbstraction gamepadAbstraction, IInputTemplate inputTemplate, IGameInfo gameInfo)
        {

            
            var controllerMappings = gamepadAbstraction.ProfileType == ControllerProfileType.KEYBOARD_PROFILE ?
               controllerTemplate.KeyboardControllerMappings : controllerTemplate.GamepadControllerMappings;

            string deviceName = gamepadAbstraction.DeviceName;
            IList<IInputDevice> devices = new InputManager().GetGamepads();
            if (deviceName == null) deviceName = String.Empty;

            int realWiimoteAmount = devices.Where(device => device.DI_ProductName.Contains("RVL-CNT")).Count();

            if(controllerDefinition.ControllerID == "WII_COMBINED_CONTROLLER")
            {
                //All wiimotes have the same attachment
                int wiimote_extension = this.ConfigurationFlagStore.GetValue(gameInfo, "wiimote_extension", ConfigurationFlagTypes.SELECT_FLAG);
                controllerMappings["default"].KeyMappings["EXTENSION"] = this.ConfigurationFlags["wiimote_extension"].SelectValues[wiimote_extension].Value; 

                if (playerIndex <= realWiimoteAmount)
                {
                    controllerMappings["default"].KeyMappings["SOURCE"] = "2"; //Real Wiimotes take precedence
                }
                else
                {
                    controllerMappings["default"].KeyMappings["SOURCE"] = "1"; //Emulated Wiimote
                }
            }
           
            if (deviceName.Equals(InputDeviceNames.KeyboardDevice, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerMappings["default"].KeyMappings["DEVICE"] = "DInput/0/Keyboard Mouse";
                return base.CompileController(playerIndex, platformInfo, controllerDefinition, controllerTemplate,
                    gamepadAbstraction, inputTemplate, controllerMappings, gameInfo);
            }
            string xinputDevice = "XInput/{0}/Gamepad";
            string dintpuDevice = "DInput/{0}/{1}";
            if (deviceName.Equals(InputDeviceNames.XInputDevice1, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerMappings["default"].KeyMappings["DEVICE"] =
                    String.Format(xinputDevice, 0);
            }
            else if (deviceName.Equals(InputDeviceNames.XInputDevice2, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerMappings["default"].KeyMappings["DEVICE"] =
                    String.Format(xinputDevice, 1);
            }
            else if (deviceName.Equals(InputDeviceNames.XInputDevice3, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerMappings["default"].KeyMappings["DEVICE"] =
                    String.Format(xinputDevice, 2);
            }
            else if (deviceName.Equals(InputDeviceNames.XInputDevice4, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerMappings["default"].KeyMappings["DEVICE"] =
                    String.Format(xinputDevice, 3);
            }
            else if (devices.Select(device => device.DI_ProductName).Contains(deviceName))
            {
                var device = devices.Where(d => d.DI_ProductName == deviceName).First();
                controllerMappings["default"].KeyMappings["DEVICE"] =
                    String.Format(dintpuDevice, device.DI_ProductName, device.DeviceIndex);
            }
            return base.CompileController(playerIndex, platformInfo, controllerDefinition, controllerTemplate, gamepadAbstraction, inputTemplate, controllerMappings, gameInfo);
        }

        /* Win32 Start */
        /* Win32 End */
       
        public override void ShutdownEmulator()
        {
            this.dolphinInstance.CloseMainWindow();
        }

        public override void HandlePrompt(string messagge)
        {
            return;
        }
    }
}
