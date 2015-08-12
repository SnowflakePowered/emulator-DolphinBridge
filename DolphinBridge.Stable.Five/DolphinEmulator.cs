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
  
    public abstract class DolphinBridge : EmulatorBridge
    {
        protected string coreName;

        protected DolphinBridge([Import("coreInstance")] ICoreService coreInstance) : 
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
            //todo implement this for dolphin
        }
        
        protected virtual void ProcessFlags(IGameInfo game, ref IConfigurationProfile configurationProfile)
        {
  
        }

        /* Win32 Start */
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        /* Win32 End */
        public override string CompileController(int playerIndex, IPlatformInfo platformInfo, IControllerDefinition controllerDefinition, IControllerTemplate controllerTemplate, IGamepadAbstraction gamepadAbstraction, IInputTemplate inputTemplate, IGameInfo game)
        {

            return null;//todo implement this for dolphin
        }
        public override void ShutdownEmulator()
        {
            this.dolphinInstance.CloseMainWindow();
        }

    }
}
