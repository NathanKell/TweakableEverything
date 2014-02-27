using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TweakableEverything
{
	public class ModuleTweakableReactionWheel : PartModule
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
		[KSPField(isPersistant = true, guiName = "Roll Torque (kN-m)", guiActiveEditor = true)]
		[UI_FloatRange(minValue = float.MinValue, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float RollTorque;

		// Stores our tweaked value for pitch torque.
		[KSPField(isPersistant = true, guiName = "Pitch Torque (kN-m)", guiActiveEditor = true)]
		[UI_FloatRange(minValue = float.MinValue, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float PitchTorque;

		// Stores our tweaked value for yaw torque.
		[KSPField(isPersistant = true, guiName = "Yaw Torque (kN-m)", guiActiveEditor = true)]
		[UI_FloatRange(minValue = float.MinValue, maxValue = float.MaxValue, stepIncrement = 1f)]
		public float YawTorque;

		// Construct ALL the objects.
		public ModuleTweakableReactionWheel()
		{
			// Default to starting enabled, per Squad's behavior.
			this.startEnabled = true;

			// -1 will indicate an uninitialized value.
			this.RollTorque = -1;
			this.PitchTorque = -1;
			this.YawTorque = -1;
		}

		// Runs on start.
		public override void OnStart(StartState state)
		{
			// Start up the underlying PartModule stuff.
			base.OnStart(state);

			// Seed the startEnabledState to the opposite of startEnabled to force first-update processing.
			this.startEnabledState = !this.startEnabled;

			// Fetch the reaction wheel module.
			this.reactionWheelModule = base.part.Modules.OfType<ModuleReactionWheel>().FirstOrDefault();

			// If our roll torque value is uninitalized...
			if (this.RollTorque == -1)
			{
				// ...fetch it from the reaction wheel module.
				this.RollTorque = this.reactionWheelModule.RollTorque;
			}

			// Set the bounds and increment for our roll torque tweakable.
			((UI_FloatRange)this.Fields["RollTorque"].uiControlEditor).minValue = 0;
			((UI_FloatRange)this.Fields["RollTorque"].uiControlEditor).maxValue =
				this.reactionWheelModule.RollTorque * 2f;
			((UI_FloatRange)this.Fields["RollTorque"].uiControlEditor).stepIncrement =
				Mathf.Pow(10f, Mathf.RoundToInt(Mathf.Log10(this.reactionWheelModule.RollTorque)) - 1);

			// Clobber the reaction wheel module's roll torque with ours.
			this.reactionWheelModule.RollTorque = this.RollTorque;

			// If our pitch torque value is uninitalized...
			if (this.PitchTorque == -1)
			{
				// ...fetch it from the reaction wheel module.
				this.PitchTorque = this.reactionWheelModule.PitchTorque;
			}

			// Set the bounds and increment for our pitch torque tweakable.
			((UI_FloatRange)this.Fields["PitchTorque"].uiControlEditor).minValue = 0;
			((UI_FloatRange)this.Fields["PitchTorque"].uiControlEditor).maxValue =
				this.reactionWheelModule.PitchTorque * 2f;
			((UI_FloatRange)this.Fields["PitchTorque"].uiControlEditor).stepIncrement =
				Mathf.Pow(10f, Mathf.RoundToInt(Mathf.Log10(this.reactionWheelModule.PitchTorque)) - 1);

			// Clobber the reaction wheel module's pitch torque with ours.
			this.reactionWheelModule.PitchTorque = this.PitchTorque;

			// If our yaw torque value is uninitalized...
			if (this.YawTorque == -1)
			{
				// ...fetch it from the reaction wheel module.
				this.YawTorque = this.reactionWheelModule.YawTorque;
			}

			// Set the bounds and increment for our yaw torque tweakable.
			((UI_FloatRange)this.Fields["YawTorque"].uiControlEditor).minValue = 0;
			((UI_FloatRange)this.Fields["YawTorque"].uiControlEditor).maxValue =
				this.reactionWheelModule.YawTorque * 2f;
			((UI_FloatRange)this.Fields["YawTorque"].uiControlEditor).stepIncrement =
				Mathf.Pow(10f, Mathf.RoundToInt(Mathf.Log10(this.reactionWheelModule.YawTorque)) - 1);

			// Clobber the reaction wheel module's yaw torque with ours.
			this.reactionWheelModule.YawTorque = this.YawTorque;
		}

		// Runs late in the update cycle
		public void LateUpdate()
		{
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
		}
	}
}