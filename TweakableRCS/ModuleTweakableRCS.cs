﻿// TweakableRCS, a TweakableEverything module
//
// ModuleTweakableRCS.cs
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
using KSPAPIExtensions;
using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;

namespace TweakableRCS
{
	#if DEBUG
	public class ModuleTweakableRCS : DebugPartModule
	#else
	public class ModuleTweakableRCS : PartModule
	#endif
	{
		protected ModuleRCS RCSModule;

		// Stores whether the RCS block should start enabled or not.
		[KSPField(isPersistant = true, guiName = "Thruster", guiActive = false, guiActiveEditor = true)]
		[UI_Toggle(enabledText = "Enabled", disabledText = "Disabled")]
		public bool startEnabled;
		// Stores the last state of startEnabled so we can tell if it's changed.
		protected bool startEnabledState;

		// Stores our thrust limiter value for the RCS block.
		[KSPField(isPersistant = true, guiName = "Thrust Limiter", guiFormat = "P0",  guiActiveEditor = true, guiActive = true)]
		[UI_FloatEdit(minValue = 0f, maxValue = 1f, incrementSlide = .025f)]
		public float thrustLimit;

		protected float baseThrusterPower;

		public ModuleTweakableRCS()
		{
			this.startEnabled = true;
			this.thrustLimit = 1f;
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			if (this.thrustLimit > 1f)
			{
				this.thrustLimit /= 100f;
			}
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			this.RCSModule = base.part.getFirstModuleOfType<ModuleRCS>();
			var prefabModule = this.part.partInfo.partPrefab.getFirstModuleOfType<ModuleRCS>();

			// Only run the assignment if the module exists.
			if (this.RCSModule != null)
			{
				this.startEnabledState = !this.startEnabled;
			}

			if (prefabModule != null)
			{
				this.baseThrusterPower = prefabModule.thrusterPower;
			}

			var thrustLimitCtl = this.Fields["thrustLimit"].uiControlCurrent();

			if (thrustLimitCtl is UI_FloatEdit)
			{
				var thrustLimitSlider = thrustLimitCtl as UI_FloatEdit;

				thrustLimitSlider.maxValue = 1f;
				thrustLimitSlider.incrementSlide = 0.025f;
			}
		}

		// Runs late in the update cycle
		public void LateUpdate()
		{
			// Do nothing if the RCS module is null.
			if (this.RCSModule == null)
			{
				return;
			}

			// If we're in the editor...
			if (HighLogic.LoadedSceneIsEditor)
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
						this.RCSModule.rcsEnabled = true;
					}
					// ...otherwise, we're starting disabled...
					else
					{
						// ...set heel module to disabled
						this.RCSModule.rcsEnabled = false;
					}
				}
			}

			this.RCSModule.thrusterPower = this.baseThrusterPower * this.thrustLimit;

			FXGroup fx;
			for (int idx = 0; idx < this.RCSModule.thrusterFX.Count; idx++)
			{
				fx = this.RCSModule.thrusterFX[idx];
				fx.Power *= this.thrustLimit;
			}
		}

		[KSPAction("Enable Thruster")]
		public void ActionEnableThruster(KSPActionParam param)
		{
			this.RCSModule.Enable();
		}

		[KSPAction("Disable Thruster")]
		public void ActionDisableThruster(KSPActionParam param)
		{
			this.RCSModule.Disable();
		}

	}
}

