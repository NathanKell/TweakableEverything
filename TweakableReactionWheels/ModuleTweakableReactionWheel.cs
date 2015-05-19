// TweakableReactionWheels, a TweakableEverything module
//
// ModuleTweakableReactionWheel.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using KSPAPIEL;
using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;

namespace TweakableEverything
{
	#if DEBUG && false
	public class ModuleTweakableReactionWheel : DebugPartModule
	#else
	public class ModuleTweakableReactionWheel : PartModule
	#endif
	{
		// Stores the reaction wheel module we're tweaking.
		protected ModuleReactionWheel reactionWheelModule;

		// Stores whether or not the wheel will start enabled.
		[KSPField(isPersistant = true, guiName = "Reaction Wheels", guiActive = false, guiActiveEditor = true)]
		[UI_Toggle(enabledText = "Enabled", disabledText = "Disabled")]
		public bool startEnabled;
		// Stores the last state of startEnabled so we can tell if it's changed.
		protected bool startEnabledState;

		// Stores our tweaked value for roll torque.
		[KSPField(isPersistant = true, guiName = "Roll Torque", guiUnits = "kN-m",
			guiFormat = "F2", guiActiveEditor = true)]
		[UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 1f)]
		public float RollTorque;

		// Stores our tweaked value for pitch torque.
		[KSPField(isPersistant = true, guiName = "Pitch Torque", guiUnits = "kN-m",
			guiFormat = "F2", guiActiveEditor = true)]
		[UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 1f)]
		public float PitchTorque;

		// Stores our tweaked value for yaw torque.
		[KSPField(isPersistant = true, guiName = "Yaw Torque", guiUnits = "kN-m",
			guiFormat = "F2", guiActiveEditor = true)]
		[UI_FloatEdit(scene = UI_Scene.Editor, incrementSlide = 1f)]
		public float YawTorque;

		// Stores our value for all-axis torque gain
		[KSPField(isPersistant = true, guiName = "Torque Limiter", guiActive = true, guiActiveEditor = false)]
		[UI_FloatEdit(minValue = 0f, maxValue = 100f, incrementSlide = 2f)]
		public float TorqueGain;

		// Construct ALL the objects.
		public ModuleTweakableReactionWheel()
		{
			// Default to starting enabled, per Squad's behavior.
			this.startEnabled = true;

			// -1 will indicate an uninitialized value.
			this.RollTorque = -1;
			this.PitchTorque = -1;
			this.YawTorque = -1;

			this.TorqueGain = 100f;
		}

		// Runs on start.
		public override void OnStart(StartState state)
		{
			DebugLogger log = DebugLogger.New(this);

			#if DEBUG
			try {
			#endif
			
			log.AppendFormat("{0}: starting up", this.ToString());

			// Start up the underlying PartModule stuff.
			base.OnStart(state);

			log.Append("\n\tbase started up");

			// Seed the startEnabledState to the opposite of startEnabled to force first-update processing.
			this.startEnabledState = !this.startEnabled;

			log.AppendFormat("\n\tlast state seeded ({0} != {1})", this.startEnabledState, this.startEnabled);

			ModuleReactionWheel prefabModule;

			// Fetch the reaction wheel module.
			if (this.part.tryGetFirstModuleOfType<ModuleReactionWheel>(out this.reactionWheelModule))
			{
				log.AppendFormat("\n\tFound ModuleReactionWheel {0}", this.reactionWheelModule);

				if (PartLoader.getPartInfoByName(this.part.partInfo.name).partPrefab
					.tryGetFirstModuleOfType<ModuleReactionWheel>(out prefabModule))
				{
					log.AppendFormat("\n\tFound prefab module {0}", prefabModule);

					TweakableTools.InitializeTweakable<ModuleTweakableReactionWheel>(
						this.Fields["RollTorque"].uiControlCurrent(),
						ref this.RollTorque,
						ref this.reactionWheelModule.RollTorque,
						prefabModule.RollTorque
					);

					log.Append("\n\tRollTorque setup");

					TweakableTools.InitializeTweakable<ModuleTweakableReactionWheel>(
						this.Fields["PitchTorque"].uiControlCurrent(),
						ref this.PitchTorque,
						ref this.reactionWheelModule.PitchTorque,
						prefabModule.PitchTorque
					);

					log.Append("\n\tPitchTorque setup");

					TweakableTools.InitializeTweakable<ModuleTweakableReactionWheel>(
						this.Fields["YawTorque"].uiControlCurrent(),
						ref this.YawTorque,
						ref this.reactionWheelModule.YawTorque,
						prefabModule.YawTorque
					);

					log.Append("\n\tYawTorque setup");
				}
			}

			log.Append("\n\tStarted!");
			#if DEBUG
			} finally {
			log.Print();
			}
			#endif
		}

		// Runs late in the update cycle
		public void LateUpdate()
		{
			// If we're in the editor...
			if (HighLogic.LoadedSceneIsEditor && this.reactionWheelModule != null)
			{
				// ...and if our startEnabled state has changed...
				if (this.startEnabled != this.startEnabledState)
				{
					// ...refresh startEnabledState
					this.startEnabledState = this.startEnabled;

					// ...and if we're starting enabled...
					if (this.startEnabled)
					{
						// ...set the reaction wheel module to active
						this.reactionWheelModule.State = ModuleReactionWheel.WheelState.Active;
					}
					// ...otherwise, we're starting disabled...
					else
					{
						// ...set the reaction wheel module to disabled
						this.reactionWheelModule.State = ModuleReactionWheel.WheelState.Disabled;
					}
				}
			}

			if (HighLogic.LoadedSceneIsFlight && this.reactionWheelModule != null)
			{
				float gain = this.TorqueGain / 100f;

				this.reactionWheelModule.RollTorque *= gain;
				this.reactionWheelModule.PitchTorque *= gain;
				this.reactionWheelModule.YawTorque *= gain;
			}
		}
	}
}
